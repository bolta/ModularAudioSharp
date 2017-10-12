using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularAudioSharp.Data;
// TODO ループなどを表現できないのでこの定義は力不足

namespace ModularAudioSharp.Sequencer {
	/// <summary>
	/// Tick で駆動されるシーケンサ
	/// </summary>
	public class Sequencer : ITickUser {

		// TODO いずれは複数持てるようにする
		private readonly SequenceThread thread;

		public Sequencer(Tick tick, SequenceThread thread) {
			tick.AddUser(this);
			this.thread = thread;
		}

		public void Tick() {
			this.thread.Tick();
		}
	}
}
