using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

using Cobalt.IO;

namespace DisgaeaMap
{
	public static class TextureHelper
	{
		enum PS2PixelFormat
		{
			PSMCT32 = 0x00,
			PSMCT24 = 0x01,
			PSMCT16 = 0x02,
			PSMCT16S = 0x0A,
			PSMT8 = 0x13,
			PSMT4 = 0x14,
			PSMT8H = 0x1B,
			PSMT4HL = 0x24,
			PSMT4HH = 0x2C
		}

		public static byte[][] GetPaletteData(EndianBinaryReader reader, int paletteHeight, int colorCount)
		{
			byte[][] PaletteData = new byte[paletteHeight][];
			for (int i = 0; i < PaletteData.Length; i++)
				PaletteData[i] = ReadPaletteData(reader, (colorCount == 256 ? PS2PixelFormat.PSMT8 : PS2PixelFormat.PSMT4), PS2PixelFormat.PSMCT32);

			return PaletteData;
		}

		public static byte[] GetPixelData(EndianBinaryReader reader, int imageWidth, int imageHeight, int colorCount)
		{
			return reader.ReadBytes((imageWidth * imageHeight) / (colorCount == 256 ? 1 : 2));
		}

		public static Bitmap[] GetBitmaps(int imageWidth, int imageHeight, int paletteHeight, int colorCount, byte[] pixelData, byte[][] paletteData)
		{
			Bitmap[] bitmaps = new Bitmap[paletteHeight];

			for (int i = 0; i < paletteData.Length; i++)
			{
				Bitmap bitmap = new Bitmap(imageWidth, imageHeight, PixelFormat.Format32bppArgb);

				BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);

				var pixels = new byte[bitmapData.Height * bitmapData.Stride];
				for (int j = 0, k = 0; j < pixelData.Length; j++, k += (colorCount == 256 ? 4 : 8))
				{
					if (colorCount == 256)
					{
						var cofs = pixelData[j] * 4;
						pixels[k + 3] = paletteData[i][cofs + 0];
						pixels[k + 2] = paletteData[i][cofs + 1];
						pixels[k + 1] = paletteData[i][cofs + 2];
						pixels[k + 0] = paletteData[i][cofs + 3];
					}
					else
					{
						var cofs1 = (pixelData[j] & 0xF) * 4;
						var cofs2 = (pixelData[j] >> 4) * 4;

						pixels[k + 3] = paletteData[i][cofs1 + 0];
						pixels[k + 2] = paletteData[i][cofs1 + 1];
						pixels[k + 1] = paletteData[i][cofs1 + 2];
						pixels[k + 0] = paletteData[i][cofs1 + 3];

						pixels[k + 7] = paletteData[i][cofs2 + 0];
						pixels[k + 6] = paletteData[i][cofs2 + 1];
						pixels[k + 5] = paletteData[i][cofs2 + 2];
						pixels[k + 4] = paletteData[i][cofs2 + 3];
					}
				}

				Marshal.Copy(pixels, 0, bitmapData.Scan0, pixels.Length);
				bitmap.UnlockBits(bitmapData);

				bitmaps[i] = bitmap;
			}

			return bitmaps;
		}

		private static byte ScaleAlpha(byte a)
		{
			return (byte)Math.Min((255.0f * (a / 128.0f)), 0xFF);
		}

		private static byte[] ReadPaletteData(EndianBinaryReader reader, PS2PixelFormat pixelFormat, PS2PixelFormat paletteFormat)
		{
			int colorCount = (pixelFormat == PS2PixelFormat.PSMT4 ? 16 : (pixelFormat == PS2PixelFormat.PSMT8 ? 256 : 0));
			byte[] tempPalette = new byte[colorCount * 4];

			byte r, g, b, a;
			for (int i = 0; i < tempPalette.Length; i += 4)
			{
				if (paletteFormat == PS2PixelFormat.PSMCT32)
				{
					uint color = reader.ReadUInt32();
					r = (byte)color;
					g = (byte)(color >> 8);
					b = (byte)(color >> 16);
					a = ScaleAlpha((byte)(color >> 24));
				}
				else
				{
					ushort color = reader.ReadUInt16();
					r = (byte)((color & 0x001F) << 3);
					g = (byte)(((color & 0x03E0) >> 5) << 3);
					b = (byte)(((color & 0x7C00) >> 10) << 3);
					a = ScaleAlpha((byte)(i == 0 ? 0 : 0x80));
				}

				tempPalette[i + 0] = a;
				tempPalette[i + 1] = r;
				tempPalette[i + 2] = g;
				tempPalette[i + 3] = b;
			}

			byte[] paletteData;

			if (colorCount == 256)
			{
				paletteData = new byte[tempPalette.Length];
				for (int i = 0; i < paletteData.Length; i += (32 * 4))
				{
					Buffer.BlockCopy(tempPalette, i + (0 * 4), paletteData, i + (0 * 4), (8 * 4));
					Buffer.BlockCopy(tempPalette, i + (8 * 4), paletteData, i + (16 * 4), (8 * 4));
					Buffer.BlockCopy(tempPalette, i + (16 * 4), paletteData, i + (8 * 4), (8 * 4));
					Buffer.BlockCopy(tempPalette, i + (24 * 4), paletteData, i + (24 * 4), (8 * 4));
				}
			}
			else
				paletteData = tempPalette;

			return paletteData;
		}
	}
}
