using System;
using System.Threading;
using BurningKnight.assets;
using BurningKnight.assets.lighting;
using BurningKnight.level.tile;
using BurningKnight.physics;
using BurningKnight.save;
using BurningKnight.ui.imgui;
using BurningKnight.util;
using Lens;
using Lens.assets;
using Lens.entity;
using Lens.game;
using Lens.graphics;
using Microsoft.Xna.Framework;
using Random = Lens.util.math.Random;
using Console = BurningKnight.debug.Console;

namespace BurningKnight.state {
	public class LoadState : GameState {
		public string Path;
		private Area gameArea;
		private bool ready;
		private bool down;
		private float alpha;
		private string title;
		private string prefix;
		private float titleX;
		private float prefixX;
		
		public override void Init() {
			base.Init();

			// todo: generating too
			prefix = Locale.Get("loading");
			title = LoadScreenTitles.Generate();
			
			Lights.Init();
			Physics.Init();
			gameArea = new Area();

			Run.Level = null;

			var thread = new Thread(() => {
				Tilesets.Load();
				
				SaveManager.Load(gameArea, SaveType.Game, Path);

				Random.Seed = $"{Run.Seed}_{Run.Depth}"; 
				
				SaveManager.Load(gameArea, SaveType.Level, Path);

				if (Run.Depth > 0) {
					SaveManager.Load(gameArea, SaveType.Player, Path);
				} else {
					SaveManager.Generate(gameArea, SaveType.Player);
				}

				ready = true;
			});

			thread.Priority = ThreadPriority.Lowest;
			thread.Start();

			titleX = Font.Small.MeasureString(title).Width * -0.5f;
			prefixX = Font.Medium.MeasureString($"{prefix} 102%").Width * -0.5f;
		}

		public override void Update(float dt) {
			base.Update(dt);

			if (down) {
				if (ready && ((Engine.Version.Debug) || Time > 3f)) {
					alpha -= dt * 5;
				}
			} else {
				alpha = Math.Min(1, alpha + dt * 5);

				if (alpha >= 0.95f) {
					alpha = 1;
					down = true;
				}
			}

			if (ready && ((down && alpha < 0.05f) || (Engine.Version.Debug))) {
				Engine.Instance.SetState(new InGameState(gameArea));
			}
		}

		public override void RenderUi() {
			base.RenderUi();
			
			Graphics.Color = new Color(1f, 1f, 1f, alpha);
			Graphics.Print($"{prefix} {Math.Min(102, Math.Floor(Time / 3f * 100f))}%", Font.Medium, new Vector2(Display.UiWidth / 2f + prefixX, Display.UiHeight / 2f - 8));
			Graphics.Print(title, Font.Small, new Vector2(Display.UiWidth / 2f + titleX, Display.UiHeight / 2f + 8));
			Graphics.Color = ColorUtils.WhiteColor;
		}

		public override void RenderNative() {
			ImGuiHelper.Begin();
		
			if (Console.Open) {
				DebugWindow.Render();
			}

			ImGuiHelper.End();
		}
	}
}