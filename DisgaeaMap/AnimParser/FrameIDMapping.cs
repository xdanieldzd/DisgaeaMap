using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Cobalt.IO;

namespace DisgaeaMap.AnimParser
{
	public class FrameIDMapping : ParsableData
	{
		public ushort StartFrame { get; private set; }
		public ushort SpriteID { get; private set; }

		public FrameIDMapping(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			StartFrame = reader.ReadUInt16();
			SpriteID = reader.ReadUInt16();
		}
	}
}
