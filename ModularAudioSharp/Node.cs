using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp {
	public class Node {
		private IEnumerator<float> signal;
		private float current = 0f;

		/// <summary>
		/// このモジュールの出力を何個所で使っているか。
		/// 今のところ 0 から 1 になったときに更新セットに追加するだsけ
		/// </summary>
		private int userCount = 0;

		public Node(IEnumerable<float> signal) {
			this.signal = signal.GetEnumerator();
		}

		private Out Use() {
			if (userCount == 0) {
				ModuleSpace.AddCachingModule(this);
			}
			++ this.userCount;
			return new Out(this);
		}

		public IEnumerable<float> UseAsStream() {
			var o = this.Use();
			// ここで別メソッドに分けないと Use() の呼び出しがループ中まで遅延されてしまい、
			// コレクションのループ中にコレクションをいじった由の例外が発生する
			return OutAsStream(o);
		}

		private IEnumerable<float> OutAsStream(Out o) {
			while (true) {
				// 呼び出しの間で適宜 Update() が呼ばれて更新される
				yield return o.Value;
			}
		}

		public void Update() {
			if (this.signal.MoveNext()) {
				this.current = this.signal.Current;
			} else {
				this.current = 0;
			}
		}

//		private ModuleSpace Space { get { return ModuleSpace.Instance; } }

		internal class Out {
			private Node owner;
			internal Out(Node owner) { this.owner = owner; }
			public float Value { get { return this.owner.current; } }
		}

		public static implicit operator Node(float value) {
			return Nodes.Const(value);
		}

		public static Node operator +(Node lhs, Node rhs) {
			return Nodes.Calc(lhs, rhs, (l, r) => l + r);
		}

		public static Node operator -(Node lhs, Node rhs) {
			return Nodes.Calc(lhs, rhs, (l, r) => l - r);
		}

		public static Node operator *(Node lhs, Node rhs) {
			return Nodes.Calc(lhs, rhs, (l, r) => l * r);
		}

		public static Node operator /(Node lhs, Node rhs) {
			return Nodes.Calc(lhs, rhs, (l, r) => l / r);
		}
	}
}
