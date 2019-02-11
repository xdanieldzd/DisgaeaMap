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
	public class MeshData : ParsableData
	{
		public uint NumTriangles { get; private set; }
		public uint Unknown0x04 { get; private set; }       //num texcoords...?
		public uint NumGeometry { get; private set; }
		public uint NumTextures { get; private set; }
		public uint TextureOffset { get; private set; }
		public uint DataSize { get; private set; }
		public uint VertexDataSize { get; private set; }
		public uint Unknown0x1C { get; private set; }

		public Geometry[] Geometry { get; private set; }

		public Vector4[] Vertices { get; private set; }
		public Vector4[] TexCoords { get; private set; }

		public ObjectTextureData[] Textures { get; private set; }

		public MeshData(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			long startPosition = stream.Position;

			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			NumTriangles = reader.ReadUInt32();
			Unknown0x04 = reader.ReadUInt32();
			NumGeometry = reader.ReadUInt32();
			NumTextures = reader.ReadUInt32();
			TextureOffset = reader.ReadUInt32();
			DataSize = reader.ReadUInt32();
			VertexDataSize = reader.ReadUInt32();
			Unknown0x1C = reader.ReadUInt32();

			Geometry = new Geometry[NumGeometry];
			for (int i = 0; i < Geometry.Length; i++) Geometry[i] = new Geometry(stream);

			Vertices = new Vector4[NumTriangles * 3];
			for (int i = 0; i < Vertices.Length; i++) Vertices[i] = new Vector4(reader.ReadSingle(), -reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()).Zyxw;

			TexCoords = new Vector4[NumTriangles * 3];
			for (int i = 0; i < TexCoords.Length; i++) TexCoords[i] = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

			stream.Position = startPosition + TextureOffset;
			Textures = new ObjectTextureData[NumTextures];
			for (int i = 0; i < Textures.Length; i++)
			{
				long position = stream.Position;
				Textures[i] = new ObjectTextureData(stream);
				stream.Position = position + Textures[i].DataSize;
			}
		}
	}
}
