using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Cobalt.IO;
using Cobalt.Mesh;
using Cobalt.Texture;

namespace DisgaeaMap.ModelParser
{
	public class MapBinary : ParsableData
	{
		// TODO: a lot i guess

		public uint NumFloorTextures { get; private set; }
		public uint NumMeshChunks { get; private set; }
		public uint NumUnknown0x08 { get; private set; }
		public uint NumUnknown0x0C { get; private set; }    //same as 0x04?
		public string[] FloorTextureNames { get; private set; }

		public FloorTextureChunk[] FloorTextureChunks { get; private set; }
		public MeshChunk[] MeshChunks { get; private set; }
		public byte[][] UnknownData1 { get; private set; }

		public Texture[] FloorTextures { get; private set; }
		public (Mesh, Texture, TransformData[])[][] MeshSets { get; private set; }

		public MapBinary(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			NumFloorTextures = reader.ReadUInt32();
			NumMeshChunks = reader.ReadUInt32();
			NumUnknown0x08 = reader.ReadUInt32();
			NumUnknown0x0C = reader.ReadUInt32();
			FloorTextureNames = new string[NumFloorTextures];
			for (int i = 0; i < FloorTextureNames.Length; i++) FloorTextureNames[i] = Encoding.GetEncoding("SJIS").GetString(reader.ReadBytes(16)).TrimEnd('\0');

			FloorTextureChunks = new FloorTextureChunk[NumFloorTextures];
			for (int i = 0; i < FloorTextureChunks.Length; i++)
			{
				var position = stream.Position;
				FloorTextureChunks[i] = new FloorTextureChunk(stream);
				stream.Position = position + FloorTextureChunks[i].DataSize + 0x10;
			}

			MeshChunks = new MeshChunk[NumMeshChunks];
			for (int i = 0; i < MeshChunks.Length; i++)
			{
				var position = stream.Position;
				MeshChunks[i] = new MeshChunk(stream);
				stream.Position = position + MeshChunks[i].DataSize + 0x10;
			}

			UnknownData1 = new byte[NumUnknown0x0C][];
			for (int i = 0; i < UnknownData1.Length; i++)
				UnknownData1[i] = reader.ReadBytes(10);

			var dummyTexture = new Bitmap(32, 32, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			using (Graphics g = Graphics.FromImage(dummyTexture))
				g.Clear(Color.Transparent);

			FloorTextures = new Texture[FloorTextureChunks.Length + 1];
			FloorTextures[0] = new Texture(dummyTexture);
			for (int i = 0; i < FloorTextureChunks.Length; i++)
				FloorTextures[i + 1] = new Texture(FloorTextureChunks[i].Bitmaps.FirstOrDefault());

			/*foreach (var texture in FloorTextures.Skip(1))
				texture.Save(@"D:\Temp\Disgaea\" + Array.IndexOf(FloorTextures, texture) + ".png");
				*/
			GenerateMeshes();
		}

		private void GenerateMeshes()
		{
			MeshSets = new (Mesh, Texture, TransformData[])[MeshChunks.Length][];

			for (int i = 0; i < MeshSets.Length; i++)
			{
				var chunk = MeshChunks[i];

				MeshSets[i] = new (Mesh, Texture, TransformData[])[chunk.MeshData.NumGeometry];

				var currentVertex = 0;
				for (int j = 0; j < MeshSets[i].Length; j++)
				{
					var mesh = new Mesh();
					var data = chunk.MeshData.Geometry[j];

					var vertices = new List<CommonVertex>();
					for (int k = 0; k < data.NumVertices; k += 3)
					{
						vertices.Add(new CommonVertex() { Position = chunk.MeshData.Vertices[currentVertex + 2], Normal = Vector3.Zero, Color = data.PrimitiveColor, TexCoord = chunk.MeshData.TexCoords[currentVertex + 2].Xy });
						vertices.Add(new CommonVertex() { Position = chunk.MeshData.Vertices[currentVertex + 1], Normal = Vector3.Zero, Color = data.PrimitiveColor, TexCoord = chunk.MeshData.TexCoords[currentVertex + 1].Xy });
						vertices.Add(new CommonVertex() { Position = chunk.MeshData.Vertices[currentVertex + 0], Normal = Vector3.Zero, Color = data.PrimitiveColor, TexCoord = chunk.MeshData.TexCoords[currentVertex + 0].Xy });
						currentVertex += 3;
					}

					mesh.SetVertexData(vertices.ToArray());
					mesh.SetPrimitiveType(PrimitiveType.Triangles);

					Bitmap textureImage;
					var textureIdx = (data.TextureID - 1);
					if (textureIdx >= 0)
						textureImage = chunk.MeshData.Textures[textureIdx].Bitmaps.FirstOrDefault();
					else
						textureImage = new Bitmap(32, 32);

					var texture = new Texture(textureImage, TextureWrapMode.Repeat, TextureWrapMode.Repeat, TextureMinFilter.Linear, TextureMagFilter.Linear);

					MeshSets[i][j] = (mesh, texture, chunk.UnknownTransforms);
				}
			}
		}
	}
}
