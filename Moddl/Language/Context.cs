using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moddl.Language {
	class Context {
		private readonly Stack<Dictionary<string, Value>> frames = new Stack<Dictionary<string, Value>>();

		internal Context() {
			this.Push();
		}

		internal void Push() => this.frames.Push(new Dictionary<string, Value>());
		internal void Pop() {
			if (this.frames.Count <= 1) throw new InvalidOperationException("The root frame cannot be popped");
			this.frames.Pop();
		}

		internal Value this[string name] {
			get => this.frames.FirstOrDefault(f => f.ContainsKey(name))?[name]
					?? throw new ModdlNameNotDefinedException(name);
			// TODO 値の上書きは検出して ModdlException 派生を投げる
			set => this.frames.Peek().Add(name, value);
		}
	}
}
