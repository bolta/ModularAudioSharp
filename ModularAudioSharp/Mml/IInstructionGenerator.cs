using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularAudioSharp.Sequencer;

namespace ModularAudioSharp.Mml {
	interface IInstructionGenerator<AstRoot, InstructionValue> where InstructionValue : struct {
		IEnumerable<Instruction<InstructionValue>> GenerateInstructions(AstRoot root, int tickPerBeat);
	}
}
