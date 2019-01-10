using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Cobalt.IO;

namespace DisgaeaMap.MpdParser
{
	public class Chunk : ParsableData
	{
		public float MapOffsetX { get; private set; }
		public float Unknown0x04 { get; private set; }
		public float MapOffsetZ { get; private set; }
		public float Unknown0x0C { get; private set; }
		public float[] Unknown0x10 { get; private set; }
		public ushort NumTiles { get; private set; }
		public byte Unknown0x2E { get; private set; }
		public byte Unknown0x2F { get; private set; }
		public ushort ID { get; private set; }
		public byte[] Unknown0x32 { get; private set; }
		public Object[] Objects { get; private set; }
		public EventTile[] EventTiles { get; private set; }
		public Tile SpecialTile { get; private set; }

		public Chunk(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			MapOffsetX = reader.ReadSingle();
			Unknown0x04 = reader.ReadSingle();
			MapOffsetZ = reader.ReadSingle();
			Unknown0x0C = reader.ReadSingle();
			Unknown0x10 = new float[7];
			for (int i = 0; i < Unknown0x10.Length; i++) Unknown0x10[i] = reader.ReadSingle();
			NumTiles = reader.ReadUInt16();
			Unknown0x2E = reader.ReadByte();
			Unknown0x2F = reader.ReadByte();
			ID = reader.ReadUInt16();
			Unknown0x32 = new byte[14];
			for (int i = 0; i < Unknown0x32.Length; i++) Unknown0x32[i] = reader.ReadByte();

			Objects = new Object[32];
			for (int i = 0; i < Objects.Length; i++) Objects[i] = new Object(stream);

			EventTiles = new EventTile[16];
			for (int i = 0; i < EventTiles.Length; i++) EventTiles[i] = new EventTile(stream);

			SpecialTile = new Tile(stream);
		}
	}
}
