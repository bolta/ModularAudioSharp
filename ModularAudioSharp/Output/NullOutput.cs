using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp.Output {
	public class NullOutput<T> : Output<T> where T: struct {

		private readonly int samplesToPlay;

		public NullOutput(int samplesToPlay) {
			this.samplesToPlay = samplesToPlay;
		}

		public override void Play(IEnumerable<T> signal) {
			int count = 0;
			foreach (var sample in signal.Take(this.samplesToPlay)) {
				++ count;
				if (count % ModuleSpace.SampleRate == 0) {
					Console.WriteLine($"Elapsed samples: {count}");
				}
			}
		}
		//public void Stop();
	}
}
