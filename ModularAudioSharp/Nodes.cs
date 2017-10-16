using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularAudioSharp.Data;

namespace ModularAudioSharp {
	/// <summary>
	/// Node の生成や演算を扱うメソッド群。
	/// アプリケーションコードから using static でクラスごと取り込まれることを想定する
	/// </summary>
	public static class Nodes {

		/// <summary>
		/// 定数を表すノード。
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static Node<T> Const<T>(T value) where T : struct => Node.Create(Const_(value), true);

		private static IEnumerable<T> Const_<T>(T value) where T : struct {
			while (true) yield return value;
		}

		/// <summary>
		/// 可変値を保持するノード。
		/// TODO Update 不要なので、その旨 ModuleSpace へ通知するように
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="initValue"></param>
		/// <returns></returns>
		public static VarController<T> Var<T>(T initValue = default(T)) where T : struct => new VarController<T>(initValue);

		public static ExpEnvController ExpEnv(float ratioPerSec, Node<NoteOperation> oper = null)
				=> new ExpEnvController(ratioPerSec, oper);

		/// <summary>
		/// source の出力を amount_smp サンプルだけ遅らせて再現する
		/// </summary>
		/// <param name="source"></param>
		/// <param name="amount_smp"></param>
		/// <returns></returns>
		public static Node<T> Delay<T>(Node<T> source, int amount_smp) where T : struct
				=> Node.Create(Delay(source.UseAsStream(), amount_smp));

		private static IEnumerable<T> Delay<T>(IEnumerable<T> source, int amount_smp) where T : struct {
			var buffer = new T[amount_smp];
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
		public static Node<T> Calc<T>(Node<T> m, Func<T, T> calc) where T : struct
				=> Node.Create(m.UseAsStream().Select(calc));

		/// <summary>
		/// lhs・rhs の出力に二項演算 calc を適用する
		/// </summary>
		/// <param name="lhs"></param>
		/// <param name="rhs"></param>
		/// <param name="calc"></param>
		/// <returns></returns>
		public static Node<T> Calc<T>(Node<T> lhs, Node<T> rhs, Func<T, T, T> calc) where T : struct
				=> Node.Create(lhs.UseAsStream().Zip(rhs.UseAsStream(), calc));

		/// <summary>
		/// 正弦波を出力するオシレータ。周波数はモジュールで与える
		/// </summary>
		/// <param name="freq"></param>
		/// <returns></returns>
		public static Node<float> SinOsc(Node freq, bool crazy = false) {
			return Osc(freq.AsFloat(), phase => (float) Math.Sin(phase), crazy);
		}

		public static Node<float> SquareOsc(Node freq, bool crazy = false) {
			return Osc((Node<float>) freq, phase => phase % (2 * Math.PI) < Math.PI ? 1f : -1f, crazy);
		}

		public static Node<float> Osc(Node freq, Func<float, float> func, bool crazy = false) {
			var phaseDiffs = freq.AsFloat().UseAsStream().Select(f => (float) (2 * Math.PI * f / ModuleSpace.SampleRate));
			return Node.Create(Osc(phaseDiffs, func, crazy));
		}

		private static IEnumerable<float> Osc(IEnumerable<float> phaseDiffs, Func<float, float> func, bool crazy = false) {
			const float twoPi = (float) (2 * Math.PI);
			var phase = 0f;
			foreach (var dp in phaseDiffs) {
				yield return func(phase);
				phase = phase + dp;
				// 2π で余りをとらないと位相がどんどん大きくなり、演算誤差で音程が不安定になる。これはこれで面白い
				if (! crazy) phase %= twoPi;
			}
		}

		public static Node<float> Lpf(Node<float> input, Node<float> cutoffFreq, Node<float> q) {
			var w0 = (2 * (float) Math.PI * cutoffFreq / ModuleSpace.SampleRate).AsFloat();
			var cosw0 = w0.Select(w => (float) Math.Cos(w));
			var sinw0 = w0.Select(w => (float) Math.Sin(w));
			var alpha = sinw0 / (2 * q);

			var b0 = (1 - cosw0) / 2;
			var b1 = 1 - cosw0;
			var b2 = (1 - cosw0) / 2;
			var a0 = 1 + alpha;
			var a1 = -2 * cosw0;
			var a2 = 1 - alpha;

			return BiQuadFilter(input, b0.AsFloat(), b1.AsFloat(), b2.AsFloat(), a0.AsFloat(), a1.AsFloat(), a2.AsFloat());
		}

		public static Node<float> BiQuadFilter(Node<float> input,
				Node<float> b0, Node<float> b1, Node<float> b2,
				Node<float> a0, Node<float> a1, Node<float> a2)
				=> Node.Create(BiQuadFilter_(input.UseAsStream(),
						b0.UseAsStream(), b1.UseAsStream(), b2.UseAsStream(),
						a0.UseAsStream(), a1.UseAsStream(), a2.UseAsStream()));

		private static IEnumerable<float> BiQuadFilter_(IEnumerable<float> input,
				IEnumerable<float> b0, IEnumerable<float> b1, IEnumerable<float> b2,
				IEnumerable<float> a0, IEnumerable<float> a1, IEnumerable<float> a2) {
			var inDelay = new DelayBuffer<float>(2);
			var outDelay = new DelayBuffer<float>(2);

			foreach (var values in input.Zip(b0, b1, b2, a0, a1, a2, Tuple.Create)) {
				var inValue = values.Item1;
				var b0_ = values.Item2;
				var b1_ = values.Item3;
				var b2_ = values.Item4;
				var a0_ = values.Item5;
				var a1_ = values.Item6;
				var a2_ = values.Item7;
				var outValue = (b0_*inValue + b1_*inDelay[0] + b2_*inDelay[-1] - a1_*outDelay[0] - a2_*outDelay[-1]) / a0_;
				inDelay.Push(inValue);
				outDelay.Push(outValue);

				yield return outValue;
			}
		}

		public static Node<float> Portamento(Node freq, float ratio)
				=> Node.Create(Portamento(freq.AsFloat().UseAsStream(), ratio));

		private static IEnumerable<float> Portamento(IEnumerable<float> freq, float ratio) {
			float? actualFreq = null;

			foreach (var f in freq) {
				if (! actualFreq.HasValue) actualFreq = f;
				yield return actualFreq.Value;

				actualFreq = (1 - ratio) * actualFreq.Value + ratio * f;
			}
		}

		public static Node<Stereo<T>> ZipToStereo<T>(Node<T> left, Node<T> right) where T : struct
				=> Node.Create(left.UseAsStream().Zip(right.UseAsStream(), Stereo.Create));
	}
}
