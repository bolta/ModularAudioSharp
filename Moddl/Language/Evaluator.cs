using ModularAudioSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moddl.Language {
	class Evaluator {

		private readonly Context context = new Context();
		internal Evaluator() {
			foreach (var kv in Modules.BUILT_INS) {
				this.context[kv.Key] = new ModuleDefValue { Value = kv.Value };
			}
		}

		public Value Evaluate(Expr expr) => expr.Accept(new Visitor(this.context));

		private class Visitor : ExprVisitor<Value> {

			private readonly Context context;
			internal Visitor(Context context) {
				this.context = context;
			}

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

			public override Value Visit(TrackSetLiteral visitee)
					=> new TrackSetValue { Value = visitee.Value };

			public override Value Visit(IdentifierExpr visitee)
					// TODO 見つからない場合のエラー処理
					=> this.context[visitee.Identifier];

			public override Value Visit(LambdaExpr visitee) => new ModuleDefValue {
				Value = () => {
					var input = Nodes.Proxy<float>();
					var output = input;
					var inputParamModule = new Module(input, output,
							new Dictionary<string, ProxyController<float>>() { },
							new INotable[] { });

					this.context.Push();
					try {
						this.context[visitee.InputParam] = new ModuleValue { Value = inputParamModule };
						var module = visitee.Body.Accept(this).AsModule();

						return inputParamModule.Then(new Module(
								Enumerable.Empty<ProxyController<float>>(),
								module.Output,
								module.Parameters,
								module.NoteUsers));

					} finally {
						this.context.Pop();
					}
				},
			};

			public override Value Visit(ModuleParamExpr visitee) {
				var module = visitee.ModuleDef.Accept(this).AsModule();

				foreach (var param in visitee.Parameters) {
					var value = param.Item2.Accept(this);
					module.Parameters[param.Item1].Source = value.AsModule().Output;
				}

				return new ModuleValue { Value = module };
			}

		}
	}
}
