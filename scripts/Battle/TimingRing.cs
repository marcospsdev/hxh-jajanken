using Godot;

namespace HxH.Battle;

/// <summary>Comando cronometrado: um anel se fecha sobre o alvo; apertar Enter/Espaço no encontro = PERFEITO.</summary>
public partial class TimingRing : Node2D
{
    [Signal] public delegate void FinishedEventHandler(bool perfect);

    public const float Window = 0.1f;
    const float R0 = 56f, R1 = 14f;

    float duration = 0.7f, t;
    bool done;
    Color color = new(1f, 0.85f, 0.25f);
    public string Caption = "";

    public TimingRing() { }
    public TimingRing(float duration, string caption = "") { this.duration = duration; Caption = caption; }

    public static bool IsPerfect(float pressTime, float duration) => Mathf.Abs(pressTime - duration) <= Window;

    public override void _Process(double delta)
    {
        if (done) return;
        t += (float)delta;
        QueueRedraw();
        if (t > duration + Window) Finish(false);
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (done || !e.IsActionPressed("ui_accept")) return;
        GetViewport().SetInputAsHandled();
        Finish(IsPerfect(t, duration));
    }

    void Finish(bool perfect)
    {
        done = true;
        color = perfect ? new Color(0.3f, 1f, 0.4f) : new Color(1f, 0.3f, 0.3f);
        QueueRedraw();
        EmitSignal(SignalName.Finished, perfect);
        var tw = CreateTween();
        tw.TweenProperty(this, "modulate:a", 0f, 0.3f);
        tw.TweenCallback(Callable.From(QueueFree));
    }

    public override void _Draw()
    {
        DrawArc(Vector2.Zero, R1, 0, Mathf.Tau, 32, new Color(1, 1, 1, 0.9f), 2);
        float k = Mathf.Clamp(t / duration, 0f, 1.3f);
        float r = Mathf.Max(4f, Mathf.Lerp(R0, R1, k));
        bool inWindow = IsPerfect(t, duration);
        DrawArc(Vector2.Zero, done ? R1 + 3 : r, 0, Mathf.Tau, 48, done ? color : (inWindow ? Colors.White : color), 3);
        if (Caption != "")
            DrawString(ThemeDB.FallbackFont, new Vector2(-60, -R0 - 6), Caption, HorizontalAlignment.Center, 120, 14, Colors.White);
    }
}
