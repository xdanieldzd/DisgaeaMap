using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Cobalt.IO;

namespace DisgaeaMap.AnimParser
{
	public class Frame : ParsableData
	{
		public byte Unknown0x00 { get; private set; }       // spritesheet index to external file?? (ex. weapons in chara data)
		public byte Unknown0x01 { get; private set; }
		public byte SpriteSheetIndex { get; private set; }
		public byte PaletteIndex { get; private set; }
		public short Unknown0x04 { get; private set; }      // usually negative?
		public short Unknown0x06 { get; private set; }      // usually negative?
		public ushort SourceX { get; private set; }
		public ushort SourceY { get; private set; }
		public ushort SourceWidth { get; private set; }
		public ushort SourceHeight { get; private set; }
		public ushort ScaleX { get; private set; }
		public ushort ScaleY { get; private set; }
		public short Unknown0x14 { get; private set; }
		public short Unknown0x16 { get; private set; }
		public ushort RotationAngle { get; private set; }
		public byte Unknown0x1A { get; private set; }
		public byte Unknown0x1B { get; private set; }

		public Frame(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			Unknown0x00 = reader.ReadByte();
			Unknown0x01 = reader.ReadByte();
			SpriteSheetIndex = reader.ReadByte();
			PaletteIndex = reader.ReadByte();
			Unknown0x04 = reader.ReadInt16();
			Unknown0x06 = reader.ReadInt16();
			SourceX = reader.ReadUInt16();
			SourceY = reader.ReadUInt16();
			SourceWidth = reader.ReadUInt16();
			SourceHeight = reader.ReadUInt16();
			ScaleX = reader.ReadUInt16();
			ScaleY = reader.ReadUInt16();
			Unknown0x14 = reader.ReadInt16();
			Unknown0x16 = reader.ReadInt16();
			RotationAngle = reader.ReadUInt16();
			Unknown0x1A = reader.ReadByte();
			Unknown0x1B = reader.ReadByte();
		}
	}
}
