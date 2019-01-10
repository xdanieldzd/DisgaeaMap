using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Cobalt.IO;
using Cobalt.Mesh;
using Cobalt.Texture;

namespace DisgaeaMap.MpdParser
{
	// welp, this exists: https://disgaea.rustedlogic.net/MPD_format
	// also this: https://github.com/ProgSys/pg_disatools/wiki/Map-file

	public class MpdBinary : ParsableData
	{
		public enum TileFace { West, North, East, South, Top }
		public enum QuadVertex { SW, SE, NE, NW }

		public ushort NumChunks { get; private set; }
		public ushort NumActors { get; private set; }
		public ushort Unknown0x04 { get; private set; }
		public byte[] Padding { get; private set; }

		public Chunk[] Chunks { get; private set; }

		public Dictionary<Chunk, Tile[]> Tiles { get; private set; }
		public List<List<(Mesh, Texture[], BlendMode[])>> Meshes { get; private set; }

		public MpdBinary(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			NumChunks = reader.ReadUInt16();
			NumActors = reader.ReadUInt16();
			Unknown0x04 = reader.ReadUInt16();
			Padding = reader.ReadBytes(10);

			Chunks = new Chunk[NumChunks];
			for (int i = 0; i < Chunks.Length; i++) Chunks[i] = new Chunk(stream);

			Tiles = new Dictionary<Chunk, Tile[]>();
			foreach (var chunk in Chunks)
			{
				var tiles = new Tile[chunk.NumTiles];
				for (int i = 0; i < chunk.NumTiles; i++) tiles[i] = new Tile(stream);
				Tiles.Add(chunk, tiles);
			}
		}

