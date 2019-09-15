using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moddl {
	internal class ModdlException : Exception {
		internal ModdlException() : base() { }
		internal ModdlException(string message) : base(message) { }
		internal ModdlException(string message, Exception inner) : base(message, inner) { }

		internal Position Position { get; set; } = null;

		public override string Message => string.Format("{0}: {1}",
				this.Position?.ToString() ?? "(position not available)",
				base.Message);
	}

	internal class ModdlIndexOutOfRangeException : ModdlException {
		internal int Index { get; private set; }
		internal int MaxIndex { get; private set; }
		internal ModdlIndexOutOfRangeException(int index, int maxIndex)
				: base($"The index {index} does not exist (there exist only indexes less than {maxIndex}).") {
			this.Index = index;
			this.MaxIndex = maxIndex;
		}
	}

	internal class ModdlTypeException : ModdlException {
		// TODO
	}

	internal class ModdlNameNotDefinedException : ModdlException {
		internal string Name { get; private set; }
		internal ModdlNameNotDefinedException(string name)
				: base($"The name {name} is not defined in the current context.") {
			this.Name = name;
		}
	}

}
