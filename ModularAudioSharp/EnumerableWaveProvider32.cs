using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularAudioSharp.Data;
using NAudio.Wave;

namespace ModularAudioSharp {

	internal class EnumerableWaveProvider32 : WaveProvider32 {
		private readonly IEnumerator<float> source;
		private readonly int channels;

		internal static EnumerableWaveProvider32 New<T>(IEnumerable<T> source) {
			if (typeof(T) == typeof(float)) {
				return new EnumerableWaveProvider32((IEnumerable<float>) source, 1);
			} else if (typeof(T) == typeof(Stereo<float>)) {
				return new EnumerableWaveProvider32(((IEnumerable<Stereo<float>>) source).SelectMany(InterleaveStereo), 2);
			} else {
				throw new ArgumentException($"unsupported source type: {typeof(T)}");
			}
		}

		private EnumerableWaveProvider32(IEnumerable<float> source, int channels) {
			this.source = source.GetEnumerator();
			this.channels = channels;
			this.SetWaveFormat(ModuleSpace.SampleRate, channels);
		}

		private static IEnumerable<T> InterleaveStereo<T>(Stereo<T> s) {
			yield return s.Left;
			yield return s.Right;
		}

		public override int Read(float[] buffer, int offset, int sampleCount) {
			var countRead = 0;
			for (countRead = 0 ; countRead <= sampleCount - this.channels ; ) {
				for (var i=0 ; i<this.channels ; ++i) {
					if (! this.source.MoveNext()) break;
					buffer[offset + countRead] = this.source.Current;
					++ countRead;
				}
			}
			return countRead;
		}
	}
}
