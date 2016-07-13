using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp {
	public static class SequencerExper1 {

		public static Node<Output> New(Node tick, int ticksPerRate, Node portamentoRate) {
			return new Node<Output>(New(tick.AsBool().UseAsStream(), ticksPerRate,
					portamentoRate.AsFloat().UseAsStream()));
		}

		private static IEnumerable<Output> New(IEnumerable<bool> tick, int ticksPerBeat, IEnumerable<float> portamentoRate) {
			// サンプルごとに増加し、1 拍で 1 進む
			var tickCount = 0;
			// 初回 Update で trigger した結果 0 になるよう、初期状態は -1
			var innerFreqIndex = -1;

			return tick.Zip(portamentoRate, Tuple.Create).Select(tp => {
				var tk = tp.Item1;
				var p = tp.Item2;
				var trigger = false;
				if (tk) {
					// 拍の頭で trigger する
					trigger = tickCount % ticksPerBeat == 0;
					if (trigger) {
						innerFreqIndex = (innerFreqIndex + 1) % FREQS.Length;
						tickCount %= ticksPerBeat;
					}

					++ tickCount;
				}

				var freq = FREQS[innerFreqIndex];

				return new Output {
					Freq = freq,
					Trigger = trigger,
				};
			});

		}

		private static readonly float[] FREQS = new[] { 523.2511306011972f, 659.2551138257398f, 783.9908719634986f, 1046.5022612023945f };

		public struct Output {
			public float Freq { get; internal set; }
			public bool Trigger { get; internal set; }
		}
	}
}
