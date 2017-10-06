using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp {
	public abstract class NodeController<T> where T : struct {
		private readonly Node<T> node;

		protected NodeController() {
			this.node = new Node<T>(this.Signal());
		}

		public Node<T> Node => this.node;

		protected abstract IEnumerable<T> Signal();

		public static implicit operator Node<T>(NodeController<T> ctrl) => ctrl.Node;
	}
}
