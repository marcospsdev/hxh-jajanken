using Godot;

namespace HxH.UI;

/// <summary>Barra de HP/Aura com animação suave.</summary>
public partial class Bar : Control
{
    public float Value = 1f;
    public Color Fill = new(0.3f, 0.9f, 0.4f);
    float shown = 1f;

    public override void _Ready() => MouseFilter = MouseFilterEnum.Ignore;

    public override void _Process(double delta)
    {
        shown = Mathf.MoveToward(shown, Mathf.Clamp(Value, 0, 1), (float)delta * 1.2f);
        QueueRedraw();
    }

    public override void _Draw()
    {
        DrawRect(new Rect2(Vector2.Zero, Size), new Color(0, 0, 0, 0.8f));
        DrawRect(new Rect2(1, 1, (Size.X - 2) * shown, Size.Y - 2), Fill);
        DrawRect(new Rect2(Vector2.Zero, Size), new Color(1, 1, 1, 0.5f), false, 1);
    }
}

