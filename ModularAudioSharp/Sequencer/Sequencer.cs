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
	public class Sequencer : Node<Unit> {

		// TODO ここで ticksPerBeat を持っている必要性はないのでは？
		private readonly int ticksPerBeat;

		private Sequencer(Node tick, int ticksPerBeat, SequenceThread thread)
				: base(RunThread(tick.AsBool().UseAsStream(), thread)) {
			this.ticksPerBeat = ticksPerBeat;

			// 出力が使われない（他から UseAsStream() を呼ばれることがない）ため、明示的に呼ぶ必要がある。
			// 
			this.Use(true);
		}

		public static Sequencer New(Node tick, int ticksPerBeat, SequenceThread thread) {
			return new Sequencer(tick, ticksPerBeat, thread);
		}

		private static IEnumerable<Unit> RunThread(IEnumerable<bool> tick, SequenceThread thread) {
			return tick.Select(t => {
				if (t) thread.Tick();

				return Unit.Value;

				//if (thread.ValueOnce.HasValue) {
				//	var value = thread.ValueOnce.Value;
				//	thread.ValueOnce = null;
				//	return value;

				//} else {
				//	return thread.CurrentValue;
				//}
			});
		}
	}
}
