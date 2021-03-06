using System;
using Lens.entity.component;

namespace BurningKnight.entity.component {
	public class ShadowComponent : Component {
		public Action Callback;
		
		public ShadowComponent(Action render = null) {
			Callback = render ?? (() => {
				Entity.GraphicsComponent.Render(true);
			});
		}

		public override void Init() {
			base.Init();
			Entity.AddTag(Tags.HasShadow);
		}

		public override void Destroy() {
			base.Destroy();
			Entity.RemoveTag(Tags.HasShadow);
		}
	}
}