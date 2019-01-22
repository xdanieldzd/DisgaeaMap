using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Cobalt.IO;

namespace DisgaeaMap.AnimParser
{
	public class Unknown5 : ParsableData
	{
		public ushort IndexIntoUnknown2 { get; private set; }
		public byte Unknown0x02 { get; private set; }
		public byte Unknown0x03 { get; private set; }
		public short Unknown0x04 { get; private set; }
		public short Unknown0x06 { get; private set; }

		public Unknown5(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			IndexIntoUnknown2 = reader.ReadUInt16();
			Unknown0x02 = reader.ReadByte();
			Unknown0x03 = reader.ReadByte();
			Unknown0x04 = reader.ReadInt16();
			Unknown0x06 = reader.ReadInt16();
		}
	}
}
