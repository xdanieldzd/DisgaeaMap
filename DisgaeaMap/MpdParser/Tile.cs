using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Cobalt.IO;
using Cobalt.Mesh;
using Cobalt.Texture;

namespace DisgaeaMap.MpdParser
{
	public class Tile : ParsableData
	{
		public TextureMapping SouthMain { get; private set; }
		public TextureMapping EastMain { get; private set; }
		public TextureMapping NorthMain { get; private set; }
		public TextureMapping WestMain { get; private set; }
		public TextureMapping TopMain { get; private set; }
		public TextureMapping SouthShadow { get; private set; }
		public TextureMapping EastShadow { get; private set; }
		public TextureMapping NorthShadow { get; private set; }
		public TextureMapping WestShadow { get; private set; }
		public TextureMapping TopShadow1 { get; private set; }
		public TextureMapping TopShadow2 { get; private set; }
		public TextureMapping Unknown0x58 { get; private set; }     // unused?

		public sbyte TopVertexSW { get; private set; }
		public sbyte TopVertexSE { get; private set; }
		public sbyte TopVertexNW { get; private set; }
		public sbyte TopVertexNE { get; private set; }

		public sbyte SouthVertexSW { get; private set; }
		public sbyte SouthVertexSE { get; private set; }

		public sbyte EastVertexSE { get; private set; }
		public sbyte EastVertexNE { get; private set; }

		public sbyte NorthVertexNE { get; private set; }
		public sbyte NorthVertexNW { get; private set; }

		public sbyte WestVertexNW { get; private set; }
		public sbyte WestVertexSW { get; private set; }

		public byte Unknown0x6C { get; private set; }
		public byte Unknown0x6D { get; private set; }
		public byte Unknown0x6E { get; private set; }
		public byte Unknown0x6F { get; private set; }

		public byte Unknown0x70 { get; private set; }
		public byte ZCoordinate { get; private set; }
		public byte XCoordinate { get; private set; }
		public byte Unknown0x73 { get; private set; }
		public byte Unknown0x74 { get; private set; }
		public byte Unknown0x75 { get; private set; }
		public byte Unknown0x76 { get; private set; }
		public byte Mobility { get; private set; }
		public byte GeoPanelColor { get; private set; }
		public byte GeoPanelFlag { get; private set; }
		public byte Unknown0x7A { get; private set; }
		public byte Unknown0x7B { get; private set; }
		public byte Unknown0x7C { get; private set; }
		public byte Unknown0x7D { get; private set; }
		public byte Unknown0x7E { get; private set; }
		public byte Unknown0x7F { get; private set; }

		public Tile(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			SouthMain = new TextureMapping(stream);
			EastMain = new TextureMapping(stream);
			NorthMain = new TextureMapping(stream);
			WestMain = new TextureMapping(stream);
			TopMain = new TextureMapping(stream);
			SouthShadow = new TextureMapping(stream);
			EastShadow = new TextureMapping(stream);
			NorthShadow = new TextureMapping(stream);
			WestShadow = new TextureMapping(stream);
			TopShadow1 = new TextureMapping(stream);
			TopShadow2 = new TextureMapping(stream);
			Unknown0x58 = new TextureMapping(stream);

			TopVertexSW = reader.ReadSByte();
			TopVertexSE = reader.ReadSByte();
			TopVertexNW = reader.ReadSByte();
			TopVertexNE = reader.ReadSByte();

			SouthVertexSW = reader.ReadSByte();
			SouthVertexSE = reader.ReadSByte();

			EastVertexSE = reader.ReadSByte();
			EastVertexNE = reader.ReadSByte();

			NorthVertexNE = reader.ReadSByte();
			NorthVertexNW = reader.ReadSByte();

			WestVertexNW = reader.ReadSByte();
			WestVertexSW = reader.ReadSByte();

			Unknown0x6C = reader.ReadByte();
			Unknown0x6D = reader.ReadByte();
			Unknown0x6E = reader.ReadByte();
			Unknown0x6F = reader.ReadByte();

			Unknown0x70 = reader.ReadByte();
			ZCoordinate = reader.ReadByte();
			XCoordinate = reader.ReadByte();
			Unknown0x73 = reader.ReadByte();
			Unknown0x74 = reader.ReadByte();
			Unknown0x75 = reader.ReadByte();
			Unknown0x76 = reader.ReadByte();
			Mobility = reader.ReadByte();
			GeoPanelColor = reader.ReadByte();
			GeoPanelFlag = reader.ReadByte();
			Unknown0x7A = reader.ReadByte();
			Unknown0x7B = reader.ReadByte();
			Unknown0x7C = reader.ReadByte();
			Unknown0x7D = reader.ReadByte();
			Unknown0x7E = reader.ReadByte();
			Unknown0x7F = reader.ReadByte();
		}
	}
}
