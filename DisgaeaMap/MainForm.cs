using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using Cobalt;
using Cobalt.Texture;

using CobaltFont = Cobalt.Font;

using DisgaeaMap.ModelParser;
using DisgaeaMap.MpdParser;
using DisgaeaMap.MiscParsers;
using DisgaeaMap.AnimParser;

namespace DisgaeaMap
{
	public partial class MainForm : Form
	{
		static readonly string projectionMatrixName = "projection_matrix";
		static readonly string modelviewMatrixName = "modelview_matrix";
		static readonly string wireframeName = "wireframe";
		static readonly string geoPanelsName = "geoPanels";

		(string Name, Version Version, string Description, string Copyright) programInfo;

		Vector3 eye;
		float scale;
		Matrix4 modelviewMatrix;

		string mapFilename;
		MapBinary mapBinary;
		string mpdFilename;
		MpdBinary mpdBinary;

		//string faceFilename;
		//FaceBinary faceBinary;

		string mainAnimFilename;
		AnmBinary mainAnimBinary;
		Renderer mainAnimRenderer;
		Dictionary<Actor, Vector3> actorCache;

		bool animTestMode;
		ushort animTestSetId;
		int animTestAnimIdx, animTestAnimIdxMax;
		int animTestWeaponType;

		Camera camera;
		Shader floorShader, objectShader, animShader;
		CobaltFont font;
		Texture emptyTexture;

		OpenTK.Input.KeyboardState lastKbd;
		bool wireframe, geoPanels, textOverlay;

		bool busy, takeShot;

		public MainForm()
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
			System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

