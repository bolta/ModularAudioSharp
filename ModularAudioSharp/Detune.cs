using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp {
	public abstract class Detune {
		public abstract float GetDetunedFreq(float freq);
	}

	public class CentDetune : Detune {

		private readonly float cents;

		public CentDetune(float cents) {
			this.cents = cents;
		}

		public override float GetDetunedFreq(float freq) {
			return (float) (freq * Math.Pow(2, this.cents / 1200));
		}
	}
}
