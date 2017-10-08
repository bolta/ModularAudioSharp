using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularAudioSharp.Data;

namespace ModularAudioSharp.Mml {
	public struct SimpleMmlValue {
		public NoteOperation NoteOperation { get; set; }
		public Tone Tone { get; set; }
	}
}
