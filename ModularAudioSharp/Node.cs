using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp {

	public abstract class Node {
		public abstract void Update();

		public static implicit operator Node(float value) {
			return Nodes.Const(value);
		}
		public static implicit operator Node(int value) {
			return Nodes.Const(value);
		}

		/// <summary>
		/// デバッグ用の名前
		/// </summary>
		public string Name { get; set; } = "";

		public Node<float> AsFloat() {
			if (this.ValueType == typeof(float)) {
				return (Node<float>) this;
			} else if (this.ValueType == typeof(int)) {
				return new Node<float>(((Node<int>) this).UseAsStream().Select(v => (float) v));
			}

			throw new InvalidCastException($"cannot convert node of type {this.ValueType} into node of float");
		}

		public Node<int> AsInt() {
			if (this.ValueType == typeof(int)) {
				return (Node<int>) this;
			} else if (this.ValueType == typeof(float)) {
				return new Node<int>(((Node<float>) this).UseAsStream().Select(v => (int) v));
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

				return new Node<TResult>(lStream.Zip(rStream, calc));
			}

			return null;
		}

		private static Node CalcFailed(Node lhs, string op, Node rhs) {
			throw new InvalidCastException($"cannot apply operation {op} to nodes of types {lhs.ValueType} and {rhs.ValueType}");
		}

		protected virtual Type ValueType { get; }
	}

	public class Node<T> : Node where T : struct {

		private IEnumerator<T> signal;
		private T current;

		/// <summary>
		/// このモジュールの出力を何個所で使っているか。
		/// 今のところ 0 から 1 になったときに更新セットに追加するだsけ
		/// </summary>
		private int userCount = 0;

		public Node(IEnumerable<T> signal) {
			this.signal = signal.GetEnumerator();
		}

		public Out Use() {
			if (this.userCount == 0) {
				ModuleSpace.AddCachingNode(this);
			}
			++ this.userCount;
			return new Out(this);
		}

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
			public T Value { get { return this.owner.current; } }
		}

		protected override Type ValueType { get { return typeof(T); } }

	}
}
