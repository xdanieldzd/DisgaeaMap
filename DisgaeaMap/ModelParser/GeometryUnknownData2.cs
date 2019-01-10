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

namespace DisgaeaMap.ModelParser
{
	public class GeometryUnknownData2 : ParsableData
	{
		public uint NumTriangles { get; private set; }
		public uint Unknown0x04 { get; private set; }       //num texcoords...?
		public uint NumUnknownData3 { get; private set; }
		public uint NumUnknown0x0C { get; private set; }
		public uint TextureOffset { get; private set; }
		public uint DataSize { get; private set; }
		public uint VertexDataSize { get; private set; }
		public uint Unknown0x1C { get; private set; }

		public GeometryUnknownData3[] UnknownData3 { get; private set; }

		public Vector4[] Vertices { get; private set; }
		public Vector4[] VertexDataXXX { get; private set; }

		public ObjectTextureData[] Textures { get; private set; }

		public GeometryUnknownData2(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			long startPosition = stream.Position;

			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			NumTriangles = reader.ReadUInt32();
			Unknown0x04 = reader.ReadUInt32();
			NumUnknownData3 = reader.ReadUInt32();
			NumUnknown0x0C = reader.ReadUInt32();
			TextureOffset = reader.ReadUInt32();
			DataSize = reader.ReadUInt32();
			VertexDataSize = reader.ReadUInt32();
			Unknown0x1C = reader.ReadUInt32();

			UnknownData3 = new GeometryUnknownData3[NumUnknownData3];
			for (int i = 0; i < UnknownData3.Length; i++) UnknownData3[i] = new GeometryUnknownData3(stream);

			Vertices = new Vector4[NumTriangles * 3];
			for (int i = 0; i < Vertices.Length; i++) Vertices[i] = new Vector4(reader.ReadSingle(), -reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()).Zyxw;

			VertexDataXXX = new Vector4[NumTriangles * 3];
			for (int i = 0; i < VertexDataXXX.Length; i++) VertexDataXXX[i] = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

			stream.Position = startPosition + TextureOffset;
			Textures = new ObjectTextureData[NumUnknown0x0C];
			for (int i = 0; i < Textures.Length; i++)
			{
				long position = stream.Position;
				Textures[i] = new ObjectTextureData(stream);
				stream.Position = position + Textures[i].DataSize;
			}
		}
	}
}
