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
		};

		public Value Evaluate(Expr expr) => expr.Accept(new Visitor());

		private class Visitor : ExprVisitor<Value> {
			public override Value Visit(ConnectiveExpr visitee) {
				var result = visitee.Args.Aggregate((Value) null, (acc, arg) => {
					var val = arg.Accept(this);

					if (acc == null) {
						return val;
					} else {
						return new ModuleValue { Value = acc.AsModule().Then(val.AsModule()) };
					}
				});

				return result;
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
