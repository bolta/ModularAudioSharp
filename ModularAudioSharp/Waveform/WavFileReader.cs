using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularAudioSharp.Data;

namespace ModularAudioSharp.Waveform {
	public class WavFileReader {
		public static Waveform Read(string filePath) {
			using (var stream = new FileStream(filePath, FileMode.Open)) return Read(stream);
		}

		public static Waveform Read(Stream stream) {
			var riffHeader = stream.ReadBytesAsString(4);
			stream.Position += 4;
			var riffType = stream.ReadBytesAsString(4);
			if (riffHeader != "RIFF" || riffType != "WAVE") throw new IOException("not a wav file");

			// 簡単のため関係ないチャンクも全部読む
			var chunks = new List<Chunk>();
			while (stream.Position < stream.Length) {
				var header = stream.ReadBytesAsString(4);
				var size = stream.ReadUIntLE();
				var contOff = stream.Position;
				chunks.Add(new Chunk { Header = header, ContentOffset = contOff, Size = size });
				stream.Position += size;
			}

			var fmt = chunks.Where(c => c.Header == "fmt ").FirstOrDefault();
			if (fmt == null) throw new IOException("fmt chunk not found");
			var data = chunks.Where(c => c.Header == "data").FirstOrDefault();
			if (data == null) throw new IOException("data chunk not found");

			stream.Position = fmt.ContentOffset;
			var format = stream.ReadUShortLE();
			if (format != 1) throw new IOException($"unsupported format: format id = {format}");
			var channels = stream.ReadUShortLE();
			if (channels > 2) throw new IOException($"too many channels: {channels}");
			var sampleRate = (int) stream.ReadUIntLE();
			stream.Position += 6; // データ速度、ブロックサイズ
			var bitRate = stream.ReadUShortLE();

			var samples = ReadSamples(stream, data.ContentOffset, data.Size, bitRate);
			if (channels == 1) {
				return new Waveform<float>(samples, sampleRate);
			} else {
				return new Waveform<Stereo<float>>(MakeStereo(samples), sampleRate);
			}
		}

		private static IEnumerable<float> ReadSamples(Stream stream, long dataStart, uint dataSize,
				/*int channels,*/ int bitRate) {
			for (stream.Position = dataStart ; stream.Position < dataStart + dataSize ; ) {
				var sampleU = stream.ReadUIntLE(bitRate / 8);
				float sample;
				if (bitRate == 8) {
					sample = ((sbyte) sampleU) / 128f;
				} else if (bitRate == 16) {
					sample = ((short) sampleU) / 32768f;
				} else {
					throw new IOException($"unsupported bit rate: {bitRate}");
				}

				yield return sample;
			}
		}

		private static IEnumerable<Stereo<float>> MakeStereo(IEnumerable<float> samples) {
			float? left = null;
			foreach (var sample in samples) {
				if (left.HasValue) {
					var right = sample;
					yield return Stereo.Create(left.Value, right);
					left = null;
				} else {
					left = sample;
				}
			}
		}

	}

	internal class Chunk {
		internal string Header { get; set; }
		internal long ContentOffset { get; set; }
		internal uint Size { get; set; }
	}

	internal static class StreamExtension {
		public static string ReadBytesAsString(this Stream stream, int length)
				=> new string(Enumerable.Range(0, 4).Select(_ => (char) stream.ReadByteCheckingEof()).ToArray());

		public static uint ReadUIntLE(this Stream stream, int bytes = 4) {
			var result = 0u;
			for (int i=0 ; i<bytes ; ++i) {
				result |= (uint) (stream.ReadByteCheckingEof() << (8 * i));
			}

			return result;
		}

		public static ushort ReadUShortLE(this Stream stream) => (ushort) stream.ReadUIntLE(2);

		public static byte ReadByteCheckingEof(this Stream stream) {
			int b = stream.ReadByte();
			if (b == -1) throw new IOException("unexpected end of stream");

			return (byte) b;
		}


	}
}
