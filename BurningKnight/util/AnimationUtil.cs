using System;
using BurningKnight.assets.lighting;
using BurningKnight.assets.particle;
using BurningKnight.assets.particle.custom;
using BurningKnight.entity.component;
using BurningKnight.entity.creature.player;
using BurningKnight.state;
using Lens.assets;
using Lens.entity;
using Lens.util;
using Lens.util.camera;
using Lens.util.math;
using Lens.util.tween;
using Microsoft.Xna.Framework;

namespace BurningKnight.util {
	public static class AnimationUtil {
		public static void ActionFailed() {
			Camera.Instance.Shake(10);
			Audio.PlaySfx("item_nocash");
		}

		public static void Poof(Vector2 where, int depth = 0) {
			if (Settings.LowQuality) {
				return;
			}
			
			for (var i = 0; i < 4; i++) {
				var part = new ParticleEntity(Particles.Dust());
						
				part.Position = where;
				part.Particle.Scale = Rnd.Float(0.4f, 0.8f);
				Run.Level.Area.Add(part);
				part.Depth = depth;
			}
		}

		public static void Confetti(Vector2 where) {
			if (Settings.LowQuality) {
				return;
			}
			
			for (var i = 0; i < 15; i++) {
				var p = Run.Level.Area.Add(new ConfettiParticle());
				p.Center = where;
			}
		}
		
		public static void Ash(Vector2 where, int depth = 0) {
			if (Settings.LowQuality) {
				return;
			}
			
			for (var i = 0; i < 4; i++) {
				var part = new ParticleEntity(Particles.Ash());
						
				part.Position = where;
				part.Particle.Scale = Rnd.Float(1f, 2f);
				part.Particle.Velocity = new Vector2(Rnd.Float(20, 30) * (Rnd.Chance() ? -1 : 1), -Rnd.Float(40, 66));
				Run.Level.Area.Add(part);
				part.Depth = depth;
			}
		}
		
		public static void PoofFrom(Vector2 where, Vector2 from, int depth = 0) {
			for (var i = 0; i < 4; i++) {
				var part = new ParticleEntity(Particles.Dust());
						
				part.Position = where;
				part.Particle.Scale = Rnd.Float(0.4f, 0.8f);
				Run.Level.Area.Add(part);
				part.Depth = depth;
				part.Particle.Velocity = MathUtils.CreateVector((where - from).ToAngle(), 80);
			}
		}

		public static void Explosion(Vector2 where, float scale = 1) {
			var explosion = new ParticleEntity(Particles.Animated("explosion", "explosion"));
			explosion.Position = where;
			Run.Level.Area.Add(explosion);
			explosion.Depth = 32;
			explosion.Particle.Velocity = Vector2.Zero;
			explosion.Particle.Scale = scale;
			explosion.AddShadow();

			Lights.Flash = 1f;
		}

		public static void TeleportAway(Entity entity, Action callback) {
			var scale = entity is Player
				? entity.GetComponent<PlayerGraphicsComponent>().Scale
				: entity.GetAnyComponent<MobAnimationComponent>().Scale;

			if (!entity.HasComponent<ZComponent>()) {
				entity.AddComponent(new ZComponent());
			}
			
			var z = entity.GetComponent<ZComponent>();
			
			entity.GetComponent<HealthComponent>().Unhittable = true;

			Tween.To(0, (entity is Player
				? entity.GetComponent<PlayerGraphicsComponent>().Scale
				: entity.GetAnyComponent<MobAnimationComponent>().Scale).X, x => {

				if (entity is Player) {
					entity.GetComponent<PlayerGraphicsComponent>().Scale.X = x;
				} else {
					entity.GetAnyComponent<MobAnimationComponent>().Scale.X = x;
				}
			}, 0.3f, Ease.QuadIn);
			Tween.To(4, (entity is Player
				? entity.GetComponent<PlayerGraphicsComponent>().Scale
				: entity.GetAnyComponent<MobAnimationComponent>().Scale).Y, x => {
				
				if (entity is Player) {
					entity.GetComponent<PlayerGraphicsComponent>().Scale.Y = x;
				} else {
					entity.GetAnyComponent<MobAnimationComponent>().Scale.Y = x;
				}
			}, 0.3f, Ease.QuadIn);
			
			Tween.To(128, z.Z, x => z.Z = x, 0.3f, Ease.QuadIn).OnEnd = callback;
		}

		public static void TeleportIn(Entity entity) {
			var scale = new Func<Vector2>(() => entity is Player
				? entity.GetComponent<PlayerGraphicsComponent>().Scale
				: entity.GetAnyComponent<MobAnimationComponent>().Scale);

			var z = entity.GetComponent<ZComponent>();
			
			entity.GetComponent<HealthComponent>().Unhittable = true;

			Tween.To(1.5f, scale().X, x => {
				if (entity is Player) {
					entity.GetComponent<PlayerGraphicsComponent>().Scale.X = x;
				} else {
					entity.GetAnyComponent<MobAnimationComponent>().Scale.X = x;
				}
			}, 0.3f);
			Tween.To(0.1f, scale().Y, x => {
				if (entity is Player) {
					entity.GetComponent<PlayerGraphicsComponent>().Scale.Y = x;
				} else {
					entity.GetAnyComponent<MobAnimationComponent>().Scale.Y = x;
				}
			}, 0.3f).OnEnd = () => {
				Tween.To(1, scale().X, x => {
					if (entity is Player) {
						entity.GetComponent<PlayerGraphicsComponent>().Scale.X = x;
					} else {
						entity.GetAnyComponent<MobAnimationComponent>().Scale.X = x;
					}
				}, 0.3f);
				Tween.To(1f, scale().Y, x => {
					if (entity is Player) {
						entity.GetComponent<PlayerGraphicsComponent>().Scale.Y = x;
					} else {
						entity.GetAnyComponent<MobAnimationComponent>().Scale.Y = x;
					}
				}, 0.3f);
			};
			
			Tween.To(0, z.Z, x => z.Z = x, 0.2f).OnEnd = () => {
				entity.GetComponent<HealthComponent>().Unhittable = false;
			};
		}
	}
}