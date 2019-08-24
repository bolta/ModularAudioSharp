using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moddl.Language {
	class Evaluator {

		private static readonly Dictionary<string, Func<Module>> BUILT_IN_MODULES = new Dictionary<string, Func<Module>> {
			{ "exponentialDecayPulseWave", Modules.ExponentialDecayPulseWave },
			{ "filteredNoise", Modules.FilteredNoise },
			{ "nesTriangle", Modules.NesTriangle },
			{ "adsrPulseWave", Modules.AdsrPulseWave },
			{ "delay", Modules.Delay },
			{ "portamento", Modules.Portamento },
			{ "pulseOsc", Modules.PulseOsc },
			{ "expEnv", Modules.ExpEnv },
		};

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
			public override Value Visit(IdentifierLiteral visitee)
					=> new ModuleValue { Value = BUILT_IN_MODULES[visitee.Value]() };

			public override Value Visit(TrackSetLiteral visitee)
					=> new TrackSetValue { Value = visitee.Value };
		}
	}
}
