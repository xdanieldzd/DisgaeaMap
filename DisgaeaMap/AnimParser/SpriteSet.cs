using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Cobalt.IO;

namespace DisgaeaMap.AnimParser
{
	public class SpriteSet : BaseChunk
	{
		/* Header; offsets relative to here (i.e. 0x10) */
		public ushort FrameIDMappingsCount { get; private set; }       // zero for hitspr1, 0x6D for laharl
		public ushort Unknown2Count { get; private set; }       // 0x06 for hitspr1, 0x17C for laharl
		public ushort Unknown3Count { get; private set; }       // 0x01 for hitspr1, 0x01 for laharl
		public ushort SpriteSheetCount { get; private set; }       // 0x01 for hitspr1, 0x08 for laharl

		public ushort Unknown5Count0x18 { get; private set; }       // zero for hitspr1, 0x349 for laharl
		public ushort Unknown6Count0x1A { get; private set; }       // 0x06 for hitspr1, 0x40A for laharl
		public ushort Unknown7Count0x1C { get; private set; }       // 0x01 for hitspr1, 0x01 for laharl -- palette data block count or smth?
		public ushort Unknown8Count0x1E { get; private set; }       // 0x01 for hitspr1, 0x08 for laharl -- same but pixel data blocks?

		public uint Unknown5Offset { get; private set; }        // ???? -- 0x10 bytes/entry
		public uint Unknown6Offset { get; private set; }        // sprites, animframes or whatever -- 0x1C bytes/entry
		public uint PaletteDataOffset { get; private set; }
		public uint PixelDataOffset { get; private set; }

		public FrameIDMapping[] FrameIDMappings { get; private set; }
		public Unknown2[] Unknown2s { get; private set; }
		public Unknown3[] Unknown3s { get; private set; }
		public SpriteSheet[] SpriteSheets { get; private set; }

		//

		public SpriteSet(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			base.ReadFromStream(stream, endianness);

			long startPosition = stream.Position;

			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			FrameIDMappingsCount = reader.ReadUInt16();
			Unknown2Count = reader.ReadUInt16();
			Unknown3Count = reader.ReadUInt16();
			SpriteSheetCount = reader.ReadUInt16();
			Unknown5Count0x18 = reader.ReadUInt16();
			Unknown6Count0x1A = reader.ReadUInt16();
			Unknown7Count0x1C = reader.ReadUInt16();
			Unknown8Count0x1E = reader.ReadUInt16();
			Unknown5Offset = reader.ReadUInt32();
			Unknown6Offset = reader.ReadUInt32();
			PaletteDataOffset = reader.ReadUInt32();
			PixelDataOffset = reader.ReadUInt32();

			FrameIDMappings = new FrameIDMapping[FrameIDMappingsCount];
			for (int i = 0; i < FrameIDMappings.Length; i++) FrameIDMappings[i] = new FrameIDMapping(stream);

			Unknown2s = new Unknown2[Unknown2Count];
			for (int i = 0; i < Unknown2s.Length; i++) Unknown2s[i] = new Unknown2(stream);

			Unknown3s = new Unknown3[Unknown3Count];
			for (int i = 0; i < Unknown3s.Length; i++) Unknown3s[i] = new Unknown3(stream);

			SpriteSheets = new SpriteSheet[SpriteSheetCount];
			for (int i = 0; i < SpriteSheets.Length; i++) SpriteSheets[i] = new SpriteSheet(stream);

			stream.Position = (startPosition + Unknown5Offset);
			// read unknown5

			stream.Position = (startPosition + Unknown6Offset);
			// read unknown6


			// read palettes, pixeldata

			// TODO actually parse this and stuff, for now just skip to next spriteset
			stream.Position = (startPosition + (DataSize - 0x10));
		}
	}
}
