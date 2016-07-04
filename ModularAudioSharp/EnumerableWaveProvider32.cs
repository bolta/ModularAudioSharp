using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace ModularAudioSharp {
	internal class EnumerableWaveProvider32 : WaveProvider32 {
		IEnumerator<float> source;

		public EnumerableWaveProvider32(IEnumerable<float> source) { this.source = source.GetEnumerator(); }

		public override int Read(float[] buffer, int offset, int sampleCount) {
			var countRead = 0;
			for (countRead = 0 ; countRead < sampleCount ; ++countRead) {
				if (! this.source.MoveNext()) break;
				buffer[offset + countRead] = this.source.Current;
			}
			return countRead;
		}
	}

}