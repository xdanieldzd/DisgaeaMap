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
	public class SpriteSet : ParsableData
	{
		public ushort FirstSpriteIndex { get; private set; }
		public ushort NumSprites { get; private set; }

		public SpriteSet(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			FirstSpriteIndex = reader.ReadUInt16();
			NumSprites = reader.ReadUInt16();
		}

		private string DebuggerDisplay { get { return $"First Sprite Idx = {FirstSpriteIndex}, NumSprites = {NumSprites}"; } }
	}
}
