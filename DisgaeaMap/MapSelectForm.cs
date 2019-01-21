using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace DisgaeaMap
{
	// TODO: all this is pretty ugly, tho it'll work for now

	public partial class MapSelectForm : Form
	{
		// EO4 stuffs copypasta
		readonly static Encoding shiftJisEncoding = Encoding.GetEncoding(932);
		readonly static Dictionary<char, char> shiftJisToAscii = new Dictionary<char, char>()
		{
			{ '　', ' ' }, { '，', ',' }, { '．', '.' }, { '：', ':' }, { '；', ';' }, { '？', '?' }, { '！', '!' }, { '－', '-' },
			{ '／', '/' }, { '～', '~' }, { '’', '\'' }, { '”', '\"' }, { '（', '(' }, { '）', ')' }, { '［', '[' }, { '］', ']' },
			{ '〈', '<' }, { '〉', '>' }, { '＋', '+' }, { '＊', '*' }, { '＆', '&' }, { '％', '%' },

			{ '０', '0' }, { '１', '1' }, { '２', '2' }, { '３', '3' }, { '４', '4' }, { '５', '5' }, { '６', '6' }, { '７', '7' },
			{ '８', '8' }, { '９', '9' },

			{ 'Ａ', 'A' }, { 'Ｂ', 'B' }, { 'Ｃ', 'C' }, { 'Ｄ', 'D' }, { 'Ｅ', 'E' }, { 'Ｆ', 'F' }, { 'Ｇ', 'G' }, { 'Ｈ', 'H' },
			{ 'Ｉ', 'I' }, { 'Ｊ', 'J' }, { 'Ｋ', 'K' }, { 'Ｌ', 'L' }, { 'Ｍ', 'M' }, { 'Ｎ', 'N' }, { 'Ｏ', 'O' }, { 'Ｐ', 'P' },
			{ 'Ｑ', 'Q' }, { 'Ｒ', 'R' }, { 'Ｓ', 'S' }, { 'Ｔ', 'T' }, { 'Ｕ', 'U' }, { 'Ｖ', 'V' }, { 'Ｗ', 'W' }, { 'Ｘ', 'X' },
			{ 'Ｙ', 'Y' }, { 'Ｚ', 'Z' },

			{ 'ａ', 'a' }, { 'ｂ', 'b' }, { 'ｃ', 'c' }, { 'ｄ', 'd' }, { 'ｅ', 'e' }, { 'ｆ', 'f' }, { 'ｇ', 'g' }, { 'ｈ', 'h' },
			{ 'ｉ', 'i' }, { 'ｊ', 'j' }, { 'ｋ', 'k' }, { 'ｌ', 'l' }, { 'ｍ', 'm' }, { 'ｎ', 'n' }, { 'ｏ', 'o' }, { 'ｐ', 'p' },
			{ 'ｑ', 'q' }, { 'ｒ', 'r' }, { 'ｓ', 's' }, { 'ｔ', 't' }, { 'ｕ', 'u' }, { 'ｖ', 'v' }, { 'ｗ', 'w' }, { 'ｘ', 'x' },
			{ 'ｙ', 'y' }, { 'ｚ', 'z' },
		};

		const string startDatName = "start.dat";
		const string dungeonDatName = "dungeon.dat";

		static string dataDirectory = string.Empty;
		static Dictionary<string, (string, string)> validMaps = new Dictionary<string, (string, string)>();
		static string lastSelectedMap = string.Empty;

		public MapSelectForm()
		{
			InitializeComponent();

			if (dataDirectory == string.Empty)
			{
				if (Environment.MachineName.ToUpperInvariant() == "RIN-CORE")
				{
					fbdMapsDirectory.SelectedPath = @"D:\Games\PlayStation 2\Disgaea Misc\Output\";
				}

				if (fbdMapsDirectory.ShowDialog() == DialogResult.OK)
				{
					dataDirectory = fbdMapsDirectory.SelectedPath;
				}
				else
				{
					DialogResult = DialogResult.Cancel;
				}
			}

			if (validMaps.Count == 0)
			{
				var dungeonDatData = GetDungeonData();
				if (dungeonDatData != null)
				{
					var maps = ParseDungeonData(dungeonDatData);

					foreach (var (mapName, mapId) in maps)
					{
						var idString = $"mp{mapId:D5}";
						var mapMpd = Path.Combine(dataDirectory, $"{idString}.mpd");
						var mapDat = Path.Combine(dataDirectory, $"{idString}.dat");

						if (File.Exists(mapMpd) && File.Exists(mapDat))
						{
							var mapNameAscii = new StringBuilder();
							foreach (var chr in mapName)
								mapNameAscii.Append(shiftJisToAscii.ContainsKey(chr) ? shiftJisToAscii[chr] : chr);

							validMaps.Add($"{mapNameAscii} ({idString})", (mapDat, mapMpd));
						}
					}

					var mapDatFiles = Directory.EnumerateFiles(dataDirectory, "mp*.dat");
					var mapMpdFiles = Directory.EnumerateFiles(dataDirectory, "mp*.mpd");

					foreach (var mapDat in mapDatFiles.Where(x => !validMaps.Any(y => y.Key.Contains(Path.GetFileNameWithoutExtension(x)))))
					{
						var mapMpd = mapMpdFiles.Where(x => x.Contains(Path.GetFileNameWithoutExtension(mapDat)));
						if (mapMpd.Count() == 1)
						{
							validMaps.Add(Path.GetFileNameWithoutExtension(mapDat), (mapDat, mapMpd.FirstOrDefault()));
						}
					}
				}
			}

			lbMaps.DataSource = validMaps.ToList();
			lbMaps.DisplayMember = "Key";
			lbMaps.ValueMember = "Value";

			if (lastSelectedMap != string.Empty)
				lbMaps.SelectedItem = validMaps.FirstOrDefault(x => x.Key == lastSelectedMap);
		}

		private byte[] GetDungeonData()
		{
			byte[] dungeonDatData = null;

			string startDatPath = Path.Combine(dataDirectory, startDatName);
			if (File.Exists(startDatPath))
			{
				using (var datReader = new BinaryReader(new FileInfo(startDatPath).Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
				{
					var list = new List<(uint, string)>();
					var numFiles = datReader.ReadUInt64();
					datReader.BaseStream.Seek(8, SeekOrigin.Current);
					for (ulong num = 0; num < numFiles; num++)
						list.Add((datReader.ReadUInt32(), Encoding.GetEncoding("SJIS").GetString(datReader.ReadBytes(28)).TrimEnd('\0')));

					var dataStartOffset = datReader.BaseStream.Position;
					var startOffset = dataStartOffset;
					foreach (var (endOffset, name) in list)
					{
						datReader.BaseStream.Seek(startOffset, SeekOrigin.Begin);
						if (name == dungeonDatName)
						{
							dungeonDatData = datReader.ReadBytes((int)(endOffset - startOffset + dataStartOffset));
							break;
						}
						startOffset = dataStartOffset + endOffset;
					}
				}
			}

			return dungeonDatData;
		}

		private List<(string, ushort)> ParseDungeonData(byte[] dungeonDatData)
		{
			var maps = new List<(string, ushort)>();

			using (var reader = new BinaryReader(new MemoryStream(dungeonDatData)))
			{
				var numMaps = reader.ReadUInt32();
				var numMapsCopy = reader.ReadUInt32();
				for (int i = 0; i < numMaps; i++)
				{
					var mapName = Encoding.GetEncoding("SJIS").GetString(reader.ReadBytes(32)).TrimEnd('\0');
					reader.BaseStream.Seek(2, SeekOrigin.Current);
					var mapIdPrimary = reader.ReadUInt16();
					var mapIdSecondary = reader.ReadUInt16();
					reader.BaseStream.Seek(22, SeekOrigin.Current);

					maps.Add((mapName, mapIdPrimary));
				}
			}

			return maps;
		}

		private void btnOpen_Click(object sender, EventArgs e)
		{
			if (lbMaps.SelectedItem is KeyValuePair<string, (string, string)> map)
				OpenMap(map);
		}

		private void lbMaps_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			var index = (sender as ListBox).IndexFromPoint(e.Location);
			if (index != ListBox.NoMatches)
			{
				if (lbMaps.Items[index] is KeyValuePair<string, (string, string)> map)
					OpenMap(map);
			}
		}

		private void OpenMap(KeyValuePair<string, (string, string)> map)
		{
			lastSelectedMap = map.Key;
			Tag = map.Value;
			DialogResult = DialogResult.OK;
		}
	}
}
