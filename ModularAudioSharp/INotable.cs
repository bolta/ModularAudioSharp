using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp {
	/// <summary>
	/// ノートオン・オフをサポートするインタフェース
	/// </summary>
	public interface INotable {
		void NoteOn();
		void NoteOff();
	}
}
