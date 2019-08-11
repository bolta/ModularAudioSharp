using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moddl.Language {
	public class CompilationUnit {
		public IEnumerable<Statement> Statements { get; set; }
		public override string ToString() => string.Join(" ", this.Statements.Select(s => s.ToString()));
	}

	public class Statement {

	}

	public class DirectiveStatement : Statement {
		public string Name { get; set; }
		public IList<Value> Arguments { get; set; }
	}

	public class MmlStatement : Statement {
		public IEnumerable<string> Tracks { get; set; }
		public string Mml { get; set; }
		public override string ToString() => string.Format("{0} {1}",
				string.Join("", this.Tracks),
				this.Mml);
	}

	public class Value {

	}

	public class FloatValue : Value {
		public float Value { get; set; }
	}

	public class TrackSetValue : Value {
		public IList<string> Value { get; set; }
	}

	public class IdentifierValue : Value {
		public string Value { get; set; }
	}
}
