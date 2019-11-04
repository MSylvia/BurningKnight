using BurningKnight.entity.component;
using BurningKnight.entity.creature.drop;
using BurningKnight.entity.creature.player;
using Lens.entity;
using Lens.util.math;

namespace BurningKnight.level.entities.chest {
	public class GoldChest : Chest {
		public GoldChest() {
			Sprite = "gold_chest";
		}
	
		public override void AddComponents() {
			base.AddComponents();

			var drops = GetComponent<DropsComponent>();
			
			drops.Add(new OneOfDrop(
				new SingleDrop("bk:halo"),
				new SingleDrop("bk:wings"),
				new SingleDrop("bk:potatoo")	
			));
		}

		protected override bool TryOpen(Entity entity) {
			if (!entity.TryGetComponent<ConsumablesComponent>(out var c) || c.Keys < 1) {
				return false;
			}

			c.Keys--;
			return true;
		}

		protected override void SpawnDrops() {
			if (Random.Chance(5)) {
				var chest = Random.Chance(60) ? (Chest) new WoodenChest {
					Scale = Scale * 0.9f
				} : (Chest) new GoldChest {
					Scale = Scale * 0.9f
				};

				Area.Add(chest);
				chest.TopCenter = BottomCenter;
				
				return;
			}
			
			base.SpawnDrops();
		}
	}
}