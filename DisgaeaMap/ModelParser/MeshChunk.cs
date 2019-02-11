using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using OpenTK;

using Cobalt.IO;

namespace DisgaeaMap.ModelParser
{
	public class MeshChunk : BaseChunk
	{
		public int Unknown0x00 { get; private set; }        // 2
		public int Unknown0x04 { get; private set; }        // 1
		public int Unknown0x08 { get; private set; }        // -1
		public int NumUnknownTransforms { get; private set; }
		public int Unknown0x10 { get; private set; }        // 0
		public int UnknownData2Offset { get; private set; }
		public int Unknown0x18 { get; private set; }        // 1
		public int NumUnknownData1 { get; private set; }

		public UnknownData1[] UnknownData1 { get; private set; }
		public TransformData[] UnknownTransforms { get; private set; }
		public MeshData MeshData { get; private set; }

		public MeshChunk(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			base.ReadFromStream(stream, endianness);

			long startPosition = stream.Position;
			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			Unknown0x00 = reader.ReadInt32();
			Unknown0x04 = reader.ReadInt32();
			Unknown0x08 = reader.ReadInt32();
			NumUnknownTransforms = reader.ReadInt32();
			Unknown0x10 = reader.ReadInt32();
			UnknownData2Offset = reader.ReadInt32();
			Unknown0x18 = reader.ReadInt32();
			NumUnknownData1 = reader.ReadInt32();

			UnknownData1 = new UnknownData1[NumUnknownData1];
			for (int i = 0; i < UnknownData1.Length; i++) UnknownData1[i] = new UnknownData1(stream);

			UnknownTransforms = new TransformData[NumUnknownTransforms];
			for (int i = 0; i < UnknownTransforms.Length; i++) UnknownTransforms[i] = new TransformData(stream);

			stream.Position = startPosition + UnknownData2Offset;
			MeshData = new MeshData(stream);
		}
	}
}
