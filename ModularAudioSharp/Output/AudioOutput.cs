using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp.Output {
	public class AudioOutput<T> : Output<T>, IDisposable where T: struct {
		private WaveOut waveOut = null;

		public override void Play(IEnumerable<T> signal) {
			var wave = EnumerableWaveProvider32.New(signal);
			this.waveOut = new WaveOut {
				DesiredLatency = 200,
			};
			this.waveOut.Init(wave);
			this.waveOut.Play();
		}

		public void Dispose() {
			this.waveOut?.Dispose();
		}
	}
}
