using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;

using Cobalt.IO;

namespace DisgaeaMap.AnimParser
{
	public class AnmBinary : ParsableData
	{
		public uint NumSpriteSets { get; private set; }
		public ushort[] SpriteSetIDs { get; private set; }

		public SpriteSet[] SpriteSets { get; private set; }

		public AnmBinary(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			NumSpriteSets = reader.ReadUInt32();
			SpriteSetIDs = new ushort[NumSpriteSets];
			for (int i = 0; i < SpriteSetIDs.Length; i++) SpriteSetIDs[i] = reader.ReadUInt16();

			stream.Position += ((reader.BaseStream.Position % 16) != 0 ? (16 - (reader.BaseStream.Position % 16)) : 16);

			SpriteSets = new SpriteSet[NumSpriteSets];
			for (int i = 0; i < SpriteSets.Length; i++) SpriteSets[i] = new SpriteSet(stream);


			if (false)
			{
				// TEMP dump all spritesheets
				foreach (var s in SpriteSetIDs)
					TESTDumpSheets(s);
			}

			if (true)
			{
				// TEMP dump stuff -- animations (tho they dont work right yet, herp derp!), spritesheets
				var id = (ushort)0;

				id = 09101; // 1st hit effect
				id = 09201; // 1st sword
				id = 01000; // battle ui stuff
				id = 01070; // female samurai

				TESTDumpSheets(id);
				TESTDumpAnims(id);
			}
		}

		private void TESTDumpSheets(ushort setId)
		{
			var path = @"D:\Temp\Disgaea\Anim\";
			var dir = $"Set {setId:D5}";

			var set = GetSpriteSet(setId);
			if (set == null) return;

			var subdir1 = "Sheets";
			Directory.CreateDirectory(Path.Combine(path, dir, subdir1));

			for (int b = 0; b < set.SpriteSheetCount; b++)
			{
				var bmps = set.GetSpriteSheetBitmaps(b);
				for (int p = 0; p < bmps.Length; p++)
				{
					string file = $"Sheet {b:D2} Palette {p:D2}.png";
					bmps[p].Save(Path.Combine(path, dir, subdir1, file));
				}
			}
		}

		private void TESTDumpAnims(ushort setId)
		{
			// unk1 -> unk5 -> unk2

			var path = @"D:\Temp\Disgaea\Anim\";
			var dir = $"Set {setId:D5}";

			var set = GetSpriteSet(setId);

			for (int i = 0; i < set.Unknown1s.Length; i++)
			{
				var unk1 = set.Unknown1s[i];

				var subdir = $"Unknown1 {i}";
				Directory.CreateDirectory(Path.Combine(path, dir, subdir));

				var unk5 = set.Unknown5s[unk1.IndexIntoUnknown5];
				for (int j = unk5.IndexIntoUnknown2; j < unk5.IndexIntoUnknown2 + unk5.Unknown0x02; j++)
				{
					var unk2 = set.Unknown2s[j];

					var startFrame = unk2.Unknown0x00;
					var endFrame = unk2.Unknown0x00 + unk2.Unknown0x02;

					for (int f = startFrame; f < endFrame; f++)
					{
						var file = $"Unknown5 {unk1.IndexIntoUnknown5} - Unknown2 {j} - Frame {f:D2}.png";

						var frame = set.Frames[f];
						var bitmap = GetFrameBitmap(set, frame);
						bitmap.Save(Path.Combine(path, dir, subdir, file));
					}
				}
			}

			var subdir2 = "All Frames";
			Directory.CreateDirectory(Path.Combine(path, dir, subdir2));

			for (int f = 0; f < set.Frames.Length; f++)
			{
				var frame = set.Frames[f];
				var file = $"Frame {f:D2}.png";
				var bitmap = GetFrameBitmap(set, frame);
				bitmap.Save(Path.Combine(path, dir, subdir2, file));
			}
		}

		public SpriteSet GetSpriteSet(ushort setId)
		{
			int idx = idx = Array.IndexOf(SpriteSetIDs, setId);
			if (idx == -1)
			{
				var mainId = (ushort)((setId / 10) * 10);
				idx = Array.IndexOf(SpriteSetIDs, mainId);
			}
			return (idx != -1 ? SpriteSets[idx] : null);
		}

		public Bitmap GetFrameBitmap(SpriteSet set, Frame frame)
		{
			var mainSheet = set.GetSpriteSheetBitmaps(frame.SpriteSheetIndex)[frame.PaletteIndex];

			Bitmap subSheet = null;
			if (frame.Unknown0x00 != 0)
			{
				var subSetId = (ushort)(9001 + (frame.Unknown0x00 * 100));
				var subSet = GetSpriteSet(subSetId);
				if (subSet != null)
					subSheet = subSet.GetSpriteSheetBitmaps(0)[0];
			}

			return GetFrameBitmap(mainSheet, subSheet, frame);
		}

		public Bitmap GetFrameBitmap(Bitmap mainSheet, Bitmap subSheet, Frame frame)
		{
			// http://csharphelper.com/blog/2016/03/rotate-images-in-c/
			var rotOrigin = new Matrix();
			rotOrigin.Rotate(frame.RotationAngle);
			var points = new Point[]
			{
				new Point(0, 0),
				new Point(frame.SourceWidth, 0),
				new Point(frame.SourceWidth, frame.SourceHeight),
				new Point(0, frame.SourceHeight)
			};
			rotOrigin.TransformPoints(points);

			int xmin = points[0].X;
			int xmax = xmin;
			int ymin = points[0].Y;
			int ymax = ymin;
			foreach (var point in points)
			{
				if (xmin > point.X) xmin = point.X;
				if (xmax < point.X) xmax = point.X;
				if (ymin > point.Y) ymin = point.Y;
				if (ymax < point.Y) ymax = point.Y;
			}

			var width = (xmax - xmin);
			var height = (ymax - ymin);
			var scaleX = (frame.ScaleX / 100.0f);
			var scaleY = (frame.ScaleY / 100.0f);
			var rotated = new Bitmap(width, height);

			var rotCenter = new Matrix();
			rotCenter.RotateAt(frame.RotationAngle, new PointF(rotated.Width / 2, rotated.Height / 2));

			using (var g = Graphics.FromImage(rotated))
			{
				g.Transform = rotCenter;

				var sheet = (frame.Unknown0x00 == 0 ? mainSheet : subSheet);
				if (sheet != null)
				{
					int x = ((rotated.Width - frame.SourceWidth) / 2);
					int y = ((rotated.Height - frame.SourceHeight) / 2);
					g.DrawImage(sheet, x, y, new Rectangle(frame.SourceX, frame.SourceY, frame.SourceWidth, frame.SourceHeight), GraphicsUnit.Pixel);
				}
			}

			var scaled = new Bitmap((int)(rotated.Width * scaleX), (int)(rotated.Height * scaleY));
			using (var g = Graphics.FromImage(scaled))
			{
				g.DrawImage(rotated, new Rectangle(0, 0, scaled.Width, scaled.Height));
			}

			// TODO verify me
			if ((frame.Unknown0x1B & 0x18) == 0x18)
				scaled.RotateFlip(RotateFlipType.RotateNoneFlipXY);
			else if ((frame.Unknown0x1B & 0x08) == 0x08)
				scaled.RotateFlip(RotateFlipType.RotateNoneFlipX);
			else if ((frame.Unknown0x1B & 0x10) == 0x10)
				scaled.RotateFlip(RotateFlipType.RotateNoneFlipY);

			return scaled;
		}
	}
}
