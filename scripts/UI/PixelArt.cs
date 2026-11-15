using System.Collections.Generic;
using System.Linq;
using Godot;

namespace HxH.UI;

/// <summary>
/// Sprites dos personagens. Se existir res://assets/sprites/{id}.png (sprite real, virado para a direita),
/// ele é usado. Caso contrário, gera um sprite PROVISÓRIO em pixel art a partir dos mapas abaixo.
/// </summary>
public static class PixelArt
{
    static readonly Dictionary<char, Color> Palette = new()
    {
        ['K'] = new Color("1a1420"), // cabelo preto / contorno
        ['S'] = new Color("f2c29b"), // pele
        ['s'] = new Color("d39a74"), // pele sombra
        ['G'] = new Color("2f9e4f"), // verde (roupa do Gon)
        ['g'] = new Color("1d6b34"), // verde escuro
        ['B'] = new Color("7a4a26"), // botas
        ['E'] = new Color("3a2a1a"), // olhos
        ['R'] = new Color("c24a4a"), // boca / olhos da Pitou
        ['W'] = new Color("f4f1ec"), // cabelo branco da Pitou
        ['w'] = new Color("cfd6e0"), // roupa clara
        ['N'] = new Color("2d3354"), // calça azul-marinho
        ['P'] = new Color("8a5bb8"), // detalhe roxo
        ['Y'] = new Color("ffd75a"),
    };

    static readonly Dictionary<string, string[]> Maps = new()
    {
        ["gon"] = new[]
        {
            "....K...K..K.......",
            "...KK..KK.KK.......",
            "...KKKKKKKKKK.K....",
            "..KKKKKKKKKKKKK....",
            "..KKKKKKKKKKKKK....",
            "...KKSSSSSSSKKK....",
            "...KSSESSSESSKK....",
            "...KSSESSSESSK.....",
            "....SSSSSSSSSs.....",
            "....SSSSRRSSSs.....",
            ".....SSSSSSSs......",
            "......sSSSSs.......",
            "....GGGGGGGGGG.....",
            "...GGGGgGGGGGGG....",
            "..SGGGGgGGGGGGGS...",
            "..SGGGGgGGGGGGGS...",
            "..SGGGGgGGGGGGGS...",
            "..SSGGGgGGGGGGSS...",
            "...KKKKKKKKKKKK....",
            "....GGGGG.GGGGG....",
            "....GGGGG.GGGGG....",
            "....gGGG...GGGg....",
            ".....SSS...SSS.....",
            ".....SSS...SSS.....",
            ".....sSS...SSs.....",
            "....BBBB...BBBB....",
            "...BBBBB...BBBBB...",
        },
        ["gon_adulto"] = new[]
        {
            "........K.K.K..........",
            ".......KKKKKKKK........",
            "......KKKKKKKKKK.......",
            ".....KKKKKKKKKKKK......",
            ".....KKKSSSSSSKKKK.....",
            ".....KKSESSSSESKKK.....",
            ".....KKSSSSSSSSKKK.....",
            ".....KKSSSRRSSSKKK.....",
            "....KKKKSSSSSSKKKKK....",
            "...KKKKSSSSSSSSKKKKK...",
            "...KKKSSSSSSSSSSKKKK...",
            "..KKKSSSsSSSSsSSSKKKK..",
            "..KKSSSSSSSSSSSSSSKKK..",
            "..KKSSSSSsSSsSSSSSSKK..",
            "..KSSSSSSSSSSSSSSSSSK..",
            "..SS.SSSSsSSsSSSS.SSK..",
            "..SS.SSSSSSSSSSSS.SSK..",
            "..SS.SSSSsSSsSSSS.SSK..",
            "..SS..SSSSSSSSSS..SSK..",
            "..SS..KKKKKKKKKK..SS...",
            "......GGGGGGGGGG.......",
            "......GGGG..GGGG.......",
            "......GgGG..GGgG.......",
            "......SSSS..SSSS.......",
            "......SSSS..SSSS.......",
            "......SSSS..SSSS.......",
            "......sSSS..SSSs.......",
            ".....BBBBB..BBBBB......",
            "....BBBBBB..BBBBBB.....",
        },
        ["pitou"] = new[]
        {
            "...W..........W.....",
            "..WWW........WWW....",
            "..WWWWWWWWWWWWWW....",
            ".WWWWWWWWWWWWWWWW...",
            ".WWWWWWWWWWWWWWWW...",
            ".WWWSSSSSSSSSSWWW...",
            ".WWSSRRSSSSRRSSWW...",
            ".WWSSRSSSSSSRSSWW...",
            ".WWSSSSSSSSSSSSWW...",
            "..WSSSSSKKSSSSSW....",
            "..W.SSSSSSSSSS.W....",
            "..W..sSSSSSSs..W....",
            "....wwwwwwwwww......",
            "...wwwwPPPPwwwww....",
            "..SwwwwwwwwwwwwwS...",
            "..SwwwwwwwwwwwwwS...",
            "..SSwwwwwwwwwwwSS...",
            "....KKKKKKKKKK......",
            "....NNNN..NNNN......",
            "....NNNN..NNNN...WW.",
            "....NNN....NNN..W..W",
            "....NNN....NNN.W....",
            "....NNN....NNN.W....",
            "....sSS....SSs.W....",
            "...KKKK....KKKKW....",
            "...KKKK....KKKK.....",
        },
        ["komugi"] = new[]
        {
            "....KKKKKK....",
            "...KKKKKKKK...",
            "..KKSSSSSSKK..",
            "..KSEESSEESK..",
            "..KSSSSSSSSK..",
            "..KSSSSRSSSK..",
            "...SSSSSSSS...",
            "...PPPPPPPP...",
            "..PPPPPPPPPP..",
            "..SPPPPPPPPS..",
            "..SPPPPPPPPS..",
            "...PPPPPPPP...",
            "...PPPPPPPP...",
            "....SS..SS....",
            "...KKK..KKK...",
        },
    };

