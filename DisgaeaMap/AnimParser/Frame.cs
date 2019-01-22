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
		public short Unknown0x00 { get; private set; }  // low number, maybe unsigned? some id?
		public short Unknown0x02 { get; private set; }  // low number, maybe unsigned? some id?
		public short Unknown0x04 { get; private set; }  // usually negative?
		public short Unknown0x06 { get; private set; }  // usually negative?
		public ushort SourceX { get; private set; }
		public ushort SourceY { get; private set; }
		public ushort SourceWidth { get; private set; }
		public ushort SourceHeight { get; private set; }
		public ushort ScaleX { get; private set; }
		public ushort ScaleY { get; private set; }
		public short Unknown0x14 { get; private set; }
		public short Unknown0x16 { get; private set; }
		public ushort Unknown0x18 { get; private set; }
		public byte Unknown0x1A { get; private set; }
		public byte Unknown0x1B { get; private set; }

		public Frame(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			//
		}
	}
}
