using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moddl.Language {
	class Evaluator {

		private static readonly Dictionary<string, Func<Instrument>> INSTRUMENTS = new Dictionary<string, Func<Instrument>> {
			{ "exponentialDecayPulseWave", Instruments.ExponentialDecayPulseWave },
			{ "filteredNoise", Instruments.FilteredNoise },
			{ "nesTriangle", Instruments.NesTriangle },
			{ "adsrPulseWave", Instruments.AdsrPulseWave },
			{ "delay", Instruments.Delay },
			{ "portamento", Instruments.Portamento },
		};

		public Value Evaluate(Expr expr) => expr.Accept(new Visitor());

		private class Visitor : ExprVisitor<Value> {
			public override Value Visit(ConnectiveExpr visitee) {
				var result = visitee.Args.Aggregate((Value) null, (acc, arg) => {
					var val = arg.Accept(this);
					if (acc == null) return val;

					return new InstrumentValue { Value = acc.AsInstrument().Then(val.AsInstrument()) };
				});

				return result; //new InstrumentValue { Value = result };
			}

			public override Value Visit(FloatLiteral visitee) => new FloatValue { Value = visitee.Value };

			// TODO 今のところ識別子は常に Instrument の名前とする。いずれは他の型の値もサポートする必要があるだろう
			public override Value Visit(IdentifierLiteral visitee)
					=> new InstrumentValue { Value = INSTRUMENTS[visitee.Value]() };

			public override Value Visit(TrackSetLiteral visitee)
					=> new TrackSetValue { Value = visitee.Value };
		}
	}
}
