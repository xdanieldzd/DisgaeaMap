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
	public class Frame : ParsableData
	{
		public ushort SpriteSetIndex { get; private set; }
		public byte MaybeDuration { get; private set; }
		public byte MaybeFlags { get; private set; }            // 01 == first frame, 00 == in-between frame, 02 == last frame? something like that
		public short Unknown0x04 { get; private set; }
		public short Unknown0x06 { get; private set; }

		public Frame(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			SpriteSetIndex = reader.ReadUInt16();
			MaybeDuration = reader.ReadByte();
			MaybeFlags = reader.ReadByte();
			Unknown0x04 = reader.ReadInt16();
			Unknown0x06 = reader.ReadInt16();
		}

		private string DebuggerDisplay { get { return $"Sprite Set Idx = {SpriteSetIndex}, Maybe Duration = {MaybeDuration}, Maybe Flags = {MaybeFlags}, Unknown4 = {Unknown0x04}, Unknown6 = {Unknown0x06}"; } }
	}
}
