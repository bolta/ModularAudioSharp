using Moddl.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moddl {
	class ModuleConstructor {
		private readonly Func<IDictionary<string, Value>, Module> function;

		/// <summary>
		/// function に引数として与える値のセットだが、function が必要とする全てのエントリが揃っているとは限らない
		/// </summary>
		private readonly IDictionary<string, Value> parameters;
		
		internal ModuleConstructor(Func<IDictionary<string, Value>, Module> function,
				IDictionary<string, Value> parameters = null) {
			this.function = function;
			this.parameters = parameters ?? new Dictionary<string, Value>();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parameters"></param>
		/// <returns></returns>
		internal ModuleConstructor AddParameters(IDictionary<string, Value> parameters) =>
				// TODO キーが重複すると ArgumentException が投げられる。ModDL の例外に変換すること
				new ModuleConstructor(this.function, this.parameters.Concat(parameters).ToDictionary(kv => kv.Key, kv => kv.Value));

		// TODO パラメータの不足で起こる例外を変換する
		internal Module CreateModule() => this.function(this.parameters);
	}
}
