using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp.Mml {
	public class AstVisitor {
		public virtual void Visit(CompilationUnit visitee) { }
		public virtual void Visit(OctaveStatement visitee) { }
		public virtual void Visit(OctaveIncrStatement visitee) { }
		public virtual void Visit(OctaveDecrStatement visitee) { }
		public virtual void Visit(LengthStatement visitee) { }
		public virtual void Visit(ToneStatement visitee) { }
		public virtual void Visit(RestStatement visitee) { }
		public virtual void Visit(ParameterStatement visitee) { }
		public virtual void Visit(LoopStatement visitee) { }
	}

	public static class AstVisitorExtensions {
		public static void Accept(this CompilationUnit visitee, AstVisitor visitor) => visitor.Visit(visitee);
		public static void Accept(this Statement visitee, AstVisitor visitor) {
			var visited = TryVisitConcreteStatement<OctaveStatement>(visitee, c => visitor.Visit(c))
					|| TryVisitConcreteStatement<OctaveIncrStatement>(visitee, c => visitor.Visit(c))
					|| TryVisitConcreteStatement<OctaveDecrStatement>(visitee, c => visitor.Visit(c))
					|| TryVisitConcreteStatement<LengthStatement>(visitee, c => visitor.Visit(c))
					|| TryVisitConcreteStatement<ToneStatement>(visitee, c => visitor.Visit(c))
					|| TryVisitConcreteStatement<RestStatement>(visitee, c => visitor.Visit(c))
					|| TryVisitConcreteStatement<ParameterStatement>(visitee, c => visitor.Visit(c))
					|| TryVisitConcreteStatement<LoopStatement>(visitee, c => visitor.Visit(c))
					;

			if (! visited) throw new Exception($"unknown Statement type: {visitee.GetType()}");
		}

		private static bool TryVisitConcreteStatement<ConcreteStatement>(Statement visitee,
				Action<ConcreteStatement> visit) where ConcreteStatement : Statement {
			var conc = visitee as ConcreteStatement;
			if (conc != null) {
				visit(conc);
				return true;

			} else {
				return false;
			}
		}



	}
}
