using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Cobalt;
using Cobalt.Mesh;
using Cobalt.Texture;

namespace DisgaeaMap.AnimParser
{
	public class Renderer
	{
		AnmBinary animBinary;
		Shader shader;
		string modelviewMatrixName;

		Mesh spriteMesh;
		int lastAnimIdx;

		int currentFrameIdx;
		double currentFrameDuration;
		DateTime lastRenderTime;

		Dictionary<AnimSet, Bitmap[][]> spriteSheetBitmapDict;
		Dictionary<(int, int), Texture> spriteSheetTextureDict;
		Dictionary<(ushort, int), Frame[]> setAnimDataDict;

		public Renderer(AnmBinary animBinary, Shader shader, string modelviewMatrixName)
		{
			this.animBinary = animBinary;
			this.shader = shader;
			this.modelviewMatrixName = modelviewMatrixName;

			var spriteVertices = new TESTAnimVertex[]
			{
				new TESTAnimVertex() { Position = new Vector4(0.0f, 0.0f, 0.0f, 1.0f), TexCoord = new Vector2(0.0f, 1.0f) },
				new TESTAnimVertex() { Position = new Vector4(1.0f, 0.0f, 0.0f, 1.0f), TexCoord = new Vector2(1.0f, 1.0f) },
				new TESTAnimVertex() { Position = new Vector4(1.0f, 1.0f, 0.0f, 1.0f), TexCoord = new Vector2(1.0f, 0.0f) },
				new TESTAnimVertex() { Position = new Vector4(0.0f, 1.0f, 0.0f, 1.0f), TexCoord = new Vector2(0.0f, 0.0f) },
			};
			spriteMesh = new Mesh();
			spriteMesh.SetPrimitiveType(PrimitiveType.Quads);
			spriteMesh.SetVertexData(spriteVertices);

			lastAnimIdx = -1;

			currentFrameIdx = -1;
			lastRenderTime = DateTime.Now;

			spriteSheetBitmapDict = new Dictionary<AnimSet, Bitmap[][]>();
			spriteSheetTextureDict = new Dictionary<(int, int), Texture>();
			setAnimDataDict = new Dictionary<(ushort, int), Frame[]>();
		}

		public void Render(ushort setId, int animIdx)
		{
			if (shader == null) return;

			if (animIdx != lastAnimIdx)
			{
				currentFrameIdx = -1;
				lastAnimIdx = animIdx;
			}

			var set = animBinary.GetAnimSet(setId);
			if (set == null) return;

			Frame[] frameData;

			var setAnimKey = (setId, animIdx);
			if (!setAnimDataDict.ContainsKey(setAnimKey))
			{
				if (!spriteSheetBitmapDict.ContainsKey(set))
				{
					var bitmaps = new Bitmap[set.SpriteSheets.Length][];
					for (int s = 0; s < set.SpriteSheets.Length; s++)
						bitmaps[s] = set.GetSpriteSheetBitmaps(s);

					spriteSheetBitmapDict.Add(set, bitmaps);
				}

				var animIdent = set.AnimIdentifiers[animIdx];
				var frames = set.Frames.Skip(animIdent.FirstFrameIndex).TakeWhile(x => (x.MaybeFlags != 2)).ToArray();

				foreach (var frame in frames)
				{
					var spriteSet = set.SpriteSets[frame.SpriteSetIndex];
					var sprites = set.Sprites.Skip(spriteSet.FirstSpriteIndex).Take(spriteSet.NumSprites).ToArray();

					foreach (var sprite in sprites)
					{
						var spriteKey = (sprite.SpriteSheetIndex, sprite.PaletteIndex);
						if (!spriteSheetTextureDict.ContainsKey(spriteKey))
							spriteSheetTextureDict.Add(spriteKey, new Texture(spriteSheetBitmapDict[set][sprite.SpriteSheetIndex][sprite.PaletteIndex],
								TextureWrapMode.Clamp, TextureWrapMode.Clamp, TextureMinFilter.Nearest, TextureMagFilter.Nearest));
					}
				}

				frameData = frames;
			}
			else
				frameData = setAnimDataDict[setAnimKey];

			if (currentFrameIdx == -1 || lastRenderTime.AddSeconds(currentFrameDuration) <= DateTime.Now)
			{
				currentFrameIdx = (currentFrameIdx + 1) % frameData.Length;
				currentFrameDuration = (frameData[currentFrameIdx].MaybeDuration / ((20.0 * 255.0) / 60.0));  // TODO err not correct, pretty sure
				lastRenderTime = DateTime.Now;
			}

			var currentFrame = frameData[currentFrameIdx];
			var currentSpriteSet = set.SpriteSets[currentFrame.SpriteSetIndex];
			var currentSprites = set.Sprites.Skip(currentSpriteSet.FirstSpriteIndex).Take(currentSpriteSet.NumSprites).ToArray();
			foreach (var currentSprite in currentSprites)
			{
				var currentTexture = spriteSheetTextureDict[(currentSprite.SpriteSheetIndex, currentSprite.PaletteIndex)];
				currentTexture.Activate();

				shader.Activate();
				shader.SetUniform("sprite_rect", new Vector4(currentSprite.SourceX, currentSprite.SourceY, currentSprite.SourceWidth, currentSprite.SourceHeight));
				shader.SetUniform("sheet_size", new Vector2(currentTexture.Width, currentTexture.Height));

				spriteMesh.Render();
			}
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct TESTAnimVertex : IVertexStruct
	{
		[VertexElement(AttributeIndex = 0)]
		public Vector4 Position;
		[VertexElement(AttributeIndex = 1)]
		public Vector2 TexCoord;
	}
}