		private MpdVertex[] GenerateTileVertices(Chunk chunk, Tile tile, TileFace tileFace)
		{
			var x = (((chunk.MapOffsetX - 6) / 12) + tile.XCoordinate) * 12.0f;
			var z = (((chunk.MapOffsetZ - 6) / 12) + tile.ZCoordinate) * 12.0f;
			var color = new Color4(255, 255, 255, 0);

			// DIS SW SE NE NW
			// OGL SW SE NW NE
			// GL# 0  1  3  2

			// GL      DIS
			//  2---3   NW---NE
			//  |   |    |   |
			//  |   |    |   |
			//  0---1   SW---SE

			MpdVertex[] vertices;
			switch (tileFace)
			{
				case TileFace.Top:
					/*if (tile.GeoPanelFlag == 100)
					{
						switch (tile.GeoPanelColor)
						{
							case 0: color = Color4.PaleVioletRed; break;
							case 1: color = Color4.LightGreen; break;
							case 2: color = Color4.CornflowerBlue; break;
						}
					}*/
					vertices = new MpdVertex[]
					{
						new MpdVertex() { Position = new Vector4(x - 6.0f, -tile.TopVertexSW, z - 6.0f, 1.0f), TexCoord0 = GetTextureCoordinates(tile.TopMain, tileFace, QuadVertex.SW), TexCoord1 = GetTextureCoordinates(tile.TopShadow1, tileFace, QuadVertex.SW), Color = color },
						new MpdVertex() { Position = new Vector4(x - 6.0f, -tile.TopVertexSE, z + 6.0f, 1.0f), TexCoord0 = GetTextureCoordinates(tile.TopMain, tileFace, QuadVertex.SE), TexCoord1 = GetTextureCoordinates(tile.TopShadow1, tileFace, QuadVertex.SE), Color = color },
						new MpdVertex() { Position = new Vector4(x + 6.0f, -tile.TopVertexNE, z + 6.0f, 1.0f), TexCoord0 = GetTextureCoordinates(tile.TopMain, tileFace, QuadVertex.NE), TexCoord1 = GetTextureCoordinates(tile.TopShadow1, tileFace, QuadVertex.NE), Color = color },
						new MpdVertex() { Position = new Vector4(x + 6.0f, -tile.TopVertexNW, z - 6.0f, 1.0f), TexCoord0 = GetTextureCoordinates(tile.TopMain, tileFace, QuadVertex.NW), TexCoord1 = GetTextureCoordinates(tile.TopShadow1, tileFace, QuadVertex.NW), Color = color },
					};
					break;

				case TileFace.South:
					vertices = new MpdVertex[]
					{
						new MpdVertex() { Position = new Vector4(x - 6.0f, -tile.SouthVertexSW, z - 6.0f, 1.0f), TexCoord0 = GetTextureCoordinates(tile.SouthMain, tileFace, QuadVertex.SW), TexCoord1 = GetTextureCoordinates(tile.SouthShadow, tileFace, QuadVertex.SW), Color = color },
						new MpdVertex() { Position = new Vector4(x - 6.0f, -tile.SouthVertexSE, z + 6.0f, 1.0f), TexCoord0 = GetTextureCoordinates(tile.SouthMain, tileFace, QuadVertex.SE), TexCoord1 = GetTextureCoordinates(tile.SouthShadow, tileFace, QuadVertex.SE), Color = color },
						new MpdVertex() { Position = new Vector4(x - 6.0f, -tile.TopVertexSE, z + 6.0f, 1.0f), TexCoord0 = GetTextureCoordinates(tile.SouthMain, tileFace, QuadVertex.NE), TexCoord1 = GetTextureCoordinates(tile.SouthShadow, tileFace, QuadVertex.NE), Color = color },
						new MpdVertex() { Position = new Vector4(x - 6.0f, -tile.TopVertexSW, z - 6.0f, 1.0f), TexCoord0 = GetTextureCoordinates(tile.SouthMain, tileFace, QuadVertex.NW), TexCoord1 = GetTextureCoordinates(tile.SouthShadow, tileFace, QuadVertex.NW), Color = color },
					};
					break;

				case TileFace.East:
					vertices = new MpdVertex[]
					{
						new MpdVertex() { Position = new Vector4(x - 6.0f, -tile.EastVertexSE, z + 6.0f, 1.0f), TexCoord0 = GetTextureCoordinates(tile.EastMain, tileFace, QuadVertex.SW), TexCoord1 = GetTextureCoordinates(tile.EastShadow, tileFace, QuadVertex.SW), Color = color },
						new MpdVertex() { Position = new Vector4(x + 6.0f, -tile.EastVertexNE, z + 6.0f, 1.0f), TexCoord0 = GetTextureCoordinates(tile.EastMain, tileFace, QuadVertex.SE), TexCoord1 = GetTextureCoordinates(tile.EastShadow, tileFace, QuadVertex.SE), Color = color },
						new MpdVertex() { Position = new Vector4(x + 6.0f, -tile.TopVertexNE, z + 6.0f, 1.0f), TexCoord0 = GetTextureCoordinates(tile.EastMain, tileFace, QuadVertex.NE), TexCoord1 = GetTextureCoordinates(tile.EastShadow, tileFace, QuadVertex.NE), Color = color },
						new MpdVertex() { Position = new Vector4(x - 6.0f, -tile.TopVertexSE, z + 6.0f, 1.0f), TexCoord0 = GetTextureCoordinates(tile.EastMain, tileFace, QuadVertex.NW), TexCoord1 = GetTextureCoordinates(tile.EastShadow, tileFace, QuadVertex.NW), Color = color },
					};
					break;

				case TileFace.North:
					vertices = new MpdVertex[]
					{
						new MpdVertex() { Position = new Vector4(x + 6.0f, -tile.NorthVertexNE, z + 6.0f, 1.0f), TexCoord0 = GetTextureCoordinates(tile.NorthMain, tileFace, QuadVertex.SW), TexCoord1 = GetTextureCoordinates(tile.NorthShadow, tileFace, QuadVertex.SW), Color = color },
						new MpdVertex() { Position = new Vector4(x + 6.0f, -tile.NorthVertexNW, z - 6.0f, 1.0f), TexCoord0 = GetTextureCoordinates(tile.NorthMain, tileFace, QuadVertex.SE), TexCoord1 = GetTextureCoordinates(tile.NorthShadow, tileFace, QuadVertex.SE), Color = color },
						new MpdVertex() { Position = new Vector4(x + 6.0f, -tile.TopVertexNW, z - 6.0f, 1.0f), TexCoord0 = GetTextureCoordinates(tile.NorthMain, tileFace, QuadVertex.NE), TexCoord1 = GetTextureCoordinates(tile.NorthShadow, tileFace, QuadVertex.NE), Color = color },
						new MpdVertex() { Position = new Vector4(x + 6.0f, -tile.TopVertexNE, z + 6.0f, 1.0f), TexCoord0 = GetTextureCoordinates(tile.NorthMain, tileFace, QuadVertex.NW), TexCoord1 = GetTextureCoordinates(tile.NorthShadow, tileFace, QuadVertex.NW), Color = color },
					};
					break;

				case TileFace.West:
					vertices = new MpdVertex[]
					{
						new MpdVertex() { Position = new Vector4(x + 6.0f, -tile.WestVertexNW, z - 6.0f, 1.0f), TexCoord0 = GetTextureCoordinates(tile.WestMain, tileFace, QuadVertex.SW), TexCoord1 = GetTextureCoordinates(tile.WestShadow, tileFace, QuadVertex.SW), Color = color },
						new MpdVertex() { Position = new Vector4(x - 6.0f, -tile.WestVertexSW, z - 6.0f, 1.0f), TexCoord0 = GetTextureCoordinates(tile.WestMain, tileFace, QuadVertex.SE), TexCoord1 = GetTextureCoordinates(tile.WestShadow, tileFace, QuadVertex.SE), Color = color },
						new MpdVertex() { Position = new Vector4(x - 6.0f, -tile.TopVertexSW, z - 6.0f, 1.0f), TexCoord0 = GetTextureCoordinates(tile.WestMain, tileFace, QuadVertex.NE), TexCoord1 = GetTextureCoordinates(tile.WestShadow, tileFace, QuadVertex.NE), Color = color },
						new MpdVertex() { Position = new Vector4(x + 6.0f, -tile.TopVertexNW, z - 6.0f, 1.0f), TexCoord0 = GetTextureCoordinates(tile.WestMain, tileFace, QuadVertex.NW), TexCoord1 = GetTextureCoordinates(tile.WestShadow, tileFace, QuadVertex.NW), Color = color },
					};
					break;

				default:
					vertices = new MpdVertex[0];
					break;
			}

			return vertices;
		}

