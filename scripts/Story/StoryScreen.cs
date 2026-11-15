using System.Collections.Generic;
using Godot;
using HxH.UI;

namespace HxH.Story;

/// <summary>Tela de história: retrato, nome e caixa de texto com efeito de máquina de escrever.</summary>
public partial class StoryScreen : Node2D
{
    [Signal] public delegate void FinishedEventHandler();

    const float CharsPerSecond = 45f;
    List<StoryLine> lines = new();
    int index = -1;
    float elapsed;
    Label nameLabel, textLabel, hint;
    Sprite2D portrait;

    public StoryScreen() { }
    public StoryScreen(List<StoryLine> lines) { this.lines = lines; }

    public override void _Ready()
    {
        AddChild(new Background());
        UiKit.Box(this, new Rect2(0, 0, 640, 360), new Color(0, 0, 0, 0.45f));
        portrait = new Sprite2D { Position = new Vector2(95, 225) };
        AddChild(portrait);
        UiKit.Box(this, new Rect2(170, 236, 460, 112), new Color(0.04f, 0.04f, 0.08f, 0.92f));
        UiKit.Box(this, new Rect2(170, 236, 460, 2), new Color(0.3f, 0.9f, 0.45f));
        nameLabel = UiKit.Label(this, "", new Vector2(180, 242), new Vector2(440, 16), 12, new Color(0.45f, 1f, 0.55f));
        textLabel = UiKit.Label(this, "", new Vector2(180, 262), new Vector2(440, 80), 11);
        hint = UiKit.Label(this, "[Enter]", new Vector2(560, 330), new Vector2(64, 14), 9, new Color(1, 1, 1, 0.6f), HorizontalAlignment.Right);
        Next();
    }

    void Next()
    {
        index++;
        if (index >= lines.Count) { EmitSignal(SignalName.Finished); return; }
        var line = lines[index];
        nameLabel.Text = line.Speaker;
        textLabel.Text = line.Text;
        textLabel.VisibleCharacters = 0;
        elapsed = 0;
        if (string.IsNullOrEmpty(line.Portrait)) portrait.Visible = false;
        else
        {
            portrait.Visible = true;
            portrait.Texture = PixelArt.Get(line.Portrait);
            float s = PixelArt.ScaleFor(portrait.Texture, 150);
            portrait.Scale = new Vector2(s, s);
        }
    }

    bool Typing => textLabel.VisibleCharacters >= 0 && textLabel.VisibleCharacters < textLabel.GetTotalCharacterCount();

    public override void _Process(double delta)
    {
        if (index >= lines.Count || !Typing) { hint.Visible = true; return; }
        hint.Visible = false;
        elapsed += (float)delta;
        textLabel.VisibleCharacters = (int)(elapsed * CharsPerSecond);
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (!e.IsActionPressed("ui_accept")) return;
        GetViewport().SetInputAsHandled();
        if (Typing) textLabel.VisibleCharacters = -1;
        else Next();
    }
}
