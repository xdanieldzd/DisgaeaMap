using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Cobalt.IO;

namespace DisgaeaMap
{
	public abstract class BaseChunk : ParsableData
	{
		public uint DataSize { get; private set; }
		public uint[] MaybePadding { get; private set; }

		public BaseChunk(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			base.ReadFromStream(stream, endianness);

			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			DataSize = reader.ReadUInt32();
			MaybePadding = new uint[3];
			for (int i = 0; i < MaybePadding.Length; i++) MaybePadding[i] = reader.ReadUInt32();
		}
	}
}
