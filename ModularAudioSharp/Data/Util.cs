using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp.Data {
	static class Util {
		internal static string RepeatString(object element, int count) =>
				string.Concat(Enumerable.Repeat(element, count));

		internal static V? TryGetStructValue<K, V>(this IDictionary<K, V> dict, K key) where V : struct {
			V value;
			if (dict.TryGetValue(key, out value)) return value; else return null;
		}

		internal static V TryGetClassValue<K, V>(this IDictionary<K, V> dict, K key) where V : class {
			V value;
			if (dict.TryGetValue(key, out value)) return value; else return null;
		}
	}
}
