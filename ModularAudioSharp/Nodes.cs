using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp {
	/// <summary>
	/// Module の生成や演算を扱うメソッド群。
	/// アプリケーションコードから using static でクラスごと取り込まれることを想定する
	/// </summary>
	public static class Nodes {

		/// <summary>
		/// 定数を表すモジュール。
		/// TODO Update 不要なので、その旨 ModuleSpace へ通知するように
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static Node Const(float value) { return new Node(Const_(value)); }

		private static IEnumerable<float> Const_(float value) {
			while (true) yield return value;
		}

		/// <summary>
		/// source の出力を amount_smp サンプルだけ遅らせて再現する
		/// </summary>
		/// <param name="source"></param>
		/// <param name="amount_smp"></param>
		/// <returns></returns>
		public static Node Delay(Node source, int amount_smp) {
			return new Node(Delay(source.UseAsStream(), amount_smp));
		}

		private static IEnumerable<float> Delay(IEnumerable<float> source, int amount_smp) {
			var buffer = new float[amount_smp];
			var index = 0;
			foreach (var value in source) {
				yield return buffer[index];
				buffer[index] = value;
				index = (index + 1) % amount_smp;
			}
		}

		/// <summary>
		/// m の出力に単項演算 calc を適用する
		/// </summary>
		/// <param name="m"></param>
		/// <param name="calc"></param>
		/// <returns></returns>
		public static Node Calc(Node m, Func<float, float> calc) {
			return new Node(m.UseAsStream().Select(calc));
		}

		/// <summary>
		/// lhs・rhs の出力に二項演算 calc を適用する
		/// </summary>
		/// <param name="lhs"></param>
		/// <param name="rhs"></param>
		/// <param name="calc"></param>
		/// <returns></returns>
		public static Node Calc(Node lhs, Node rhs, Func<float, float, float> calc) {
			return new Node(lhs.UseAsStream().Zip(rhs.UseAsStream(), calc));
		}

		/// <summary>
		/// 正弦波を出力するオシレータ。周波数はモジュールで与える
		/// </summary>
		/// <param name="freq"></param>
		/// <returns></returns>
		public static Node SinOsc(Node freq) {
			return Osc(freq, phase => (float) Math.Sin(phase));
		}

		public static Node SquareOsc(Node freq) {
			return Osc(freq, phase => phase % (2 * Math.PI) < Math.PI ? 1 : -1);
		}

		public static Node Osc(Node freq, Func<float, float> func) {
			var phaseDiffs = freq.UseAsStream().Select(f => (float) (2 * Math.PI * f / ModuleSpace.SampleRate));
			return new Node(Osc(phaseDiffs, func));
		}

		private static IEnumerable<float> Osc(IEnumerable<float> phaseDiffs, Func<float, float> func) {
			var phase = 0f;
			foreach (var dp in phaseDiffs) {
				yield return func(phase);
				phase += dp;
			}
		}

	}
}
