using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp {
	public static class Amplitude {

		/// <summary>
		/// 振幅がこの値以下の場合は 0（無音）と同一と見なす。
		/// 音量から振幅を計算（指数変換）する際は指数の底となる
		/// </summary>
		private const float EPSILON = 1f / 65536;

		public static float VolumeToAmplitude(float vol) {
			if (vol <= 0f) {
				return 0f;

			} else {
				return (float) (Math.Pow(EPSILON, 1 - vol));
			}
		}

	}
}
