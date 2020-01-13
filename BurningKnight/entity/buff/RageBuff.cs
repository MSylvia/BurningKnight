using BurningKnight.entity.events;
using BurningKnight.entity.item.util;
using Lens.entity;

namespace BurningKnight.entity.buff {
	public class RageBuff : Buff {
		public const string Id = "bk:rage";
		
		public RageBuff() : base(Id) {
			Duration = 10;
		}

		public override string GetIcon() {
			return "rage";
		}

		public override void HandleEvent(Event e) {
			if (e is MeleeArc.CreatedEvent meae) {
				meae.Arc.Damage *= 2;
			} else if (e is ProjectileCreatedEvent pce) {
				pce.Projectile.Damage *= 2;
			}
			
			base.HandleEvent(e);
		}
	}
}