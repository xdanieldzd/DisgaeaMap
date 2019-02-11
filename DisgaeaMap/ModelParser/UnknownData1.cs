using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Cobalt.IO;

namespace DisgaeaMap.ModelParser
{
	public class UnknownData1 : ParsableData
	{
		public float Unknown0x00 { get; private set; }
		public float Unknown0x04 { get; private set; }
		public float Unknown0x08 { get; private set; }
		public float Unknown0x0C { get; private set; }
		public float Unknown0x10 { get; private set; }
		public float Unknown0x14 { get; private set; }
		public float Unknown0x18 { get; private set; }

		public UnknownData1(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			Unknown0x00 = reader.ReadSingle();
			Unknown0x04 = reader.ReadSingle();
			Unknown0x08 = reader.ReadSingle();
			Unknown0x0C = reader.ReadSingle();
			Unknown0x10 = reader.ReadSingle();
			Unknown0x14 = reader.ReadSingle();
			Unknown0x18 = reader.ReadSingle();
		}
	}
}
