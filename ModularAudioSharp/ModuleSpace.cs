using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace ModularAudioSharp {
	public static class ModuleSpace {

		// TODO 初期化手段
		public static int SampleRate { get; } = 44101;

		// ステレオ未対応のため固定
		public static int Channels { get; } = 1;

		private static IList<Node> cachingNodes = new List<Node>();
		public static void AddCachingNode<T>(Node<T> node) where T : struct {
			Debug.Assert(! cachingNodes.Contains(node));
			cachingNodes.Add(node);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="master"></param>
		/// <returns>再生用のオブジェクトの寿命を管理するためのオブジェクト。
		/// 再生が終わったら Dispose すること</returns>
		public static Player Play(Node<float> master) {
			var signal = new EnumerableWaveProvider32(MakeMasterSignal(master));
			signal.SetWaveFormat(SampleRate, Channels);

			var waveOut = new WaveOut {
				DesiredLatency = 200,
			};
			waveOut.Init(signal);
			waveOut.Play();

			return new Player(waveOut);
		}

		private static IEnumerable<float> MakeMasterSignal(Node<float> master) {
			// TODO use してしまうと 2 回再生できない
			var masterOut = master.UseAsStream();
			foreach (var cache in cachingNodes) cache.Update();
			foreach (var value in masterOut) {
				yield return value;
				foreach (var cache in cachingNodes) cache.Update();
			}
		}
	}
}
