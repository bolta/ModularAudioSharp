using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ModularAudioSharp.Data;

namespace ModularAudioSharp {

	public abstract class Node {

		/// <summary>
		/// 全体で Update() が呼ばれた回数。パフォーマンスチューニング用
		/// </summary>
		public static int TimesUpdated { get; set; } = 0;

		public static Node<T> Create<T>(IEnumerable<T> signal, bool omitUpdate = false) where T : struct
				=> new Node<T>(signal, omitUpdate);

		// TODO ノードを一度使った状態から初期状態に戻すためのメソッドを提供する
		// public virtual void Initialize() { }

		public abstract void Update();

		public static implicit operator Node(float value) => Nodes.Const(value);
		public static implicit operator Node(int value) => Nodes.Const(value);

		/// <summary>
		/// デバッグ用の名前
		/// </summary>
		public string Name { get; set; } = "";


		public Node<float> AsFloat() {
			if (this.ValueType == typeof(float)) {
				return (Node<float>) this;
			} else if (this.ValueType == typeof(int)) {
				return Node.Create(((Node<int>) this).UseAsStream().Select(v => (float) v));
			}

			throw new InvalidCastException($"cannot convert node of type {this.ValueType} into node of float");
		}

		public Node<Stereo<float>> AsStereoFloat() {
			if (this.ValueType == typeof(Stereo<float>)) {
				return (Node<Stereo<float>>) this;
			} else if (this.ValueType == typeof(Stereo<int>)) {
				return Node.Create(((Node<Stereo<int>>) this).UseAsStream().Select(v => Stereo.Create((float) v.Left, (float) v.Right)));
			} else if (this.ValueType == typeof(float)) {
				return ((Node<float>) this).AsStereo();
			} else if (this.ValueType == typeof(int)) {
				return ((Node<int>) this).AsStereo().AsStereoFloat();
			}

			throw new InvalidCastException($"cannot convert node of type {this.ValueType} into node of float");
		}

		public Node<int> AsInt() {
			if (this.ValueType == typeof(int)) {
				return (Node<int>) this;
			} else if (this.ValueType == typeof(float)) {
				return Node.Create(((Node<float>) this).UseAsStream().Select(v => (int) v));
			}

			throw new InvalidCastException($"cannot convert node of type {this.ValueType} into node of int");
		}

		public Node<bool> AsBool() {
			if (this.ValueType == typeof(bool)) {
				return (Node<bool>) this;
			}

			throw new InvalidCastException($"cannot convert node of type {this.ValueType} into node of bool");
		}

		public static Node operator +(Node lhs, Node rhs) {
			return TryCalc<float, float, float>(lhs, rhs, (l, r) => l + r)
					?? TryCalc<float, int, float>(lhs, rhs, (l, r) => l + r)
					?? TryCalc<int, float, float>(lhs, rhs, (l, r) => l + r)
					?? TryCalc<int, int, int>(lhs, rhs, (l, r) => l + r)
					?? CalcFailed(lhs, "+", rhs);
		}

		public static Node operator -(Node lhs, Node rhs) {
			return TryCalc<float, float, float>(lhs, rhs, (l, r) => l - r)
					?? TryCalc<float, int, float>(lhs, rhs, (l, r) => l - r)
					?? TryCalc<int, float, float>(lhs, rhs, (l, r) => l - r)
					?? TryCalc<int, int, int>(lhs, rhs, (l, r) => l - r)
					?? CalcFailed(lhs, "-", rhs);
		}

		public static Node operator *(Node lhs, Node rhs) {
			return TryCalc<float, float, float>(lhs, rhs, (l, r) => l * r)
					?? TryCalc<float, int, float>(lhs, rhs, (l, r) => l * r)
					?? TryCalc<int, float, float>(lhs, rhs, (l, r) => l * r)
					?? TryCalc<int, int, int>(lhs, rhs, (l, r) => l * r)
					?? CalcFailed(lhs, "*", rhs);
		}

