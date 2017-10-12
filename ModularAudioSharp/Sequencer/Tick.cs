using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp.Sequencer {
	public class Tick {
		private IList<ITickUser> users = new List<ITickUser>();

		public float Tempo { get; set; }

		private readonly int ticksPerBeat;

		/// <summary>
		/// タイマ。1 以上で Sample() が呼ばれると Tick が発行され、タイマは 1 で割った余りまで減る。
		/// 最初のサンプルで最初の Tick を発行するよう 1 から始める
		/// </summary>
		private float timer = 1f;

		public Tick(float tempo, int ticksPerBeat) {
			this.Tempo = tempo;
			this.ticksPerBeat = ticksPerBeat;
			ModuleSpace.AddTick(this);
		}

		public void AddUser(ITickUser user) {
			this.users.Add(user);
		}

		internal void Sample() {
			// while の中が複数回実行されることは通常ありえない
			// （tempo * ticksPerBeat がきわめて大きい場合のみ）が、
			// その場合でも発行する tick の数は正しくなるよう if ではなく while にしておく
			while (this.timer >= 1f) {
				foreach (var u in this.users) u.Tick();
				this.timer -= 1f;
			}
			this.timer += this.Tempo * this.ticksPerBeat / 60 / ModuleSpace.SampleRate;
		}
	}
}
