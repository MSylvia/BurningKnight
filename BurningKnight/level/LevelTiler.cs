using BurningKnight.level.tile;
using BurningKnight.util;
using Lens.util;
using Lens.util.math;

namespace BurningKnight.level {
	public class LevelTiler {
		public static void TileUp(Level level, bool full = false) {
			for (var i = 0; i < level.Variants.Length; i++) {
				level.Variants[i] = 0;
				level.LiquidVariants[i] = 0;
			}

			if (full) {
				for (var i = 0; i < level.Variants.Length; i++) {
					level.Light[i] = 0;
					level.WallDecor[i] = 0;
				}	
				
				level.CreatePassable();
			}
			
			for (int y = 0; y < level.Height; y++) {
				for (int x = 0; x < level.Width; x++) {
					TileUp(level, level.ToIndex(x, y));
				}
			}
		}

		public static void TileUp(Level level, int index) {
			if (index < 0 || index >= level.Size) {
				return;
			}
			
			var liquid = level.Liquid[index];
			var x = level.FromIndexX(index);
			var y = level.FromIndexY(index);
			
			if (((Tile) liquid).Matches(Tile.Rock, Tile.TintedRock, Tile.MetalBlock)) {
				level.LiquidVariants[index] = (byte) Rnd.Int(4);
			} else {
				byte lmask = 0;

				for (int i = 0; i < 4; i++) {
					var m = PathFinder.Circle4[i];
					var v = PathFinder.VCircle4[i];
					var n = index + m;

					if (!level.IsInside(x + (int) v.X, y + (int) v.Y) ||
					    level.IsInside(n) && ShouldTile(liquid, level.Tiles[n], level.Liquid[n])) {
						lmask |= (byte) (1 << i);
					}
				}

				level.LiquidVariants[index] = lmask;
			}

			byte mask = 0;
			var tile = level.Tiles[index];
			var t = (Tile) tile;

			if (t == Tile.Transition) {
				foreach (var d in MathUtils.AllDirections) {
					var xx = x + (int) d.X;
					var yy = y + (int) d.Y;

					if (level.IsInside(xx, yy) && !level.Get(xx, yy).IsWall()) {
						t = Tile.WallA;
					
						level.Variants[index] = 0;
						level.Tiles[index] = tile = (byte) t;
						
						break;
					}
				}
			}

			if (t.Matches(Tile.FloorA, Tile.FloorB, Tile.FloorC, Tile.FloorD, Tile.EvilFloor, Tile.GrannyFloor)) {
				if (level.Variants[index] != 0 && level.Variants[index] < 16) {
					return;
				}

				var v = Rnd.Int(9);

				if (v == 8 || v == 9) {
					if (level.IsInside(index + level.Width + 1) && level.Tiles[index + 1] == tile && level.Tiles[index + level.Width] == tile && level.Tiles[index + 1 + level.Width] == tile 
					    && level.Variants[index + 1] == 0 && level.Variants[index + level.Width] == 0 && level.Variants[index + 1 + level.Width] == 0) {

						var st = v == 8 ? 8 : 10; 
						
						level.Variants[index] = (byte) st;
						level.Variants[index + 1] = (byte) (st + 1);
						level.Variants[index + level.Width] = (byte) (st + 4);
						level.Variants[index + 1 + level.Width] = (byte) (st + 5);
		
						return;
					}

					v = Rnd.Int(8);
				}

				level.Variants[index] = (byte) v;
				return;
			}

			if (!(t.Matches(TileFlags.LiquidLayer) || t.Matches(TileFlags.Solid) || t == Tile.PistonDown)) {
				return;
			}
			
			for (int i = 0; i < 4; i++) {
				var m = PathFinder.Circle4[i];
				var v = PathFinder.VCircle4[i];
				var n = index + m;
				
				if (!level.IsInside(x + (int) v.X, y + (int) v.Y) || level.IsInside(n) && ShouldTile(tile, level.Tiles[n], level.Liquid[n])) {
					mask |= (byte) (1 << i);
				}
			}

			if (t.IsWall() || t == Tile.PistonDown) {
				for (int i = 0; i < 4; i++) {
					var m = PathFinder.Corner[i];
					var v = PathFinder.VCorner[i];
					var n = index + m;
				
					if (!level.IsInside(x + (int) v.X, y + (int) v.Y) || level.IsInside(n) && ShouldTile(tile, level.Tiles[n], level.Liquid[n])) {
						mask |= (byte) (1 << (4 + i));
					}
				}
			}
			
			level.Variants[index] = mask;
		}

		public static bool ShouldTile(byte tile, byte to, byte l) {
			var t = (Tile) tile;
			var tt = (Tile) to;
			var ll = (Tile) l;

			if (t == Tile.Planks) {
				return tt == Tile.Planks;
			}

			if (t == Tile.WallA || t == Tile.Piston || t == Tile.GrannyWall || t == Tile.EvilWall) {
				return tt == Tile.WallA || tt == Tile.Planks || tt == Tile.Crack || tt == Tile.Piston || tt == Tile.Transition || tt == Tile.GrannyWall || tt == Tile.EvilWall;
			}
			
			if (t == Tile.WallB) {
				// || tt == Tile.Transition 
				return tt == Tile.WallB || tt == Tile.Planks || tt == Tile.Crack || tt == Tile.GrannyWall || tt == Tile.EvilWall;
			}

			if (t == Tile.Transition) {
				return tt == Tile.Transition;
			}

			if (t == Tile.PistonDown) {
				return tt == Tile.PistonDown;
			}
			
			if (t.IsWall() && t != Tile.Planks) {
				return tt.IsWall();
			}
			
			if (t == Tile.Grass || t == Tile.HighGrass) {
				return ll == Tile.Grass || ll == Tile.HighGrass || tt.IsWall();
			}

			if (t == Tile.Dirt) {
				return tt.IsSimpleWall() || tt == Tile.Chasm || ll == Tile.Grass || ll == Tile.HighGrass || ll == Tile.Dirt;
			}

			if (t.Matches(TileFlags.LiquidLayer)) {
				return tt.IsSimpleWall() || (t.Matches(Tile.Lava, Tile.Water, Tile.Venom) && tt == Tile.Chasm) || ll == t;
			}
			
			return tile == to || tile == l;
		}
	}
}