		private Vector2 GetTextureCoordinates(TextureMapping textureMapping, TileFace tileFace, QuadVertex vertex)
		{
			bool mirrorX = textureMapping.GetMirrorX();
			bool mirrorY = textureMapping.GetMirrorY();
			if (mirrorX && mirrorY)
			{
				// Mirror X- and Y-axis
				if (vertex == QuadVertex.NW) vertex = QuadVertex.SE;
				else if (vertex == QuadVertex.SE) vertex = QuadVertex.NW;
				else if (vertex == QuadVertex.NE) vertex = QuadVertex.SW;
				else if (vertex == QuadVertex.SW) vertex = QuadVertex.NE;
			}
			else if (mirrorX)
			{
				// Mirror X-axis
				if (vertex == QuadVertex.NW) vertex = QuadVertex.NE;
				else if (vertex == QuadVertex.NE) vertex = QuadVertex.NW;
				else if (vertex == QuadVertex.SW) vertex = QuadVertex.SE;
				else if (vertex == QuadVertex.SE) vertex = QuadVertex.SW;
			}
			else if (mirrorY)
			{
				// Mirror Y-axis
				if (vertex == QuadVertex.NW) vertex = QuadVertex.SW;
				else if (vertex == QuadVertex.SW) vertex = QuadVertex.NW;
				else if (vertex == QuadVertex.NE) vertex = QuadVertex.SE;
				else if (vertex == QuadVertex.SE) vertex = QuadVertex.NE;
			}

			switch (textureMapping.GetRotation())
			{
				case 1:
					// 90 degrees
					if (vertex == QuadVertex.NW) vertex = QuadVertex.NE;
					else if (vertex == QuadVertex.NE) vertex = QuadVertex.SE;
					else if (vertex == QuadVertex.SE) vertex = QuadVertex.SW;
					else if (vertex == QuadVertex.SW) vertex = QuadVertex.NW;
					break;

				case 2:
					// 180 degrees
					if (vertex == QuadVertex.NW) vertex = QuadVertex.SE;
					else if (vertex == QuadVertex.SE) vertex = QuadVertex.NW;
					else if (vertex == QuadVertex.NE) vertex = QuadVertex.SW;
					else if (vertex == QuadVertex.SW) vertex = QuadVertex.NE;
					break;

				case 3:
					// 270 degress
					if (vertex == QuadVertex.NW) vertex = QuadVertex.SW;
					else if (vertex == QuadVertex.SW) vertex = QuadVertex.SE;
					else if (vertex == QuadVertex.SE) vertex = QuadVertex.NE;
					else if (vertex == QuadVertex.NE) vertex = QuadVertex.NW;
					break;
			}

			var div = 256.0f;
			var x1 = (textureMapping.X + 0.5f) / div;
			var x2 = ((textureMapping.X + textureMapping.Width) - 0.5f) / div;
			var y1 = (textureMapping.Y + 0.5f) / div;
			var y2 = ((textureMapping.Y + textureMapping.Height) - 0.5f) / div;

			Vector2 coord;
			switch (vertex)
			{
				case QuadVertex.SE: coord = new Vector2(x2, y2); break;
				case QuadVertex.SW: coord = new Vector2(x1, y2); break;
				case QuadVertex.NW: coord = new Vector2(x1, y1); break;
				case QuadVertex.NE: coord = new Vector2(x2, y1); break;
				default: throw new Exception("Unknown vertex for coord");
			}

			return coord;
		}

