using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularAudioSharp.Data;
using ModularAudioSharp.Sequencer;

namespace ModularAudioSharp.Mml {
	public class SimpleMmlInstructionGenerator : IInstructionGenerator<CompilationUnit> {
		public const string PARAM_TRACK_VOLUME = "#volume";
		public const string PARAM_TRACK_VELOCITY = "#velocity";
		public const float MAX_VOLUME = 15f;
		public const float MAX_VELOCITY = 15f;
		public const float MAX_GATE_RATE = 8f;

		private readonly List<VarController<float>> freqUsers = new List<VarController<float>>();
		private readonly List<INotable> noteUsers = new List<INotable>();
		private readonly IDictionary<string, IEnumerable<Command>> macros = new Dictionary<string, IEnumerable<Command>>();

		public IEnumerable<Instruction> GenerateInstructions(CompilationUnit astRoot, int ticksPerBeat,
				Temperament temper) {
			var result = new List<Instruction>();
			new Visitor(this, result, ticksPerBeat, temper).Visit(astRoot);

			return result;
		}

		public SimpleMmlInstructionGenerator AddFreqUsers(IEnumerable<VarController<float>> users) {
			this.freqUsers.AddRange(users);
			return this;
		}
		public SimpleMmlInstructionGenerator AddFreqUsers(params VarController<float>[] users) {
			return this.AddFreqUsers((IEnumerable<VarController<float>>) users);
		}

		public SimpleMmlInstructionGenerator AddNoteUsers(IEnumerable<INotable> users) {
			this.noteUsers.AddRange(users);
			return this;
		}
		public SimpleMmlInstructionGenerator AddNoteUsers(params INotable[] users) {
			return this.AddNoteUsers((IEnumerable<INotable>) users);
		}

		public SimpleMmlInstructionGenerator AddMacros(IDictionary<string, IEnumerable<Command>> macros) {
			foreach (var m in macros) {
				// TODO 重複をチェックして専用例外にする
				this.macros.Add(m.Key, m.Value);
			}

			return this;
		}

		private class Visitor : AstVisitor {
			private readonly SimpleMmlInstructionGenerator owner;
			private readonly List<Instruction> result;
			private readonly int ticksPerBar;
			private readonly Temperament temperament;

			private int octave = 4;
			private int length = 4;

			/// <summary>
			/// スラーの途中（前の音符にスラーがついていた）かどうか
			/// </summary>
			private bool slur = false;
			private float gateRate = 1f;
			private Detune detune = null;

			internal Visitor(SimpleMmlInstructionGenerator owner, List<Instruction> result, int ticksPerBeat,
					Temperament temperament) {
				this.owner = owner;
				this.result = result;
				this.ticksPerBar = 4 * ticksPerBeat;
				this.temperament = temperament;
			}

			public override void Visit(CompilationUnit visitee) {
				foreach (var s in visitee.Commands) s.Accept(this);
			}

			public override void Visit(OctaveCommand visitee) { this.octave = visitee.Value; }
			public override void Visit(OctaveIncrCommand visitee) { this.octave += 1; }
			public override void Visit(OctaveDecrCommand visitee) { this.octave -= 1; }
			public override void Visit(LengthCommand visitee) { this.length = visitee.Value; }
			public override void Visit(GateRateCommand visitee) { this.gateRate = visitee.Value / MAX_GATE_RATE; }

			public override void Visit(VolumeCommand visitee) {
				this.result.Add(new ParameterInstruction(PARAM_TRACK_VOLUME, visitee.Value / MAX_VOLUME));
			}

			public override void Visit(VelocityCommand visitee) {
				this.result.Add(new ParameterInstruction(PARAM_TRACK_VELOCITY, visitee.Value / MAX_VELOCITY));
			}

			public override void Visit(DetuneCommand visitee) {
				//this.result.Add(new DetuneInstruction(visitee.Value));
				this.detune = new CentDetune(visitee.Value);
			}

			public override void Visit(ToneCommand visitee) {
				var stepTicks = CalcTicksFromLength(visitee.Length, this.ticksPerBar, this.length);
				var gateTicks = (int) (stepTicks * this.gateRate);

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
				var freq = this.detune?.GetDetunedFreq(this.temperament[tone]) ?? this.temperament[tone];

				this.result.AddRange(this.owner.freqUsers.Select(u => new ValueInstruction<float>(u, freq)));
				if (! this.slur) {
					this.result.AddRange(this.owner.noteUsers.Select(u => new NoteInstruction(u, true)));
				}
				this.result.Add(new WaitInstruction(gateTicks));

				if (! visitee.Slur) {
					this.result.AddRange(this.owner.noteUsers.Select(u => new NoteInstruction(u, false)));
				}

				if (stepTicks - gateTicks > 0) {
					this.result.Add(new WaitInstruction(stepTicks - gateTicks));
				}

				this.slur = visitee.Slur;
			}

			public override void Visit(RestCommand visitee) {
				var ticks = CalcTicksFromLength(visitee.Length, this.ticksPerBar, this.length);
				this.result.Add(new WaitInstruction(ticks));
			}

			public override void Visit(ParameterCommand visitee) {
				this.result.Add(new ParameterInstruction(visitee.Name.Name, visitee.Value));
			}

			public override void Visit(LoopCommand visitee) {
				if (visitee.Times.HasValue) {
					// とりあえず有限ループは展開する実装とする。メモリ効率的に問題があればループのまま演奏する実装を検討する
					for (int i=0 ; i<visitee.Times.Value ; ++i) {
						foreach (var child in visitee.Content) {
							if (child is LoopBreakCommand) {
								if (i == visitee.Times.Value - 1) break;
								else continue;
							}
							child.Accept(this);
						}
					}
				} else {
					var start = this.result.Count;
					foreach (var child in visitee.Content) child.Accept(this);
					this.result.Add(new JumpInstruction(start));
				}
			}

			public override void Visit(LoopBreakCommand visitee) {
				// ループ中では別途チェックされるので、これを visit することはありえない
				throw new Exception("The loop break command (:) is available only in a finite loop.");
			}

			public override void Visit(ExpandMacroCommand visitee) {
				// TODO マクロが見つからないときのエラー処理
				// TODO 無限再帰検出
				foreach (var command in this.owner.macros[visitee.Name.Name]) {
					command.Accept(this);
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
