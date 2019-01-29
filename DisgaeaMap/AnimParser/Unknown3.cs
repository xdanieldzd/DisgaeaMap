using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Cobalt.IO;

namespace DisgaeaMap.AnimParser
{
	[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class Unknown3 : ParsableData
	{
		public ushort Unknown0x00 { get; private set; }
		public ushort Unknown0x02 { get; private set; }

		public Unknown3(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			Unknown0x00 = reader.ReadUInt16();
			Unknown0x02 = reader.ReadUInt16();
		}

		private string DebuggerDisplay { get { return $"Unknown0 = {Unknown0x00}, Unknown2 = {Unknown0x02}"; } }
	}
}
