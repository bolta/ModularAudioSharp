using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp.Data {
	public static class Util {
		public static string RepeatString(object element, int count) =>
				string.Concat(Enumerable.Repeat(element, count));

		public static V? TryGetStructValue<K, V>(this IDictionary<K, V> dict, K key) where V : struct {
			if (dict.TryGetValue(key, out var value)) return value; else return null;
		}

		public static V TryGetClassValue<K, V>(this IDictionary<K, V> dict, K key) where V : class {
			if (dict.TryGetValue(key, out var value)) return value; else return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="U"></typeparam>
		/// <param name="_">型引数を明示しなくてよいためのダミーの引数。値は使わない（default(T) でよい）</param>
		/// <param name="whenMono"></param>
		/// <param name="whenStereo"></param>
		/// <returns></returns>
		public static U OnChannelCount<T, U>(T _, U whenMono, U whenStereo) {
			if (typeof(T) == typeof(float)) return whenMono;
			if (typeof(T) == typeof(Stereo<float>)) return whenStereo;
			throw new ArgumentException($"unsupported type: {typeof(T)}");
		}

		public static U OnChannelCountLazy<T, U>(T _, Func<U> whenMono, Func<U> whenStereo)
				=> OnChannelCount(_, whenMono, whenStereo)();


		public static IEnumerable<TResult> Zip<T1, T2, T3, TResult>(this IEnumerable<T1> t1s,
				IEnumerable<T2> t2s, IEnumerable<T3> t3s,
				Func<T1, T2, T3, TResult> zipElem) {
			var e1 = t1s.GetEnumerator();
			var e2 = t2s.GetEnumerator();
			var e3 = t3s.GetEnumerator();
			while (e1.MoveNext() && e2.MoveNext() && e3.MoveNext()) {
				yield return zipElem(e1.Current, e2.Current, e3.Current);
			}
		}
		public static IEnumerable<TResult> Zip<T1, T2, T3, T4, TResult>(this IEnumerable<T1> t1s,
				IEnumerable<T2> t2s, IEnumerable<T3> t3s, IEnumerable<T4> t4s,
				Func<T1, T2, T3, T4, TResult> zipElem) {
			var e1 = t1s.GetEnumerator();
			var e2 = t2s.GetEnumerator();
			var e3 = t3s.GetEnumerator();
			var e4 = t4s.GetEnumerator();
			while (e1.MoveNext() && e2.MoveNext() && e3.MoveNext() && e4.MoveNext()) {
				yield return zipElem(e1.Current, e2.Current, e3.Current, e4.Current);
			}
		}

		public static IEnumerable<TResult> Zip<T1, T2, T3, T4, T5, T6, T7, TResult>(this IEnumerable<T1> t1s,
				IEnumerable<T2> t2s, IEnumerable<T3> t3s, IEnumerable<T4> t4s, IEnumerable<T5> t5s, IEnumerable<T6> t6s, IEnumerable<T7> t7s,
				Func<T1, T2, T3, T4, T5, T6, T7, TResult> zipElem) {
			var e1 = t1s.GetEnumerator();
			var e2 = t2s.GetEnumerator();
			var e3 = t3s.GetEnumerator();
			var e4 = t4s.GetEnumerator();
			var e5 = t5s.GetEnumerator();
			var e6 = t6s.GetEnumerator();
			var e7 = t7s.GetEnumerator();
			while (e1.MoveNext() && e2.MoveNext() && e3.MoveNext() && e4.MoveNext() && e5.MoveNext() && e6.MoveNext() && e7.MoveNext()) {
				yield return zipElem(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current);
			}
		}
	}
}