    static readonly Dictionary<string, Texture2D> Cache = new();

    public static string RealPath(string id) => $"res://assets/sprites/{id}.png";
    public static bool IsPlaceholder(string id) => !ResourceLoader.Exists(RealPath(id));

    public static Texture2D Get(string id)
    {
        if (Cache.TryGetValue(id, out var cached)) return cached;
        Texture2D tex = !IsPlaceholder(id) ? GD.Load<Texture2D>(RealPath(id)) : Build(Maps[id]);
        Cache[id] = tex;
        return tex;
    }

    /// <summary>Escala inteira (sempre Nearest) para o sprite ficar perto da altura desejada.</summary>
    public static float ScaleFor(Texture2D tex, float targetHeight) =>
        Mathf.Max(1, Mathf.Round(targetHeight / tex.GetHeight()));

    static Texture2D Build(string[] rows)
    {
        int w = rows.Max(r => r.Length) + 2, h = rows.Length + 2;
        var img = Image.CreateEmpty(w, h, false, Image.Format.Rgba8);
        img.Fill(new Color(0, 0, 0, 0));
        for (int y = 0; y < rows.Length; y++)
            for (int x = 0; x < rows[y].Length; x++)
                if (Palette.TryGetValue(rows[y][x], out var c)) img.SetPixel(x + 1, y + 1, c);

        // contorno escuro de 1px ao redor dos pixels opacos
        var outline = new Color("0d0a12");
        var copy = (Image)img.Duplicate();
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                if (copy.GetPixel(x, y).A > 0) continue;
                bool near = (x > 0 && copy.GetPixel(x - 1, y).A > 0) || (x < w - 1 && copy.GetPixel(x + 1, y).A > 0)
                         || (y > 0 && copy.GetPixel(x, y - 1).A > 0) || (y < h - 1 && copy.GetPixel(x, y + 1).A > 0);
                if (near) img.SetPixel(x, y, outline);
            }
        return ImageTexture.CreateFromImage(img);
    }
}
