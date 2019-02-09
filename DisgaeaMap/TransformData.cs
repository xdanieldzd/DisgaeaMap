using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Cobalt.IO;

namespace DisgaeaMap
{
	public class TransformData : ParsableData
	{
		public short XPosition { get; private set; }
		public short YPosition { get; private set; }
		public short ZPosition { get; private set; }
		public short XRotation { get; private set; }
		public short YRotation { get; private set; }
		public short ZRotation { get; private set; }
		public short XScale { get; private set; }
		public short YScale { get; private set; }
		public short ZScale { get; private set; }
		public short Unknown0x12 { get; private set; }

		public TransformData(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			XPosition = reader.ReadInt16();
			YPosition = reader.ReadInt16();
			ZPosition = reader.ReadInt16();
			XRotation = reader.ReadInt16();
			YRotation = reader.ReadInt16();
			ZRotation = reader.ReadInt16();
			XScale = reader.ReadInt16();
			YScale = reader.ReadInt16();
			ZScale = reader.ReadInt16();
			Unknown0x12 = reader.ReadInt16();
		}
	}
}
