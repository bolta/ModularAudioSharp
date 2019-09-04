using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularAudioSharp.Output;
using ModularAudioSharp.Sequencer;
using NAudio.Wave;

namespace ModularAudioSharp {
	public static class ModuleSpace {

		// TODO 初期化手段
		public static int SampleRate { get; } = 44100;

		/// <summary>
		/// （本質的かどうかに依らず）能動的な全てのノード
		/// </summary>
		private static readonly IList<Node> activeNodes = new List<Node>();

		/// <summary>
		/// （本質的かどうかに依らず）能動的なノードを管理下に加える
		/// </summary>
		/// <param name="node"></param>
		public static void AddActiveNode(Node node) {
			Debug.Assert(! activeNodes.Contains(node));
			activeNodes.Add(node);
		}

		private static readonly IList<Tick> ticks = new List<Tick>();
		public static void AddTick(Tick tick) {
			Debug.Assert(! ticks.Contains(tick));
			ticks.Add(tick);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T">float（モノラル）、Stereo&lt;float&gt;（ステレオ）のみ受け付ける。これ以外の型は実行時エラー</typeparam>
		/// <param name="master"></param>
		/// <returns>再生用のオブジェクトの寿命を管理するためのオブジェクト。
		/// 再生が終わったら Dispose すること</returns>
		public static void Play<T>(Node<T> master, Output<T> output) where T : struct {
			var signal = MakeMasterSignal(master);
			output.Play(signal);
		}

		/// <summary>
		/// ノードを演奏（つまり、能動的ノードと tick を更新しながらノードの出力を取り出す）して
		/// サンプルを列挙する IEnumerable を作る
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="master"></param>
		/// <returns></returns>
		private static IEnumerable<T> MakeMasterSignal<T>(Node<T> master) where T : struct {
			// TODO use してしまうと 2 回再生できない
			var masterOut = master.UseAsStream();

			foreach (var value in masterOut) {
				foreach (var tick in ticks) tick.Sample();
				foreach (var cache in activeNodes) cache.Update();
				yield return value;
			}
		}
	}
}
