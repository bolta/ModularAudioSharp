using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp.Sequencer {
	public abstract class Instruction<T> where T : struct {
		internal abstract void Execute(SequenceThread<T> thread);
	}

	//public class LoopInstruction<T> : Instruction<T> where T : struct {

	//}

	public class ValueInstruction<T> : Instruction<T> where T : struct {
		private readonly T value;

		public ValueInstruction(T value) { this.value = value; }

		internal override void Execute(SequenceThread<T> thread) {
			thread.CurrentValue = this.value;
		}
	}

	public class ValueOnceInstruction<T> : Instruction<T> where T : struct {
		private readonly T value;

		public ValueOnceInstruction(T value) { this.value = value; }

		internal override void Execute(SequenceThread<T> thread) {
			thread.ValueOnce = this.value;
		}
	}

	public class WaitInstruction<T> : Instruction<T> where T : struct {
		private readonly int wait;
		public WaitInstruction(int wait) { this.wait = wait; }
		internal override void Execute(SequenceThread<T> thread) {
			Debug.Assert(thread.Wait == 0);
			thread.Wait = this.wait;
		}
	}

	public class JumpInstruction<T> : Instruction<T> where T : struct {
		private readonly int destination;
		public JumpInstruction(int destination) { this.destination = destination; }
		internal override void Execute(SequenceThread<T> thread) {
			// この後、thread 側で 1 足されることを見越して 1 引いておく
			thread.Pointer = destination - 1;
		}
	}
}
