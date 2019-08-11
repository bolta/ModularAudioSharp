using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp {
	public class PlainEnvController : NodeController<float>, INotable {

		private float amplitude = 0f;

		public PlainEnvController() : base(true) { }

		protected override IEnumerable<float> Signal() {
			while (true) {
				yield return this.amplitude;
			}
		}

		public void NoteOn() {
			this.amplitude = 1f;
		}

		public void NoteOff() {
			this.amplitude = 0f;
		}

	}
}
