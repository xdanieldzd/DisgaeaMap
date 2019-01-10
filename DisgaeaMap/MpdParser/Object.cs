using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Cobalt.IO;

namespace DisgaeaMap.MpdParser
{
	public class Object : ParsableData
	{
		public TransformData Transforms { get; private set; }
		public byte Unknown0x14 { get; private set; }
		public byte ObjectType { get; private set; }
		public byte Unknown0x16 { get; private set; }
		public byte Unknown0x17 { get; private set; }
		public byte FlowU { get; private set; }
		public byte FlowV { get; private set; }
		public byte Unknown0x1A { get; private set; }
		public byte Unknown0x1B { get; private set; }
		public byte Unknown0x1C { get; private set; }
		public byte Unknown0x1D { get; private set; }
		public byte Unknown0x1E { get; private set; }
		public byte Unknown0x1F { get; private set; }
		public byte Unknown0x20 { get; private set; }
		public byte Unknown0x21 { get; private set; }
		public byte Unknown0x22 { get; private set; }
		public byte Unknown0x23 { get; private set; }

		public Object(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			Transforms = new TransformData(stream);
			Unknown0x14 = reader.ReadByte();
			ObjectType = reader.ReadByte();
			Unknown0x16 = reader.ReadByte();
			Unknown0x17 = reader.ReadByte();
			FlowU = reader.ReadByte();
			FlowV = reader.ReadByte();
			Unknown0x1A = reader.ReadByte();
			Unknown0x1B = reader.ReadByte();
			Unknown0x1C = reader.ReadByte();
			Unknown0x1D = reader.ReadByte();
			Unknown0x1E = reader.ReadByte();
			Unknown0x1F = reader.ReadByte();
			Unknown0x20 = reader.ReadByte();
			Unknown0x21 = reader.ReadByte();
			Unknown0x22 = reader.ReadByte();
			Unknown0x23 = reader.ReadByte();
		}
	}
}
