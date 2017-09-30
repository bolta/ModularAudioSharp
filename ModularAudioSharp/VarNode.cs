using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp {
	public class VarNode<T> : Node<T> where T : struct {
		private readonly Var var;

		private VarNode(Var var) : base(var.Signal()) { this.var = var; }

		public VarNode(T initValue) : this(new Var(initValue)) { }

		public void Set(T value) { this.var.Set(value); }

		/// <summary>
		/// 値を保持するヘルパークラス
		/// コンストラクタで base の引数にインスタンスメンバを与えられないため、値の管理を別クラスに切り出し、外から与えるようにした
		/// </summary>
		private class Var {
			T value;
			internal Var(T initValue) { this.value = initValue; }
			internal void Set(T value) => this.value = value;
			internal IEnumerable<T> Signal() { while (true) yield return this.value; }
		}

	}
}
