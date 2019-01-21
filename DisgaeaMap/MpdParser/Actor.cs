using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Cobalt.IO;

namespace DisgaeaMap.MpdParser
{
	public class Actor : ParsableData
	{
		public ushort ID { get; private set; }
		public ushort Level { get; private set; }
		public byte Unknown0x04 { get; private set; }
		public sbyte X { get; private set; }
		public sbyte Z { get; private set; }
		public sbyte Rotation { get; private set; }
		public byte Unknown0x08 { get; private set; }
		public byte AI { get; private set; }
		public byte Unknown0x0A { get; private set; }
		public byte Unknown0x0B { get; private set; }
		public ushort Item1 { get; private set; }
		public ushort Item2 { get; private set; }
		public ushort Item3 { get; private set; }
		public ushort Item4 { get; private set; }
		public byte Appearance { get; private set; }
		public byte Unknown0x15 { get; private set; }
		public ushort GeoEffect { get; private set; }
		public ushort Unknown0x18 { get; private set; }
		public ushort Magic1 { get; private set; }
		public ushort Magic2 { get; private set; }
		public ushort Magic3 { get; private set; }
		public ushort Magic4 { get; private set; }
		public byte[] Unknown0x22 { get; private set; } //0x1E bytes

		public Actor(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			ID = reader.ReadUInt16();
			Level = reader.ReadUInt16();
			Unknown0x04 = reader.ReadByte();
			X = reader.ReadSByte();
			Z = reader.ReadSByte();
			Rotation = reader.ReadSByte();
			Unknown0x08 = reader.ReadByte();
			AI = reader.ReadByte();
			Unknown0x0A = reader.ReadByte();
			Unknown0x0B = reader.ReadByte();
			Item1 = reader.ReadUInt16();
			Item2 = reader.ReadUInt16();
			Item3 = reader.ReadUInt16();
			Item4 = reader.ReadUInt16();
			Appearance = reader.ReadByte();
			Unknown0x15 = reader.ReadByte();
			GeoEffect = reader.ReadUInt16();
			Unknown0x18 = reader.ReadUInt16();
			Magic1 = reader.ReadUInt16();
			Magic2 = reader.ReadUInt16();
			Magic3 = reader.ReadUInt16();
			Magic4 = reader.ReadUInt16();
			Unknown0x22 = reader.ReadBytes(0x1E);
		}
	}
}
