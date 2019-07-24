using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp.Data {
	public static class Stereo {
		public static Stereo<T> Create<T>(T left, T right) => new Stereo<T>(left, right);
	}

	public struct Stereo<T> {
		public T Left { get; private set; }
		public T Right { get; private set; }

		public Stereo(T left, T right) {
			this.Left = left;
			this.Right = right;
		}

		public override string ToString() => $"({this.Left}, {this.Right})";

		//public Stereo<U> Select<U>(Func<T, U> selector) => new Stereo<U>(selector(this.Left), selector(this.Right));
	}


}
