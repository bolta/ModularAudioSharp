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
		/// 定数を表すノード
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static Node<T> Const<T>(T value) where T : struct {
			IEnumerable<T> signal() { while (true) yield return value; }

			return Node.Create(signal(), false);
		}

		/// <summary>
		/// 可変値を保持するノード
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="initValue"></param>
		/// <returns></returns>
		public static VarController<T> Var<T>(T initValue = default) where T : struct
				=> new VarController<T>(initValue);

		public static ProxyController<T> Proxy<T>() where T : struct => new ProxyController<T>();
		public static ProxyController<T> Proxy<T>(T initValue) where T : struct => new ProxyController<T>(initValue);

		public static ExpEnvController ExpEnv(Node<float> ratioPerSec)
				=> new ExpEnvController(ratioPerSec);

		public static AdsrEnvController AdsrEnv(Node<float> attackTimeSecs, Node<float> decayTimeSecs,
				Node<float> sustainLevelVol, Node<float> releaseTimeSecs)
				=> new AdsrEnvController(attackTimeSecs, decayTimeSecs, sustainLevelVol, releaseTimeSecs);

		public static PlainEnvController PlainEnv()
				=> new PlainEnvController();

		/// <summary>
		/// 正弦波を出力するオシレータ
		/// </summary>
		/// <param name="freq"></param>
		/// <returns></returns>
		public static Node<float> SinOsc(Node freq, bool crazy = false) {
			return Osc(freq.AsFloat(), phase => (float) Math.Sin(phase), crazy);
		}

		public static Node<float> SquareOsc(Node freq, bool crazy = false) {
			return Osc((Node<float>) freq, phase => phase % (2 * Math.PI) < Math.PI ? 1f : -1f, crazy);
		}

		public static Node<float> TriangleOsc(Node freq, bool crazy = false) {
			const float twoPi = (float) (2 * Math.PI);
			return Osc((Node<float>) freq,
					phase => {
						var p = phase % twoPi;
						return (float) (
 								p < Math.PI * 0.5
										? (2 / Math.PI) * p
								: p < Math.PI * 1.5
										? - (2 / Math.PI) * p + 2
								:
										(2 / Math.PI) * p - 4);
					}, crazy);
		}

		public static Node<float> Osc(Node freq, Func<float, float> func, bool crazy = false) {
			var phaseDiffs = freq.AsFloat().UseAsStream().Select(f => (float) (2 * Math.PI * f / ModuleSpace.SampleRate));

			IEnumerable<float> signal() {
				const float twoPi = (float) (2 * Math.PI);
				var phase = 0f;
				foreach (var dp in phaseDiffs) {
					yield return func(phase);
					phase += dp;
					// 2π で余りをとらないと位相がどんどん大きくなり、演算誤差で音程が不安定になる。これはこれで面白い
					if (! crazy) phase %= twoPi;
				}
			}

			return Node.Create(signal(), true, freq);
		}

		// TODO パルス波のオシレータはデューティ比も参照するため既存の Osc では対応できなかった。
		// これも合わせて Osc() で一般化できるようにしたい

		public static Node<float> PulseOsc(Node freq, Node duty, bool crazy = false) {
			var phaseDiffs = freq.AsFloat().UseAsStream().Select(f => (float) (2 * Math.PI * f / ModuleSpace.SampleRate));
			var dutyStr = duty.AsFloat().UseAsStream();
			IEnumerable<float> signal() {
				const float twoPi = (float) (2 * Math.PI);
				var phase = 0f;
				foreach (var input in phaseDiffs.Zip(dutyStr, Tuple.Create)) {
					var dp = input.Item1;
					var d = input.Item2;
					yield return phase % twoPi < twoPi * d ? 1f : -1f;
					phase += dp;
					// 2π で余りをとらないと位相がどんどん大きくなり、演算誤差で音程が不安定になる。これはこれで面白い
					if (! crazy) phase %= twoPi;
				}
			}

			return Node.Create(signal(), true, freq, duty);
		}

		public static Node<float> Noise() {
			IEnumerable<float> signal() {
				var rand = new Random();
				while (true) {
					var value = (float) (rand.NextDouble() * 2 - 1);
					yield return value;
				}
			}

			return Node.Create(signal(), true);
		}

		/// <summary>
		/// Audio EQ Cookbook に依ったローパスフィルタ
		/// </summary>
		/// <param name="input"></param>
		/// <param name="cutoffFreq"></param>
		/// <param name="q"></param>
		/// <returns></returns>
		public static Node<float> Lpf(this Node input, Node cutoffFreq, Node q) {
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

			return BiQuadFilter(input.AsFloat(),
					b0.AsFloat(), b1.AsFloat(), b2.AsFloat(),
					a0.AsFloat(), a1.AsFloat(), a2.AsFloat());
		}

		/// <summary>
		/// Audio EQ Cookbook に依ったハイパスフィルタ
		/// </summary>
		/// <param name="input"></param>
		/// <param name="cutoffFreq"></param>
		/// <param name="q"></param>
		/// <returns></returns>
		public static Node<float> Hpf(this Node input, Node cutoffFreq, Node q) {
			// TODO この 4 つ LPF と共通化
			var w0 = (2 * (float) Math.PI * cutoffFreq / ModuleSpace.SampleRate).AsFloat();
			var cosw0 = w0.Select(w => (float) Math.Cos(w));
			var sinw0 = w0.Select(w => (float) Math.Sin(w));
			var alpha = sinw0 / (2 * q);

			var b0 = (1 + cosw0) / 2;
			var b1 = -1 * (1 + cosw0);
			var b2 = (1 + cosw0) / 2;
			var a0 = 1 + alpha;
			var a1 = -2 * cosw0;
			var a2 = 1 - alpha;

			return BiQuadFilter(input.AsFloat(),
					b0.AsFloat(), b1.AsFloat(), b2.AsFloat(),
					a0.AsFloat(), a1.AsFloat(), a2.AsFloat());
		}

		/// <summary>
		/// Audio EQ Cookbook に依ったバンドパスフィルタ
		/// </summary>
		/// <param name="input"></param>
		/// <param name="cutoffFreq"></param>
		/// <param name="q"></param>
		/// <returns></returns>
		public static Node<float> Bpf(this Node input, Node cutoffFreq, Node q) {
			// TODO この 4 つ LPF と共通化
			var w0 = (2 * (float) Math.PI * cutoffFreq / ModuleSpace.SampleRate).AsFloat();
			var cosw0 = w0.Select(w => (float) Math.Cos(w));
			var sinw0 = w0.Select(w => (float) Math.Sin(w));
			var alpha = sinw0 / (2 * q);

			var b0 = q * alpha;
			var b1 = Const(0f);
			var b2 = -1 * q * alpha;
			var a0 = 1 + alpha;
			var a1 = -2 * cosw0;
			var a2 = 1 - alpha;

			return BiQuadFilter(input.AsFloat(),
					b0.AsFloat(), b1.AsFloat(), b2.AsFloat(),
					a0.AsFloat(), a1.AsFloat(), a2.AsFloat());
		}

		/// <summary>
		/// Audio EQ Cookbook に依った Bi-Quad フィルタ
		/// </summary>
		/// <param name="input"></param>
		/// <param name="b0"></param>
		/// <param name="b1"></param>
		/// <param name="b2"></param>
		/// <param name="a0"></param>
		/// <param name="a1"></param>
		/// <param name="a2"></param>
		/// <returns></returns>
		public static Node<float> BiQuadFilter(Node<float> input,
				Node<float> b0, Node<float> b1, Node<float> b2,
				Node<float> a0, Node<float> a1, Node<float> a2) {
			IEnumerable<float> signal() {
				var inDelay = new DelayBuffer<float>(2);
				var outDelay = new DelayBuffer<float>(2);

				foreach (var values in input.UseAsStream().Zip(
						b0.UseAsStream(),
						b1.UseAsStream(),
						b2.UseAsStream(),
						a0.UseAsStream(),
						a1.UseAsStream(),
						a2.UseAsStream(),
						Tuple.Create)) {
					var inValue = values.Item1;
					var b0_ = values.Item2;
					var b1_ = values.Item3;
					var b2_ = values.Item4;
					var a0_ = values.Item5;
					var a1_ = values.Item6;
					var a2_ = values.Item7;
					var outValue = (b0_ * inValue + b1_ * inDelay[0] + b2_ * inDelay[-1] - a1_ * outDelay[0] - a2_ * outDelay[-1]) / a0_;
					inDelay.Push(inValue);
					outDelay.Push(outValue);

					yield return outValue;
				}
			}

			return Node.Create(signal(), true, input, b0, b1, b2, a0, a1, a2);
		}

		public static Node<float> Portamento(Node freq, float ratio) {
			float? actualFreq = null;

			IEnumerable<float> signal() {
				foreach (var f in freq.AsFloat().UseAsStream()) {
					if (! actualFreq.HasValue) actualFreq = f;
					yield return actualFreq.Value;
					actualFreq = (1 - ratio) * actualFreq.Value + ratio * f;
				}
			}

			return Node.Create(signal(), true, freq);
		}

		public static Node<float> Delay(this Node src, Node time_smp, Node feedbackLevel, Node wetLevel, int maxTime_smp) {
			var buffer = new DelayBuffer<float>(maxTime_smp);

			IEnumerable<float> signal() {
				foreach (var stfw in src.AsFloat().UseAsStream().Zip(
						time_smp.AsInt().UseAsStream(),
						feedbackLevel.AsFloat().UseAsStream(),
						wetLevel.AsFloat().UseAsStream(),
						Tuple.Create)) {
					// TODO 添字が誤っていないかチェック
					yield return stfw.Item1 + buffer[- (stfw.Item2 - 1)] * stfw.Item4;
					buffer.Push(stfw.Item1 + stfw.Item3 * buffer[- (stfw.Item2 - 1)]);
				}
			}

			return Node.Create(signal(), true, src, time_smp, feedbackLevel, wetLevel);
		}

		public static Node<float> Limit(this Node src, Node min, Node max) {
			var signal = src.AsFloat().UseAsStream().Zip(
					min.AsFloat().UseAsStream(),
					max.AsFloat().UseAsStream(),
					(s, mn, mx) => s < mn ? mn : s > mx ? mx : s);

			return Node.Create(signal, false, src, min, max);
		}

		public static Node<float> QuantCrush(this Node src, Node min, Node max, Node resolution) {
			var signal = src.AsFloat().UseAsStream().Zip(
					min.AsFloat().UseAsStream(),
					max.AsFloat().UseAsStream(),
					resolution.AsInt().UseAsStream(),
					(s, mn, mx, r) =>
							mn == mx
									? 0f
							: s < mn
									? mn
							: s > mx
									? mx
							:
									(float) (Math.Floor(r * (s - mn) / (mx - mn)) / r * (mx - mn) + mn));

			return Node.Create(signal, false, src, min, max, resolution);
		}

		public static Node<Stereo<T>> ZipToStereo<T>(Node<T> left, Node<T> right) where T : struct
				=> Node.Create(left.UseAsStream().Zip(right.UseAsStream(), Stereo.Create), false, left, right);
	}
}
