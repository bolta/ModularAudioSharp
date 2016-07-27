using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp.Sequencer {
	public class Tick {
		/// <summary>
		/// シーケンサの時間の最小単位となる tick を発生させるノード
		/// </summary>
		/// <param name="tempo">テンポ（BPM）</param>
		/// <param name="ticksPerBeat">1 拍に何回 tick を発生させるか</param>
		/// <returns>tick が発生するとき true、そうでないとき false の値をとるノード</returns>
		public static Node<bool> New(Node tempo, int ticksPerBeat) {
			return new Node<bool>(New(tempo.AsFloat().UseAsStream(), ticksPerBeat));
		}

		private static IEnumerable<bool> New(IEnumerable<float> tempo, int ticksPerBeat) {
			// 初回（演奏開始の瞬間）は trigger する
			yield return true;

			// その後は timer をサンプルごとに増やし、1 増えるごとに trigger する
			var timer = 0f;
			var trigger = true;
			foreach (var t in tempo) {
				var newTimer = timer + t * ticksPerBeat / 60 / ModuleSpace.SampleRate;
				trigger = Math.Floor(newTimer) != Math.Floor(timer);
				timer = newTimer % 1f;

				yield return trigger;
			}
		}

	}
}
