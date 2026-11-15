using System.Collections.Generic;
using System.Text.Json;
using Godot;

namespace HxH.Story;

public class StoryLine
{
    public string Speaker { get; set; } = "";
    public string Portrait { get; set; } = "";
    public string Text { get; set; } = "";
}

public static class StoryData
{
    public const string Path = "res://data/story.json";
    static Dictionary<string, List<StoryLine>> data;

    public static Dictionary<string, List<StoryLine>> All()
    {
        if (data != null) return data;
        string json = FileAccess.GetFileAsString(Path);
        data = JsonSerializer.Deserialize<Dictionary<string, List<StoryLine>>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return data;
    }

    public static List<StoryLine> Get(string key) => All().TryGetValue(key, out var l) ? l : new List<StoryLine>();
}
