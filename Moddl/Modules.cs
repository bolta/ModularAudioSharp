﻿using ModularAudioSharp;
using ModularAudioSharp.Data;
using ModularAudioSharp.Sequencer;
using ModularAudioSharp.Waveform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static ModularAudioSharp.Nodes;

namespace Moddl {
	/// <summary>
	/// 組み込みモジュール各種
	/// </summary>
	static class Modules {

		/// <summary>
		/// 全ての組み込みモジュールの一覧。
		/// 名前から、モジュールを生成する関数を引くことができる
		/// </summary>
		/// <remarks>
		/// Dictionary {
		///   { "pulseOsc", PulseOsc },
		///	  { "expEnv", ExpEnv },
		///	  ...
		///	} 
		///	のような内容をリフレクションで生成している。
		///	シグネチャが internal static Module [ModuleName]() であるメソッドをこのクラスに作ると勝手に登録される
		/// </remarks>
		internal static readonly Dictionary<string, Func<Module>> BUILT_INS;
		static Modules() {
			BUILT_INS = typeof(Modules).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
					.Where(m =>
							(m.IsPublic || m.IsAssembly)
							&& m.IsStatic
							&& m.ReturnType == typeof(Module)
							&& m.GetParameters().Length == 0)
					.ToDictionary(
							m => m.Name.Substring(0, 1).ToLower() + m.Name.Substring(1),
							m => { return (Func<Module>) (() => (Module) m.Invoke(null, new object[] { })); });
		}

		/// <summary>
		/// 指数的に減衰するパルス波
		/// </summary>
		/// <returns></returns>
		internal static Module ExponentialDecayPulseWave() {
			var freq = Proxy<float>();

			var dutyInit = 0.5f; // TODO 初期値はスタック管理のため外から見える必要がありそう
			var duty = Proxy(dutyInit);

			var osc = Nodes.PulseOsc(freq, duty);
			var decay = Proxy(1 / 8f);
			var env = Nodes.ExpEnv(decay);
			var output = (osc * env).AsFloat();

			return new Module(freq, output, new Dictionary<string, ProxyController<float>>() {
				{ "duty", duty },
				{ "decay", decay },
			}, new INotable[] { env });
		}

		internal static Module AdsrPulseWave() {
			var freq = Proxy<float>();

			// TODO 初期値はスタック管理のため外から見える必要がありそう
			var duty = Proxy(0.5f);

			var attack = Proxy(0.15f);
			var decay = Proxy(0.1f);
			var sustain = Proxy(0.8f);
			var release = Proxy(0.25f);

			var osc = Nodes.PulseOsc(freq, duty);
			var env = Nodes.AdsrEnv(attack, decay, sustain, release);
			var output = (osc * env).AsFloat();

			return new Module(freq, output, new Dictionary<string, ProxyController<float>>() {
				{ "duty", duty },
				{ "attack", attack },
				{ "decay", decay },
				{ "sustain", sustain },
				{ "release", release },
			}, new INotable[] { env });
		}

		internal static Module FilteredNoise() {
			var freq = Proxy<float>();
			var qInit = 17.5f;
			var q = Proxy(qInit);

			// BPF だとノイズが残っていまいちな音だった
//			var osc = (Noise()).Bpf(freq, q);//.Limit(-1f, 1f);
			var osc = (Noise() * 0.125f).Lpf(freq, q).Hpf(freq, q);//.Limit(-1f, 1f);
			var env = Nodes.PlainEnv();
			var output = (osc * env).AsFloat();

			return new Module(freq, output, new Dictionary<string, ProxyController<float>>() {
				{ "q", q },
			}, new INotable[] { env });

		}

		internal static Module NesTriangle() {
			var freq = Proxy<float>();

			var osc = TriangleOsc(freq);
			var env = Nodes.PlainEnv();
			var output = (osc * env).QuantCrush(-1f, 1f, 16);

			return new Module(freq, output, new Dictionary<string, ProxyController<float>>() {
			}, new INotable[] { env });

		}

