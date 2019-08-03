using ModularAudioSharp;
using ModularAudioSharp.Data;
using ModularAudioSharp.Sequencer;
using ModularAudioSharp.Waveform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ModularAudioSharp.Nodes;

namespace Moddl {
	/// <summary>
	/// 組み込みインストゥルメント各種
	/// </summary>
	static class Instruments {

		/// <summary>
		/// 指数的に減衰するパルス波
		/// </summary>
		/// <returns></returns>
		public static Instrument ExponentialDecayPulseWave() {
			var tone = Var<Tone>();
			var freq = Temperament.Equal(tone, 440);

			var dutyInit = 0.5f; // TODO 初期値はスタック管理のため外から見える必要がありそう
			var duty = Var(dutyInit);

			var osc = PulseOsc(freq, duty);
			var env = ExpEnv(1 / 8f);
			var output = osc * env;

			return new Instrument(output, new Dictionary<string, Node>() {
				{ "duty", duty },
			}, new [] { tone }, new INotable[] { /*osc,*/ env });

		}

	}
}
