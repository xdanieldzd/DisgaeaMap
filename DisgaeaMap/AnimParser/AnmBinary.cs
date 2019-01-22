using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Cobalt.IO;

namespace DisgaeaMap.AnimParser
{
	public class AnmBinary : ParsableData
	{
		public uint NumSpriteSets { get; private set; }
		public ushort[] SpriteSetIDs { get; private set; }

		public SpriteSet[] SpriteSets { get; private set; }

		public AnmBinary(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			NumSpriteSets = reader.ReadUInt32();
			SpriteSetIDs = new ushort[NumSpriteSets];
			for (int i = 0; i < SpriteSetIDs.Length; i++) SpriteSetIDs[i] = reader.ReadUInt16();

			stream.Position += ((reader.BaseStream.Position % 16) != 0 ? (16 - (reader.BaseStream.Position % 16)) : 16);

			SpriteSets = new SpriteSet[NumSpriteSets];
			for (int i = 0; i < SpriteSets.Length; i++) SpriteSets[i] = new SpriteSet(stream);
		}
	}
}
