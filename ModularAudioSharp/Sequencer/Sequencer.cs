using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularAudioSharp.Data;
// TODO ループなどを表現できないのでこの定義は力不足

namespace ModularAudioSharp.Sequencer {
	/// <summary>
	/// シーケンサとして動作するノード
	/// </summary>
	public class Sequencer<T> : Node<T> where T : struct {

		// TODO ここで ticksPerBeat を持っている必要性はないのでは？
		private readonly int ticksPerBeat;

		private Sequencer(Node tick, int ticksPerBeat, SequenceThread<T> thread)
				: base(RunThread(tick.AsBool().UseAsStream(), thread)) {
			this.ticksPerBeat = ticksPerBeat;
		}

		public static Sequencer<T> New(Node tick, int ticksPerBeat, SequenceThread<T> thread) {
			return new Sequencer<T>(tick, ticksPerBeat, thread);
		}

		private static IEnumerable<T> RunThread(IEnumerable<bool> tick, SequenceThread<T> thread) {
			return tick.Select(t => {
				if (t) thread.Tick();

				return thread.CurrentValue;
			});
		}
	}
}
