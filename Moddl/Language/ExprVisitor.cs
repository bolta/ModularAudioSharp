using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moddl.Language {
	public class ExprVisitor<Result> {
		public virtual Result Visit(ConnectiveExpr visitee) { return default; }
		public virtual Result Visit(PowerExpr visitee) { return default; }
		public virtual Result Visit(MultiplicativeExpr visitee) { return default; }
		public virtual Result Visit(DivisiveExpr visitee) { return default; }
		public virtual Result Visit(ModuloExpr visitee) { return default; }
		public virtual Result Visit(AdditiveExpr visitee) { return default; }
		public virtual Result Visit(SubtractiveExpr visitee) { return default; }
		public virtual Result Visit(FloatLiteral visitee) { return default; }
		public virtual Result Visit(TrackSetLiteral visitee) { return default; }
		public virtual Result Visit(AssocArrayLiteral visitee) { return default; }
		public virtual Result Visit(IdentifierExpr visitee) { return default; }
		public virtual Result Visit(LambdaExpr visitee) { return default; }
		public virtual Result Visit(ModuleParamExpr visitee) { return default; }
	}

	public static class AstVisitorExtensions {
		//public static void Accept(this ConnectiveExpr visitee, ExprVisitor visitor) => visitor.Visit(visitee);
		public static Result Accept<Result>(this Expr visitee, ExprVisitor<Result> visitor) {
			var result = default(Result);
			var visited = TryVisitConcreteExpr<ConnectiveExpr, Result>(visitee, ref result, c => visitor.Visit(c))
					|| TryVisitConcreteExpr<PowerExpr, Result>(visitee, ref result, c => visitor.Visit(c))
					|| TryVisitConcreteExpr<MultiplicativeExpr, Result>(visitee, ref result, c => visitor.Visit(c))
					|| TryVisitConcreteExpr<DivisiveExpr, Result>(visitee, ref result, c => visitor.Visit(c))
					|| TryVisitConcreteExpr<ModuloExpr, Result>(visitee, ref result, c => visitor.Visit(c))
					|| TryVisitConcreteExpr<AdditiveExpr, Result>(visitee, ref result, c => visitor.Visit(c))
					|| TryVisitConcreteExpr<SubtractiveExpr, Result>(visitee, ref result, c => visitor.Visit(c))
					|| TryVisitConcreteExpr<FloatLiteral, Result>(visitee, ref result, c => visitor.Visit(c))
					|| TryVisitConcreteExpr<TrackSetLiteral, Result>(visitee, ref result, c => visitor.Visit(c))
					|| TryVisitConcreteExpr<AssocArrayLiteral, Result>(visitee, ref result, c => visitor.Visit(c))
					|| TryVisitConcreteExpr<IdentifierExpr, Result>(visitee, ref result, c => visitor.Visit(c))
					|| TryVisitConcreteExpr<LambdaExpr, Result>(visitee, ref result, c => visitor.Visit(c))
					|| TryVisitConcreteExpr<ModuleParamExpr, Result>(visitee, ref result, c => visitor.Visit(c))
					;

			if (!visited) throw new Exception($"unknown Command type: {visitee.GetType()}");

			return result;
		}

		private static bool TryVisitConcreteExpr<ConcreteExpr, Result>(Expr visitee, ref Result result,
				Func<ConcreteExpr, Result> visit) where ConcreteExpr : Expr {
			if (visitee is ConcreteExpr conc) {
				result = visit(conc);
				return true;

			} else {
				return false;
			}
		}

	}
}
