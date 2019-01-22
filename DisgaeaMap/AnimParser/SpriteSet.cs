using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

using Cobalt.IO;

namespace DisgaeaMap.AnimParser
{
	public class SpriteSet : BaseChunk
	{
		/* Header; offsets relative to here (i.e. 0x10) */
		public ushort Unknown1Count { get; private set; }
		public ushort Unknown2Count { get; private set; }
		public ushort Unknown3Count { get; private set; }
		public ushort SpriteSheetCount { get; private set; }
		public ushort Unknown5Count { get; private set; }
		public ushort FramesCount { get; private set; }
		public ushort Unknown7Count { get; private set; }           // 0x01 for hitspr1, 0x01 for laharl -- palette data block count or smth?
		public ushort Unknown8Count { get; private set; }           // 0x01 for hitspr1, 0x08 for laharl -- same but pixel data blocks?
		public uint Unknown5Offset { get; private set; }
		public uint FramesOffset { get; private set; }
		public uint PaletteDataOffset { get; private set; }
		public uint PixelDataOffset { get; private set; }

		public Unknown1[] Unknown1s { get; private set; }
		public Unknown2[] Unknown2s { get; private set; }
		public Unknown3[] Unknown3s { get; private set; }
		public SpriteSheet[] SpriteSheets { get; private set; }

		public Unknown5[] Unknown5s { get; private set; }
		public Frame[] Frames { get; private set; }

		public Bitmap[][] SpriteSheetBitmaps { get; private set; }

		public SpriteSet(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			base.ReadFromStream(stream, endianness);

			long startPosition = stream.Position;
			//if (startPosition == 0x5FBf0) { bool tmp = false; }

			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			/* Read header */
			Unknown1Count = reader.ReadUInt16();
			Unknown2Count = reader.ReadUInt16();
			Unknown3Count = reader.ReadUInt16();
			SpriteSheetCount = reader.ReadUInt16();
			Unknown5Count = reader.ReadUInt16();
			FramesCount = reader.ReadUInt16();
			Unknown7Count = reader.ReadUInt16();
			Unknown8Count = reader.ReadUInt16();
			Unknown5Offset = reader.ReadUInt32();
			FramesOffset = reader.ReadUInt32();
			PaletteDataOffset = reader.ReadUInt32();
			PixelDataOffset = reader.ReadUInt32();

			/* Read data (1) */
			Unknown1s = new Unknown1[Unknown1Count];
			for (int i = 0; i < Unknown1s.Length; i++) Unknown1s[i] = new Unknown1(stream);

			Unknown2s = new Unknown2[Unknown2Count];
			for (int i = 0; i < Unknown2s.Length; i++) Unknown2s[i] = new Unknown2(stream);

			Unknown3s = new Unknown3[Unknown3Count];
			for (int i = 0; i < Unknown3s.Length; i++) Unknown3s[i] = new Unknown3(stream);

			SpriteSheets = new SpriteSheet[SpriteSheetCount];
			for (int i = 0; i < SpriteSheets.Length; i++) SpriteSheets[i] = new SpriteSheet(stream);

			/* Read data (2) */
			stream.Position = (startPosition + Unknown5Offset);
			Unknown5s = new Unknown5[Unknown5Count];
			for (int i = 0; i < Unknown5s.Length; i++) Unknown5s[i] = new Unknown5(stream);

			stream.Position = (startPosition + FramesOffset);
			Frames = new Frame[FramesCount];
			for (int i = 0; i < Frames.Length; i++) Frames[i] = new Frame(stream);

			/* Create bitmaps, etc. */
			SpriteSheetBitmaps = new Bitmap[SpriteSheetCount][];
			for (int i = 0; i < SpriteSheetBitmaps.Length; i++)
			{
				// TODO/HACK: get palette count via max palette number in framedata
				var palCount = Frames.Max(x => x.PaletteIndex) + 1;

				SpriteSheetBitmaps[i] = CreateSpriteSheetBitmaps(reader, startPosition, palCount, SpriteSheets[i]);
			}

			stream.Position = (startPosition + (DataSize - 0x10));
		}

		private Bitmap[] CreateSpriteSheetBitmaps(EndianBinaryReader reader, long startPosition, int paletteCount, SpriteSheet spriteSheet)
		{
			reader.BaseStream.Position = (startPosition + PaletteDataOffset);
			var paletteData = TextureHelper.GetPaletteData(reader, paletteCount, (1 << spriteSheet.Unknown0x08));

			reader.BaseStream.Position = (startPosition + spriteSheet.PixelDataOffset);
			var pixelData = TextureHelper.GetPixelData(reader, spriteSheet.Width, spriteSheet.Height, (1 << spriteSheet.Unknown0x08));

			return TextureHelper.GetBitmaps(spriteSheet.Width, spriteSheet.Height, paletteCount, (1 << spriteSheet.Unknown0x08), pixelData, paletteData); ;
		}
	}
}
