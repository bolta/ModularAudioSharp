using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moddl.Language {
	class Evaluator {

		public Value Evaluate(Expr expr) => expr.Accept(new Visitor());

		private class Visitor : ExprVisitor<Value> {
			public override Value Visit(ConnectiveExpr visitee)
					=> this.VisitBinary(visitee, (lhs, rhs) => lhs.Then(rhs));
			public override Value Visit(MultiplicativeExpr visitee)
					=> this.VisitBinary(visitee, (lhs, rhs) => lhs.Multiply(rhs));
			public override Value Visit(DivisiveExpr visitee)
					=> this.VisitBinary(visitee, (lhs, rhs) => lhs.Divide(rhs));
			public override Value Visit(AdditiveExpr visitee)
					=> this.VisitBinary(visitee, (lhs, rhs) => lhs.Add(rhs));
			public override Value Visit(SubtractiveExpr visitee)
					=> this.VisitBinary(visitee, (lhs, rhs) => lhs.Subtract(rhs));

			private Value VisitBinary(BinaryExpr visitee, Func<Module, Module, Module> oper) {
				var lhs = visitee.Lhs.Accept(this).AsModule();
				var rhs = visitee.Rhs.Accept(this).AsModule();

				return new ModuleValue { Value = oper(lhs, rhs) };
			}

			public override Value Visit(FloatLiteral visitee) => new FloatValue { Value = visitee.Value };

			// TODO 今のところ識別子は常に Module の名前とする。いずれは他の型の値もサポートする必要があるだろう
			public override Value Visit(ModuleCallExpr visitee) {
				var module = Modules.BUILT_INS[visitee.Identifier]();

				foreach (var param in visitee.Parameters) {
					var value = param.Item2.Accept(this);
					module.Parameters[param.Item1].Source = value.AsModule().Output;
				}

				return new ModuleValue { Value = module };
			}

			public override Value Visit(TrackSetLiteral visitee)
					=> new TrackSetValue { Value = visitee.Value };
		}
	}
}
