﻿using Lens.util.math;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Lens.graphics.animation {
	public delegate void AnimationCallback();
	
	public class Animation {
		public AnimationData Data;
		public AnimationCallback OnEnd;
		
		private AnimationFrame frame;
		
		private uint currentFrame;
		public uint StartFrame { get; private set; }
		public uint EndFrame { get; private set; }
		
		public float SpeedModifier = 1f;
		public bool Paused;
		public bool AutoStop;

		public uint Frame {
			get => currentFrame;
			set {
				currentFrame = value % (EndFrame - StartFrame + 1);
				ReadFrame();
			}
		}

		private string layer;
		private string tag;

		public string Layer {
			get => layer;

			set {
				if (layer == value) {
					return;
				}
				
				layer = value;
				ReadFrame();
			}
		}
		
		public string Tag {
			get => tag;

			set {
				currentFrame = 0;
				tag = value;
				Paused = false;
				
				ReadFrame();
			}
		}
		
		private float timer;

		public float Timer {
			get => timer;

			set {
				timer = value;

				if (timer >= frame.Duration) {
					timer = 0;

					if (!AutoStop || currentFrame < EndFrame - StartFrame) {
						Frame++;
					
						if (SkipNextFrame) {
							SkipNextFrame = false;
							Frame++;
						}

						ReadFrame();
					} else {
						Paused = true;
						OnEnd?.Invoke();
					}
				}
			}
		}

		public bool PingGoingForward;
		public bool SkipNextFrame;
		
		public Animation(AnimationData data, string layer = null) {
			Data = data;

			if (layer != null) {
				Layer = layer;
			} else {
				ReadFrame(true);				
			}
		}

		public void Update(float dt) {
			if (!Paused) {
				var frame = currentFrame;
				Timer += dt * SpeedModifier;
				var newFrame = currentFrame;

				if (frame != newFrame && newFrame == 0) {
					OnEnd?.Invoke();	
				}
			}
		}

		public void Render(Vector2 position, bool flipped = false, bool vflip = false) {
			Graphics.Render(frame.Texture, position, 0, Vector2.Zero, Vector2.One, Graphics.ParseEffect(flipped, vflip));
		}

		public TextureRegion GetCurrentTexture() {
			return frame.Texture;
		}
		
		public void Reset() {
			Frame = 0;
		}

		public bool HasTag(string tag) {
			return Data.Tags.ContainsKey(tag);
		}

		private void ReadFrame(bool rand = false) {
			var nullableTag = Data.GetTag(tag);

			if (nullableTag == null) {
				return;
			}

			var currentTag = nullableTag.Value;

			StartFrame = currentTag.StartFrame;
			EndFrame = currentTag.EndFrame;
			
			if (rand) {
				currentFrame = (uint) Random.Int(0, (int) (currentTag.EndFrame - currentTag.StartFrame));
			}
			
			var frame = Data.GetFrame(layer, currentTag.Direction.GetFrameId(this));

			if (frame != null) {
				this.frame = (AnimationFrame) frame;
			}
		}
	}
}