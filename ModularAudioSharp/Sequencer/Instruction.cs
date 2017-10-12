using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp.Sequencer {

	// クラスを設けるまでもなく、Action<SequenceThread> だけでいいかもしれない。
	// ただ、演奏を記録して再現することを考えるとクラスがあった方がいいかも…という程度
	public abstract class Instruction {
		internal abstract void Execute(SequenceThread thread);
	}

	//public class LoopInstruction<T> : Instruction<T> where T : struct {

	//}

	public class ValueInstruction<T> : Instruction where T : struct {
		private readonly VarController<T> target;
		private readonly T value;

		public ValueInstruction(VarController<T> target, T value) {
			this.target = target;
			this.value = value;
		}

		internal override void Execute(SequenceThread thread) {
			this.target.Set(this.value);
		}
	}

	public class NoteInstruction : Instruction {
		private readonly INotable target;
		private readonly bool noteOn;

		public NoteInstruction(INotable target, bool noteOn) {
			this.target = target;
			this.noteOn = noteOn;
		}

		internal override void Execute(SequenceThread thread) {
			if (this.noteOn) this.target.NoteOn(); else this.target.NoteOff();
		}
	}

	//public class ValueOnceInstruction<T> : Instruction<T> where T : struct {
	//	private readonly T value;

	//	public ValueOnceInstruction(T value) { this.value = value; }

	//	internal override void Execute(SequenceThread<T> thread) {
	//		thread.ValueOnce = this.value;
	//	}
	//}

	public class WaitInstruction : Instruction {
		private readonly int wait;
		public WaitInstruction(int wait) { this.wait = wait; }
		internal override void Execute(SequenceThread thread) {
			Debug.Assert(thread.Wait == 0);
			thread.Wait = this.wait;
		}
	}

	public class JumpInstruction : Instruction {
		private readonly int destination;
		public JumpInstruction(int destination) { this.destination = destination; }
		internal override void Execute(SequenceThread thread) {
			// この後、thread 側で 1 足されることを見越して 1 引いておく
			thread.Pointer = destination - 1;
		}
	}
}
