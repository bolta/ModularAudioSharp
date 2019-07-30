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

			var waveform = new Waveform<float>(
					// 100 smp/s = 441 Hz の矩形波
					Enumerable.Repeat(1f, 13).Concat(Enumerable.Repeat(-1f, 87)).ToList(),
					44100);
			var osc = new WaveformPlayer<float>(waveform, 441, freq, loopOffset:0);
			var env = ExpEnv(1 / 8f);
			var output = osc * env;

			return new Instrument(output, new Dictionary<string, Node>(), new [] { tone }, new INotable[] { osc, env });

		}

	}
}
