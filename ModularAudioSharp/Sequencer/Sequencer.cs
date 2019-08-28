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

		public IDictionary<string, ProxyController<float>> Parameters { get; private set; }

		// TODO いずれは複数持てるようにする
		private readonly SequenceThread thread;

		public Sequencer(Tick tick, IDictionary<string, ProxyController<float>> parameters,
				IEnumerable<Instruction> instrcs) {
			tick.AddUser(this);
			this.Parameters = new Dictionary<string, ProxyController<float>>(parameters);
			this.thread = new SequenceThread(this, instrcs);
		}

		public void Tick() {
			this.thread.Tick();
		}
	}
}
