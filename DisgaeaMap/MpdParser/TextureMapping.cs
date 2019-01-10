using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Cobalt.IO;

namespace DisgaeaMap.MpdParser
{
	public enum BlendMode : byte { Normal = 0, Translucent = 1, Addition = 2, Subtraction = 3 }

	public class TextureMapping : ParsableData
	{
		public byte X { get; private set; }
		public byte Y { get; private set; }
		public byte YDuplicate { get; private set; }
		public byte Width { get; private set; }
		public byte Height { get; private set; }
		public byte HeightDuplicate { get; private set; }
		public byte TextureID { get; private set; }
		public byte Flags { get; private set; }

		public TextureMapping(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			X = reader.ReadByte();
			Y = reader.ReadByte();
			YDuplicate = reader.ReadByte();
			Width = reader.ReadByte();
			Height = reader.ReadByte();
			HeightDuplicate = reader.ReadByte();
			TextureID = reader.ReadByte();
			Flags = reader.ReadByte();
		}

		public BlendMode GetBlendMode()
		{
			return (BlendMode)((Flags & 0x30) >> 4);
		}

		public int GetRotation()
		{
			return (Flags & 0x03);
		}

		public bool GetMirrorX()
		{
			return ((Flags & 0x04) == 0x04);
		}

		public bool GetMirrorY()
		{
			return ((Flags & 0x08) == 0x08);
		}
	}
}
