using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp.Data {
	static class Util {
		internal static string RepeatString(object element, int count) =>
				string.Concat(Enumerable.Repeat(element, count));
	}
}
