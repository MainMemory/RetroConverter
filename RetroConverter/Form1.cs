using SonicRetro.SonLVL.API;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using BGLayout3 = RSDKv3.BGLayout;
using Gameconfig3 = RSDKv3.Gameconfig;
using Object3 = RSDKv3.Object;
using PaletteColour3 = RSDKv3.PaletteColour;
using Scene3 = RSDKv3.Scene;
using Stageconfig3 = RSDKv3.Stageconfig;
using Tileconfig3 = RSDKv3.Tileconfig;
using Tiles128x1283 = RSDKv3.Tiles128x128;
using BGLayout4 = RSDKv4.BGLayout;
using Gameconfig4 = RSDKv4.Gameconfig;
using Object4 = RSDKv4.Object;
using PaletteColour4 = RSDKv4.PaletteColour;
using Scene4 = RSDKv4.Scene;
using Stageconfig4 = RSDKv4.Stageconfig;
using Tileconfig4 = RSDKv4.Tileconfig;
using Tiles128x1284 = RSDKv4.Tiles128x128;
using PaletteColor5 = RSDKv5.PaletteColor;
using Scene5 = RSDKv5.Scene;
using SceneLayer5 = RSDKv5.SceneLayer;
using ScrollInfo5 = RSDKv5.ScrollInfo;
using StageConfig5 = RSDKv5.StageConfig;
using Tileconfig5 = RSDKv5.Tileconfig;

