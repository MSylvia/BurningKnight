using System.Collections.Generic;
using BurningKnight.level.builders;
using BurningKnight.level.rooms;
using BurningKnight.state;
using Lens.util.math;
using Microsoft.Xna.Framework;

namespace BurningKnight.level.biome {
	public class LibraryBiome : Biome {
		public LibraryBiome() : base("Hidden knowledge", Biome.Library, "library_biome", new Color(28, 18, 28)) {
		}

		public override void ModifyPainter(Level level, Painter painter) {
			base.ModifyPainter(level, painter);

			painter.Grass = 0;
			painter.Water = 0;
			painter.Dirt = 0;
			painter.Cobweb = 0.3f;
		}

		public override void ModifyRooms(List<RoomDef> rooms) {
			base.ModifyRooms(rooms);

			if (Run.Depth % 2 == 0) {
				rooms.Add(RoomRegistry.Generate(RoomType.Treasure, this));
			}
		}

		public override int GetNumRegularRooms() {
			return base.GetNumRegularRooms() * 2;
		}

		public override Builder GetBuilder() {
			var builder = new LoopBuilder().SetShape(2,
				Rnd.Float(0.4f, 0.7f),
				Rnd.Float(0f, 0.5f));

			return builder;
		}
	}
}