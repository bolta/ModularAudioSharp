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
		internal static Instrument ExponentialDecayPulseWave() {
			var freq = Proxy<float>();

			var dutyInit = 0.5f; // TODO 初期値はスタック管理のため外から見える必要がありそう
			var duty = Var(dutyInit);

			var osc = PulseOsc(freq, duty);
			var decay = Var(1 / 8f);
			var env = ExpEnv(decay);
			var output = (osc * env).AsFloat();

			return new Instrument(freq, output, new Dictionary<string, VarController<float>>() {
				{ "duty", duty },
				{ "decay", decay },
			}, new INotable[] { env });
		}

		internal static Instrument AdsrPulseWave() {
			var freq = Proxy<float>();

			// TODO 初期値はスタック管理のため外から見える必要がありそう
			var duty = Var(0.5f);

			var attack = Var(0.15f);
			var decay = Var(0.1f);
			var sustain = Var(0.8f);
			var release = Var(0.25f);

			var osc = PulseOsc(freq, duty);
			var env = AdsrEnv(attack, decay, sustain, release);
			var output = (osc * env).AsFloat();

			return new Instrument(freq, output, new Dictionary<string, VarController<float>>() {
				{ "duty", duty },
				{ "attack", attack },
				{ "decay", decay },
				{ "sustain", sustain },
				{ "release", release },
			}, new INotable[] { env });
		}

		internal static Instrument FilteredNoise() {
			var freq = Proxy<float>();
			var qInit = 17.5f;
			var q = Var(qInit);

			// BPF だとノイズが残っていまいちな音だった
//			var osc = (Noise()).Bpf(freq, q);//.Limit(-1f, 1f);
			var osc = (Noise() * 0.125f).Lpf(freq, q).Hpf(freq, q);//.Limit(-1f, 1f);
			var env = PlainEnv();
			var output = (osc * env).AsFloat();

			return new Instrument(freq, output, new Dictionary<string, VarController<float>>() {
				{ "q", q },
			}, new INotable[] { env });

		}

		internal static Instrument NesTriangle() {
			var freq = Proxy<float>();

			var osc = TriangleOsc(freq);
			var env = PlainEnv();
			var output = (osc * env).QuantCrush(-1f, 1f, 16);

			return new Instrument(freq, output, new Dictionary<string, VarController<float>>() {
			}, new INotable[] { env });

		}

		// 接続の動作検証用
		internal static Instrument Delay() {
			var input = Proxy<float>();
			var output = input.Node.Delay(24806, 0.5f, 0.4f, 24806);

			return new Instrument(input, output, new Dictionary<string, VarController<float>>(),
				Enumerable.Empty<INotable>());
		}

		// 接続の動作検証用
		internal static Instrument Portamento() {
			var input = Proxy<float>();
			var output = Nodes.Portamento(input, 0.002f);

			return new Instrument(input, output, new Dictionary<string, VarController<float>>(),
				Enumerable.Empty<INotable>());
		}
	}
}
