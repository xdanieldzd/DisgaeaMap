using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Cobalt.IO;

namespace DisgaeaMap.AnimParser
{
	public class Unknown1 : ParsableData
	{
		public ushort IndexIntoUnknown5 { get; private set; }
		public ushort Unknown0x02 { get; private set; }

		public Unknown1(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			IndexIntoUnknown5 = reader.ReadUInt16();
			Unknown0x02 = reader.ReadUInt16();
		}
	}
}
