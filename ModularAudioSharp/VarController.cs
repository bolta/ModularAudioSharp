using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp {
	public class VarController<T> : NodeController<T> where T : struct {

		private T value;
		public VarController(T initValue) { this.value = initValue; }
		public void Set(T value) => this.value = value;
		protected override IEnumerable<T> Signal() { while (true) yield return this.value; }
	}
}
