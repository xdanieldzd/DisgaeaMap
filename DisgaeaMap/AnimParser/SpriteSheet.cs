using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Cobalt.IO;

namespace DisgaeaMap.AnimParser
{
	public class SpriteSheet : ParsableData
	{
		public uint PixelDataOffset { get; private set; }
		public ushort Width { get; private set; }
		public ushort Height { get; private set; }
		public byte[] Unknown0x08 { get; private set; }

		public SpriteSheet(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			PixelDataOffset = reader.ReadUInt32();
			Width = reader.ReadUInt16();
			Height = reader.ReadUInt16();
			Unknown0x08 = reader.ReadBytes(4);
		}
	}
}