namespace RetroConverter
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		static readonly Regex actregex = new Regex("Act ([0-9]+)", RegexOptions.Compiled);
		Settings settings = new Settings();
		string settingsPath;
		List<string> Levels;
		string gameConfig = null;

		private void Form1_Load(object sender, EventArgs e)
		{
			versionSelector.SelectedIndex = 0;
			settingsPath = Path.Combine(Directory.GetCurrentDirectory(), "Settings.ini");
			if (File.Exists(settingsPath))
			{
				settings = IniSerializer.Deserialize<Settings>(settingsPath);
				versionSelector.SelectedIndex = Math.Min(Math.Max(settings.RSDKVer - 3, 0), versionSelector.Items.Count);
				titleCardScriptBox.AutoCompleteCustomSource.AddRange(settings.TitleCards.ToArray());
			}
		}

		private void Form1_FormClosed(object sender, FormClosedEventArgs e)
		{
			settings.RSDKVer = versionSelector.SelectedIndex + 3;
			IniSerializer.Serialize(settings, settingsPath);
		}

		private void loadProjectButton_Click(object sender, EventArgs e)
		{
			if (openProjectDialog.ShowDialog(this) == DialogResult.OK)
				LoadProject(openProjectDialog.FileName);
		}

		private void loadProjectButton_ShowMenu(object sender, EventArgs e)
		{
			if (settings.Projects.Count > 0)
			{
				mruMenuStrip.Items.Clear();
				foreach (var item in settings.Projects)
				{
					string str = item;
					if (str.Length > 60)
						str = "…" + str.Substring(item.Length - 59);
					mruMenuStrip.Items.Add(str, null, (s, e_) => LoadProject(item));
				}
				mruMenuStrip.Show(loadProjectButton, 0, loadProjectButton.Height);
			}
		}

		private void LoadProject(string fileName)
		{
			LevelData.LoadGame(fileName);
			levelSelector.BeginUpdate();
			levelSelector.Items.Clear();
			Levels = new List<string>();
			foreach (KeyValuePair<string, LevelInfo> item in LevelData.Game.Levels)
			{
				Levels.Add(item.Key);
				levelSelector.Items.Add(LevelData.Game.GetLevelInfo(item.Key).DisplayName);
			}
			levelSelector.EndUpdate();
			settings.Projects.Remove(fileName);
			settings.Projects.Insert(0, fileName);
		}

		private void levelSelector_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (versionSelector.SelectedIndex == 3)
				convertRSDKButton.Enabled = levelSelector.SelectedIndex > -1;
			else
				convertRSDKButton.Enabled = levelSelector.SelectedIndex > -1 && gameConfig != null;
		}

		private void versionSelector_SelectedIndexChanged(object sender, EventArgs e)
		{
			titleCardScriptBox.Enabled = titleCardScriptButton.Enabled = versionSelector.SelectedIndex == 0;
			if (versionSelector.SelectedIndex == 3)
			{
				loadGameConfigButton.Enabled = false;
				if (levelSelector.SelectedIndex > -1) convertRSDKButton.Enabled = true;
			}
			else
			{
				loadGameConfigButton.Enabled = true;
				convertRSDKButton.Enabled = false;
			}
			gameConfig = null;
		}

		private void loadGameConfigButton_Click(object sender, EventArgs e)
		{
			if (openGameConfigDialog.ShowDialog(this) == DialogResult.OK)
				LoadGameConfig(openGameConfigDialog.FileName);
		}

		private void loadGameConfigButton_ShowMenu(object sender, EventArgs e)
		{
			if (settings.GameConfigs.Count > 0)
			{
				mruMenuStrip.Items.Clear();
				foreach (var item in settings.GameConfigs)
				{
					string str = item;
					if (str.Length > 60)
						str = "…" + str.Substring(item.Length - 59);
					mruMenuStrip.Items.Add(str, null, (s, e_) => LoadGameConfig(item));
				}
				mruMenuStrip.Show(loadProjectButton, 0, loadGameConfigButton.Height);
			}
		}

		private void LoadGameConfig(string fileName)
		{
			gameConfig = fileName;
			convertRSDKButton.Enabled = levelSelector.SelectedIndex > -1;
			settings.GameConfigs.Remove(gameConfig);
			settings.GameConfigs.Insert(0, gameConfig);
		}

		private void titleCardScriptButton_Click(object sender, EventArgs e)
		{
			if (titleCardScriptDialog.ShowDialog(this) == DialogResult.OK)
			{
				if (titleCardScriptDialog.FileName.Contains("\\Scripts\\"))
					titleCardScriptBox.Text = titleCardScriptDialog.FileName.Substring(titleCardScriptDialog.FileName.LastIndexOf("\\Scripts\\") + 9).Replace('\\', '/');
			}
		}

		private void convertRSDKButton_Click(object sender, EventArgs e)
		{
			rsdkExportDialog.SelectedPath = Environment.CurrentDirectory;
			if (rsdkExportDialog.ShowDialog(this) == DialogResult.OK)
			{
				switch (versionSelector.SelectedIndex)
				{
					case 0: // v3
						{
							Gameconfig3 gc = new Gameconfig3(gameConfig);
							List<string> objNames = gc.ObjectsNames;
							LevelData.LoadLevel(Levels[levelSelector.SelectedIndex], true);
							Stageconfig3 cfg = new Stageconfig3() { LoadGlobalScripts = true };
							for (int l = 0; l < 2; l++)
								for (int c = 0; c < 16; c++)
									cfg.StagePalette.Colors[l][c] = new PaletteColour3(LevelData.Palette[0][l + 2, c].R, LevelData.Palette[0][l + 2, c].G, LevelData.Palette[0][l + 2, c].B);
							if (!string.IsNullOrWhiteSpace(titleCardScriptBox.Text))
							{
								settings.TitleCards.Remove(titleCardScriptBox.Text);
								settings.TitleCards.Insert(0, titleCardScriptBox.Text);
								titleCardScriptBox.AutoCompleteCustomSource.AddRange(settings.TitleCards.ToArray());
								objNames.Add(Path.GetFileNameWithoutExtension(titleCardScriptBox.Text));
								cfg.ScriptPaths.Add(titleCardScriptBox.Text);
							}
							int ringid = objNames.IndexOf("Ring");
							List<string> newobjs = new List<string>();
							Dictionary<byte, byte> objMap = new Dictionary<byte, byte>();
							if (includeObjectsCheckBox.Checked)
							{
								if (LevelData.RingFormat is RingLayoutFormat && ringid == -1)
								{
									ringid = objNames.Count + 1;
									objNames.Add("Ring");
								}
								foreach (var id in LevelData.Objects.Select(a => a.ID).Distinct().OrderBy(a => a))
								{
									string name;
									if (LevelData.ObjTypes.TryGetValue(id, out ObjectDefinition def))
										name = def.Name;
									else
										name = $"Unk{id:X2}";
									int newid = objNames.IndexOf(name) + 1;
									if (newid == 0)
									{
										newobjs.Add(name);
										objNames.Add(name);
										newid = objNames.Count;
									}
									objMap[id] = (byte)newid;
								}
							}
							cfg.ObjectsNames = newobjs;
							foreach (string name in newobjs)
								cfg.ScriptPaths.Add(LevelData.Level.DisplayName + "\\" + name + ".txt");
							if (!string.IsNullOrWhiteSpace(titleCardScriptBox.Text))
								newobjs.Insert(0, Path.GetFileNameWithoutExtension(titleCardScriptBox.Text));
							cfg.Write(Path.Combine(rsdkExportDialog.SelectedPath, "StageConfig.bin"));
							List<ushort> bgchunks = LevelData.Layout.BGLayout.OfType<ushort>().Distinct().ToList();
							List<ushort> bgblocks = bgchunks.Select(a => LevelData.Chunks[a]).Where(b => b != null).SelectMany(c => c.Blocks.OfType<ChunkBlock>().Select(d => d.Block)).Distinct().ToList();
							BitmapBits blocks = new BitmapBits(16, 16384);
							for (ushort i = 0; i < LevelData.CompBlockBmpBits.Count; i++)
							{
								LevelData.CompBlockBmpBits[i].IncrementIndexes(0x80);
								if (i != 0 && bgblocks.Contains(i))
									LevelData.CompBlockBmpBits[i].ReplaceColor(0, 0xA0);
								blocks.DrawBitmap(LevelData.CompBlockBmpBits[i], 0, i * 16);
							}
							blocks.FillRectangle(0xA0, 0, 16368, 16, 16);
							Color[] pal = new Color[256];
							pal.Fill(Color.Fuchsia);
							for (int l = 0; l < 4; l++)
								for (int c = 0; c < 16; c++)
									pal[0x80 + (l * 16) + c] = LevelData.Palette[0][l, c].RGBColor;
							using (Bitmap bmp = blocks.ToBitmap(pal))
								bmp.Save(Path.Combine(rsdkExportDialog.SelectedPath, "16x16Tiles.gif"), System.Drawing.Imaging.ImageFormat.Gif);
							foreach (ushort c in bgchunks)
							{
								if (c == 0) continue;
								Chunk ch = LevelData.Chunks[c];
								if (ch == null) continue;
								for (int y = 0; y < LevelData.Level.ChunkHeight / 16; y++)
									for (int x = 0; x < LevelData.Level.ChunkWidth / 16; x++)
										if (ch.Blocks[x, y].Block == 0)
											ch.Blocks[x, y].Block = 0x3FF;
							}
							List<Chunk> tmpchnk = ConvertChunks();
							if (bgchunks.Contains(0))
							{
								for (int y = 0; y < LevelData.BGHeight; y++)
									for (int x = 0; x < LevelData.BGWidth; x++)
										if (LevelData.Layout.BGLayout[x, y] == 0)
											LevelData.Layout.BGLayout[x, y] = (ushort)tmpchnk.Count;
								Chunk ch = new Chunk();
								for (int y = 0; y < 8; y++)
									for (int x = 0; x < 8; x++)
										ch.Blocks[x, y].Block = 0x3FF;
								tmpchnk.Add(ch);
							}
							Tileconfig3 col = new Tileconfig3();
							for (int i = 0; i < LevelData.ColInds1.Count; i++)
							{
								col.CollisionPath1[i].isCeiling = LevelData.ColArr1[LevelData.ColInds1[i]].Where(a => a != 16).Sum(a => Math.Sign(a)) < 0;
								col.CollisionPath1[i].FloorAngle = LevelData.Angles[LevelData.ColInds1[i]];
								if (col.CollisionPath1[i].FloorAngle == 0xFF)
									col.CollisionPath1[i].FloorAngle = 0;
								col.CollisionPath1[i].LWallAngle = (byte)(col.CollisionPath1[i].FloorAngle + 0x40);
								col.CollisionPath1[i].CeilingAngle = (byte)(col.CollisionPath1[i].FloorAngle + 0x80);
								col.CollisionPath1[i].RWallAngle = (byte)(col.CollisionPath1[i].FloorAngle + 0xC0);
								for (int j = 0; j < 16; j++)
									switch (Math.Sign(LevelData.ColArr1[LevelData.ColInds1[i]][j]))
									{
										case 1:
											col.CollisionPath1[i].HasCollision[j] = true;
											if (col.CollisionPath1[i].isCeiling && LevelData.ColArr1[LevelData.ColInds1[i]][j] == 16)
												col.CollisionPath1[i].Collision[j] = 15;
											else
												col.CollisionPath1[i].Collision[j] = (byte)(16 - LevelData.ColArr1[LevelData.ColInds1[i]][j]);
											break;
										case -1:
											col.CollisionPath1[i].HasCollision[j] = true;
											col.CollisionPath1[i].Collision[j] = (byte)(-1 - LevelData.ColArr1[LevelData.ColInds1[i]][j]);
											break;
									}
								col.CollisionPath2[i].FloorAngle = LevelData.Angles[LevelData.ColInds2[i]];
								col.CollisionPath2[i].isCeiling = LevelData.ColArr1[LevelData.ColInds2[i]].Sum(a => Math.Sign(a)) < 0;
								if (col.CollisionPath2[i].FloorAngle == 0xFF)
									col.CollisionPath2[i].FloorAngle = 0;
								col.CollisionPath2[i].LWallAngle = (byte)(col.CollisionPath2[i].FloorAngle + 0x40);
								col.CollisionPath2[i].CeilingAngle = (byte)(col.CollisionPath2[i].FloorAngle + 0x80);
								col.CollisionPath2[i].RWallAngle = (byte)(col.CollisionPath2[i].FloorAngle + 0xC0);
								for (int j = 0; j < 16; j++)
									switch (Math.Sign(LevelData.ColArr1[LevelData.ColInds2[i]][j]))
									{
										case 1:
											col.CollisionPath2[i].HasCollision[j] = true;
											if (col.CollisionPath2[i].isCeiling && LevelData.ColArr1[LevelData.ColInds2[i]][j] == 16)
												col.CollisionPath2[i].Collision[j] = 15;
											else
												col.CollisionPath2[i].Collision[j] = (byte)(16 - LevelData.ColArr1[LevelData.ColInds2[i]][j]);
											break;
										case -1:
											col.CollisionPath2[i].HasCollision[j] = true;
											col.CollisionPath2[i].Collision[j] = (byte)(-1 - LevelData.ColArr1[LevelData.ColInds2[i]][j]);
											break;
									}
							}
							col.Write(Path.Combine(rsdkExportDialog.SelectedPath, "CollisionMasks.bin"));
							BGLayout3 bg = new BGLayout3();
							bg.HLines.Add(new BGLayout3.ScrollInfo() { RelativeSpeed = 0x100 });
							bg.Layers.Add(new BGLayout3.BGLayer((byte)LevelData.BGWidth, (byte)LevelData.BGHeight) { Behaviour = 1 });
							for (int y = 0; y < LevelData.BGHeight; y++)
								for (int x = 0; x < LevelData.BGWidth; x++)
									bg.Layers[0].MapLayout[y][x] = LevelData.Layout.BGLayout[x, y];
							bg.Write(Path.Combine(rsdkExportDialog.SelectedPath, "Backgrounds.bin"));
							Tiles128x1283 chunks = new Tiles128x1283();
							for (int i = 0; i < tmpchnk.Count; i++)
								if (tmpchnk[i] != null)
								{
									for (int y = 0; y < 8; y++)
										for (int x = 0; x < 8; x++)
										{
											chunks.BlockList[i].Mapping[y][x].Tile16x16 = tmpchnk[i].Blocks[x, y].Block;
											if (tmpchnk[i].Blocks[x, y].XFlip && tmpchnk[i].Blocks[x, y].YFlip)
												chunks.BlockList[i].Mapping[y][x].FlipType = Tiles128x1283.Tile128.Tile16.FlipTypes.FLIPXY;
											else if (tmpchnk[i].Blocks[x, y].YFlip)
												chunks.BlockList[i].Mapping[y][x].FlipType = Tiles128x1283.Tile128.Tile16.FlipTypes.FLIPY;
											else if (tmpchnk[i].Blocks[x, y].XFlip)
												chunks.BlockList[i].Mapping[y][x].FlipType = Tiles128x1283.Tile128.Tile16.FlipTypes.FLIPX;
											chunks.BlockList[i].Mapping[y][x].CollisionFlag0 = SolidMap[(byte)tmpchnk[i].Blocks[x, y].Solid1];
											chunks.BlockList[i].Mapping[y][x].CollisionFlag1 = SolidMap[(byte)((S2ChunkBlock)tmpchnk[i].Blocks[x, y]).Solid2];
											Block blk = LevelData.Blocks[tmpchnk[i].Blocks[x, y].Block];
											if (blk != null && blk.Tiles.OfType<PatternIndex>().Count(a => a.Priority) > 1)
												chunks.BlockList[i].Mapping[y][x].VisualPlane = 1;
										}
								}
							chunks.Write(Path.Combine(rsdkExportDialog.SelectedPath, "128x128Tiles.bin"));
							Scene3 scene = new Scene3
							{
								Title = LevelData.Level.DisplayName,
								width = (ushort)LevelData.FGWidth,
								height = (ushort)LevelData.FGHeight,
								MapLayout = new ushort[LevelData.FGHeight][]
							};
							for (int y = 0; y < LevelData.FGHeight; y++)
							{
								scene.MapLayout[y] = new ushort[LevelData.FGWidth];
								for (int x = 0; x < LevelData.FGWidth; x++)
									scene.MapLayout[y][x] = LevelData.Layout.FGLayout[x, y];
							}
							scene.objectTypeNames = objNames;
							if (LevelData.StartPositions.Count > 0)
								scene.objects.Add(new Object3((byte)(objNames.IndexOf("Player Object") + 1), 0, (short)LevelData.StartPositions[0].X, (short)LevelData.StartPositions[0].Y));
							if (includeObjectsCheckBox.Checked)
								foreach (Entry ent in LevelData.Objects.Cast<Entry>().Concat(LevelData.Rings).OrderBy(a => a))
									switch (ent)
									{
										case ObjectEntry obj:
											scene.objects.Add(new Object3(objMap[obj.ID], obj.SubType, (short)obj.X, (short)obj.Y));
											break;
										case SonicRetro.SonLVL.API.S2.S2RingEntry rng2:
											Point rpos = new Point(rng2.X, rng2.Y);
											for (int r = 0; r < rng2.Count; r++)
											{
												scene.objects.Add(new Object3((byte)ringid, 0, (short)rpos.X, (short)rpos.Y));
												switch (rng2.Direction)
												{
													case Direction.Horizontal:
														rpos.X += 0x18;
														break;
													case Direction.Vertical:
														rpos.Y += 0x18;
														break;
												}
											}
											break;
										case RingEntry rng:
											scene.objects.Add(new Object3((byte)ringid, 0, (short)rng.X, (short)rng.Y));
											break;
									}
							string act = "1";
							Match actmatch = actregex.Match(LevelData.Level.DisplayName);
							if (actmatch.Success)
								act = actmatch.Groups[1].Value;
							scene.Write(Path.Combine(rsdkExportDialog.SelectedPath, $"Act{act}.bin"));
						}
						break;
					case 1: // v4
						{
							Gameconfig4 gc = new Gameconfig4(gameConfig);
							List<string> objNames = gc.ObjectsNames;
							LevelData.LoadLevel(Levels[levelSelector.SelectedIndex], true);
							Stageconfig4 cfg = new Stageconfig4() { LoadGlobalScripts = true };
							for (int l = 0; l < 2; l++)
								for (int c = 0; c < 16; c++)
									cfg.StagePalette.Colors[l][c] = new PaletteColour4(LevelData.Palette[0][l + 2, c].R, LevelData.Palette[0][l + 2, c].G, LevelData.Palette[0][l + 2, c].B);
							int ringid = objNames.IndexOf("Ring");
							List<string> newobjs = new List<string>();
							Dictionary<byte, byte> objMap = new Dictionary<byte, byte>();
							if (includeObjectsCheckBox.Checked)
							{
								if (LevelData.RingFormat is RingLayoutFormat && ringid == -1)
								{
									ringid = objNames.Count + 1;
									objNames.Add("Ring");
								}
								foreach (var id in LevelData.Objects.Select(a => a.ID).Distinct().OrderBy(a => a))
								{
									string name;
									if (LevelData.ObjTypes.TryGetValue(id, out ObjectDefinition def))
										name = def.Name;
									else
										name = $"Unk{id:X2}";
									int newid = objNames.IndexOf(name) + 1;
									if (newid == 0)
									{
										newobjs.Add(name);
										objNames.Add(name);
										newid = objNames.Count;
									}
									objMap[id] = (byte)newid;
								}
							}
							cfg.ObjectsNames = newobjs;
							foreach (string name in newobjs)
								cfg.ScriptPaths.Add(LevelData.Level.DisplayName + "\\" + name + ".txt");
							cfg.Write(Path.Combine(rsdkExportDialog.SelectedPath, "StageConfig.bin"));
							List<ushort> bgchunks = LevelData.Layout.BGLayout.OfType<ushort>().Distinct().ToList();
							List<ushort> bgblocks = bgchunks.Select(a => LevelData.Chunks[a]).Where(b => b != null).SelectMany(c => c.Blocks.OfType<ChunkBlock>().Select(d => d.Block)).Distinct().ToList();
							BitmapBits blocks = new BitmapBits(16, 16384);
							for (ushort i = 0; i < LevelData.CompBlockBmpBits.Count; i++)
							{
								LevelData.CompBlockBmpBits[i].IncrementIndexes(0x80);
								if (i != 0 && bgblocks.Contains(i))
									LevelData.CompBlockBmpBits[i].ReplaceColor(0, 0xA0);
								blocks.DrawBitmap(LevelData.CompBlockBmpBits[i], 0, i * 16);
							}
							blocks.FillRectangle(0xA0, 0, 16368, 16, 16);
							Color[] pal = new Color[256];
							pal.Fill(Color.Fuchsia);
							for (int l = 0; l < 4; l++)
								for (int c = 0; c < 16; c++)
									pal[0x80 + (l * 16) + c] = LevelData.Palette[0][l, c].RGBColor;
							using (Bitmap bmp = blocks.ToBitmap(pal))
								bmp.Save(Path.Combine(rsdkExportDialog.SelectedPath, "16x16Tiles.gif"), System.Drawing.Imaging.ImageFormat.Gif);
							foreach (ushort c in bgchunks)
							{
								if (c == 0) continue;
								Chunk ch = LevelData.Chunks[c];
								if (ch == null) continue;
								for (int y = 0; y < LevelData.Level.ChunkHeight / 16; y++)
									for (int x = 0; x < LevelData.Level.ChunkWidth / 16; x++)
										if (ch.Blocks[x, y].Block == 0)
											ch.Blocks[x, y].Block = 0x3FF;
							}
							List<Chunk> tmpchnk = ConvertChunks();
							if (bgchunks.Contains(0))
							{
								for (int y = 0; y < LevelData.BGHeight; y++)
									for (int x = 0; x < LevelData.BGWidth; x++)
										if (LevelData.Layout.BGLayout[x, y] == 0)
											LevelData.Layout.BGLayout[x, y] = (ushort)tmpchnk.Count;
								Chunk ch = new Chunk();
								for (int y = 0; y < 8; y++)
									for (int x = 0; x < 8; x++)
										ch.Blocks[x, y].Block = 0x3FF;
								tmpchnk.Add(ch);
							}
							Tileconfig4 col = new Tileconfig4();
							for (int i = 0; i < LevelData.ColInds1.Count; i++)
							{
								col.CollisionPath1[i].isCeiling = LevelData.ColArr1[LevelData.ColInds1[i]].Where(a => a != 16).Sum(a => Math.Sign(a)) < 0;
								col.CollisionPath1[i].FloorAngle = LevelData.Angles[LevelData.ColInds1[i]];
								if (col.CollisionPath1[i].FloorAngle == 0xFF)
									col.CollisionPath1[i].FloorAngle = 0;
								col.CollisionPath1[i].LWallAngle = (byte)(col.CollisionPath1[i].FloorAngle + 0x40);
								col.CollisionPath1[i].CeilingAngle = (byte)(col.CollisionPath1[i].FloorAngle + 0x80);
								col.CollisionPath1[i].RWallAngle = (byte)(col.CollisionPath1[i].FloorAngle + 0xC0);
								for (int j = 0; j < 16; j++)
									switch (Math.Sign(LevelData.ColArr1[LevelData.ColInds1[i]][j]))
									{
										case 1:
											col.CollisionPath1[i].HasCollision[j] = true;
											if (col.CollisionPath1[i].isCeiling && LevelData.ColArr1[LevelData.ColInds1[i]][j] == 16)
												col.CollisionPath1[i].Collision[j] = 15;
											else
												col.CollisionPath1[i].Collision[j] = (byte)(16 - LevelData.ColArr1[LevelData.ColInds1[i]][j]);
											break;
										case -1:
											col.CollisionPath1[i].HasCollision[j] = true;
											col.CollisionPath1[i].Collision[j] = (byte)(-1 - LevelData.ColArr1[LevelData.ColInds1[i]][j]);
											break;
									}
								col.CollisionPath2[i].FloorAngle = LevelData.Angles[LevelData.ColInds2[i]];
								col.CollisionPath2[i].isCeiling = LevelData.ColArr1[LevelData.ColInds2[i]].Sum(a => Math.Sign(a)) < 0;
								if (col.CollisionPath2[i].FloorAngle == 0xFF)
									col.CollisionPath2[i].FloorAngle = 0;
								col.CollisionPath2[i].LWallAngle = (byte)(col.CollisionPath2[i].FloorAngle + 0x40);
								col.CollisionPath2[i].CeilingAngle = (byte)(col.CollisionPath2[i].FloorAngle + 0x80);
								col.CollisionPath2[i].RWallAngle = (byte)(col.CollisionPath2[i].FloorAngle + 0xC0);
								for (int j = 0; j < 16; j++)
									switch (Math.Sign(LevelData.ColArr1[LevelData.ColInds2[i]][j]))
									{
										case 1:
											col.CollisionPath2[i].HasCollision[j] = true;
											if (col.CollisionPath2[i].isCeiling && LevelData.ColArr1[LevelData.ColInds2[i]][j] == 16)
												col.CollisionPath2[i].Collision[j] = 15;
											else
												col.CollisionPath2[i].Collision[j] = (byte)(16 - LevelData.ColArr1[LevelData.ColInds2[i]][j]);
											break;
										case -1:
											col.CollisionPath2[i].HasCollision[j] = true;
											col.CollisionPath2[i].Collision[j] = (byte)(-1 - LevelData.ColArr1[LevelData.ColInds2[i]][j]);
											break;
									}
							}
							col.Write(Path.Combine(rsdkExportDialog.SelectedPath, "CollisionMasks.bin"));
							BGLayout4 bg = new BGLayout4();
							bg.HLines.Add(new BGLayout4.ScrollInfo() { RelativeSpeed = 0x100 });
							bg.Layers.Add(new BGLayout4.BGLayer((byte)LevelData.BGWidth, (byte)LevelData.BGHeight) { Behaviour = 1 });
							for (int y = 0; y < LevelData.BGHeight; y++)
								for (int x = 0; x < LevelData.BGWidth; x++)
									bg.Layers[0].MapLayout[y][x] = LevelData.Layout.BGLayout[x, y];
							bg.Write(Path.Combine(rsdkExportDialog.SelectedPath, "Backgrounds.bin"));
							Tiles128x1284 chunks = new Tiles128x1284();
							for (int i = 0; i < tmpchnk.Count; i++)
								if (tmpchnk[i] != null)
								{
									for (int y = 0; y < 8; y++)
										for (int x = 0; x < 8; x++)
										{
											chunks.BlockList[i].Mapping[y][x].Tile16x16 = tmpchnk[i].Blocks[x, y].Block;
											if (tmpchnk[i].Blocks[x, y].XFlip && tmpchnk[i].Blocks[x, y].YFlip)
												chunks.BlockList[i].Mapping[y][x].Direction = 3;
											else if (tmpchnk[i].Blocks[x, y].YFlip)
												chunks.BlockList[i].Mapping[y][x].Direction = 2;
											else if (tmpchnk[i].Blocks[x, y].XFlip)
												chunks.BlockList[i].Mapping[y][x].Direction = 1;
											chunks.BlockList[i].Mapping[y][x].CollisionFlag0 = SolidMap[(byte)tmpchnk[i].Blocks[x, y].Solid1];
											chunks.BlockList[i].Mapping[y][x].CollisionFlag1 = SolidMap[(byte)((S2ChunkBlock)tmpchnk[i].Blocks[x, y]).Solid2];
											Block blk = LevelData.Blocks[tmpchnk[i].Blocks[x, y].Block];
											if (blk != null && blk.Tiles.OfType<PatternIndex>().Count(a => a.Priority) > 1)
												chunks.BlockList[i].Mapping[y][x].VisualPlane = 1;
										}
								}
							chunks.Write(Path.Combine(rsdkExportDialog.SelectedPath, "128x128Tiles.bin"));
							Scene4 scene = new Scene4
							{
								Title = LevelData.Level.DisplayName,
								width = (ushort)LevelData.FGWidth,
								height = (ushort)LevelData.FGHeight,
								MapLayout = new ushort[LevelData.FGHeight][]
							};
							for (int y = 0; y < LevelData.FGHeight; y++)
							{
								scene.MapLayout[y] = new ushort[LevelData.FGWidth];
								for (int x = 0; x < LevelData.FGWidth; x++)
									scene.MapLayout[y][x] = LevelData.Layout.FGLayout[x, y];
							}
							if (LevelData.StartPositions.Count > 0)
								scene.objects.Add(new Object4((byte)(objNames.IndexOf("Player Object") + 1), 0, (short)LevelData.StartPositions[0].X, (short)LevelData.StartPositions[0].Y));
							if (includeObjectsCheckBox.Checked)
								foreach (Entry ent in LevelData.Objects.Cast<Entry>().Concat(LevelData.Rings).OrderBy(a => a))
									switch (ent)
									{
										case ObjectEntry obj:
											scene.objects.Add(new Object4(objMap[obj.ID], obj.SubType, (short)obj.X, (short)obj.Y));
											break;
										case SonicRetro.SonLVL.API.S2.S2RingEntry rng2:
											Point rpos = new Point(rng2.X, rng2.Y);
											for (int r = 0; r < rng2.Count; r++)
											{
												scene.objects.Add(new Object4((byte)ringid, 0, (short)rpos.X, (short)rpos.Y));
												switch (rng2.Direction)
												{
													case Direction.Horizontal:
														rpos.X += 0x18;
														break;
													case Direction.Vertical:
														rpos.Y += 0x18;
														break;
												}
											}
											break;
										case RingEntry rng:
											scene.objects.Add(new Object4((byte)ringid, 0, (short)rng.X, (short)rng.Y));
											break;
									}
							string act = "1";
							Match actmatch = actregex.Match(LevelData.Level.DisplayName);
							if (actmatch.Success)
								act = actmatch.Groups[1].Value;
							scene.Write(Path.Combine(rsdkExportDialog.SelectedPath, $"Act{act}.bin"));
						}
						break;
					case 2: // v5
						{
							LevelData.LoadLevel(Levels[levelSelector.SelectedIndex], true);
							StageConfig5 cfg = new StageConfig5() { LoadGlobalObjects = true };
							for (int l = 0; l < 2; l++)
								for (int c = 0; c < 16; c++)
									cfg.Palettes[0].Colors[l][c] = new PaletteColor5(LevelData.Palette[0][l + 2, c].R, LevelData.Palette[0][l + 2, c].G, LevelData.Palette[0][l + 2, c].B);
							cfg.Write(Path.Combine(rsdkExportDialog.SelectedPath, "StageConfig.bin"));
							List<ushort> bgblockids = LevelData.Layout.BGLayout.OfType<ushort>().Distinct().ToList().Select(a => LevelData.Chunks[a]).Where(b => b != null).SelectMany(c => c.Blocks.OfType<ChunkBlock>().Select(d => d.Block)).Distinct().ToList();
							BitmapBits blocks = new BitmapBits(16, 16384);
							for (ushort i = 0; i < LevelData.CompBlockBmpBits.Count; i++)
							{
								LevelData.CompBlockBmpBits[i].IncrementIndexes(0x80);
								if (i != 0 && bgblockids.Contains(i))
									LevelData.CompBlockBmpBits[i].ReplaceColor(0, 0xA0);
								blocks.DrawBitmap(LevelData.CompBlockBmpBits[i], 0, i * 16);
							}
							blocks.FillRectangle(0xA0, 0, 16368, 16, 16);
							Color[] pal = new Color[256];
							pal.Fill(Color.Fuchsia);
							for (int l = 0; l < 4; l++)
								for (int c = 0; c < 16; c++)
									pal[0x80 + (l * 16) + c] = LevelData.Palette[0][l, c].RGBColor;
							using (Bitmap bmp = blocks.ToBitmap(pal))
								bmp.Save(Path.Combine(rsdkExportDialog.SelectedPath, "16x16Tiles.gif"), System.Drawing.Imaging.ImageFormat.Gif);
							MakeDualPath();
							Tileconfig5 col = new Tileconfig5();
							for (int i = 0; i < LevelData.ColInds1.Count; i++)
							{
								col.CollisionPath1[i].IsCeiling = LevelData.ColArr1[LevelData.ColInds1[i]].Where(a => a != 16).Sum(a => Math.Sign(a)) < 0;
								col.CollisionPath1[i].FloorAngle = LevelData.Angles[LevelData.ColInds1[i]];
								if (col.CollisionPath1[i].FloorAngle == 0xFF)
									col.CollisionPath1[i].FloorAngle = 0;
								col.CollisionPath1[i].LWallAngle = (byte)(col.CollisionPath1[i].FloorAngle + 0x40);
								col.CollisionPath1[i].CeilingAngle = (byte)(col.CollisionPath1[i].FloorAngle + 0x80);
								col.CollisionPath1[i].RWallAngle = (byte)(col.CollisionPath1[i].FloorAngle + 0xC0);
								for (int j = 0; j < 16; j++)
									switch (Math.Sign(LevelData.ColArr1[LevelData.ColInds1[i]][j]))
									{
										case 1:
											col.CollisionPath1[i].HasCollision[j] = true;
											if (col.CollisionPath1[i].IsCeiling && LevelData.ColArr1[LevelData.ColInds1[i]][j] == 16)
												col.CollisionPath1[i].Collision[j] = 15;
											else
												col.CollisionPath1[i].Collision[j] = (byte)(16 - LevelData.ColArr1[LevelData.ColInds1[i]][j]);
											break;
										case -1:
											col.CollisionPath1[i].HasCollision[j] = true;
											col.CollisionPath1[i].Collision[j] = (byte)(-1 - LevelData.ColArr1[LevelData.ColInds1[i]][j]);
											break;
									}
								col.CollisionPath2[i].FloorAngle = LevelData.Angles[LevelData.ColInds2[i]];
								col.CollisionPath2[i].IsCeiling = LevelData.ColArr1[LevelData.ColInds2[i]].Sum(a => Math.Sign(a)) < 0;
								if (col.CollisionPath2[i].FloorAngle == 0xFF)
									col.CollisionPath2[i].FloorAngle = 0;
								col.CollisionPath2[i].LWallAngle = (byte)(col.CollisionPath2[i].FloorAngle + 0x40);
								col.CollisionPath2[i].CeilingAngle = (byte)(col.CollisionPath2[i].FloorAngle + 0x80);
								col.CollisionPath2[i].RWallAngle = (byte)(col.CollisionPath2[i].FloorAngle + 0xC0);
								for (int j = 0; j < 16; j++)
									switch (Math.Sign(LevelData.ColArr1[LevelData.ColInds2[i]][j]))
									{
										case 1:
											col.CollisionPath2[i].HasCollision[j] = true;
											if (col.CollisionPath2[i].IsCeiling && LevelData.ColArr1[LevelData.ColInds2[i]][j] == 16)
												col.CollisionPath2[i].Collision[j] = 15;
											else
												col.CollisionPath2[i].Collision[j] = (byte)(16 - LevelData.ColArr1[LevelData.ColInds2[i]][j]);
											break;
										case -1:
											col.CollisionPath2[i].HasCollision[j] = true;
											col.CollisionPath2[i].Collision[j] = (byte)(-1 - LevelData.ColArr1[LevelData.ColInds2[i]][j]);
											break;
									}
							}
							col.Write(Path.Combine(rsdkExportDialog.SelectedPath, "TileConfig.bin"));
							int w = LevelData.Level.ChunkWidth / 16;
							int h = LevelData.Level.ChunkHeight / 16;
							S2ChunkBlock[,] fgblocks = new S2ChunkBlock[LevelData.FGWidth * w, LevelData.FGHeight * h];
							for (int y = 0; y < LevelData.FGHeight; y++)
								for (int x = 0; x < LevelData.FGWidth; x++)
								{
									Chunk cnk = LevelData.Chunks[LevelData.Layout.FGLayout[x, y]];
									if (cnk != null)
										for (int cy = 0; cy < h; cy++)
											for (int cx = 0; cx < w; cx++)
												fgblocks[x * w + cx, y * h + cy] = (S2ChunkBlock)cnk.Blocks[cx, cy];
								}
							S2ChunkBlock[,] bgblocks = new S2ChunkBlock[LevelData.BGWidth * w, LevelData.BGHeight * h];
							for (int y = 0; y < LevelData.BGHeight; y++)
								for (int x = 0; x < LevelData.BGWidth; x++)
								{
									Chunk cnk = LevelData.Chunks[LevelData.Layout.BGLayout[x, y]];
									if (cnk != null)
										for (int cy = 0; cy < h; cy++)
											for (int cx = 0; cx < w; cx++)
											{
												bgblocks[x * w + cx, y * h + cy] = (S2ChunkBlock)cnk.Blocks[cx, cy];
												if (bgblocks[x * w + cx, y * h + cy].Block == 0)
													bgblocks[x * w + cx, y * h + cy].Block = 0x3FF;
											}
								}
							Scene5 scene = new Scene5();
							SceneLayer5 layer = new SceneLayer5("Background", (ushort)(LevelData.BGWidth * w), (ushort)(LevelData.BGHeight * h));
							layer.Behaviour = 1;
							layer.RelativeSpeed = 256;
							layer.ScrollingInfo.Add(new ScrollInfo5());
							for (int y = 0; y < LevelData.BGHeight * h; y++)
								for (int x = 0; x < LevelData.BGWidth * w; x++)
									layer.Tiles[y][x] = ByteConverter.ToUInt16(bgblocks[x, y].GetBytes(), 0);
							scene.Layers.Add(layer);
							layer = new SceneLayer5("FG Low", (ushort)(LevelData.FGWidth * w), (ushort)(LevelData.FGHeight * h));
							layer.Behaviour = 1;
							layer.RelativeSpeed = 256;
							layer.DrawingOrder = 1;
							layer.ScrollingInfo.Add(new ScrollInfo5());
							for (int y = 0; y < LevelData.FGHeight * h; y++)
								for (int x = 0; x < LevelData.FGWidth * w; x++)
								{
									Block blk = LevelData.Blocks[fgblocks[x, y].Block];
									if (blk != null && blk.Tiles.OfType<PatternIndex>().Count(a => a.Priority) < 2)
										layer.Tiles[y][x] = ByteConverter.ToUInt16(fgblocks[x, y].GetBytes(), 0);
								}
							scene.Layers.Add(layer);
							layer = new SceneLayer5("FG High", (ushort)(LevelData.FGWidth * w), (ushort)(LevelData.FGHeight * h));
							layer.Behaviour = 1;
							layer.RelativeSpeed = 256;
							layer.DrawingOrder = 2;
							layer.ScrollingInfo.Add(new ScrollInfo5());
							for (int y = 0; y < LevelData.FGHeight * h; y++)
								for (int x = 0; x < LevelData.FGWidth * w; x++)
								{
									Block blk = LevelData.Blocks[fgblocks[x, y].Block];
									if (blk != null && blk.Tiles.OfType<PatternIndex>().Count(a => a.Priority) > 1)
										layer.Tiles[y][x] = ByteConverter.ToUInt16(fgblocks[x, y].GetBytes(), 0);
								}
							scene.Layers.Add(layer);
							string act = "1";
							Match actmatch = actregex.Match(LevelData.Level.DisplayName);
							if (actmatch.Success)
								act = actmatch.Groups[1].Value;
							scene.Write(Path.Combine(rsdkExportDialog.SelectedPath, $"Scene{act}.bin"));
						}
						break;
				}
			}
		}

		private static void MakeDualPath()
		{
			int w = LevelData.Level.ChunkWidth / 16;
			int h = LevelData.Level.ChunkHeight / 16;
			if (LevelData.Chunks[0].Blocks[0, 0] is S1ChunkBlock)
			{
				LevelData.ColInds2 = new List<byte>(LevelData.ColInds1);
				for (int item = 0; item < LevelData.Chunks.Count; item++)
				{
					Chunk cnk = LevelData.Chunks[item];
					Chunk cnk2 = LevelData.Chunks[item + 1];
					for (int y = 0; y < h; y++)
						for (int x = 0; x < w; x++)
						{
							ChunkBlock old = cnk.Blocks[x, y];
							Solidity solid2 = old.Solid1;
							if (LevelData.Level.LoopChunks.Contains((byte)item))
							{
								ChunkBlock old2 = cnk2.Blocks[x, y];
								solid2 = old2.Solid1;
								if (old.Block < LevelData.ColInds2.Count)
									LevelData.ColInds2[old.Block] = LevelData.ColInds1[old2.Block];
							}
							cnk.Blocks[x, y] = new S2ChunkBlock() { Block = old.Block, Solid1 = old.Solid1, Solid2 = solid2, XFlip = old.XFlip, YFlip = old.YFlip };
						}
					if (LevelData.Level.LoopChunks.Contains((byte)item))
						LevelData.Chunks[++item] = cnk;
				}
				LevelData.Level.ChunkFormat = EngineVersion.S2;
			}
		}

		private static List<Chunk> ConvertChunks()
		{
			MakeDualPath();
			int w = LevelData.Level.ChunkWidth / 16;
			int h = LevelData.Level.ChunkHeight / 16;
			if (LevelData.Level.ChunkWidth != 128 || LevelData.Level.ChunkHeight != 128)
			{
				LevelData.Level.ChunkWidth = 128;
				LevelData.Level.ChunkHeight = 128;
				List<Chunk> tmpchnk = new List<Chunk>() { new Chunk() };
				List<byte[]> cnkbytes = new List<byte[]>() { tmpchnk[0].GetBytes() };
				ChunkBlock[,] blocks = new ChunkBlock[LevelData.FGWidth * w, LevelData.FGHeight * h];
				for (int y = 0; y < LevelData.FGHeight; y++)
					for (int x = 0; x < LevelData.FGWidth; x++)
					{
						Chunk cnk = LevelData.Chunks[LevelData.Layout.FGLayout[x, y]];
						if (cnk != null)
							for (int cy = 0; cy < h; cy++)
								for (int cx = 0; cx < w; cx++)
									blocks[x * w + cx, y * h + cy] = cnk.Blocks[cx, cy];
					}
				int newwidth = Math.DivRem(blocks.GetLength(0), 8, out int rem);
				if (rem != 0)
					++newwidth;
				int newheight = Math.DivRem(blocks.GetLength(1), 8, out rem);
				if (rem != 0)
					++newheight;
				LevelData.ResizeFG(newwidth, newheight);
				for (int y = 0; y < newheight; y++)
					for (int x = 0; x < newwidth; x++)
					{
						Chunk cnk = new Chunk();
						for (int cy = 0; cy < 8; cy++)
							if (y * 8 + cy < blocks.GetLength(1))
								for (int cx = 0; cx < 8; cx++)
									if (x * 8 + cx < blocks.GetLength(0))
										cnk.Blocks[cx, cy] = blocks[x * 8 + cx, y * 8 + cy];
						byte[] tmp = cnk.GetBytes();
						int id = cnkbytes.FindIndex(a => a.FastArrayEqual(tmp));
						if (id == -1)
						{
							LevelData.Layout.FGLayout[x, y] = (ushort)tmpchnk.Count;
							tmpchnk.Add(cnk);
							cnkbytes.Add(tmp);
						}
						else
							LevelData.Layout.FGLayout[x, y] = (ushort)id;
					}
				blocks = new ChunkBlock[LevelData.BGWidth * w, LevelData.BGHeight * h];
				for (int y = 0; y < LevelData.BGHeight; y++)
					for (int x = 0; x < LevelData.BGWidth; x++)
					{
						Chunk cnk = LevelData.Chunks[LevelData.Layout.BGLayout[x, y]];
						if (cnk != null)
							for (int cy = 0; cy < h; cy++)
								for (int cx = 0; cx < w; cx++)
									blocks[x * w + cx, y * h + cy] = cnk.Blocks[cx, cy];
					}
				newwidth = Math.DivRem(blocks.GetLength(0), 8, out rem);
				if (rem != 0)
					++newwidth;
				newheight = Math.DivRem(blocks.GetLength(1), 8, out rem);
				if (rem != 0)
					++newheight;
				LevelData.ResizeBG(newwidth, newheight);
				for (int y = 0; y < newheight; y++)
					for (int x = 0; x < newwidth; x++)
					{
						Chunk cnk = new Chunk();
						for (int cy = 0; cy < 8; cy++)
							if (y * 8 + cy < blocks.GetLength(1))
								for (int cx = 0; cx < 8; cx++)
									if (x * 8 + cx < blocks.GetLength(0))
										cnk.Blocks[cx, cy] = blocks[x * 8 + cx, y * 8 + cy];
						byte[] tmp = cnk.GetBytes();
						int id = cnkbytes.FindIndex(a => a.FastArrayEqual(tmp));
						if (id == -1)
						{
							LevelData.Layout.BGLayout[x, y] = (ushort)tmpchnk.Count;
							tmpchnk.Add(cnk);
							cnkbytes.Add(tmp);
						}
						else
							LevelData.Layout.BGLayout[x, y] = (ushort)id;
					}
				return tmpchnk;
			}
			else
				return LevelData.Chunks.ToList();
		}

		readonly byte[] SolidMap = { 3, 1, 2, 0 };
	}

	public class Settings
	{
		[IniName("Project")]
		[IniCollection(IniCollectionMode.NoSquareBrackets, StartIndex = 1)]
		public List<string> Projects { get; set; } = new List<string>();
		public int RSDKVer { get; set; } = 3;
		[IniName("GameConfig")]
		[IniCollection(IniCollectionMode.NoSquareBrackets, StartIndex = 1)]
		public List<string> GameConfigs { get; set; } = new List<string>();
		[IniName("TitleCard")]
		[IniCollection(IniCollectionMode.NoSquareBrackets, StartIndex = 1)]
		public List<string> TitleCards { get; set; } = new List<string>();
	}
}
