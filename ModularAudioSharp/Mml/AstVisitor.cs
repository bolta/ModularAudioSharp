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
		public virtual void Visit(GateRateCommand visitee) { }
		public virtual void Visit(VolumeCommand visitee) { }
		public virtual void Visit(VelocityCommand visitee) { }
		public virtual void Visit(DetuneCommand visitee) { }
		public virtual void Visit(ToneCommand visitee) { }
		public virtual void Visit(RestCommand visitee) { }
		public virtual void Visit(ParameterCommand visitee) { }
		public virtual void Visit(LoopCommand visitee) { }
		public virtual void Visit(LoopBreakCommand visitee) { }
		public virtual void Visit(StackCommand visitee) { }
		public virtual void Visit(ExpandMacroCommand visitee) { }
	}

	public static class AstVisitorExtensions {
		public static void Accept(this CompilationUnit visitee, AstVisitor visitor) => visitor.Visit(visitee);
		public static void Accept(this Command visitee, AstVisitor visitor) {
			var visited = TryVisitConcreteCommand<OctaveCommand>(visitee, c => visitor.Visit(c))
					|| TryVisitConcreteCommand<OctaveIncrCommand>(visitee, c => visitor.Visit(c))
					|| TryVisitConcreteCommand<OctaveDecrCommand>(visitee, c => visitor.Visit(c))
					|| TryVisitConcreteCommand<LengthCommand>(visitee, c => visitor.Visit(c))
					|| TryVisitConcreteCommand<GateRateCommand>(visitee, c => visitor.Visit(c))
					|| TryVisitConcreteCommand<VolumeCommand>(visitee, c => visitor.Visit(c))
					|| TryVisitConcreteCommand<VelocityCommand>(visitee, c => visitor.Visit(c))
					|| TryVisitConcreteCommand<DetuneCommand>(visitee, c => visitor.Visit(c))
					|| TryVisitConcreteCommand<ToneCommand>(visitee, c => visitor.Visit(c))
					|| TryVisitConcreteCommand<RestCommand>(visitee, c => visitor.Visit(c))
					|| TryVisitConcreteCommand<ParameterCommand>(visitee, c => visitor.Visit(c))
					|| TryVisitConcreteCommand<LoopCommand>(visitee, c => visitor.Visit(c))
					|| TryVisitConcreteCommand<LoopBreakCommand>(visitee, c => visitor.Visit(c))
					|| TryVisitConcreteCommand<StackCommand>(visitee, c => visitor.Visit(c))
					|| TryVisitConcreteCommand<ExpandMacroCommand>(visitee, c => visitor.Visit(c))
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
