using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp.Data {
	/// <summary>
	///  ノートに関する操作
	/// </summary>
	public enum NoteOperation {
		/// <summary>
		/// 操作を行わない
		/// </summary>
		None = 0,
		NoteOff = 1,
		NoteOn = 2,
	}
}
