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
		readonly string modelviewMatrixName;

		AnmBinary animBinary;
		Shader shader;

		Mesh spriteMesh;

		Dictionary<(ushort setId, int animIdx), (int currentFrameIdx, double currentFrameDuration, DateTime lastRenderTime)> renderDict;

		Dictionary<AnimSet, Bitmap[][]> spriteSheetBitmapDict;
		Dictionary<(ushort setId, byte spriteSheetIdx, byte paletteIdx, byte unknown0x00), Texture> spriteSheetTextureDict;
		Dictionary<(ushort setId, int animIdx), Frame[]> setAnimDataDict;

		public Renderer(AnmBinary animBinary, Shader shader, string modelviewMatrixName)
		{
			this.animBinary = animBinary;
			this.shader = shader;
			this.modelviewMatrixName = modelviewMatrixName;

			var spriteVertices = new AnimSpriteVertex[]
			{
				new AnimSpriteVertex() { Position = new Vector4(-0.5f, 0.0f, 0.0f, 1.0f), TexCoord = new Vector2(0.0f, 1.0f) },
				new AnimSpriteVertex() { Position = new Vector4( 0.5f, 0.0f, 0.0f, 1.0f), TexCoord = new Vector2(1.0f, 1.0f) },
				new AnimSpriteVertex() { Position = new Vector4( 0.5f, 1.0f, 0.0f, 1.0f), TexCoord = new Vector2(1.0f, 0.0f) },
				new AnimSpriteVertex() { Position = new Vector4(-0.5f, 1.0f, 0.0f, 1.0f), TexCoord = new Vector2(0.0f, 0.0f) },
			};
			spriteMesh = new Mesh();
			spriteMesh.SetPrimitiveType(PrimitiveType.Quads);
			spriteMesh.SetVertexData(spriteVertices);

			renderDict = new Dictionary<(ushort setId, int animIdx), (int currentFrameIdx, double currentFrameDuration, DateTime lastRenderTime)>();

			spriteSheetBitmapDict = new Dictionary<AnimSet, Bitmap[][]>();
			spriteSheetTextureDict = new Dictionary<(ushort setId, byte spriteSheetIdx, byte paletteIdx, byte unknown0x00), Texture>();
			setAnimDataDict = new Dictionary<(ushort setId, int animIdx), Frame[]>();
		}

		public void Render(Matrix4 modelviewMatrix, ushort setId, int animIdx)
		{
			if (shader == null) return;
			shader.Activate();

			var renderKey = (setId, animIdx);
			if (!renderDict.ContainsKey(renderKey))
				renderDict.Add(renderKey, (-1, 0.0, DateTime.Now));

			var set = animBinary.GetAnimSet(setId);
			if (set == null) return;

			Frame[] frameData;

			var setAnimKey = (setId, animIdx);
			if (!setAnimDataDict.ContainsKey(setAnimKey))
			{
				var animIdent = set.AnimIdentifiers[animIdx];
				var frames = set.Frames.Skip(animIdent.FirstFrameIndex).TakeWhile(x => (x.MaybeFlags != 2)).ToArray();

				foreach (var frame in frames)
				{
					var spriteSet = set.SpriteSets[frame.SpriteSetIndex];
					var sprites = set.Sprites.Skip(spriteSet.FirstSpriteIndex).Take(spriteSet.NumSprites).ToArray();

					foreach (var sprite in sprites)
					{
						Bitmap[][] spriteSheets = null;
						if (sprite.Unknown0x00 == 0)
						{
							// normal sprites
							spriteSheets = GetAndCacheSpriteSheets(set);
						}
						else
						{
							// weapons and stuff
							var subSetId = (ushort)(9001 + (sprite.Unknown0x00 * 100));
							var subSet = animBinary.GetAnimSet(subSetId);
							if (subSet != null)
								spriteSheets = GetAndCacheSpriteSheets(subSet);
						}

						if (spriteSheets != null)
						{
							var spriteKey = (setId, sprite.SpriteSheetIndex, sprite.PaletteIndex, sprite.Unknown0x00);
							if (!spriteSheetTextureDict.ContainsKey(spriteKey))
								spriteSheetTextureDict.Add(spriteKey, new Texture(spriteSheets[sprite.SpriteSheetIndex][sprite.PaletteIndex],
									TextureWrapMode.Clamp, TextureWrapMode.Clamp, TextureMinFilter.Nearest, TextureMagFilter.Nearest));
						}
					}
				}

				frameData = frames;
			}
			else
				frameData = setAnimDataDict[setAnimKey];

			var currentFrameIdx = renderDict[renderKey].currentFrameIdx;
			if (currentFrameIdx == -1 || renderDict[renderKey].lastRenderTime.AddSeconds(renderDict[renderKey].currentFrameDuration) <= DateTime.Now)
			{
				currentFrameIdx = (renderDict[renderKey].currentFrameIdx + 1) % frameData.Length;
				renderDict[renderKey] = (
					currentFrameIdx,
					(frameData[currentFrameIdx].MaybeDuration / ((20.0 * 255.0) / 60.0)),  // TODO err not correct, pretty sure
					DateTime.Now);
			}

			var currentFrame = frameData[renderDict[renderKey].currentFrameIdx];
			var currentSpriteSet = set.SpriteSets[currentFrame.SpriteSetIndex];
			var currentSprites = set.Sprites.Skip(currentSpriteSet.FirstSpriteIndex).Take(currentSpriteSet.NumSprites).ToArray();

			float z = 0.0f;
			foreach (var currentSprite in currentSprites)
			{
				z += 0.05f;

				// TODO: currently, if sprite uses external sheet, only allow axes
				if (currentSprite.Unknown0x00 != 0 && currentSprite.Unknown0x00 != 6) continue;

				var spriteMatrix = Matrix4.Identity;

				// TODO: very busted, need more research
				//spriteMatrix *= Matrix4.CreateScale(currentSprite.ScaleX / 100.0f, currentSprite.ScaleY / 100.0f, 1.0f);

				//spriteMatrix *= Matrix4.CreateTranslation(currentSprite.RotationCenterX, currentSprite.RotationCenterY, 0.0f);
				//spriteMatrix *= Matrix4.CreateRotationZ(-MathHelper.DegreesToRadians(currentSprite.RotationAngle));
				//spriteMatrix *= Matrix4.CreateTranslation(-currentSprite.RotationCenterX, -currentSprite.RotationCenterY, 0.0f);

				//spriteMatrix *= Matrix4.CreateTranslation(currentSprite.Unknown0x14, currentSprite.Unknown0x16, 0.0f);

				spriteMatrix *= Matrix4.CreateTranslation(0.0f, 0.0f, z);
				spriteMatrix *= modelviewMatrix;

				var spriteKey = (setId, currentSprite.SpriteSheetIndex, currentSprite.PaletteIndex, currentSprite.Unknown0x00);
				if (spriteSheetTextureDict.ContainsKey(spriteKey))
				{
					var currentTexture = spriteSheetTextureDict[spriteKey];
					currentTexture.Activate();

					shader.SetUniformMatrix(modelviewMatrixName, false, spriteMatrix);
					shader.SetUniform("sprite_rect", new Vector4(currentSprite.SourceX, currentSprite.SourceY, currentSprite.SourceWidth, currentSprite.SourceHeight));
					shader.SetUniform("sheet_size", new Vector2(currentTexture.Width, currentTexture.Height));

					spriteMesh.Render();
				}
			}
		}

		private Bitmap[][] GetAndCacheSpriteSheets(AnimSet set)
		{
			if (!spriteSheetBitmapDict.ContainsKey(set))
			{
				var bitmaps = new Bitmap[set.SpriteSheets.Length][];
				for (int s = 0; s < set.SpriteSheets.Length; s++)
					bitmaps[s] = set.GetSpriteSheetBitmaps(s);

				spriteSheetBitmapDict.Add(set, bitmaps);
			}
			return spriteSheetBitmapDict[set];
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct AnimSpriteVertex : IVertexStruct
	{
		[VertexElement(AttributeIndex = 0)]
		public Vector4 Position;
		[VertexElement(AttributeIndex = 1)]
		public Vector2 TexCoord;
	}
}
