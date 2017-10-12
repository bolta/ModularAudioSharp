using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp.Data {

	/// <summary>
	/// 意味のない値の型。意味のある出力を行わない Node は Node&lt;Unit&gt; とする
	/// </summary>
	public struct Unit {
		public static readonly Unit Value = new Unit(0);
		private Unit(int dummy) { }
	}
}
