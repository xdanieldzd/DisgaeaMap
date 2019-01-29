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
	public class AnimSet : BaseChunk
	{
		/* Header; offsets relative to here (i.e. 0x10) */
		public ushort AnimIdentifierCount { get; private set; }
		public ushort SpriteSetCount { get; private set; }
		public ushort Unknown3Count { get; private set; }
		public ushort SpriteSheetCount { get; private set; }
		public ushort FrameCount { get; private set; }
		public ushort SpriteCount { get; private set; }
		public ushort Unknown7Count { get; private set; }           // 0x01 for hitspr1, 0x01 for laharl -- palette data block count or smth?
		public ushort Unknown8Count { get; private set; }           // 0x01 for hitspr1, 0x08 for laharl -- same but pixel data blocks?
		public uint FramesOffset { get; private set; }
		public uint SpritesOffset { get; private set; }
		public uint PaletteDataOffset { get; private set; }
		public uint PixelDataOffset { get; private set; }

		public AnimIdentifier[] AnimIdentifiers { get; private set; }
		public SpriteSet[] SpriteSets { get; private set; }
		public Unknown3[] Unknown3s { get; private set; }
		public SpriteSheet[] SpriteSheets { get; private set; }

		public Frame[] Frames { get; private set; }
		public Sprite[] Sprites { get; private set; }

		byte[] rawPaletteData, rawPixelData;
		Dictionary<SpriteSheet, Bitmap[]> spriteSheetBitmaps;

		public AnimSet(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			base.ReadFromStream(stream, endianness);

			long startPosition = stream.Position;
			if (startPosition == 0x381630) { bool tmp = false; }

			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			/* Read header */
			AnimIdentifierCount = reader.ReadUInt16();
			SpriteSetCount = reader.ReadUInt16();
			Unknown3Count = reader.ReadUInt16();
			SpriteSheetCount = reader.ReadUInt16();
			FrameCount = reader.ReadUInt16();
			SpriteCount = reader.ReadUInt16();
			Unknown7Count = reader.ReadUInt16();
			Unknown8Count = reader.ReadUInt16();
			FramesOffset = reader.ReadUInt32();
			SpritesOffset = reader.ReadUInt32();
			PaletteDataOffset = reader.ReadUInt32();
			PixelDataOffset = reader.ReadUInt32();

			/* Read data (1) */
			AnimIdentifiers = new AnimIdentifier[AnimIdentifierCount];
			for (int i = 0; i < AnimIdentifiers.Length; i++) AnimIdentifiers[i] = new AnimIdentifier(stream);

			SpriteSets = new SpriteSet[SpriteSetCount];
			for (int i = 0; i < SpriteSets.Length; i++) SpriteSets[i] = new SpriteSet(stream);

			Unknown3s = new Unknown3[Unknown3Count];
			for (int i = 0; i < Unknown3s.Length; i++) Unknown3s[i] = new Unknown3(stream);

			SpriteSheets = new SpriteSheet[SpriteSheetCount];
			for (int i = 0; i < SpriteSheets.Length; i++) SpriteSheets[i] = new SpriteSheet(stream);

			/* Read data (2) */
			stream.Position = (startPosition + FramesOffset);
			Frames = new Frame[FrameCount];
			for (int i = 0; i < Frames.Length; i++) Frames[i] = new Frame(stream);

			stream.Position = (startPosition + SpritesOffset);
			Sprites = new Sprite[SpriteCount];
			for (int i = 0; i < Sprites.Length; i++) Sprites[i] = new Sprite(stream);

			stream.Position = (startPosition + PaletteDataOffset);
			rawPaletteData = reader.ReadBytes((int)(PixelDataOffset - PaletteDataOffset));

			stream.Position = (startPosition + PixelDataOffset);
			rawPixelData = reader.ReadBytes((int)(DataSize - 0x10 - PixelDataOffset));

			spriteSheetBitmaps = new Dictionary<SpriteSheet, Bitmap[]>();

			stream.Position = (startPosition + (DataSize - 0x10));
		}

		public Bitmap[] GetSpriteSheetBitmaps(int sheet)
		{
			var spriteSheet = SpriteSheets[sheet];
			if (!spriteSheetBitmaps.ContainsKey(spriteSheet))
			{
				byte[][] paletteData;
				byte[] pixelData;

				// TODO/HACK: get palette count via max palette number in sprites
				var paletteCount = Sprites.Max(x => x.PaletteIndex) + 1;

				using (var reader = new EndianBinaryReader(new MemoryStream(rawPaletteData)))
				{
					paletteData = TextureHelper.GetPaletteData(reader, paletteCount, (1 << spriteSheet.Unknown0x08));
				}
				using (var reader = new EndianBinaryReader(new MemoryStream(rawPixelData)))
				{
					reader.BaseStream.Position = (spriteSheet.PixelDataOffset - PixelDataOffset);
					pixelData = TextureHelper.GetPixelData(reader, spriteSheet.Width, spriteSheet.Height, (1 << spriteSheet.Unknown0x08));
				}
				spriteSheetBitmaps[spriteSheet] = TextureHelper.GetBitmaps(spriteSheet.Width, spriteSheet.Height, paletteCount, (1 << spriteSheet.Unknown0x08), pixelData, paletteData);
			}

			return spriteSheetBitmaps[spriteSheet];
		}
	}
}
