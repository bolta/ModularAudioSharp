﻿using ModularAudioSharp;
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
			var freq = Var<float>();

			var dutyInit = 0.5f; // TODO 初期値はスタック管理のため外から見える必要がありそう
			var duty = Var(dutyInit);

			var osc = PulseOsc(freq, duty);
			var env = ExpEnv(1 / 8f);
			var output = osc * env;

			return new Instrument(output, new Dictionary<string, VarController<float>>() {
				{ "duty", duty },
			}, new [] { freq }, new INotable[] { env });

		}

		public static Instrument FilteredNoise() {
			var freq = Var<float>();
			var qInit = 30f;
			var q = Var(qInit);

			// BPF だとノイズが残っていまいちな音だった
//			var osc = (Noise()).Bpf(freq, q);//.Limit(-1f, 1f);
			var osc = (Noise() * 0.06125f).Lpf(freq, q).Hpf(freq, q);//.Limit(-1f, 1f);
//			var env = ExpEnv(1 / 8f);
			var output = osc;// * env;

			return new Instrument(output, new Dictionary<string, VarController<float>>() {
				{ "q", q },
			}, new [] { freq }, new INotable[] {
				//env,
			});

		}

	}
}
