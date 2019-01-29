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
		public uint NumAnimSets { get; private set; }
		public ushort[] AnimSetIDs { get; private set; }

		public AnimSet[] AnimSets { get; private set; }

		public AnmBinary(Stream stream, Endian endianness = Endian.LittleEndian) : base(stream, endianness) { }

		public override void ReadFromStream(Stream stream, Endian endianness)
		{
			EndianBinaryReader reader = new EndianBinaryReader(stream, endianness);

			NumAnimSets = reader.ReadUInt32();
			AnimSetIDs = new ushort[NumAnimSets];
			for (int i = 0; i < AnimSetIDs.Length; i++) AnimSetIDs[i] = reader.ReadUInt16();

			stream.Position += ((reader.BaseStream.Position % 16) != 0 ? (16 - (reader.BaseStream.Position % 16)) : 16);

			AnimSets = new AnimSet[NumAnimSets];
			for (int i = 0; i < AnimSets.Length; i++) AnimSets[i] = new AnimSet(stream);


			if (false)
			{
				// TEMP dump all spritesheets
				foreach (var s in AnimSetIDs)
					TESTDumpSheets(s);
			}

			if (false)
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

			var set = GetAnimSet(setId);
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

			var set = GetAnimSet(setId);

			for (int i = 0; i < set.AnimIdentifiers.Length; i++)
			{
				var unk1 = set.AnimIdentifiers[i];

				var subdir = $"Unknown1 {i}";
				Directory.CreateDirectory(Path.Combine(path, dir, subdir));

				var unk5 = set.Frames[unk1.FirstFrameIndex];
				for (int j = unk5.SpriteSetIndex; j < unk5.SpriteSetIndex + unk5.MaybeFlags; j++)
				{
					var unk2 = set.SpriteSets[j];

					var startFrame = unk2.FirstSpriteIndex;
					var endFrame = unk2.FirstSpriteIndex + unk2.NumSprites;

					for (int f = startFrame; f < endFrame; f++)
					{
						var file = $"Unknown5 {unk1.FirstFrameIndex} - Unknown2 {j} - Frame {f:D2}.png";

						var frame = set.Sprites[f];
						var bitmap = GetSpriteBitmap(set, frame);
						bitmap.Save(Path.Combine(path, dir, subdir, file));
					}
				}
			}

			var subdir2 = "All Frames";
			Directory.CreateDirectory(Path.Combine(path, dir, subdir2));

			for (int f = 0; f < set.Sprites.Length; f++)
			{
				var frame = set.Sprites[f];
				var file = $"Frame {f:D2}.png";
				var bitmap = GetSpriteBitmap(set, frame);
				bitmap.Save(Path.Combine(path, dir, subdir2, file));
			}
		}

		public AnimSet GetAnimSet(ushort setId)
		{
			int idx = idx = Array.IndexOf(AnimSetIDs, setId);
			if (idx == -1)
			{
				var mainId = (ushort)((setId / 10) * 10);
				idx = Array.IndexOf(AnimSetIDs, mainId);
			}
			return (idx != -1 ? AnimSets[idx] : null);
		}

		public Bitmap GetSpriteBitmap(AnimSet set, Sprite sprite)
		{
			var mainSheet = set.GetSpriteSheetBitmaps(sprite.SpriteSheetIndex)[sprite.PaletteIndex];

			Bitmap subSheet = null;
			if (sprite.Unknown0x00 != 0)
			{
				var subSetId = (ushort)(9001 + (sprite.Unknown0x00 * 100));
				var subSet = GetAnimSet(subSetId);
				if (subSet != null)
					subSheet = subSet.GetSpriteSheetBitmaps(0)[0];
			}

			return GetSpriteBitmap(mainSheet, subSheet, sprite);
		}

		public Bitmap GetSpriteBitmap(Bitmap mainSheet, Bitmap subSheet, Sprite sprite)
		{
			// http://csharphelper.com/blog/2016/03/rotate-images-in-c/
			var rotOrigin = new Matrix();
			rotOrigin.Rotate(sprite.RotationAngle);
			var points = new Point[]
			{
				new Point(0, 0),
				new Point(sprite.SourceWidth, 0),
				new Point(sprite.SourceWidth, sprite.SourceHeight),
				new Point(0, sprite.SourceHeight)
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
			var scaleX = (sprite.ScaleX / 100.0f);
			var scaleY = (sprite.ScaleY / 100.0f);
			var rotated = new Bitmap(width, height);

			var rotCenter = new Matrix();
			rotCenter.RotateAt(sprite.RotationAngle, new PointF(rotated.Width / 2, rotated.Height / 2));

			using (var g = Graphics.FromImage(rotated))
			{
				g.Transform = rotCenter;

				var sheet = (sprite.Unknown0x00 == 0 ? mainSheet : subSheet);
				if (sheet != null)
				{
					int x = ((rotated.Width - sprite.SourceWidth) / 2);
					int y = ((rotated.Height - sprite.SourceHeight) / 2);
					g.DrawImage(sheet, x, y, new Rectangle(sprite.SourceX, sprite.SourceY, sprite.SourceWidth, sprite.SourceHeight), GraphicsUnit.Pixel);
				}
			}

			var scaled = new Bitmap((int)(rotated.Width * scaleX), (int)(rotated.Height * scaleY));
			using (var g = Graphics.FromImage(scaled))
			{
				g.DrawImage(rotated, new Rectangle(0, 0, scaled.Width, scaled.Height));
			}

			// TODO verify me
			if ((sprite.Unknown0x1B & 0x18) == 0x18)
				scaled.RotateFlip(RotateFlipType.RotateNoneFlipXY);
			else if ((sprite.Unknown0x1B & 0x08) == 0x08)
				scaled.RotateFlip(RotateFlipType.RotateNoneFlipX);
			else if ((sprite.Unknown0x1B & 0x10) == 0x10)
				scaled.RotateFlip(RotateFlipType.RotateNoneFlipY);

			return scaled;
		}
	}
}
