using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ModularAudioSharp.Data;

namespace ModularAudioSharp {

	/// <summary>
	/// 相互に接続し、サンプル単位で波形を生成するオブジェクトのクラス。
	/// 
	/// ノードには「能動的」なものと「受動的」なものがある。
	/// 能動的なノードには、さらに「本質的に」能動的なものと「非本質的に」（用語検討）能動的なものがある。
	///
	/// 本質的に能動的、とは、内部状態を持ち、自発的に変化を起こすこと。つまり次のいずれかに該当すること：
	/// 　・オシレータなど、自ら内部状態（位相）を変化させて波形を生成する
	/// 　・フィルタやディレイなど、バッファを持ち、入力が必ずしも変化しなくても出力を変化させる
	/// つまり、「時間の概念を持った」ノードであること、ともいえる。
	/// こういったノードは毎サンプルで必ず Update を行い、連続的な出力を生成してやる必要がある。
	///
	/// 非本質的に能動的とは、それ自体は状態を持たないが、
	/// 本質的に能動的なノードに直接・間接に依存するために毎サンプルの更新が必要になること。
	/// たとえば SinOsc(freq) * 0.5 + 0.25 のかけ算・足し算ノードがそれに当たる。
	/// 本質的には受動的だが、更新処理の都合により能動扱いされているノード、ともいえる。
	/// （本質的かどうかによらず）能動的なノードの更新処理は ModuleSpace から一元的に行われる。
	///
	/// 受動的なノードは、上記のいずれにも該当しないもの、つまり、それ自体状態を持たず、
	/// 能動的なノードに依存もしていないノードをいう。
	/// 定数や変数のノードや、それらのみの四則演算を行うノードなどは受動的である。
	/// 受動的なノードは、それが依存するノードの出力が更新されたときだけ更新すれば十分である。
	/// 受動的なノードの更新は、依存されるノードから依存するノードに向かう順で再帰的に行われる。
	/// </summary>
	public abstract class Node {

		/// <summary>
		/// Active のバッキングフィールド
		/// </summary>
		private readonly bool active;

		/// <summary>
		/// このノードが（本質的かどうかによらず）能動的かどうか
		/// </summary>
		internal bool Active => this.active;

		/// <summary>
		/// 依存ノード（このノードの出力を使う、能動的でないノード）のセット。
		/// このノードの状態を更新する際は、ついでにこれらの状態も更新する。
		/// 能動的なノードはここから更新しなくても更新されるので含めない
		/// </summary>
		private readonly List<Node> dependents = new List<Node>();

		/// <summary>
		/// 依存ノードのセット。能動的なノードは追加しない
		/// </summary>
		/// <param name="dependent"></param>
		internal void AddDependent(Node dependent) {
			if (dependent.Active) {
				Debug.Assert(false);
				return;
			}

			this.dependents.Add(dependent);
		}

		/// <summary>
		/// 次に作ったノードに振られる通し番号
		/// </summary>
		private static int nextNodeId = 0;

		/// <summary>
		/// ノードを作った順に振られる通し番号。デバッグ用
		/// </summary>
		private readonly int nodeId;

		/// <summary>
		/// ノードを識別する手がかりとなる文字列。デバッグ用
		/// </summary>
		private readonly string nodeTag;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="active">このノードが（本質的かどうかに依らず）能動的かどうか</param>
		protected Node(bool active) {
			this.active = active;

			// 以下はデバッグ用の情報。タグの付け方はあまりうまくないため検討の余地あり
			this.nodeId = nextNodeId;
			++ nextNodeId;
			var m = new StackFrame(3).GetMethod();
			this.nodeTag = $"{m.DeclaringType}.{m.Name}";
		}

		/// <summary>
		/// 全体で Update() が呼ばれた回数。パフォーマンスチューニング用
		/// </summary>
		public static int TimesUpdated { get; set; } = 0;

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="signal"></param>
		/// <param name="intrinsicallyActive"></param>
		/// <param name="dependencies"></param>
		/// <returns></returns>
		public static Node<T> Create<T>(IEnumerable<T> signal, bool intrinsicallyActive,
				params Node[] dependencies) where T : struct
				=> new Node<T>(signal, intrinsicallyActive, dependencies);

		// TODO ノードを一度使った状態から初期状態に戻すためのメソッドを提供する
		// public virtual void Initialize() { }

		/// <summary>
		/// このノードの内部状態を更新する。
		/// その後、このノードに依存する受動的ノードの内部状態を再帰的に更新する
		/// </summary>
		public void Update() {
			this.UpdateInner();
			// for の方がわずかに速いというので…
			// http://ftvoid.com/blog/post/291
			var depCount = this.dependents.Count;
			for (var i = 0 ; i < depCount ; ++i) this.dependents[i].Update();
		}

		/// <summary>
		/// このノードのみの内部状態を更新する
		/// </summary>
		protected abstract void UpdateInner();

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
				return Node.Create(((Node<int>) this).UseAsStream().Select(v => (float) v), false, this);
			}

			throw new InvalidCastException($"cannot convert node of type {this.ValueType} into node of float");
		}

		public Node<Stereo<float>> AsStereoFloat() {
			if (this.ValueType == typeof(Stereo<float>)) {
				return (Node<Stereo<float>>) this;
			} else if (this.ValueType == typeof(Stereo<int>)) {
				return Node.Create(((Node<Stereo<int>>) this).UseAsStream().Select(v => Stereo.Create((float) v.Left, (float) v.Right)), false, this);
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
				return Node.Create(((Node<float>) this).UseAsStream().Select(v => (int) v), false, this);
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

				return Node.Create(lStream.Zip(rStream, calc), false, lhs, rhs);
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
		//private readonly bool omitUpdate;
		private T current;

		/// <summary>
		/// このモジュールの出力を何個所で使っているか。
		/// 今のところ 0 から 1 になったときに更新セットに追加するだけ
		/// </summary>
		private int userCount = 0;

		public Node(IEnumerable<T> signal, bool intrinsicallyActive, params Node[] dependencies)
			: base(intrinsicallyActive || dependencies.Any(d => d.Active))
		{
			this.signal = signal.GetEnumerator();
			if (! this.Active) {
				foreach (var d in dependencies) d.AddDependent(this);
			}
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
				if (this.Active) {
					ModuleSpace.AddActiveNode(this);
				} else {
					// 一度も Update しないと値が出力されないので、ここで一度だけ
					this.Update();
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

		protected override void UpdateInner() {
			++ Node.TimesUpdated;
			if (this.signal.MoveNext()) {
				this.current = this.signal.Current;
			} else {
				this.current = default;
			}
		}

		// TODO 名前が適当なので変えたい
		public class Out {
			private readonly Node<T> owner;
			internal Out(Node<T> owner) { this.owner = owner; }

			/// <summary>
			/// 自分が属するノードの現在の値
			/// </summary>
			public T Value { get { return this.owner.current; } }
		}

		protected override Type ValueType { get { return typeof(T); } }

		public Node<TResult> Select<TResult>(Func<T, TResult> selector) where TResult : struct {
			var newStream = this.UseAsStream().Select(selector);
			return Node.Create(newStream, false, this);
		}

		public Node<Stereo<T>> AsStereo()
				// TODO Stereo の Stereo は作れないようにしたい
				=> this.Select(v => Stereo.Create(v, v));
	}
}
