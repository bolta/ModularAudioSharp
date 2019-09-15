using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp {
	/// <summary>
	/// 他のノードの出力をそのまま自らの出力とする NodeController。
	/// source が設定されていない場合は自らが保持する値を出力する。
	/// 保持する値は Set() で設定することができる。
	/// つまりこのノードは Var の上位互換である
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ProxyController<T> : NodeController<T> where T : struct {

		private IEnumerator<T> source = null;
		private T value = default;

		public ProxyController(Node<T> source) { this.Source = source; }
		public ProxyController(T initValue) { this.value = initValue; }
		public ProxyController() { }

		public Node<T> Source {
			set {
				if (this.source != null) throw new InvalidOperationException("source node already set");
				if (value == null) throw new ArgumentNullException();

				//this.source = value.UseAsStream();
				this.source = value.UseAsStream().GetEnumerator();
				value.AddDependent(this);
				this.Node.Update();
			}
		}

		/// <summary>
		/// 自ら保持する値を更新する。source が設定されている状態では無視する
		/// </summary>
		/// <param name="value"></param>
		public void Set(T value) {
			if (this.source != null) return;

			this.value = value;
			// 値が設定されたときだけ更新が必要。普段は不要
			this.Node.Update();
		}

		protected override IEnumerable<T> Signal() {
			while (true) {
				// TODO source を設定する前に this.source.MoveNext() が評価されてしまうため、
				// ?. をかましているが、これでいいのか？　最初に余計な 0 が出力されたりしていないか？
				// （演奏までに source を設定して Update() していれば問題なさそうだが…）
				yield return this.source?.MoveNext() ?? false ? this.source.Current : this.value;
			}
		}
	}
}
