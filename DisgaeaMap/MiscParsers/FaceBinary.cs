using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

using Cobalt.IO;

namespace DisgaeaMap.MiscParsers
{
	public class FaceBinary : ParsableData
	{
		public uint NumFaces { get; private set; }
		public ushort[] FaceIDs { get; private set; }

		public ModelParser.FloorTextureChunk[] FaceTextures { get; private set; }

		public FaceBinary(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			NumFaces = reader.ReadUInt32();

			FaceIDs = new ushort[NumFaces];
			for (int i = 0; i < FaceIDs.Length; i++) FaceIDs[i] = reader.ReadUInt16();

			stream.Position += (16 - (stream.Position % 16));

			FaceTextures = new ModelParser.FloorTextureChunk[NumFaces];
			for (int i = 0; i < FaceTextures.Length; i++)
			{
				var position = stream.Position;
				FaceTextures[i] = new ModelParser.FloorTextureChunk(stream);
				stream.Position = position + FaceTextures[i].DataSize + 0x10;
			}

			for (int i = 0; i < FaceTextures.Length; i++)
			{
				for (int j = 0; j < FaceTextures[i].Bitmaps.Length; j++)
				{
					FaceTextures[i].Bitmaps[j].Save($@"D:\Temp\Disgaea\Face\{FaceIDs[i]:D5}_{j:D2}.png");
				}
			}
		}
	}
}
