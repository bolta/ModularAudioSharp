﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularAudioSharp.Sequencer;

namespace ModularAudioSharp.Mml {
	interface IInstructionGenerator<AstRoot> {
		IEnumerable<Instruction> GenerateInstructions(AstRoot root, int ticksPerBeat, Temperament temper);
	}
}
