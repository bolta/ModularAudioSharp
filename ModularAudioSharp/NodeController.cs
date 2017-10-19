using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp {
	public abstract class NodeController<T> where T : struct {
		private readonly Node<T> node;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="intrinsicallyActive">ノードが本質的に能動的かどうか</param>
		protected NodeController(bool intrinsicallyActive = false) {
			this.node = new Node<T>(this.Signal(), intrinsicallyActive);
		}

		public Node<T> Node => this.node;

		protected abstract IEnumerable<T> Signal();

		public static implicit operator Node<T>(NodeController<T> ctrl) => ctrl.Node;

		// 二項演算の一方が Node ならもう一方も Node に変換されるが、
		// 両方 NodeController だとだめなのでこちらでも演算子を定義する
		public static Node operator +(NodeController<T> lhs, NodeController<T> rhs) => lhs.Node + rhs.Node;
		public static Node operator -(NodeController<T> lhs, NodeController<T> rhs) => lhs.Node - rhs.Node;
		public static Node operator *(NodeController<T> lhs, NodeController<T> rhs) => lhs.Node * rhs.Node;
		public static Node operator /(NodeController<T> lhs, NodeController<T> rhs) => lhs.Node / rhs.Node;
	}
}
