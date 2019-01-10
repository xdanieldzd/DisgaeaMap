using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Cobalt.IO;

namespace DisgaeaMap.MpdParser
{
	public class EventTile : ParsableData
	{
		public byte X { get; private set; }
		public byte Z { get; private set; }
		public byte Index { get; private set; }
		public byte Padding { get; private set; }

		public EventTile(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			X = reader.ReadByte();
			Z = reader.ReadByte();
			Index = reader.ReadByte();
			Padding = reader.ReadByte();
		}
	}
}
