using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularAudioSharp.Data;
// TODO ループなどを表現できないのでこの定義は力不足

namespace ModularAudioSharp.Sequencer {
	/// <summary>
	/// シーケンサとして動作するノード
	/// </summary>
	public class Sequencer<T> : Node<T> where T : struct {

		private readonly Sequence<T> topLevel;
		private readonly List<Thread<T>> threads;

		private readonly int ticksPerBeat;

		private class Context {
			// 何を持つ？　型引数で外から与える必要があるかも。
			// もしくは Dictionary<string, object> を 1 つだけ持つか…
		}

		private Sequencer(Node tick, int ticksPerBeat, Sequence<T> sequence, List<Thread<T>> threads)
				: base(RunThreads(tick.AsBool().UseAsStream(), threads)) {
			this.topLevel = sequence;
			this.threads = threads;
			this.ticksPerBeat = ticksPerBeat;
		}

		public static Sequencer<T> New(Node tick, int ticksPerBeat, Sequence<T> sequence) {
			return new Sequencer<T>(tick, ticksPerBeat, sequence,
					new List<Thread<T>>() { new Thread<T>(sequence) });
		}

		private IEnumerable<T> RunThreads(IEnumerable<bool> tick) {
			var output = default(T);

			var finishedThreads = new List<Thread<T>>();
			while (true) {
				foreach (var thread in threads) {
					var hasMore = thread.Tick(ref output);
					if (! hasMore) finishedThreads.Add(thread);
				}
				// スレッドが全て削除されたら、演奏は終了しているので
				// 最後の状態を永久に保って停止（将来的には終了シグナルを挙げることになるだろう）
				yield return output;

				foreach (var finished in finishedThreads) threads.Remove(finished);
				finishedThreads.Clear();
			}
		}
	}
}
