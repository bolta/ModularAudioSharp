using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularAudioSharp.Data;
using ModularAudioSharp.Sequencer;

namespace ModularAudioSharp.Mml {
	public class SimpleMmlInstructionGenerator : IInstructionGenerator<CompilationUnit, SimpleMmlValue> {
		public IEnumerable<Instruction<SimpleMmlValue>> GenerateInstructions(CompilationUnit astRoot, int tickPerBeat) {
			var result = new List<Instruction<SimpleMmlValue>>();
			new Visitor(result, tickPerBeat).Visit(astRoot);

			return result;
		}
		private class Visitor : AstVisitor {
			private readonly IList<Instruction<SimpleMmlValue>> result;
			private readonly int tickPerBar;
			private int octave = 4;
			private int length = 8;
			private float gateRatio = 1f;

			internal Visitor(IList<Instruction<SimpleMmlValue>> result, int tickPerBeat) {
				this.result = result;
				this.tickPerBar = 4 * tickPerBeat;
			}

			public override void Visit(CompilationUnit visitee) {
				foreach (var s in visitee.Statements) s.Accept(this);
			}

			public override void Visit(OctaveStatement visitee) { this.octave = visitee.Value; }
			public override void Visit(OctaveIncrStatement visitee) { this.octave += 1; }
			public override void Visit(OctaveDecrStatement visitee) { this.octave -= 1; }
			public override void Visit(LengthStatement visitee) { this.length = visitee.Value; }
			public override void Visit(ToneStatement visitee) {
				var stepTicks = CalcTicksFromLength(visitee.Length, this.tickPerBar, this.length);
				var gateTicks = (int) (stepTicks * this.gateRatio);

				// TODO ちゃんと書き直す
				Data.ToneName toneName; switch (visitee.ToneName.BaseName.ToUpper()) {
				case "C": toneName = Data.ToneName.C; break;
				case "D": toneName = Data.ToneName.D; break;
				case "E": toneName = Data.ToneName.E; break;
				case "F": toneName = Data.ToneName.F; break;
				case "G": toneName = Data.ToneName.G; break;
				case "A": toneName = Data.ToneName.A; break;
				case "B": toneName = Data.ToneName.B; break;
				default: throw new Exception();
				}

				var tone = new Tone { Octave = this.octave, ToneName = toneName, Accidental = visitee.ToneName.Accidental };

				this.result.Add(new ValueOnceInstruction<SimpleMmlValue>(new SimpleMmlValue {
					NoteOperation = NoteOperation.NoteOn,
					Tone = tone,
				}));
				this.result.Add(new ValueInstruction<SimpleMmlValue>(new SimpleMmlValue {
					NoteOperation = NoteOperation.None,
					Tone = tone,
				}));
				this.result.Add(new WaitInstruction<SimpleMmlValue>(gateTicks));

				this.result.Add(new ValueOnceInstruction<SimpleMmlValue>(new SimpleMmlValue {
					NoteOperation = NoteOperation.NoteOff,
					Tone = tone,
				}));
				this.result.Add(new ValueInstruction<SimpleMmlValue>(new SimpleMmlValue {
					NoteOperation = NoteOperation.None,
					Tone = tone,
				}));
				if (stepTicks - gateTicks > 0) {
					this.result.Add(new WaitInstruction<SimpleMmlValue>(stepTicks - gateTicks));
				}
			}

			/// <summary>
			/// Length から Tick 数を計算する
			/// TODO 汎用性のある処理なのでここではなく全体から使える場所に置く
			/// </summary>
			private static int CalcTicksFromLength(Length lengthSpec, int ticksPerBeat, int defaultLength) {
				if (lengthSpec == null) return DivideTick(ticksPerBeat, defaultLength);

				return lengthSpec.Elements.Select(e => CalcTicksFromLengthElement(e, ticksPerBeat, defaultLength))
						.Sum();
			}

			private static int CalcTicksFromLengthElement(LengthElement elemSpec, int ticksPerBeat, int defaultLength) {
				var number = elemSpec.Number ?? defaultLength;
				var numberTick = DivideTick(ticksPerBeat, number);
				
				// n 個の付点（n >= 0）が付くと、音長は元の音長の 2 倍から元の音長の 2^(n+1) 分の 1 を引いた長さになる
				return numberTick * 2 - DivideTick(numberTick, Pow(2, elemSpec.Dots));
			}

			/// <summary>
			/// tick を denominator で割る。割り切れない（テンポずれが起こる）場合は例外を投げる
			/// </summary>
			/// <param name="tick"></param>
			/// <param name="denominator"></param>
			/// <returns></returns>
			private static int DivideTick(int tick, int denominator) {
				var result = tick / denominator;
				if (result * denominator != tick) {
					// TODO 例外の種類と文言を検討。ここで意味不明でない文言ははたして出せるのか…？
					throw new Exception("テンポずれ");
				}

				return result;
			}

			private static int Pow(int @base, int times) {
				int result = 1;
				for (int i=0 ; i<times ; ++i) result *= @base;
				return result;
			}
		}
	}
}
