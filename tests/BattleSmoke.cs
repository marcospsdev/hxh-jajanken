using Godot;
using HxH.Battle;

namespace HxH.Tests;

/// <summary>Teste de fumaça: roda a batalha real apertando Enter periodicamente. godot --headless --path . res://tests/BattleSmoke.tscn</summary>
public partial class BattleSmoke : Node
{
    double t, next;
    int presses;
    bool ended;

    public override void _Ready()
    {
        BattleScene.Verbose = true;
        var b = new BattleScene();
        b.BattleEnded += won => { ended = true; GD.Print($"SMOKE: batalha terminou, vitória={won}, apertos={presses}"); GetTree().Quit(0); };
        AddChild(b);
    }

    public override void _Process(double delta)
    {
        t += delta;
        if (t > 240) { GD.PrintErr("SMOKE: tempo esgotado"); GetTree().Quit(1); }
        if (t < next || ended) return;
        next = t + 0.35;
        presses++;
        string act = presses % 5 == 0 ? "ui_down" : "ui_accept";
        var e = new InputEventAction { Action = act, Pressed = true };
        Input.ParseInputEvent(e);
        Input.ParseInputEvent(new InputEventAction { Action = act, Pressed = false });
    }
}
