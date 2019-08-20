using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp {
	/// <summary>
	/// 他のノードの出力をそのまま自らの出力とする NodeController
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ProxyController<T> : NodeController<T> where T : struct {

		private IEnumerator<T> source = null;

		public Node<T> Source {
			set {
				if (this.source != null) throw new InvalidOperationException("source node already set");
				if (value == null) throw new ArgumentNullException();

				//this.source = value.UseAsStream();
				this.source = value.UseAsStream().GetEnumerator();
				value.AddDependent(this);
			}
		}

		protected override IEnumerable<T> Signal() {
			while (true) {
				// TODO source を設定する前に this.source.MoveNext() が評価されてしまうため、
				// ?. をかましているが、これでいいのか？　最初に余計な 0 が出力されたりしていないか？
				// （演奏までに source を設定して Update() していれば問題なさそうだが…）
				yield return this.source?.MoveNext() ?? false ? this.source.Current : default(T);
			}
		}
	}
}