		public static Node operator /(Node lhs, Node rhs) {
			return TryCalc<float, float, float>(lhs, rhs, (l, r) => l / r)
					?? TryCalc<float, int, float>(lhs, rhs, (l, r) => l / r)
					?? TryCalc<int, float, float>(lhs, rhs, (l, r) => l / r)
					// 整数の割り算は常に実数に拡張する
					?? TryCalc<int, int, float>(lhs, rhs, (l, r) => ((float) l) / r)
					?? CalcFailed(lhs, "/", rhs);
		}

		private static Node TryCalc<TLhs, TRhs, TResult>(Node lhs, Node rhs, Func<TLhs, TRhs, TResult> calc)
				where TLhs : struct
				where TRhs : struct
				where TResult : struct {
			if (lhs.ValueType == typeof(TLhs) && rhs.ValueType == typeof(TRhs)) {
				var lStream = ((Node<TLhs>) lhs).UseAsStream();
				var rStream = ((Node<TRhs>) rhs).UseAsStream();

				return Node.Create(lStream.Zip(rStream, calc));
			}

			return null;
		}

		private static Node CalcFailed(Node lhs, string op, Node rhs) {
			throw new InvalidCastException($"cannot apply operation {op} to nodes of types {lhs.ValueType} and {rhs.ValueType}");
		}

		protected virtual Type ValueType { get; }
	}

	public class Node<T> : Node where T : struct {

		//public static implicit operator Node<T>(NodeController<T> ctrl) => ctrl.Node;

		private readonly IEnumerator<T> signal;
		private readonly bool omitUpdate;
		private T current;

		/// <summary>
		/// このモジュールの出力を何個所で使っているか。
		/// 今のところ 0 から 1 になったときに更新セットに追加するだけ
		/// </summary>
		private int userCount = 0;

		public Node(IEnumerable<T> signal, bool omitUpdate = false) {
			this.signal = signal.GetEnumerator();
			this.omitUpdate = omitUpdate;
		}

		/// <summary>
		/// このノードの出力を使うための Out オブジェクトを得る。
		/// 1 度だけ、ノードを ModuleSpace に登録する。
		/// 普通はこれをラップした UseAsStream() を使えばよい
		/// </summary>
		/// <param param name="updatePrior">
		/// 他のノードに先立って更新することを明示する場合に true を指定する。
		/// Sequencer での使用を想定
		/// </param>
		/// <returns></returns>
		public Out Use() {
			if (this.userCount == 0) {
				if (this.omitUpdate) {
					// 一度も Update しないと値が出力されないので、ここで一度だけ
					this.Update();
				} else {
					ModuleSpace.AddCachingNode(this);
				}
			}
			++ this.userCount;
			return new Out(this);
		}

		/// <summary>
		/// ノードの出力を IEnumerable によるストリームとして得る。
		/// Use() をラップしたもの
		/// </summary>
		/// <returns></returns>
		public IEnumerable<T> UseAsStream() {
			var o = this.Use();
			// ここで別メソッドに分けないと Use() の呼び出しがループ中まで遅延されてしまい、
			// コレクションのループ中にコレクションをいじった由の例外が発生する
			return OutAsStream(o);
		}

		private IEnumerable<T> OutAsStream(Out o) {
			while (true) {
				// 呼び出しの間で適宜 Update() が呼ばれて更新される
				yield return o.Value;
			}
		}

		public override void Update() {
			++ Node.TimesUpdated;
			if (this.signal.MoveNext()) {
				this.current = this.signal.Current;
			} else {
				this.current = default(T);
			}
		}

		// TODO 名前が適当なので変えたい
		public class Out {
			private Node<T> owner;
			internal Out(Node<T> owner) { this.owner = owner; }

			/// <summary>
			/// 自分が属するノードの現在の値
			/// </summary>
			public T Value { get { return this.owner.current; } }
		}

		protected override Type ValueType { get { return typeof(T); } }

		public Node<TResult> Select<TResult>(Func<T, TResult> selector) where TResult : struct {
			var newStream = this.UseAsStream().Select(selector);
			return Node.Create(newStream);
		}

		public Node<Stereo<T>> AsStereo()
				// TODO Stereo の Stereo は作れないようにしたい
				=> this.Select(v => Stereo.Create(v, v));
	}
}
