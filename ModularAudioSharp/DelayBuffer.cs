using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAudioSharp {
	/// <summary>
	/// 遅延バッファ。
	/// 新たな値を加えることと、指定した数だけ過去の値を取得することができる
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class DelayBuffer<T> {
		/// <summary>
		/// 内部バッファの配列。リングバッファとして使い、head が指す場所を現在値と見なす
		/// </summary>
		private readonly T[] buffer;

		/// <summary>
		/// 現在値の添字。
		/// 次に Push すると、head が 1 つ進んだ上で値が書き込まれる
		/// </summary>
		private int head = 0;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="size">バッファのサイズ（現在から size - 1 回前までの値を保持する、）</param>
		public DelayBuffer(int size) {
			if (size < 1) throw new ArgumentException("buffer size must be positive");
			this.buffer = new T[size];
		}

		/// <summary>
		/// 新しい値を追加する。size 回前の値は消去される
		/// </summary>
		/// <param name="value"></param>
		public void Push(T value) {
			this.head = (this.head + 1) % this.buffer.Length;
			this.buffer[this.head] = value;
		}

		/// <summary>
		/// 値を取り出す。
		/// 添字は - (size - 1)（size - 1 回前の値）から 0（現在値）までが有効
		/// 追加していない状態で取り出した値は default(T) となる
		/// </summary>
		/// <param name="offset"></param>
		/// <returns></returns>
		/// <exception cref="IndexOutOfRangeException">添字が無効のとき</exception>
		public T this[int offset] {
			get {
				if (offset <= -this.buffer.Length || 0 < offset) {
					throw new IndexOutOfRangeException("offset must satisfy -size < offseet <= 0");
				}

				return this.buffer[(this.head + offset + this.buffer.Length) % this.buffer.Length];
			}
		}

		public override string ToString()
				=> "[" + string.Join(", ", Enumerable.Range(-(this.buffer.Length - 1), this.buffer.Length).Select(i => this[i])) + "]";
	}
}
