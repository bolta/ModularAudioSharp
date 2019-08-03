using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp.Mml {
	public class AstVisitor {
		public virtual void Visit(CompilationUnit visitee) { }
		public virtual void Visit(OctaveCommand visitee) { }
		public virtual void Visit(OctaveIncrCommand visitee) { }
		public virtual void Visit(OctaveDecrCommand visitee) { }
		public virtual void Visit(LengthCommand visitee) { }
		public virtual void Visit(ToneCommand visitee) { }
		public virtual void Visit(RestCommand visitee) { }
		public virtual void Visit(ParameterCommand visitee) { }
		public virtual void Visit(LoopCommand visitee) { }
	}

	public static class AstVisitorExtensions {
		public static void Accept(this CompilationUnit visitee, AstVisitor visitor) => visitor.Visit(visitee);
		public static void Accept(this Command visitee, AstVisitor visitor) {
			var visited = TryVisitConcreteCommand<OctaveCommand>(visitee, c => visitor.Visit(c))
					|| TryVisitConcreteCommand<OctaveIncrCommand>(visitee, c => visitor.Visit(c))
					|| TryVisitConcreteCommand<OctaveDecrCommand>(visitee, c => visitor.Visit(c))
					|| TryVisitConcreteCommand<LengthCommand>(visitee, c => visitor.Visit(c))
					|| TryVisitConcreteCommand<ToneCommand>(visitee, c => visitor.Visit(c))
					|| TryVisitConcreteCommand<RestCommand>(visitee, c => visitor.Visit(c))
					|| TryVisitConcreteCommand<ParameterCommand>(visitee, c => visitor.Visit(c))
					|| TryVisitConcreteCommand<LoopCommand>(visitee, c => visitor.Visit(c))
					;

			if (! visited) throw new Exception($"unknown Command type: {visitee.GetType()}");
		}

		private static bool TryVisitConcreteCommand<ConcreteCommand>(Command visitee,
				Action<ConcreteCommand> visit) where ConcreteCommand : Command {
			var conc = visitee as ConcreteCommand;
			if (conc != null) {
				visit(conc);
				return true;

			} else {
				return false;
			}
		}



	}
}
