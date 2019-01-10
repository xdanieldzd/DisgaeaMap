using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

using Cobalt.IO;

namespace DisgaeaMap.ModelParser
{
	// https://github.com/xdanieldzd/Scarlet/blob/a125b74241130f2ac47929af66139ef53fa64b7d/Scarlet/Platform/Sony/PS2.cs

	public class FloorTextureChunk : BaseChunk
	{
		enum PS2PixelFormat
		{
			PSMCT32 = 0x00,
			PSMCT24 = 0x01,
			PSMCT16 = 0x02,
			PSMCT16S = 0x0A,
			PSMT8 = 0x13,
			PSMT4 = 0x14,
			PSMT8H = 0x1B,
			PSMT4HL = 0x24,
			PSMT4HH = 0x2C
		}

		public ushort ImageWidth { get; private set; }
		public ushort ImageHeight { get; private set; }
		public ushort ColorCount { get; private set; }
		public byte Unknown0x06 { get; private set; }
		public byte Unknown0x07 { get; private set; }
		public ushort PaletteWidth { get; private set; }
		public ushort PaletteHeight { get; private set; }
		public ushort Unknown0x0C { get; private set; }
		public ushort Unknown0x0E { get; private set; }

		public byte[][] PaletteData { get; private set; }
		public byte[] PixelData { get; private set; }

		public Bitmap[] Bitmaps { get; private set; }

		public FloorTextureChunk(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			base.ReadFromStream(stream, endianness);

			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			ImageWidth = reader.ReadUInt16();
			ImageHeight = reader.ReadUInt16();
			ColorCount = reader.ReadUInt16();
			Unknown0x06 = reader.ReadByte();
			Unknown0x07 = reader.ReadByte();
			PaletteWidth = reader.ReadUInt16();
			PaletteHeight = reader.ReadUInt16();
			Unknown0x0C = reader.ReadUInt16();
			Unknown0x0E = reader.ReadUInt16();

			if (ColorCount != 16 && ColorCount != 256) throw new Exception($"Unsupported color count {ColorCount}");

			PaletteData = TextureHelper.GetPaletteData(reader, PaletteHeight, ColorCount);
			PixelData = TextureHelper.GetPixelData(reader, ImageWidth, ImageHeight, ColorCount);

			Bitmaps = TextureHelper.GetBitmaps(ImageWidth, ImageHeight, PaletteHeight, ColorCount, PixelData, PaletteData);
		}
	}
}
