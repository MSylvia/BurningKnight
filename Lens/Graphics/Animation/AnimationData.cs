﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace Lens.Graphics.Animation {
	public class AnimationData {
		public Dictionary<string, List<AnimationFrame>> Layers = new Dictionary<string, List<AnimationFrame>>();
		public Dictionary<string, AnimationTag> Tags = new Dictionary<string, AnimationTag>();
		public Texture2D Texture;
		
		public AnimationTag? GetTag(string tagName) {
			AnimationTag tag;

			if (tagName == null) {
				tag = Tags.FirstOrDefault().Value;
			} else if (!Tags.TryGetValue(tagName, out tag)) {
				return null;
			}

			return tag;
		}
		
		public AnimationFrame? GetFrame(string layer, uint id) {
			List<AnimationFrame> frames;

			if (layer == null) {
				frames = Layers.FirstOrDefault().Value;
			} else if (!Layers.TryGetValue(layer, out frames)) {
				return null;
			}

			if (frames.Count < id) {
				return null;
			}

			return frames[(int) id];
		}

		public Animation CreateAnimation() {
			return new Animation(this);			
		}
	}
}