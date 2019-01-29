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
	public class AnimIdentifier : ParsableData
	{
		public ushort FirstFrameIndex { get; private set; }
		public ushort AnimationID { get; private set; }

		public AnimIdentifier(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			FirstFrameIndex = reader.ReadUInt16();
			AnimationID = reader.ReadUInt16();
		}

		private string DebuggerDisplay { get { return $"Index = {FirstFrameIndex}, Unknown2 = {AnimationID}"; } }
	}
}
