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
		public IList<Expr> Arguments { get; set; }
	}

	public class MmlStatement : Statement {
		public IEnumerable<string> Tracks { get; set; }
		public string Mml { get; set; }
		public override string ToString() => string.Format("{0} {1}",
				string.Join("", this.Tracks),
				this.Mml);
	}

	public class Expr {

	}

	public class BinaryExpr : Expr {
		public Expr Lhs { get; set; }
		public Expr Rhs { get; set; }
	}

	public class ConnectiveExpr : BinaryExpr { }
	public class MultiplicativeExpr : BinaryExpr { }
	public class DivisiveExpr : BinaryExpr { }
	public class AdditiveExpr : BinaryExpr { }
	public class SubtractiveExpr : BinaryExpr { }

	public class FloatLiteral : Expr {
		public float Value { get; set; }
	}

	public class TrackSetLiteral : Expr {
		public IList<string> Value { get; set; }
	}

	public class IdentifierLiteral : Expr {
		public string Value { get; set; }
	}
}
