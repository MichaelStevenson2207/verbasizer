using System.Text;
using System.Text.RegularExpressions;

namespace verbasizer.Services;

public sealed class VerbasizerComposer
{
    private static readonly Regex TokenPattern = new(@"\p{L}[\p{L}\p{N}'-]*|\p{N}+|[^\s]", RegexOptions.Compiled);
    private static readonly HashSet<string> TerminalTokens = [".", "!", "?"];
    private static readonly HashSet<string> PauseTokens = [",", ";", ":"];
    private static readonly HashSet<string> OpeningPunctuation = ["(", "[", "{", "\""];
    private static readonly HashSet<string> ClosingPunctuation = [".", ",", "!", "?", ";", ":", ")", "]", "}", "\""];

    public VerbasizerComposition Compose(IReadOnlyCollection<string> sources, int lineCount)
    {
        var populatedSources = sources
            .Where(source => !string.IsNullOrWhiteSpace(source))
            .Select(source => Regex.Replace(source.Trim(), @"\s+", " "))
            .ToList();

        if (populatedSources.Count == 0)
        {
            return VerbasizerComposition.Empty;
        }

        var corpus = BuildCorpus(populatedSources);
        if (corpus.SentenceStarts.Count == 0)
        {
            return VerbasizerComposition.Empty;
        }

        var boundedLineCount = Math.Clamp(lineCount, 2, 12);
        var lines = new List<string>(boundedLineCount);
        var generatedLines = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var index = 0; index < boundedLineCount; index++)
        {
            string? line = null;
            string? fallbackLine = null;

            for (var attempt = 0; attempt < 4 && line is null; attempt++)
            {
                var tokens = GenerateLineTokens(corpus, minWords: 5, maxWords: 12);
                var candidate = FormatTokens(tokens);

                if (candidate.Length == 0)
                {
                    continue;
                }

                fallbackLine ??= candidate;

                if (generatedLines.Add(candidate))
                {
                    line = candidate;
                }
            }

            if (line is not null)
            {
                lines.Add(line);
            }
            else if (fallbackLine is not null)
            {
                lines.Add(fallbackLine);
            }
        }

        return lines.Count == 0
            ? VerbasizerComposition.Empty
            : new VerbasizerComposition(
                string.Join(Environment.NewLine, lines),
                populatedSources.Count,
                corpus.DistinctWords.Count,
                corpus.TokenCount);
    }

    private static Corpus BuildCorpus(IReadOnlyCollection<string> sources)
    {
        var transitions = new Dictionary<string, List<string>>();
        var sentenceStarts = new List<string>();
        var distinctWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var tokenCount = 0;

        foreach (var source in sources)
        {
            var tokens = Tokenize(source);
            if (tokens.Count == 0)
            {
                continue;
            }

            tokenCount += tokens.Count;

            for (var index = 0; index < tokens.Count; index++)
            {
                var token = tokens[index];

                if (IsWordLike(token))
                {
                    distinctWords.Add(token);

                    if (IsSentenceStart(tokens, index))
                    {
                        sentenceStarts.Add(token);
                    }
                }

                if (index == tokens.Count - 1)
                {
                    continue;
                }

                if (!transitions.TryGetValue(token, out var nextTokens))
                {
                    nextTokens = [];
                    transitions[token] = nextTokens;
                }

                nextTokens.Add(tokens[index + 1]);
            }
        }

        return new Corpus(transitions, sentenceStarts, distinctWords, tokenCount);
    }

    private static List<string> GenerateLineTokens(Corpus corpus, int minWords, int maxWords)
    {
        if (corpus.SentenceStarts.Count == 0)
        {
            return [];
        }

        var tokens = new List<string>();
        var current = corpus.SentenceStarts[Random.Shared.Next(corpus.SentenceStarts.Count)];
        tokens.Add(current);

        var wordCount = 1;
        var safetyCounter = 0;

        while (safetyCounter < maxWords * 4)
        {
            safetyCounter++;

            if (!corpus.Transitions.TryGetValue(current, out var options) || options.Count == 0)
            {
                break;
            }

            var nextToken = PickNextToken(options, wordCount, minWords, maxWords);
            tokens.Add(nextToken);
            current = nextToken;

            if (IsWordLike(nextToken))
            {
                wordCount++;
            }

            if (TerminalTokens.Contains(nextToken) && wordCount >= minWords)
            {
                break;
            }
        }

        while (tokens.Count > 0 && PauseTokens.Contains(tokens[^1]))
        {
            tokens.RemoveAt(tokens.Count - 1);
        }

        if (tokens.Count > 0 && !TerminalTokens.Contains(tokens[^1]))
        {
            tokens.Add(".");
        }

        return tokens;
    }

    private static string PickNextToken(List<string> options, int wordCount, int minWords, int maxWords)
    {
        if (wordCount < minWords)
        {
            var nonTerminalOptions = options.Where(option => !TerminalTokens.Contains(option)).ToList();
            if (nonTerminalOptions.Count > 0)
            {
                return nonTerminalOptions[Random.Shared.Next(nonTerminalOptions.Count)];
            }
        }
        else if (wordCount >= maxWords)
        {
            var terminalOptions = options.Where(option => TerminalTokens.Contains(option)).ToList();
            if (terminalOptions.Count > 0)
            {
                return terminalOptions[Random.Shared.Next(terminalOptions.Count)];
            }
        }

        return options[Random.Shared.Next(options.Count)];
    }

    private static string FormatTokens(IReadOnlyList<string> tokens)
    {
        if (tokens.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        string? previous = null;

        foreach (var token in tokens)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                continue;
            }

            if (builder.Length > 0 && NeedsSpace(previous, token))
            {
                builder.Append(' ');
            }

            builder.Append(token);
            previous = token;
        }

        if (builder.Length == 0)
        {
            return string.Empty;
        }

        var line = builder.ToString().Trim();
        return char.ToUpperInvariant(line[0]) + line[1..];
    }

    private static bool NeedsSpace(string? previous, string current)
    {
        if (previous is null)
        {
            return false;
        }

        if (ClosingPunctuation.Contains(current) || OpeningPunctuation.Contains(previous) || current == "'" || previous == "'")
        {
            return false;
        }

        return true;
    }

    private static bool IsSentenceStart(IReadOnlyList<string> tokens, int index)
    {
        for (var previousIndex = index - 1; previousIndex >= 0; previousIndex--)
        {
            var previousToken = tokens[previousIndex];

            if (OpeningPunctuation.Contains(previousToken))
            {
                continue;
            }

            return TerminalTokens.Contains(previousToken);
        }

        return true;
    }

    private static bool IsWordLike(string token) => char.IsLetterOrDigit(token[0]);

    private static List<string> Tokenize(string source) =>
        TokenPattern.Matches(source).Select(match => match.Value).ToList();

    private sealed record Corpus(
        Dictionary<string, List<string>> Transitions,
        List<string> SentenceStarts,
        HashSet<string> DistinctWords,
        int TokenCount);
}

public sealed record VerbasizerComposition(
    string Lyrics,
    int SourceCount,
    int DistinctWordCount,
    int TokenCount)
{
    public static VerbasizerComposition Empty { get; } = new(string.Empty, 0, 0, 0);
}
