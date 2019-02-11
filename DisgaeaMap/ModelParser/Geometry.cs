using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using OpenTK.Graphics;

using Cobalt.IO;

namespace DisgaeaMap.ModelParser
{
	public class Geometry : ParsableData
	{
		public Color4 PrimitiveColor { get; private set; }
		public ushort TextureID { get; private set; }
		public ushort NumVertices { get; private set; }
		public uint Unknown0x08 { get; private set; }
		public uint Unknown0x0C { get; private set; }

		public Geometry(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			PrimitiveColor = new Color4(TextureHelper.ScaleComponent(reader.ReadByte()), TextureHelper.ScaleComponent(reader.ReadByte()), TextureHelper.ScaleComponent(reader.ReadByte()), TextureHelper.ScaleComponent(reader.ReadByte()));
			TextureID = reader.ReadUInt16();
			NumVertices = reader.ReadUInt16();
			Unknown0x08 = reader.ReadUInt32();
			Unknown0x0C = reader.ReadUInt32();
		}
	}
}