		// 接続の動作検証用
		internal static Module Delay() {
			var input = Proxy<float>();
			var output = input.Node.Delay(24806, 0.5f, 0.4f, 24806);

			return new Module(input, output, new Dictionary<string, ProxyController<float>>(),
				Enumerable.Empty<INotable>());
		}

		// 接続の動作検証用
		internal static Module Portamento() {
			var input = Proxy<float>();
			var output = Nodes.Portamento(input, 0.002f);

			return new Module(input, output, new Dictionary<string, ProxyController<float>>(),
				Enumerable.Empty<INotable>());
		}

		#region Oscillators

		/// <summary>
		/// 正弦波オシレータ
		/// </summary>
		/// <returns></returns>
		internal static Module SinOsc() {
			var freq = Proxy<float>();

			var output = Nodes.SinOsc(freq);

			return new Module(freq, output,
					new Dictionary<string, ProxyController<float>>() {
					},
					new INotable[] {
					});
		}

		/// <summary>
		/// 正弦波オシレータ（テスト用設定）
		/// </summary>
		/// <returns></returns>
		internal static Module FixedSinOsc() {
			var output = (Nodes.SinOsc(5f) * 2000 + 4000).AsFloat();

			return new Module(Enumerable.Empty<ProxyController<float>>(), output,
					new Dictionary<string, ProxyController<float>>() {
					},
					new INotable[] {
					});
		}

		internal static Module FixedSinOscWithDummyInput() {
			var dummy = Proxy<float>();
			var output = (Nodes.SinOsc(5f) * 2000 + 4000).AsFloat();

			return new Module(new[] { dummy }, output,
					new Dictionary<string, ProxyController<float>>() {
					},
					new INotable[] {
					});
		}

		/// <summary>
		/// パルス波オシレータ
		/// </summary>
		/// <returns></returns>
		internal static Module PulseOsc() {
			var freq = Proxy<float>();

			var dutyInit = 0.5f; // TODO 初期値はスタック管理のため外から見える必要がありそう
			var duty = Proxy(dutyInit);

			var output = Nodes.PulseOsc(freq, duty);

			return new Module(freq, output,
					new Dictionary<string, ProxyController<float>>() {
						{ "duty", duty },
					},
					new INotable[] {
					});
		}

		#endregion

		#region Filters

		internal static Module Lpf() {
			var input = Proxy<float>();

			var cutoff = Proxy(500f);
			var q = Proxy(5f);
			var output = Nodes.Lpf(input, cutoff, q);

			return new Module(input, output,
					new Dictionary<string, ProxyController<float>>() {
						{ "cutoff", cutoff },
						{ "q", q },
					},
					new INotable[] {
					});
		}


		#endregion

		#region Envelopes

		/// <summary>
		/// 音量が 1 か 0 しかないエンベロープ
		/// </summary>
		/// <returns></returns>
		internal static Module PlainEnv() {
			var output = Nodes.PlainEnv();

			return new Module(Enumerable.Empty<ProxyController<float>>(), output,
					new Dictionary<string, ProxyController<float>>() {
					},
					new INotable[] {
						output,
					});
		}

		/// <summary>
		/// 指数的に減衰するエンベロープ
		/// </summary>
		/// <returns></returns>
		internal static Module ExpEnv() {
			var decay = Proxy(1 / 8f);
			var output = Nodes.ExpEnv(decay);

			return new Module(Enumerable.Empty<ProxyController<float>>(), output,
					new Dictionary<string, ProxyController<float>>() {
						{ "decay", decay },
					},
					new INotable[] {
						output,
					});
		}

		internal static Module AdsrEnv() {
			var attack = Proxy(0.15f);
			var decay = Proxy(0.1f);
			var sustain = Proxy(0.8f);
			var release = Proxy(0.25f);

			var output = Nodes.AdsrEnv(attack, decay, sustain, release);

			return new Module(Enumerable.Empty<ProxyController<float>>(), output,
					new Dictionary<string, ProxyController<float>>() {
						{ "attack", attack },
						{ "decay", decay },
						{ "sustain", sustain },
						{ "release", release },
					},
					new INotable[] {
						output,
					});
		}

		#endregion

	}
}