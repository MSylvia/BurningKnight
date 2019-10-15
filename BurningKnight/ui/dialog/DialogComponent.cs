using BurningKnight.assets;
using BurningKnight.entity.component;
using BurningKnight.entity.creature.player;
using ImGuiNET;
using Lens;
using Lens.assets;
using Lens.entity;
using Lens.entity.component;
using Lens.graphics;
using Lens.util.tween;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BurningKnight.ui.dialog {
	public delegate void DialogCallback(DialogComponent d);
	
	public class DialogComponent : Component {
		public UiDialog Dialog;
		public Dialog Last;
		public Dialog Current;

		public DialogCallback OnNext;
		public Entity To;

		private bool added;
		private float tillClose = -1;

		private void HandleInput(object sender, TextInputEventArgs args) {
			if (Current is AnswerDialog a) {
				if (a.Focused) {
					a.HandleInput(args);
				}
			}
		}

		public override void Init() {
			base.Init();
			
			Engine.Instance.Window.TextInput += HandleInput;

			Dialog = new UiDialog();
			Dialog.Owner = Entity;
			
			Dialog.OnEnd += () => {
				Dialog next = null;
				
				foreach (var c in Current.Callbacks) {
					var d = c(Current, this);

					if (d != null) {
						next = d;
					}
				}

				if (next == null) {
					next = Current.GetNext();
				}
				
				Current.Reset();

				if (next == null) {
					Last = Current;
					Current = null;
					OnNext?.Invoke(this);

					if (To != null) {
						var input = To.GetComponent<PlayerInputComponent>();

						input.InDialog = false;
						input.Dialog = null;
							
						To = null;
					}
						
					return true;
				}

				Setup(next);
				OnNext?.Invoke(this);
				return false;
			};
		}

		public override void Destroy() {
			base.Destroy();
			
			Dialog.Close(() => { Dialog.Done = true; });
			Engine.Instance.Window.TextInput -= HandleInput;
		}

		private bool tweening;

		public override void Update(float dt) {
			base.Update(dt);

			if (tillClose > -1 && Dialog.DoneSaying) {
				tillClose -= dt;

				if (tillClose <= 0) {
					Close();
				}
			}
			
			if (added) {
				return;
			}

			Engine.Instance.State.Ui.Add(Dialog);

			Dialog.Str.CharTyped += (s, i, c) => {
				if (!tweening && c != ' ' && Entity.TryGetComponent<AnimationComponent>(out var a)) {
					tweening = true;
					
					Tween.To(1.3f, a.Scale.X, x => a.Scale.X = x, 0.15f);
					Tween.To(0.75f, a.Scale.Y, x => a.Scale.Y = x, 0.15f).OnEnd = () => {
						Tween.To(1, a.Scale.X, x => a.Scale.X = x, 0.1f);
						Tween.To(1, a.Scale.Y, x => a.Scale.Y = x, 0.1f);

						tweening = false;
					};
				}
			};
			
			added = true;
		}

		public void Start(string id, Entity to = null) {
			var dialog = Dialogs.Get(id);

			if (dialog == null) {
				Setup(new Dialog(id), to);
				return;
			}

			Setup(dialog, to);
		}

		public void StartAndClose(string id, float time, Entity to = null) {
			Start(id, to);
			tillClose = time;
		}

		public void Close() {
			Last = Current;
			tillClose = -1;

			if (Dialog == null || Current == null) {
				Current = null;
				return;
			}

			Current = null;
			Dialog.Close();
		}

		private static string toSay;

		public override void RenderDebug() {
			base.RenderDebug();

			if (ImGui.InputText("Say", ref toSay, 256, ImGuiInputTextFlags.EnterReturnsTrue)) {
				Start(toSay);
				toSay = "";
			}

			if (ImGui.Button("Close")) {
				Close();
			}
		}

		private void Setup(Dialog dialog, Entity to = null) {
			Last = Current;
			Current = dialog;

			var c = Locale.Get(dialog.Id);
			var s = dialog.Modify(c);
			
			Dialog.Say(s);
			
			if (Dialog.Str != null) {
				Dialog.Str.Renderer = RenderChoice;
			}

			if (to is Player p) {
				var input = p.GetComponent<PlayerInputComponent>();

				input.InDialog = true;
				input.Dialog = this;

				To = to;
			}
		}

		private void RenderChoice(Vector2 pos, int i) {
			if (Current is ChoiceDialog c) {
				if (i == c.Choice) {
					Graphics.Print(">", Font.Small, pos);
				}
			} else if (Current is AnswerDialog a) {
				Graphics.Print($"{a.Answer}{(a.Focused && Engine.Time % 0.8f > 0.4f ? "|" : "")}", Font.Small, pos);
			}
		}
	}
}