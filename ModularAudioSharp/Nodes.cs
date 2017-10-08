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
		/// TODO Update 不要なので、その旨 ModuleSpace へ通知するように
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static Node<T> Const<T>(T value) where T : struct { return new Node<T>(Const_(value)); }

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
		public static VarController<T> Var<T>(T initValue) where T : struct => new VarController<T>(initValue);

		public static ExpEnvController ExpEnv(float ratioPerSec, Node<NoteOperation> oper = null)
				=> new ExpEnvController(ratioPerSec, oper);

		/// <summary>
		/// source の出力を amount_smp サンプルだけ遅らせて再現する
		/// </summary>
		/// <param name="source"></param>
		/// <param name="amount_smp"></param>
		/// <returns></returns>
		public static Node<T> Delay<T>(Node<T> source, int amount_smp) where T : struct {
			return new Node<T>(Delay(source.UseAsStream(), amount_smp));
		}

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
		public static Node<T> Calc<T>(Node<T> m, Func<T, T> calc) where T : struct {
			return new Node<T>(m.UseAsStream().Select(calc));
		}

		/// <summary>
		/// lhs・rhs の出力に二項演算 calc を適用する
		/// </summary>
		/// <param name="lhs"></param>
		/// <param name="rhs"></param>
		/// <param name="calc"></param>
		/// <returns></returns>
		public static Node<T> Calc<T>(Node<T> lhs, Node<T> rhs, Func<T, T, T> calc) where T : struct {
			return new Node<T>(lhs.UseAsStream().Zip(rhs.UseAsStream(), calc));
		}

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
			return new Node<float>(Osc(phaseDiffs, func, crazy));
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

		public static Node<TMember> Select<TStruct, TMember>(this Node<TStruct> node, Func<TStruct, TMember> getMember)
				where TStruct : struct
				where TMember : struct {
			var newStream = node.UseAsStream().Select(getMember);
			return new Node<TMember>(newStream);
		}

		//public static Node<float> ExpEnv(float timesBySec) {

		//}
	}
}
