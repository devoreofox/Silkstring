using System.Collections.Generic;

namespace Silkstring.Services.Conditions;

internal sealed class BlockParser
{
    private readonly IReadOnlyList<string> _lines;
    private readonly List<Diagnostic> _diagnostics = new();
    private int _pos;

    private BlockParser(IReadOnlyList<string> lines) =>  _lines = lines;

    public static (IReadOnlyList<BlockNode> Nodes, IReadOnlyList<Diagnostic> Diagnostics) Parse(
        IReadOnlyList<string> lines)
    {
        var p = new BlockParser(lines);
        var nodes = p.ParseBlock(true);
        return (nodes, p._diagnostics);
    }

    private List<BlockNode> ParseBlock(bool topLevel)
    {
        var nodes = new List<BlockNode>();
        while (SkipToMeaningful())
        {
            var line = _lines[_pos].Trim();
            if (line == "}")
            {
                if (!topLevel) return nodes;
                _diagnostics.Add(new("Unexpected '}'", _pos));
                _pos++;
                continue;
            }

            if (TryIfHead(line, "if", out _)) { nodes.Add(ParseIf()); continue; }

            if (IsElseIfHead(line, out _) || IsElseHead(line))
            {
                _diagnostics.Add(new("'else' without a matching 'if'", _pos));
                _pos++;
                continue;
            }
            nodes.Add(new LineNode(line));
            _pos++;
        }
        return nodes;
    }

    private IfNode ParseIf()
    {
        var branches = new List<Branch>();
        AddBranch(branches, "if");
        while (SkipToMeaningful())
        {
            var line = _lines[_pos].Trim();
            if (IsElseIfHead(line, out _)) { AddBranch(branches, "else if"); continue; }
            if (IsElseHead(line)) { AddElseBranch(branches); break; }
            break;
        }
        return new IfNode(branches);
    }

    private void AddBranch(List<Branch> branches, string keyword)
    {
        var openLine = _pos;
        TryIfHead(_lines[_pos].Trim(), keyword, out var cond);
        if (!Condition.TryParse(cond, out _, out var err)) _diagnostics.Add(new($"Invalid condition: {err}", openLine));
        _pos++;
        var body = ParseBlock(false);
        ExpectClose(openLine);
        branches.Add(new Branch(cond, body));
    }

    private void AddElseBranch(List<Branch> branches)
    {
        var openLine = _pos;
        _pos++;
        var body = ParseBlock(false);
        ExpectClose(openLine);
        branches.Add(new Branch(null, body));
    }

    private void ExpectClose(int openLine)
    {
        if (_pos < _lines.Count && _lines[_pos].Trim() == "}") { _pos++; return; }
        _diagnostics.Add(new("Missing '}' to close this block", openLine));
    }

    private bool SkipToMeaningful()
    {
        while (_pos < _lines.Count)
        {
            var line = _lines[_pos].Trim();
            if (line.Length == 0 || line.StartsWith("#")) {_pos++; continue;}
            return true;
        }
        return false;
    }

    private static bool TryIfHead(string line, string keyword, out string condition)
    {
        condition = "";
        if (!line.StartsWith(keyword)) return false;
        var rest = line[keyword.Length..].TrimStart();
        if (rest.Length == 0 || rest[0] != '(') return false;
        var close = MatchParen(rest);
        if (close < 0 || rest[(close + 1)..].Trim() != "{") return false;
        condition = rest[1..close];
        return true;
    }

    private static bool IsElseIfHead(string line, out string condition) => TryIfHead(line, "else if", out condition);
    private static bool IsElseHead(string line) => line.StartsWith("else") && line[4..].TrimStart() == "{";

    private static int MatchParen(string s)
    {
        var depth = 0;
        for (var i = 0; i < s.Length; i++)
        {
            if (s[i] == '(') depth++;
            else if (s[i] == ')' && --depth == 0) return i;
        }
        return -1;
    }
}