		public void RenderObjects((Mesh, Texture)[][] meshes, Cobalt.Shader shader)
		{
			var chunk = Chunks[0];
			//foreach (var chunk in Chunks)
			{
				//var obj = chunk.Objects[20];
				//foreach (var obj in chunk.Objects.Take(2))
				//foreach (var obj in chunk.Objects.Skip(14).Take(3))
				//foreach (var obj in new Object[] { chunk.Objects[0], chunk.Objects[1], chunk.Objects[13], chunk.Objects[20] })
				foreach (var obj in chunk.Objects)
				{
					if (obj.ObjectType < meshes.Length)
					{
						var x = ((((chunk.MapOffsetX - 6) / 12) * 12.0f) + obj.Transforms.XPosition);
						var y = (float)-obj.Transforms.YPosition;
						var z = ((((chunk.MapOffsetZ - 6) / 12) * 12.0f) + obj.Transforms.ZPosition);

						var scaleMatrix = Matrix4.CreateScale(obj.Transforms.XScale / 100.0f, obj.Transforms.YScale / 100.0f, obj.Transforms.ZScale / 100.0f);
						var rotationZMatrix = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(obj.Transforms.ZRotation));
						var rotationYMatrix = Matrix4.CreateRotationY(-MathHelper.DegreesToRadians(obj.Transforms.YRotation));
						var rotationXMatrix = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(obj.Transforms.XRotation));
						var translationMatrix = Matrix4.CreateTranslation(x, y, z);
						var finalMatrix = scaleMatrix * rotationZMatrix * rotationYMatrix * rotationXMatrix * translationMatrix;

						shader.SetUniformMatrix("local_matrix", false, finalMatrix);

						foreach (var (objMesh, objTexture) in meshes[obj.ObjectType])
						{
							objTexture.Activate();
							objMesh.Render();
						}
					}
				}
			}
		}

		public void RenderFloor(ModelParser.MapBinary mapBinary, Cobalt.Shader shader)
		{
			if (mapBinary == null) return;

			if (false)
			{
				if (Meshes == null)
				{
					Meshes = new List<List<(Mesh, Texture[], BlendMode[])>>();

					var dummyTexture = new Bitmap(32, 32, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
					using (Graphics g = Graphics.FromImage(dummyTexture))
					{
						g.Clear(Color.Transparent);
					}
					var textures = new List<Texture>();
					textures.Add(new Texture(dummyTexture));
					for (int i = 0; i < mapBinary.FloorTextureChunks.Length; i++)
						textures.Add(new Texture(mapBinary.FloorTextureChunks[i].Bitmaps.FirstOrDefault()));

					foreach (var chunk in Chunks)
					{
						var tiles = Tiles[chunk];
						var chunkMeshes = new List<(Mesh, Texture[], BlendMode[])>();

						for (int tidx0 = 0; tidx0 < textures.Count; tidx0++)
						{
							for (int tidx1 = 0; tidx1 < textures.Count; tidx1++)
							{
								foreach (var blend0 in (BlendMode[])Enum.GetValues(typeof(BlendMode)))
								{
									foreach (var blend1 in (BlendMode[])Enum.GetValues(typeof(BlendMode)))
									{
										var vertices = new List<MpdVertex>();

										foreach (var tile in tiles)
										{
											foreach (var face in (TileFace[])Enum.GetValues(typeof(TileFace)))
											{
												if (face == TileFace.West)
												{
													if (tile.WestMain.TextureID == tidx0 && tile.WestShadow.TextureID == tidx1 && tile.WestMain.GetBlendMode() == blend0 && tile.WestShadow.GetBlendMode() == blend1)
														vertices.AddRange(GenerateTileVertices(chunk, tile, face));
													continue;
												}

												//if (face == TileFace.West && (tile.WestMain.TextureID != tidx0 || tile.WestShadow.TextureID != tidx1 || tile.WestMain.GetBlendMode() != blend0 || tile.WestShadow.GetBlendMode() != blend1)) continue;
												else if (face == TileFace.North && (tile.NorthMain.TextureID != tidx0 || tile.NorthShadow.TextureID != tidx1 || tile.NorthMain.GetBlendMode() != blend0 || tile.NorthShadow.GetBlendMode() != blend1)) continue;
												else if (face == TileFace.East && (tile.EastMain.TextureID != tidx0 || tile.EastShadow.TextureID != tidx1 || tile.EastMain.GetBlendMode() != blend0 || tile.EastShadow.GetBlendMode() != blend1)) continue;
												else if (face == TileFace.South && (tile.SouthMain.TextureID != tidx0 || tile.SouthShadow.TextureID != tidx1 || tile.SouthMain.GetBlendMode() != blend0 || tile.SouthShadow.GetBlendMode() != blend1)) continue;
												//else if (face == TileFace.Top && (tile.TopMain.TextureID != tidx0 || tile.TopShadow1.TextureID != tidx1 || tile.TopMain.GetBlendMode() != blend0 || tile.TopShadow1.GetBlendMode() != blend1)) continue;
												else if (face == TileFace.Top)
												{
													if (tile.TopMain.TextureID == tidx0 && tile.TopShadow1.TextureID == tidx1 && tile.TopMain.GetBlendMode() == blend0 && tile.TopShadow1.GetBlendMode() == blend1)
														vertices.AddRange(GenerateTileVertices(chunk, tile, face));
													continue;
												}

												vertices.AddRange(GenerateTileVertices(chunk, tile, face));
											}
										}

										if (vertices.Count > 0)
										{
											var mesh = new Mesh();
											mesh.SetPrimitiveType(PrimitiveType.Quads);
											mesh.SetVertexData(vertices.ToArray());

											chunkMeshes.Add((mesh, new Texture[] { textures[tidx0], textures[tidx1] }, new BlendMode[] { blend0, blend1 }));
										}
									}
								}
							}
						}

						Meshes.Add(chunkMeshes);
					}
				}
			}
			else
			{
				if (Meshes == null)
				{
					Meshes = new List<List<(Mesh, Texture[], BlendMode[])>>();
					foreach (var chunk in Chunks)
					{
						var tiles = Tiles[chunk];
						var chunkMeshes = new List<(Mesh, Texture[], BlendMode[])>();

						foreach (var tile in tiles)
						{
							foreach (var face in (TileFace[])Enum.GetValues(typeof(TileFace)))
							{
								var mesh = new Mesh();
								mesh.SetPrimitiveType(PrimitiveType.Quads);

								mesh.SetVertexData(GenerateTileVertices(chunk, tile, face));

								var textureIds = new int[] { -1, -1 };
								var blendModes = new BlendMode[] { BlendMode.Normal, BlendMode.Normal };
								switch (face)
								{
									case TileFace.West:
										textureIds[0] = tile.WestMain.TextureID;
										textureIds[1] = tile.WestShadow.TextureID;
										blendModes[0] = tile.WestMain.GetBlendMode();
										blendModes[1] = tile.WestShadow.GetBlendMode();
										break;
									case TileFace.North:
										textureIds[0] = tile.NorthMain.TextureID;
										textureIds[1] = tile.NorthShadow.TextureID;
										blendModes[0] = tile.NorthMain.GetBlendMode();
										blendModes[1] = tile.NorthShadow.GetBlendMode();
										break;
									case TileFace.East:
										textureIds[0] = tile.EastMain.TextureID;
										textureIds[1] = tile.EastShadow.TextureID;
										blendModes[0] = tile.EastMain.GetBlendMode();
										blendModes[1] = tile.EastShadow.GetBlendMode();
										break;
									case TileFace.South:
										textureIds[0] = tile.SouthMain.TextureID;
										textureIds[1] = tile.SouthShadow.TextureID;
										blendModes[0] = tile.SouthMain.GetBlendMode();
										blendModes[1] = tile.SouthShadow.GetBlendMode();
										break;
									case TileFace.Top:
										textureIds[0] = tile.TopMain.TextureID;
										textureIds[1] = tile.TopShadow1.TextureID;
										blendModes[0] = tile.TopMain.GetBlendMode();
										blendModes[1] = tile.TopShadow1.GetBlendMode();
										break;
								}

								var textures = new Texture[textureIds.Length];
								for (int i = 0; i < textures.Length; i++)
								{
									if (textureIds[i] < mapBinary.FloorTextures.Length)
										textures[i] = mapBinary.FloorTextures[textureIds[i]];
								}

								chunkMeshes.Add((mesh, textures, blendModes));
							}
						}

						Meshes.Add(chunkMeshes);
					}
				}
			}

			foreach (var chunkMeshes in Meshes)
			{
				foreach (var (mesh, textures, blendMode) in chunkMeshes)
				{
					// ????
					//GL.DepthMask(blendMode[0] != BlendMode.Translucent && blendMode[1] != BlendMode.Translucent);

					shader.SetUniform("blendMode0", (int)blendMode[0]);
					shader.SetUniform("blendMode1", (int)blendMode[1]);

					textures[0]?.Activate(TextureUnit.Texture0);
					textures[1]?.Activate(TextureUnit.Texture1);

					mesh.Render();
				}
			}
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct MpdVertex : IVertexStruct
	{
		[VertexElement(AttributeIndex = 0)]
		public Vector4 Position;
		[VertexElement(AttributeIndex = 1)]
		public Color4 Color;
		[VertexElement(AttributeIndex = 2)]
		public Vector2 TexCoord0;
		[VertexElement(AttributeIndex = 3)]
		public Vector2 TexCoord1;
	}
}
