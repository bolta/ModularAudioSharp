using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp.Output {
	public abstract class Output<T> where T: struct {
		public abstract void Play(IEnumerable<T> signal);
		//public void Stop();
	}
}
