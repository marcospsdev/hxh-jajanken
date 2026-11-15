using System;
using Godot;
using HxH.Battle;
using HxH.Story;
using HxH.UI;

namespace HxH;

/// <summary>Fluxo: Título → Introdução → Batalha → (Transformação dentro da batalha) → Vitória → Créditos.</summary>
public partial class Main : Node
{
    Node current;

    public override void _Ready() => ShowTitle();

    void Swap(Node next)
    {
        current?.QueueFree();
        current = next;
        AddChild(next);
    }

    void ShowTitle()
    {
        var t = new ChoiceScreen("JAJANKEN", "Hunter x Hunter: Gon vs. Neferpitou", new[] { "Novo jogo", "Sair" }, true);
        t.Chosen += i => { if (i == 0) ShowStory("intro", ShowBattle); else GetTree().Quit(); };
        Swap(t);
    }

    void ShowStory(string key, Action next)
    {
        var s = new StoryScreen(StoryData.Get(key));
        s.Finished += () => next();
        Swap(s);
    }

    void ShowBattle()
    {
        var b = new BattleScene();
        b.BattleEnded += won => { if (won) ShowStory("victory", () => ShowStory("credits", ShowTitle)); else ShowDefeat(); };
        Swap(b);
    }

    void ShowDefeat()
    {
        var d = new ChoiceScreen("DERROTA", "Gon caiu diante de Neferpitou...", new[] { "Tentar novamente", "Voltar ao título" }, false);
        d.Chosen += i => { if (i == 0) ShowBattle(); else ShowTitle(); };
        Swap(d);
    }
}
