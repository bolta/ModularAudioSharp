using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp {
	public static class SequencerExper1 {

		public static Node<Output> New(Node tempo, Node portamentoRate) {
			return new Node<Output>(New(tempo.AsFloat().UseAsStream(),
					portamentoRate.AsFloat().UseAsStream()));
		}

		private static IEnumerable<Output> New(IEnumerable<float> tempo, IEnumerable<float> portamentoRate) {
			// サンプルごとに増加し、1 拍で 1 進む
			var timer = 0f;
			var innerFreqIndex = 0;

			return tempo.Zip(portamentoRate, Tuple.Create).Select(tp => {
				var t = tp.Item1;
				var p = tp.Item2;
				var newTimer = timer + t / 60 / ModuleSpace.SampleRate;
				var trigger = Math.Floor(newTimer) != Math.Floor(timer);
				if (trigger) {
					innerFreqIndex = (innerFreqIndex + 1) % FREQS.Length;
					newTimer -= 1;
				}
				timer = newTimer;

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
