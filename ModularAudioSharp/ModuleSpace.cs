using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularAudioSharp.Sequencer;
using NAudio.Wave;

namespace ModularAudioSharp {
	public static class ModuleSpace {

		// TODO 初期化手段
		public static int SampleRate { get; } = 44101;

		private static IList<Node> cachingNodes = new List<Node>();
		public static void AddCachingNode(Node node) {
			Debug.Assert(! cachingNodes.Contains(node));
			cachingNodes.Add(node);
		}

		private static IList<Tick> ticks = new List<Tick>();
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
		public static Player Play<T>(Node<T> master) where T : struct {
			var signal = EnumerableWaveProvider32.New(MakeMasterSignal(master));

			var waveOut = new WaveOut {
				DesiredLatency = 200,
			};
			waveOut.Init(signal);
			waveOut.Play();

			return new Player(waveOut);
		}

		private static IEnumerable<T> MakeMasterSignal<T>(Node<T> master) where T : struct {
			// TODO use してしまうと 2 回再生できない
			var masterOut = master.UseAsStream();

			foreach (var value in masterOut) {
				foreach (var tick in ticks) tick.Sample();
				foreach (var cache in cachingNodes) cache.Update();
				yield return value;
			}
		}
	}
}
