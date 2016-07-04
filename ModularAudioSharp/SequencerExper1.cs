using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp {
	class SequencerExper1 {
		private Node tempo;
		private Node portamentoRate;

		private float timer = 0;
		private float logicalFreq;
		private float realFreq;

		public SequencerExper1(Node tempo, Node portamentoRate) {
			this.tempo = tempo;
			this.portamentoRate = portamentoRate;
		}
	}
}