			Assembly assembly = Assembly.GetExecutingAssembly();
			programInfo = ((assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false).FirstOrDefault() as AssemblyProductAttribute).Product,
				new Version((assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false).FirstOrDefault() as AssemblyFileVersionAttribute).Version),
				(assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false).FirstOrDefault() as AssemblyDescriptionAttribute).Description,
				(assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false).FirstOrDefault() as AssemblyCopyrightAttribute).Copyright);

			InitializeComponent();
		}

		private T LoadFile<T>(string filename) where T : ParsableData
		{
			busy = true;
			T instance;
			using (System.IO.FileStream file = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
			{
				instance = (T)Activator.CreateInstance(typeof(T), new object[] { file, Cobalt.IO.Endian.LittleEndian });
			}
			busy = false;

			return instance;
		}

		private void openMapToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (var mapSelectForm = new MapSelectForm())
			{
				if (mapSelectForm.DialogResult == DialogResult.None && mapSelectForm.ShowDialog() == DialogResult.OK)
				{
					if (mapSelectForm.Tag is ValueTuple<string, string> map)
					{
						(string dat, string mpd) = map;
						mapBinary = LoadFile<MapBinary>(mapFilename = dat);
						mpdBinary = LoadFile<MpdBinary>(mpdFilename = mpd);
					}
				}
			}
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MessageBox.Show($"{programInfo.Name} v{programInfo.Version.ToString(programInfo.Version.Build != 0 ? 3 : 2)} - {programInfo.Description}\n\n{programInfo.Copyright}\n\nVarious notes from & greetings to the Netherworld Research wiki\nhttps://disgaea.rustedlogic.net/", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void renderControl_Load(object sender, EventArgs e)
		{
			eye = new Vector3(0.0f, 0.0f, -15.0f);
			scale = 0.1f;
			modelviewMatrix = Matrix4.CreateScale(scale) * Matrix4.LookAt(eye, Vector3.Zero, Vector3.UnitY);

			camera = new Camera();
			floorShader = new Shader(System.IO.File.ReadAllText(@"Assets\\FloorVertexShader.glsl"), System.IO.File.ReadAllText(@"Assets\\FloorFragmentShader.glsl"));
			objectShader = new Shader(System.IO.File.ReadAllText(@"Assets\\ObjectVertexShader.glsl"), System.IO.File.ReadAllText(@"Assets\\ObjectFragmentShader.glsl"));
			animShader = new Shader(System.IO.File.ReadAllText(@"Assets\\AnimVertexShader.glsl"), System.IO.File.ReadAllText(@"Assets\\AnimFragmentShader.glsl"));

			try
			{
				font = new CobaltFont("DejaVu Sans");
			}
			catch (ArgumentException)
			{
				font = new CobaltFont(FontType.SansSerif);
			}

			emptyTexture = new Texture(new Bitmap(@"Assets\\EmptyTexture.png"));

			lastKbd = OpenTK.Input.Keyboard.GetState();
			wireframe = false;
			geoPanels = true;
			textOverlay = true;

			mapFilename = string.Empty;
			mpdFilename = string.Empty;
			mainAnimFilename = string.Empty;

			actorCache = new Dictionary<Actor, Vector3>();

			busy = takeShot = false;

			floorShader?.SetUniform("texture0", 0);
			floorShader?.SetUniform("texture1", 1);

			objectShader?.SetUniform("texture", 0);

			animShader?.SetUniform("texture", 0);

			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

			GL.Enable(EnableCap.DepthTest);
			//GL.Enable(EnableCap.CullFace);
			GL.PointSize(5.0f);

#if DEBUG
			if (Environment.MachineName.ToUpperInvariant() == "RIN-CORE")
			{
				var file = string.Empty;

				file = "mp00201";       //blessed court
				file = "mp00202";       //corridor of love
				file = "mp03102";       //UNUSED geoeffects map

				file = "mp00501";       //icy breath
				file = "mp21002";       //rank1 exam/dark assembly
				file = "mp00504";       //forsaken land
				file = "mp00606";       //heart of evil
				file = "mp00603";       //witches den
				file = "mp01302";       //hall of justice

				file = "mp00204";       //magnificent gate
				file = "mp00101";       //tutorial basics1
				file = "mp01604";       //absolute zero
				file = "mp00104";       //practice map

				file = "mp05104";       //dark castle/shop-gate-area

				mapBinary = LoadFile<MapBinary>(mapFilename = $@"D:\Games\PlayStation 2\Disgaea Misc\Output\{file}.dat");
				mpdBinary = LoadFile<MpdBinary>(mpdFilename = $@"D:\Games\PlayStation 2\Disgaea Misc\Output\{file}.mpd");

				//faceBinary = LoadFile<FaceBinary>(faceFilename = @"D:\Games\PlayStation 2\Disgaea Misc\Output\start.dat (Output)\face.dat");

				mainAnimBinary = LoadFile<AnmBinary>(mainAnimFilename = @"D:\Games\PlayStation 2\Disgaea Misc\Output\anm00.dat");
				mainAnimRenderer = new Renderer(mainAnimBinary, animShader, modelviewMatrixName);

				animTestMode = true;

				animTestSetId = 00001; // laharl
				animTestSetId = 00002; // etna
				animTestSetId = 01070; // female samurai
				animTestAnimIdx = 16;

				animTestWeaponType = 6;

				//animTestSetId = 02110; // dragon
				//animTestSetId = 02120; // zombie
				//animTestAnimIdx = 0;

				//animTestSetId = 02210; // prinny
				//animTestAnimIdx = 23;

				//animTestSetId = 04008;
				//animTestAnimIdx = 1;

				var animSet = mainAnimBinary.GetAnimSet(animTestSetId);
				if (animSet != null) animTestAnimIdxMax = animSet.AnimIdentifiers.Length;
			}
#endif
		}

		private void renderControl_Render(object sender, EventArgs e)
		{
			Text = $"{programInfo.Name} v{programInfo.Version.ToString(programInfo.Version.Build != 0 ? 3 : 2)} - [{(mapFilename != string.Empty ? mapFilename : "no dat")}, {(mpdFilename != string.Empty ? mpdFilename : "no mpd")}, {(mainAnimFilename != string.Empty ? mainAnimFilename : "no anim")}]";

			RenderControl renderControl = (sender as RenderControl);

			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

			if (!busy && !takeShot)
			{
				if (ContainsFocus)
				{
					OpenTK.Input.KeyboardState kbdState = OpenTK.Input.Keyboard.GetState();

					if (kbdState[OpenTK.Input.Key.F1] && !lastKbd[OpenTK.Input.Key.F1])
						wireframe = !wireframe;

					if (kbdState[OpenTK.Input.Key.F2] && !lastKbd[OpenTK.Input.Key.F2])
						geoPanels = !geoPanels;

					if (kbdState[OpenTK.Input.Key.F6] && !lastKbd[OpenTK.Input.Key.F6])
						animTestMode = !animTestMode;

					if (kbdState[OpenTK.Input.Key.F10] && !lastKbd[OpenTK.Input.Key.F10])
						renderControl.VSync = !renderControl.VSync;

					if (kbdState[OpenTK.Input.Key.F11] && !lastKbd[OpenTK.Input.Key.F11])
						textOverlay = !textOverlay;

					if (kbdState[OpenTK.Input.Key.F12] && !lastKbd[OpenTK.Input.Key.F12])
						busy = takeShot = true;

					if (kbdState[OpenTK.Input.Key.KeypadPlus] && !lastKbd[OpenTK.Input.Key.KeypadPlus])
					{
						animTestAnimIdx++;
						if (animTestAnimIdx >= animTestAnimIdxMax) animTestAnimIdx = 0;
					}

					if (kbdState[OpenTK.Input.Key.KeypadMinus] && !lastKbd[OpenTK.Input.Key.KeypadMinus])
					{
						animTestAnimIdx--;
						if (animTestAnimIdx < 0) animTestAnimIdx = (animTestAnimIdxMax - 1);
					}

					if (kbdState[OpenTK.Input.Key.PageUp] && !lastKbd[OpenTK.Input.Key.PageUp])
					{
						animTestWeaponType--;
						if (animTestWeaponType < 1) animTestWeaponType = 7;
					}

					if (kbdState[OpenTK.Input.Key.PageDown] && !lastKbd[OpenTK.Input.Key.PageDown])
					{
						animTestWeaponType++;
						if (animTestWeaponType > 7) animTestWeaponType = 1;
					}

					lastKbd = kbdState;

					if (renderControl.ClientRectangle.Contains(renderControl.PointToClient(Cursor.Position)) &&
						!menuStrip1.Items.Cast<ToolStripMenuItem>().Any(x => x.DropDown.Visible))
						camera.Update(Core.DeltaTime);
				}
			}

			emptyTexture?.Activate();

			if (animTestMode)
			{
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
				animShader.SetUniform(wireframeName, 0);
				RenderAnimTest(animTestSetId, animTestAnimIdx, animTestWeaponType);

				if (wireframe)
				{
					GL.Enable(EnableCap.PolygonOffsetLine);
					GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
					GL.PolygonOffset(-1.0f, 1.0f);
					animShader.SetUniform(wireframeName, 1);
					RenderAnimTest(animTestSetId, animTestAnimIdx, animTestWeaponType);

					GL.Disable(EnableCap.PolygonOffsetLine);
					GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
				}
			}
			else
			{
				RenderScene();
				ProcessScreenshot();
			}
			RenderHUD();
		}

		private void RenderAnimTest(ushort setId, int animIdx, int weaponType)
		{
			Matrix4 tempMatrix = modelviewMatrix * camera.GetViewMatrix();
			mainAnimRenderer?.Render(tempMatrix, setId, animIdx, weaponType);
		}

		private void RenderScene()
		{
			Matrix4 tempMatrix = Matrix4.Identity;
			if (true)
			{
				tempMatrix = modelviewMatrix * camera.GetViewMatrix();
			}
			else
			{
				/*float factor = 12.0f;
				Vector3 lookat = new Vector3(1.0f, -1.0f, -1.0f);
				Vector3 pos = new Vector3(0 * factor, 6 * factor, 12 * factor);
				pos += new Vector3(mpdBinary.Chunks[0].MapOffsetX, 0.0f, mpdBinary.Chunks[0].MapOffsetZ);
				tempMatrix = Matrix4.LookAt(pos, pos + lookat, Vector3.UnitY);*/

				float aspectRatio = (renderControl.ClientRectangle.Width / (float)(renderControl.ClientRectangle.Height));
				//objectShader?.SetUniformMatrix(projectionMatrixName, false, Matrix4.CreateOrthographicOffCenter(-75.0f, 75.0f, -75.0f, 75.0f, -1000.0f, 1000.0f));
				floorShader?.SetUniformMatrix(projectionMatrixName, false, Matrix4.CreateOrthographicOffCenter(-64.0f, 64.0f, -58.0f, 58.0f, -1000.0f, 1000.0f));

				tempMatrix *= Matrix4.CreateTranslation(0.0f, -150.0f, -250.0f);

				//tempMatrix *= Matrix4.CreateRotationY(45.0f);
				tempMatrix *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(45.0f));

				tempMatrix *= Matrix4.CreateRotationX((float)(180.0f / Math.PI * Math.Atan(1.0 / Math.Sqrt(2.0))));
				//tempMatrix *= Matrix4.CreateRotationX(35.254f);

				tempMatrix *= Matrix4.CreateScale(1.0f, -1.0f, -1.0f);
			}

			if (objectShader != null)
			{
				objectShader.Activate();
				objectShader.SetUniformMatrix(modelviewMatrixName, false, tempMatrix);

				mpdBinary.RenderObjects(mapBinary.MeshSets, objectShader);
			}

			if (floorShader != null)
			{
				floorShader.Activate();
				floorShader.SetUniformMatrix(modelviewMatrixName, false, tempMatrix);
				floorShader.SetUniform(geoPanelsName, geoPanels ? 1 : 0);

				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
				floorShader.SetUniform(wireframeName, 0);
				mpdBinary?.RenderFloor(mapBinary, floorShader);

				if (wireframe)
				{
					GL.Enable(EnableCap.PolygonOffsetLine);
					GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
					GL.PolygonOffset(-1.0f, 1.0f);
					floorShader.SetUniform(wireframeName, 1);
					mpdBinary?.RenderFloor(mapBinary, floorShader);

					GL.Disable(EnableCap.PolygonOffsetLine);
					GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
				}
			}

			if (animShader != null)
			{
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
				animShader.SetUniform(wireframeName, 0);

				foreach (var actor in mpdBinary.Actors)
				{
					if (!actorCache.ContainsKey(actor))
					{
						var x = -(actor.X * 12.0f);
						var y = 0.0f;
						var z = (actor.Z * 12.0f);

						foreach (var chunk in mpdBinary.Chunks)
						{
							foreach (var tile in mpdBinary.Tiles[chunk])
							{
								var tx = (((chunk.MapOffsetX - 6.0f) / 12.0f) + tile.XCoordinate);
								var tz = (((chunk.MapOffsetZ - 6.0f) / 12.0f) + tile.ZCoordinate);

								if (tx == actor.X && tz == actor.Z)
								{
									y = -((tile.TopVertexNE + tile.TopVertexNW + tile.TopVertexSE + tile.TopVertexSW) / 4) + 0.1f;
									break;
								}
							}
						}

						actorCache.Add(actor, new Vector3(x, y, z));
					}
				}

				GL.DepthMask(false);
				foreach (var actor in mpdBinary.Actors)
				{
					var position = actorCache[actor];
					animShader.SetUniform("grid_position", position);
					mainAnimRenderer.Render(modelviewMatrix * camera.GetViewMatrix(), actor.ID, 1, 1);
				}

				GL.DepthMask(true);
				foreach (var actor in mpdBinary.Actors)
				{
					var position = actorCache[actor];
					animShader.SetUniform("grid_position", position);
					mainAnimRenderer.Render(modelviewMatrix * camera.GetViewMatrix(), actor.ID, 1, 1);
				}
			}
		}

		private void ProcessScreenshot()
		{
			if (takeShot)
			{
				using (Bitmap screenshot = renderControl.GrabScreenshot())
				{
					string directory = System.IO.Path.GetDirectoryName(mapFilename);
					string shotfile;
					int idx = -1;
					do
					{
						idx++;
						shotfile = System.IO.Path.Combine(directory, $"shot{idx:D2}.png");
					} while (System.IO.File.Exists(shotfile));
					screenshot.Save(shotfile);
				}
				GL.Disable(EnableCap.AlphaTest);
				GL.Disable(EnableCap.Blend);

				busy = false;
				takeShot = false;
			}
		}

		private void RenderHUD()
		{
			if (font != null && textOverlay && !takeShot)
			{
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

				StringBuilder builder = new StringBuilder();
				builder.Append($"Cobalt v{Core.LibraryVersion} - OpenTK v{Core.OpenTKVersion}\n");
				builder.Append($"{Core.CurrentFramesPerSecond:0.00} FPS\n");
				builder.AppendLine();
				if (!animTestMode)
				{
					builder.Append($"F1: Wireframe ({wireframe})\n");
					builder.Append($"F2: Geo Panels ({geoPanels})\n");
				}
				else
				{
					builder.Append($"Animation Set {animTestSetId:D5}\n");
					builder.Append($"Animation {animTestAnimIdx + 1}/{animTestAnimIdxMax}\n");
					builder.Append($"Weapon Type {animTestWeaponType}\n");
					builder.AppendLine();
					builder.Append($"F1: Wireframe ({wireframe})\n");
					builder.Append("Numpad -: Prev Animation\n");
					builder.Append("Numpad +: Next Animation\n");
					builder.Append("Page Up: Prev Weapon Type\n");
					builder.Append("Page Down: Next Weapon Type\n");
				}
				builder.AppendLine();
				builder.Append($"F6: Anim Test Mode ({animTestMode})\n");
				builder.AppendLine();
				builder.Append($"F10: Vsync ({renderControl.VSync})\n");
				builder.Append($"F11: Show HUD ({textOverlay})\n");
				builder.Append($"F12: Screenshot");

				font.DrawString(8.0f, 8.0f, builder.ToString());
			}
		}

		private void renderControl_Resize(object sender, EventArgs e)
		{
			RenderControl renderControl = (sender as RenderControl);
			GL.Viewport(0, 0, renderControl.Width, renderControl.Height);

			float aspectRatio = (renderControl.ClientRectangle.Width / (float)(renderControl.ClientRectangle.Height));
			float fovy = MathHelper.Pi / 16.0f;
			fovy = MathHelper.PiOver4;

			objectShader?.SetUniformMatrix(projectionMatrixName, false, Matrix4.CreatePerspectiveFieldOfView(fovy, aspectRatio, 0.1f, 15000.0f));
			floorShader?.SetUniformMatrix(projectionMatrixName, false, Matrix4.CreatePerspectiveFieldOfView(fovy, aspectRatio, 0.1f, 15000.0f));
			animShader?.SetUniformMatrix(projectionMatrixName, false, Matrix4.CreatePerspectiveFieldOfView(fovy, aspectRatio, 0.1f, 15000.0f));

			if (font != null)
				font.SetScreenSize(renderControl.ClientRectangle.Width, renderControl.ClientRectangle.Height);
		}
	}
}
