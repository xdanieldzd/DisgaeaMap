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
	public class ObjectTextureData : ParsableData
	{
		public uint DataSize { get; private set; }
		public ushort Width { get; private set; }
		public ushort Height { get; private set; }
		public ushort ColorCount { get; private set; }
		public byte Unknown0x0A { get; private set; }
		public byte Unknown0x0B { get; private set; }
		public uint Unknown0x0C { get; private set; }

		public byte[][] PaletteData { get; private set; }
		public byte[] PixelData { get; private set; }

		public Bitmap[] Bitmaps { get; private set; }

		public ObjectTextureData(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			DataSize = reader.ReadUInt32();
			Width = reader.ReadUInt16();
			Height = reader.ReadUInt16();
			ColorCount = reader.ReadUInt16();
			Unknown0x0A = reader.ReadByte();
			Unknown0x0B = reader.ReadByte();
			Unknown0x0C = reader.ReadUInt32();

			if (ColorCount != 16 && ColorCount != 256) throw new Exception($"Unsupported color count {ColorCount}");

			PaletteData = TextureHelper.GetPaletteData(reader, 1, ColorCount);
			PixelData = TextureHelper.GetPixelData(reader, Width, Height, ColorCount);

			Bitmaps = TextureHelper.GetBitmaps(Width, Height, 1, ColorCount, PixelData, PaletteData);
		}
	}
}
