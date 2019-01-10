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
		public int NumUnknownTransforms { get; private set; }        // 1
		public int Unknown0x10 { get; private set; }        // 0
		public int UnknownData2Offset { get; private set; }
		public int Unknown0x18 { get; private set; }        // 1
		public int NumUnknownData1 { get; private set; }

		public (Vector3, Vector3, float)[] UnknownData1 { get; private set; }
		public TransformData[] UnknownTransforms { get; private set; }
		public GeometryUnknownData2 UnknownData2 { get; private set; }

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

			// errr dunno if this is vec/vec/float, maybe not
			UnknownData1 = new (Vector3, Vector3, float)[NumUnknownData1];
			for (int i = 0; i < UnknownData1.Length; i++)
			{
				var vector1 = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
				var vector2 = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
				var single = reader.ReadSingle();
				UnknownData1[i] = (vector1, vector2, single);
			}

			UnknownTransforms = new TransformData[NumUnknownTransforms];
			for (int i = 0; i < UnknownTransforms.Length; i++) UnknownTransforms[i] = new TransformData(stream);

			stream.Position = startPosition + UnknownData2Offset;
			UnknownData2 = new GeometryUnknownData2(stream);
		}
	}
}
