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

namespace RetroConverter
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		static readonly Regex actregex = new Regex("Act ([0-9]+)", RegexOptions.Compiled);
		List<string> Levels;
		string gameConfig = null;

		private void Form1_Load(object sender, EventArgs e)
		{
			comboBox2.SelectedIndex = 0;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
			{
				LevelData.LoadGame(openFileDialog1.FileName);
				comboBox1.BeginUpdate();
				comboBox1.Items.Clear();
				Levels = new List<string>();
				foreach (KeyValuePair<string, LevelInfo> item in LevelData.Game.Levels)
				{
					Levels.Add(item.Key);
					comboBox1.Items.Add(LevelData.Game.GetLevelInfo(item.Key).DisplayName);
				}
				comboBox1.EndUpdate();
			}
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			button2.Enabled = comboBox1.SelectedIndex > -1 && gameConfig != null;
		}

		private void button3_Click(object sender, EventArgs e)
		{
			if (openFileDialog2.ShowDialog(this) == DialogResult.OK)
			{
				gameConfig = openFileDialog2.FileName;
				button2.Enabled = comboBox1.SelectedIndex > -1;
			}
		}

		private void button4_Click(object sender, EventArgs e)
		{
			if (openFileDialog3.ShowDialog(this) == DialogResult.OK)
			{
				if (openFileDialog3.FileName.Contains("\\Scripts\\"))
					textBox1.Text = openFileDialog3.FileName.Substring(openFileDialog3.FileName.LastIndexOf("\\Scripts\\") + 9).Replace('\\', '/');
			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
			folderBrowserDialog1.SelectedPath = Environment.CurrentDirectory;
			if (folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
			{
				switch (comboBox2.SelectedIndex)
				{
					case 0: // v3
						{
							Gameconfig3 gc = new Gameconfig3(gameConfig);
							List<string> objNames = gc.ObjectsNames;
							LevelData.LoadLevel(Levels[comboBox1.SelectedIndex], true);
							Stageconfig3 cfg = new Stageconfig3();
							for (int l = 0; l < 2; l++)
								for (int c = 0; c < 16; c++)
									cfg.StagePalette.Colors[l][c] = new PaletteColour3(LevelData.Palette[0][l + 2, c].R, LevelData.Palette[0][l + 2, c].G, LevelData.Palette[0][l + 2, c].B);
							int ringid = objNames.IndexOf("Ring");
							List<string> newobjs = new List<string>();
							Dictionary<byte, byte> objMap = new Dictionary<byte, byte>();
							if (checkBox1.Checked)
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
							cfg.Write(Path.Combine(folderBrowserDialog1.SelectedPath, "StageConfig.bin"));
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
								bmp.Save(Path.Combine(folderBrowserDialog1.SelectedPath, "16x16Tiles.gif"), System.Drawing.Imaging.ImageFormat.Gif);
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
							List<Chunk> tmpchnk;
							switch (LevelData.Level.ChunkFormat)
							{
								case EngineVersion.S1:
								case EngineVersion.SCD:
								case EngineVersion.SCDPC:
									tmpchnk = new List<Chunk>() { new Chunk() };
									LevelData.ColInds2 = new List<byte>(LevelData.ColInds1);
									Dictionary<ushort, ushort[]> cnkinds = new Dictionary<ushort, ushort[]>() { { 0, new ushort[4] } };
									List<ushort> usedcnks =
										LevelData.Layout.FGLayout.Cast<ushort>().Concat(LevelData.Layout.BGLayout.Cast<ushort>()).Distinct().ToList();
									usedcnks.Remove(0);
									foreach (ushort usedcnk in usedcnks)
									{
										Chunk item = LevelData.Chunks[usedcnk];
										Chunk[] newchnk = new Chunk[4];
										for (int i = 0; i < 4; i++)
											newchnk[i] = new Chunk();
										for (int y = 0; y < 8; y++)
											for (int x = 0; x < 8; x++)
											{
												S2ChunkBlock blk = (S2ChunkBlock)newchnk[0].Blocks[x, y];
												blk.Block = item.Blocks[x, y].Block;
												blk.Solid1 = item.Blocks[x, y].Solid1;
												blk.Solid2 = blk.Solid1;
												if (LevelData.Level.LoopChunks.Contains((byte)usedcnk))
												{
													blk.Solid2 = LevelData.Chunks[usedcnk + 1].Blocks[x, y].Solid1;
													if (blk.Block < LevelData.ColInds2.Count)
														LevelData.ColInds2[blk.Block] = LevelData.ColInds1[LevelData.Chunks[usedcnk + 1].Blocks[x, y].Block];
												}
												blk.XFlip = item.Blocks[x, y].XFlip;
												blk.YFlip = item.Blocks[x, y].YFlip;
												blk = (S2ChunkBlock)newchnk[1].Blocks[x, y];
												blk.Block = item.Blocks[x + 8, y].Block;
												blk.Solid1 = item.Blocks[x + 8, y].Solid1;
												blk.Solid2 = blk.Solid1;
												if (LevelData.Level.LoopChunks.Contains((byte)usedcnk))
												{
													blk.Solid2 = LevelData.Chunks[usedcnk + 1].Blocks[x + 8, y].Solid1;
													if (blk.Block < LevelData.ColInds2.Count)
														LevelData.ColInds2[blk.Block] = LevelData.ColInds1[LevelData.Chunks[usedcnk + 1].Blocks[x + 8, y].Block];
												}
												blk.XFlip = item.Blocks[x + 8, y].XFlip;
												blk.YFlip = item.Blocks[x + 8, y].YFlip;
												blk = (S2ChunkBlock)newchnk[2].Blocks[x, y];
												blk.Block = item.Blocks[x, y + 8].Block;
												blk.Solid1 = item.Blocks[x, y + 8].Solid1;
												blk.Solid2 = blk.Solid1;
												if (LevelData.Level.LoopChunks.Contains((byte)usedcnk))
												{
													blk.Solid2 = LevelData.Chunks[usedcnk + 1].Blocks[x, y + 8].Solid1;
													if (blk.Block < LevelData.ColInds2.Count)
														LevelData.ColInds2[blk.Block] = LevelData.ColInds1[LevelData.Chunks[usedcnk + 1].Blocks[x, y + 8].Block];
												}
												blk.XFlip = item.Blocks[x, y + 8].XFlip;
												blk.YFlip = item.Blocks[x, y + 8].YFlip;
												blk = (S2ChunkBlock)newchnk[3].Blocks[x, y];
												blk.Block = item.Blocks[x + 8, y + 8].Block;
												blk.Solid1 = item.Blocks[x + 8, y + 8].Solid1;
												blk.Solid2 = blk.Solid1;
												if (LevelData.Level.LoopChunks.Contains((byte)usedcnk))
												{
													blk.Solid2 = LevelData.Chunks[usedcnk + 1].Blocks[x + 8, y + 8].Solid1;
													if (blk.Block < LevelData.ColInds2.Count)
														LevelData.ColInds2[blk.Block] = LevelData.ColInds1[LevelData.Chunks[usedcnk + 1].Blocks[x + 8, y + 8].Block];
												}
												blk.XFlip = item.Blocks[x + 8, y + 8].XFlip;
												blk.YFlip = item.Blocks[x + 8, y + 8].YFlip;
											}
										ushort[] ids = new ushort[4];
										for (int i = 0; i < 4; i++)
										{
											byte[] b = newchnk[i].GetBytes();
											int match = -1;
											for (int c = 0; c < tmpchnk.Count; c++)
												if (b.FastArrayEqual(tmpchnk[c].GetBytes()))
												{
													match = c;
													break;
												}
											if (match != -1)
												ids[i] = (ushort)match;
											else
											{
												ids[i] = (ushort)tmpchnk.Count;
												tmpchnk.Add(newchnk[i]);
											}
										}
										cnkinds.Add(usedcnk, ids);
									}
									ushort[,] newFG = new ushort[LevelData.FGWidth * 2, LevelData.FGHeight * 2];
									for (int y = 0; y < LevelData.FGHeight; y++)
										for (int x = 0; x < LevelData.FGWidth; x++)
											if (LevelData.Layout.FGLayout[x, y] != 0)
											{
												ushort[] ids = cnkinds[LevelData.Layout.FGLayout[x, y]];
												newFG[x * 2, y * 2] = ids[0];
												newFG[(x * 2) + 1, y * 2] = ids[1];
												newFG[x * 2, (y * 2) + 1] = ids[2];
												newFG[(x * 2) + 1, (y * 2) + 1] = ids[3];
											}
									LevelData.Layout.FGLayout = newFG;
									ushort[,] newBG = new ushort[LevelData.BGWidth * 2, LevelData.BGHeight * 2];
									for (int y = 0; y < LevelData.BGHeight; y++)
										for (int x = 0; x < LevelData.BGWidth; x++)
											if (LevelData.Layout.BGLayout[x, y] != 0)
											{
												ushort[] ids = cnkinds[LevelData.Layout.BGLayout[x, y]];
												newBG[x * 2, y * 2] = ids[0];
												newBG[(x * 2) + 1, y * 2] = ids[1];
												newBG[x * 2, (y * 2) + 1] = ids[2];
												newBG[(x * 2) + 1, (y * 2) + 1] = ids[3];
											}
									LevelData.Layout.BGLayout = newBG;
									break;
								default:
									tmpchnk = LevelData.Chunks.ToList();
									break;
							}
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
								col.CollisionPath1[i].isCeiling = LevelData.ColArr1[LevelData.ColInds1[i]].Sum(a => Math.Sign(a)) < 0;
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
											col.CollisionPath2[i].Collision[j] = (byte)(16 - LevelData.ColArr1[LevelData.ColInds2[i]][j]);
											break;
										case -1:
											col.CollisionPath2[i].HasCollision[j] = true;
											col.CollisionPath2[i].Collision[j] = (byte)(-1 - LevelData.ColArr1[LevelData.ColInds2[i]][j]);
											break;
									}
							}
							col.Write(Path.Combine(folderBrowserDialog1.SelectedPath, "CollisionMasks.bin"));
							BGLayout3 bg = new BGLayout3();
							bg.HLines.Add(new BGLayout3.ScrollInfo() { RelativeSpeed = 0x100 });
							bg.Layers.Add(new BGLayout3.BGLayer((byte)LevelData.BGWidth, (byte)LevelData.BGHeight));
							for (int y = 0; y < LevelData.BGHeight; y++)
								for (int x = 0; x < LevelData.BGWidth; x++)
									bg.Layers[0].MapLayout[y][x] = LevelData.Layout.BGLayout[x, y];
							bg.Write(Path.Combine(folderBrowserDialog1.SelectedPath, "Backgrounds.bin"));
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
							chunks.Write(Path.Combine(folderBrowserDialog1.SelectedPath, "128x128Tiles.bin"));
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
							if (checkBox1.Checked)
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
							scene.Write(Path.Combine(folderBrowserDialog1.SelectedPath, $"Act{act}.bin"));
						}
						break;
					case 1: // v4
						{
							Gameconfig4 gc = new Gameconfig4(gameConfig);
							List<string> objNames = gc.ObjectsNames;
							LevelData.LoadLevel(Levels[comboBox1.SelectedIndex], true);
							Stageconfig4 cfg = new Stageconfig4();
							for (int l = 0; l < 2; l++)
								for (int c = 0; c < 16; c++)
									cfg.StagePalette.Colors[l][c] = new PaletteColour4(LevelData.Palette[0][l + 2, c].R, LevelData.Palette[0][l + 2, c].G, LevelData.Palette[0][l + 2, c].B);
							int ringid = objNames.IndexOf("Ring");
							List<string> newobjs = new List<string>();
							Dictionary<byte, byte> objMap = new Dictionary<byte, byte>();
							if (checkBox1.Checked)
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
							cfg.Write(Path.Combine(folderBrowserDialog1.SelectedPath, "StageConfig.bin"));
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
								bmp.Save(Path.Combine(folderBrowserDialog1.SelectedPath, "16x16Tiles.gif"), System.Drawing.Imaging.ImageFormat.Gif);
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
							List<Chunk> tmpchnk;
							switch (LevelData.Level.ChunkFormat)
							{
								case EngineVersion.S1:
								case EngineVersion.SCD:
								case EngineVersion.SCDPC:
									tmpchnk = new List<Chunk>() { new Chunk() };
									LevelData.ColInds2 = new List<byte>(LevelData.ColInds1);
									Dictionary<ushort, ushort[]> cnkinds = new Dictionary<ushort, ushort[]>() { { 0, new ushort[4] } };
									List<ushort> usedcnks =
										LevelData.Layout.FGLayout.Cast<ushort>().Concat(LevelData.Layout.BGLayout.Cast<ushort>()).Distinct().ToList();
									usedcnks.Remove(0);
									foreach (ushort usedcnk in usedcnks)
									{
										Chunk item = LevelData.Chunks[usedcnk];
										Chunk[] newchnk = new Chunk[4];
										for (int i = 0; i < 4; i++)
											newchnk[i] = new Chunk();
										for (int y = 0; y < 8; y++)
											for (int x = 0; x < 8; x++)
											{
												S2ChunkBlock blk = (S2ChunkBlock)newchnk[0].Blocks[x, y];
												blk.Block = item.Blocks[x, y].Block;
												blk.Solid1 = item.Blocks[x, y].Solid1;
												blk.Solid2 = blk.Solid1;
												if (LevelData.Level.LoopChunks.Contains((byte)usedcnk))
												{
													blk.Solid2 = LevelData.Chunks[usedcnk + 1].Blocks[x, y].Solid1;
													if (blk.Block < LevelData.ColInds2.Count)
														LevelData.ColInds2[blk.Block] = LevelData.ColInds1[LevelData.Chunks[usedcnk + 1].Blocks[x, y].Block];
												}
												blk.XFlip = item.Blocks[x, y].XFlip;
												blk.YFlip = item.Blocks[x, y].YFlip;
												blk = (S2ChunkBlock)newchnk[1].Blocks[x, y];
												blk.Block = item.Blocks[x + 8, y].Block;
												blk.Solid1 = item.Blocks[x + 8, y].Solid1;
												blk.Solid2 = blk.Solid1;
												if (LevelData.Level.LoopChunks.Contains((byte)usedcnk))
												{
													blk.Solid2 = LevelData.Chunks[usedcnk + 1].Blocks[x + 8, y].Solid1;
													if (blk.Block < LevelData.ColInds2.Count)
														LevelData.ColInds2[blk.Block] = LevelData.ColInds1[LevelData.Chunks[usedcnk + 1].Blocks[x + 8, y].Block];
												}
												blk.XFlip = item.Blocks[x + 8, y].XFlip;
												blk.YFlip = item.Blocks[x + 8, y].YFlip;
												blk = (S2ChunkBlock)newchnk[2].Blocks[x, y];
												blk.Block = item.Blocks[x, y + 8].Block;
												blk.Solid1 = item.Blocks[x, y + 8].Solid1;
												blk.Solid2 = blk.Solid1;
												if (LevelData.Level.LoopChunks.Contains((byte)usedcnk))
												{
													blk.Solid2 = LevelData.Chunks[usedcnk + 1].Blocks[x, y + 8].Solid1;
													if (blk.Block < LevelData.ColInds2.Count)
														LevelData.ColInds2[blk.Block] = LevelData.ColInds1[LevelData.Chunks[usedcnk + 1].Blocks[x, y + 8].Block];
												}
												blk.XFlip = item.Blocks[x, y + 8].XFlip;
												blk.YFlip = item.Blocks[x, y + 8].YFlip;
												blk = (S2ChunkBlock)newchnk[3].Blocks[x, y];
												blk.Block = item.Blocks[x + 8, y + 8].Block;
												blk.Solid1 = item.Blocks[x + 8, y + 8].Solid1;
												blk.Solid2 = blk.Solid1;
												if (LevelData.Level.LoopChunks.Contains((byte)usedcnk))
												{
													blk.Solid2 = LevelData.Chunks[usedcnk + 1].Blocks[x + 8, y + 8].Solid1;
													if (blk.Block < LevelData.ColInds2.Count)
														LevelData.ColInds2[blk.Block] = LevelData.ColInds1[LevelData.Chunks[usedcnk + 1].Blocks[x + 8, y + 8].Block];
												}
												blk.XFlip = item.Blocks[x + 8, y + 8].XFlip;
												blk.YFlip = item.Blocks[x + 8, y + 8].YFlip;
											}
										ushort[] ids = new ushort[4];
										for (int i = 0; i < 4; i++)
										{
											byte[] b = newchnk[i].GetBytes();
											int match = -1;
											for (int c = 0; c < tmpchnk.Count; c++)
												if (b.FastArrayEqual(tmpchnk[c].GetBytes()))
												{
													match = c;
													break;
												}
											if (match != -1)
												ids[i] = (ushort)match;
											else
											{
												ids[i] = (ushort)tmpchnk.Count;
												tmpchnk.Add(newchnk[i]);
											}
										}
										cnkinds.Add(usedcnk, ids);
									}
									ushort[,] newFG = new ushort[LevelData.FGWidth * 2, LevelData.FGHeight * 2];
									for (int y = 0; y < LevelData.FGHeight; y++)
										for (int x = 0; x < LevelData.FGWidth; x++)
											if (LevelData.Layout.FGLayout[x, y] != 0)
											{
												ushort[] ids = cnkinds[LevelData.Layout.FGLayout[x, y]];
												newFG[x * 2, y * 2] = ids[0];
												newFG[(x * 2) + 1, y * 2] = ids[1];
												newFG[x * 2, (y * 2) + 1] = ids[2];
												newFG[(x * 2) + 1, (y * 2) + 1] = ids[3];
											}
									LevelData.Layout.FGLayout = newFG;
									ushort[,] newBG = new ushort[LevelData.BGWidth * 2, LevelData.BGHeight * 2];
									for (int y = 0; y < LevelData.BGHeight; y++)
										for (int x = 0; x < LevelData.BGWidth; x++)
											if (LevelData.Layout.BGLayout[x, y] != 0)
											{
												ushort[] ids = cnkinds[LevelData.Layout.BGLayout[x, y]];
												newBG[x * 2, y * 2] = ids[0];
												newBG[(x * 2) + 1, y * 2] = ids[1];
												newBG[x * 2, (y * 2) + 1] = ids[2];
												newBG[(x * 2) + 1, (y * 2) + 1] = ids[3];
											}
									LevelData.Layout.BGLayout = newBG;
									break;
								default:
									tmpchnk = LevelData.Chunks.ToList();
									break;
							}
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
								col.CollisionPath1[i].isCeiling = LevelData.ColArr1[LevelData.ColInds1[i]].Sum(a => Math.Sign(a)) < 0;
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
											col.CollisionPath2[i].Collision[j] = (byte)(16 - LevelData.ColArr1[LevelData.ColInds2[i]][j]);
											break;
										case -1:
											col.CollisionPath2[i].HasCollision[j] = true;
											col.CollisionPath2[i].Collision[j] = (byte)(-1 - LevelData.ColArr1[LevelData.ColInds2[i]][j]);
											break;
									}
							}
							col.Write(Path.Combine(folderBrowserDialog1.SelectedPath, "CollisionMasks.bin"));
							BGLayout4 bg = new BGLayout4();
							bg.HLines.Add(new BGLayout4.ScrollInfo() { RelativeSpeed = 0x100 });
							bg.Layers.Add(new BGLayout4.BGLayer((byte)LevelData.BGWidth, (byte)LevelData.BGHeight));
							for (int y = 0; y < LevelData.BGHeight; y++)
								for (int x = 0; x < LevelData.BGWidth; x++)
									bg.Layers[0].MapLayout[y][x] = LevelData.Layout.BGLayout[x, y];
							bg.Write(Path.Combine(folderBrowserDialog1.SelectedPath, "Backgrounds.bin"));
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
							chunks.Write(Path.Combine(folderBrowserDialog1.SelectedPath, "128x128Tiles.bin"));
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
							if (checkBox1.Checked)
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
							scene.Write(Path.Combine(folderBrowserDialog1.SelectedPath, $"Act{act}.bin"));
						}
						break;
				}
			}
		}

		byte[] SolidMap = { 3, 1, 2, 0 };
	}
}
