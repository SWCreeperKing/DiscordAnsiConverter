using System.Text.RegularExpressions;
using TextCopy;

/*
its actually ansi but w.e.
site i use https://gist.github.com/kkrypt0nn/a02506f3712ff2d1c8ca7c9e0aed7c06
*/

Regex ColorRegex = new(@"\[(@|!|#)([a-zA-Z ]+?)\]", RegexOptions.Compiled);

Dictionary<string, int> ModifierDictionary = new()
    { ["normal"] = 0, ["bold"] = 1, ["underline"] = 4 };

Dictionary<string, int> FontColorDictionary = new()
{
    ["gray"] = 30, ["grey"] = 30, ["red"] = 31, ["green"] = 32, ["yellow"] = 33, ["blue"] = 34,
    ["pink"] = 35, ["ponk"] = 35, ["cyan"] = 36, ["teal"] = 36, ["white"] = 37
};

Dictionary<string, int> BackColorDictionary = new()
{
    ["darkblue"] = 40, ["orange"] = 41, ["blue"] = 42, ["bluegray"] = 43, ["bluegrey"] = 43,
    ["gray"] = 44, ["grey"] = 44, ["lightblue"] = 45, ["lightgray"] = 46, ["lightgrey"] = 46,
    ["white"] = 47, ["reset"] = -1, ["transparent"] = -1
};

void Output(string text) => ClipboardService.SetText($"```ansi\n{text}```");

string Encoder(string input)
{
    List<Text> outputRaw = new() { new Text() };

    var lastIndex = 0;
    int currentIndex;
    var match = ColorRegex.Match(input);
    while ((currentIndex = input.IndexOf(match.Value, lastIndex, StringComparison.Ordinal)) != -1)
    {
        var prevText = input[lastIndex..currentIndex];
        if (prevText is not "")
        {
            outputRaw[^1] = outputRaw[^1] with { Content = prevText };
            outputRaw.Add(outputRaw[^1] with { Content = "" });
        }

        var lastText = outputRaw[^1];

        var rawKeyword = match.Groups[2].Value;
        var keyword = rawKeyword.Replace(" ", "").ToLower();
        switch (match.Groups[1].Value)
        {
            case "!":
                if (!ModifierDictionary.TryGetValue(keyword, out var modKeyword))
                {
                    return $"The modifier `{keyword}` does not exist";
                }

                outputRaw[^1] = lastText with { Modifier = modKeyword };
                break;
            case "#":
                if (!FontColorDictionary.TryGetValue(keyword, out var fontKeyword))
                {
                    return $"The font color `{keyword}` does not exist";
                }

                outputRaw[^1] = lastText with { FontColor = fontKeyword };
                break;
            case "@":

                if (!BackColorDictionary.TryGetValue(keyword, out var backKeyword))
                {
                    return $"The back color `{keyword}` does not exist";
                }

                outputRaw[^1] = lastText with { BackColor = backKeyword };
                break;
        }

        lastIndex = currentIndex + match.Value.Length;
        if (!ColorRegex.IsMatch(input, lastIndex)) break;
        match = ColorRegex.Match(input, lastIndex);
    }

    var output = string.Join("", outputRaw.Select(output => output.ToString()));
    output = lastIndex >= input.Length - 1 ? output : $"{output}{input[lastIndex..]}";
    Output(output);
    return "copied in clipboard!";
}

Console.WriteLine($"modifiers: {string.Join(", ", ModifierDictionary.Keys.Select(key => $"[!{key}]"))}");
Console.WriteLine($"font colors: {string.Join(", ", FontColorDictionary.Keys.Select(key => $"[#{key}]"))}");
Console.WriteLine($"background colors: {string.Join(", ", BackColorDictionary.Keys.Select(key => $"[@{key}]"))}");

while (true)
{
    Console.WriteLine("input text: ");
    Console.WriteLine($"\n{Encoder(Console.ReadLine()!.Replace("\r", ""))}\n");
}

record Text(string Content = "", int Modifier = 0, int FontColor = 37, int BackColor = 0)
{
    public static string MakeText(int mod, int font = 37, int back = 0)
    {
        return $"{(back is -1 ? "\u001b[0m" : "")}\u001b[{mod};{font}{(back is not (0 or -1) ? $";{back}" : "")}m";
    }

    public override string ToString() => $"{MakeText(Modifier, FontColor, BackColor)}{Content}";
};