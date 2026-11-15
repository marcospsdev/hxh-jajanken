using Godot;
using HxH.UI;

namespace HxH.UI;

/// <summary>Tela de título / derrota: título e lista de opções navegável com as setas.</summary>
public partial class ChoiceScreen : Node2D
{
    [Signal] public delegate void ChosenEventHandler(int index);

    string title = "", subtitle = "";
    string[] options = { "OK" };
    bool showFighters;
    int selected;
    Label optionsLabel;

    public ChoiceScreen() { }
    public ChoiceScreen(string title, string subtitle, string[] options, bool showFighters)
    {
        this.title = title; this.subtitle = subtitle; this.options = options; this.showFighters = showFighters;
    }

    public override void _Ready()
    {
        AddChild(new Background());
        if (showFighters)
        {
            AddFighter("gon", new Vector2(150, 272), false);
            AddFighter("pitou", new Vector2(490, 272), true);
        }
        UiKit.Box(this, new Rect2(0, 0, 640, 360), new Color(0, 0, 0, 0.35f));
        UiKit.Label(this, title, new Vector2(0, 50), new Vector2(640, 40), 28, new Color(0.5f, 1f, 0.6f), HorizontalAlignment.Center);
        UiKit.Label(this, subtitle, new Vector2(0, 96), new Vector2(640, 20), 12, new Color(1, 1, 1, 0.85f), HorizontalAlignment.Center);
        optionsLabel = UiKit.Label(this, "", new Vector2(220, 170), new Vector2(200, 100), 14, null, HorizontalAlignment.Center);
        UiKit.Label(this, "Setas: navegar   Enter/Espaço: confirmar", new Vector2(0, 338), new Vector2(640, 14), 9,
            new Color(1, 1, 1, 0.6f), HorizontalAlignment.Center);
        Render();
    }

    void AddFighter(string id, Vector2 pos, bool flip)
    {
        var tex = PixelArt.Get(id);
        float s = PixelArt.ScaleFor(tex, 96);
        AddChild(new Sprite2D { Texture = tex, Position = pos, Scale = new Vector2(s, s), FlipH = flip, Offset = new Vector2(0, -tex.GetHeight() / 2f) });
    }

    void Render()
    {
        var lines = new string[options.Length];
        for (int i = 0; i < options.Length; i++) lines[i] = i == selected ? $"> {options[i]} <" : options[i];
        optionsLabel.Text = string.Join("\n", lines);
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsActionPressed("ui_down")) { selected = (selected + 1) % options.Length; Render(); }
        else if (e.IsActionPressed("ui_up")) { selected = (selected + options.Length - 1) % options.Length; Render(); }
        else if (e.IsActionPressed("ui_accept")) { GetViewport().SetInputAsHandled(); EmitSignal(SignalName.Chosen, selected); }
    }
}
