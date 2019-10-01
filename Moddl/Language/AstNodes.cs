using Moddl;
using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moddl.Language {

	public abstract class AstNode {
		public Position Position { get; protected set; }
		public int Length { get; protected set; }
	}

	public abstract class PositionAwareNode<T> : AstNode, IPositionAware<T> where T : PositionAwareNode<T> {
		public T SetPos(Position startPos, int length) {
			this.Position = startPos;
			this.Length = length;
			return (T) this;
		}
	}

	public class CompilationUnit : AstNode {
		public IEnumerable<Statement> Statements { get; set; }
		public override string ToString() => string.Join(" ", this.Statements.Select(s => s.ToString()));
	}

	public class Statement : PositionAwareNode<Statement> { }

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

	public class Expr : PositionAwareNode<Expr> { }

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

	public class AssocArrayLiteral : Expr {
		// TODO ModuleParamExpr に合わせて Dictionary ではなく List にしているが、なぜそうしたんだっけ…
		public IList<Tuple<string, Expr>> Entries { get; set; }
	}

	public class IdentifierExpr : Expr {
		public string Identifier { get; set; }
	}

	public class LambdaExpr : Expr {
		public string InputParam { get; set; }
		public Expr Body { get; set; }
	}

	public class ModuleParamExpr : Expr {
		public Expr ModuleDef { get; set; }
		public string Label { get; set; }
		public IList<Tuple<string, Expr>> ConstructorParameters { get; set; }
		public IList<Tuple<string, Expr>> SignalParameters { get; set; }
	}
}

internal static class ListExtensions {
	internal static T TryGet<T>(this IList<T> dis, int i) {
		if (i >= dis.Count) {
			throw new ModdlIndexOutOfRangeException(i, dis.Count);
		}

		return dis[i];
	}
}
