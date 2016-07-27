using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularAudioSharp.Data;
// TODO ループなどを表現できないのでこの定義は力不足
using Instruction = System.Func<ModularAudioSharp.Sequencer.Sequencer.Output, ModularAudioSharp.Sequencer.Sequencer.Output>;
using Sequence = System.Collections.Generic.SortedDictionary<int,
		System.Collections.Generic.List<
				System.Func<ModularAudioSharp.Sequencer.Sequencer.Output, ModularAudioSharp.Sequencer.Sequencer.Output>>>;

namespace ModularAudioSharp.Sequencer {
	/// <summary>
	/// シーケンサとして動作するノード
	/// </summary>
	public class Sequencer : Node<Sequencer.Output> {

		private readonly Sequence sequence;
		private readonly int ticksPerBeat;

		private Sequencer(Node tick, int ticksPerBeat, Sequence sequence)
				: base(RunSequence(tick.AsBool().UseAsStream(), sequence)) {
			this.sequence = sequence;
			this.ticksPerBeat = ticksPerBeat;
		}

		public static Sequencer New(Node tick, int ticksPerBeat) {
			return new Sequencer(tick, ticksPerBeat, new Sequence());
		}

		public void AddInstruction(int tick, Instruction instruction) {
			if (! this.sequence.ContainsKey(tick)) {
				this.sequence.Add(tick, new List<Instruction>());
			}

			this.sequence[tick].Add(instruction);
		}

		private static IEnumerable<Output> RunSequence(IEnumerable<bool> tick, Sequence sequence) {
			var output = Output.Initial();

			//Output? final = null;

			var seqPtr = sequence.GetEnumerator();
			// シーケンスが空の場合、初期値のまま無限ループする
			if (! seqPtr.MoveNext()) {
				while (true) yield return output;
			}

			var tickCnt = -1;
			foreach (var t in tick) {
				if (! t) {
					yield return output;
					continue;
				}

				++tickCnt;

				if (seqPtr.Current.Key <= tickCnt) {
					foreach (var mod in seqPtr.Current.Value) output = mod(output);

					// シーケンスが終わったら状態はこれ以上変わらない。ここで無限ループする
					if (!seqPtr.MoveNext()) {
						while (true) yield return output;
					}
				}

				yield return output;
			}
		}

		/// <summary>
		/// シーケンサの出力。
		/// 各メンバの標準的な意味や値の範囲を定義しているが、これ以外の定義で用いてもよい
		/// （解釈系とシーケンサとの取り決め次第）
		/// </summary>
		public struct Output {

			public Tone Tone { get; set; }

			/// <summary>
			/// ノートオンの瞬間だけ true になる
			/// </summary>
			public bool NoteOn { get; set; }

			/// <summary>
			/// ノートオフの瞬間だけ true になる
			/// </summary>
			public bool NoteOff { get; set; }

			/// <summary>
			/// ベロシティ（）
			/// </summary>
			public float Velocity { get; set; }

			internal static Output Initial() {
				return new Output {
					Tone = new Tone {
						Octave = 4,
						ToneName = ToneName.C,
						Accidental = 0,
					},
					NoteOn = false,
					NoteOff = false,
					Velocity = 0,
				};
			}
		}

	}
}
