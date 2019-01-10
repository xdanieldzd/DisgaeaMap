using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Cobalt.IO;

namespace DisgaeaMap.ModelParser
{
	public class GeometryUnknownData3 : ParsableData
	{
		public byte[] Unknown0x00 { get; private set; }
		public ushort MaybeSomeID { get; private set; }            // maybe, i dunno yet
		public ushort NumVertices { get; private set; }             // something like that i guess
		public uint Unknown0x08 { get; private set; }
		public uint Unknown0x0C { get; private set; }

		public GeometryUnknownData3(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			Unknown0x00 = reader.ReadBytes(4);
			MaybeSomeID = reader.ReadUInt16();
			NumVertices = reader.ReadUInt16();
			Unknown0x08 = reader.ReadUInt32();
			Unknown0x0C = reader.ReadUInt32();
		}
	}
}
