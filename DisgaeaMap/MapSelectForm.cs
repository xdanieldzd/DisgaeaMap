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
	public partial class MapSelectForm : Form
	{
		static string mapsDirectory = string.Empty;
		static Dictionary<string, (string, string)> knownMaps = new Dictionary<string, (string, string)>();
		static string lastSelectedMap = string.Empty;

		public MapSelectForm()
		{
			InitializeComponent();

			if (mapsDirectory == string.Empty)
			{
				if (Environment.MachineName.ToUpperInvariant() == "RIN-CORE")
				{
					fbdMapsDirectory.SelectedPath = @"D:\Games\PlayStation 2\Disgaea Misc\Output\";
				}

				if (fbdMapsDirectory.ShowDialog() == DialogResult.OK)
				{
					mapsDirectory = fbdMapsDirectory.SelectedPath;
				}
				else
				{
					DialogResult = DialogResult.Cancel;
				}
			}

			if (knownMaps.Count == 0)
			{
				if (Directory.Exists(mapsDirectory))
				{
					var mapDatFiles = Directory.EnumerateFiles(mapsDirectory, "mp*.dat");
					var mapMpdFiles = Directory.EnumerateFiles(mapsDirectory, "mp*.mpd");

					foreach (var mapDat in mapDatFiles)
					{
						var mapMpd = mapMpdFiles.Where(x => x.Contains(Path.GetFileNameWithoutExtension(mapDat)));
						if (mapMpd.Count() == 1)
						{
							knownMaps.Add(Path.GetFileNameWithoutExtension(mapDat), (mapDat, mapMpd.FirstOrDefault()));
						}
					}
				}
			}

			lbMaps.DataSource = knownMaps.ToList();
			lbMaps.DisplayMember = "Key";
			lbMaps.ValueMember = "Value";

			if (lastSelectedMap != string.Empty && knownMaps.ContainsKey(lastSelectedMap))
				lbMaps.SelectedItem = knownMaps.FirstOrDefault(x => x.Key == lastSelectedMap);
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
