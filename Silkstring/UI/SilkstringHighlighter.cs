using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using Dalamud.Bindings.ImGui;
using ImGuiColorTextEditNet;
using ImGuiColorTextEditNet.Syntax;
using Silkstring.Services.Conditions;

namespace Silkstring.UI;

public sealed class SilkstringHighlighter : ISyntaxHighlighter
{
    private const PaletteIndex Error = PaletteIndex.CharLiteral;
    private const PaletteIndex Flag = PaletteIndex.Preprocessor;

    private static readonly object Empty = new();
    private static readonly Regex TokenRegex = new(@"\{(\d+\.\.\d+|\.\.\d+|\d+\.\.|\w+|\*)\}", RegexOptions.Compiled);
    private static readonly Regex StringRegex = new("\"[^\"]*\"?", RegexOptions.Compiled);
    private static readonly Regex FlagRegex = new(@"(?<!\S)-[A-Za-z]\w*", RegexOptions.Compiled);

    private readonly Func<ISet<string>> _definedVariables;

    public SilkstringHighlighter(Func<ISet<string>> definedVariables) => _definedVariables = definedVariables;

    public bool AutoIndentation => false;
    public int MaxLinesPerFrame => 1000;
    public string? GetTooltip(string id) => null;

    public object Colorize(Span<Glyph> line, object? state)
    {
        var chars = new char[line.Length];
        for (var i = 0; i < line.Length; i++)
        {
            chars[i] = line[i].Char;
            line[i] = new Glyph(line[i].Char, PaletteIndex.Default);
        }
        var text = new string(chars);
        var indent = text.Length - text.TrimStart().Length;
        var body = text.TrimStart();

        var (kind, expression) = BlockInterpreter.Classify(body);
        var exprStart = indent + body.Length - expression.Length;
        switch (kind)
        {
            case BlockKind.If:
                Paint(line, indent, exprStart, PaletteIndex.Keyword);
                if (TryParseCondition(expression)) PaintContent(line, text, exprStart);
                else Paint(line, exprStart, text.Length, Error);
                break;

            case BlockKind.Else:
            case BlockKind.EndIf:
                Paint(line, indent, exprStart, PaletteIndex.Keyword);
                break;

            case BlockKind.Set:
                Paint(line, indent, exprStart, PaletteIndex.Keyword);
                var (name, _) = BlockInterpreter.ParseSet(expression);
                if (name.Length == 0 || !_definedVariables().Contains(name))
                    Paint(line, exprStart, exprStart + Math.Max(name.Length, 1), Error);
                PaintContent(line, text, exprStart + name.Length);
                break;

            case BlockKind.Wait:
                Paint(line, indent, exprStart, PaletteIndex.Keyword);
                if (!expression.Contains('{') && !BlockInterpreter.TryParseDuration(expression.Trim(), out _))
                    Paint(line, exprStart, text.Length, Error);
                else PaintContent(line, text, exprStart);
                break;

            case BlockKind.Until:
                Paint(line, indent, exprStart, PaletteIndex.Keyword);
                var (_, untilCond) = BlockInterpreter.ParseUntil(expression);
                if (TryParseCondition(untilCond)) PaintContent(line, text, exprStart);
                else Paint(line, exprStart, text.Length, Error);
                break;

            default:
                if (body.Length > 1 && body[0] == ':' && char.IsLetter(body[1]))
                {
                    var kw = body.IndexOf(' ');
                    if (kw < 0) kw = body.Length;
                    Paint(line, indent, indent + kw, Error);
                    PaintContent(line, text, indent + kw);
                }
                else
                {
                    if (body.StartsWith('/'))
                    {
                        var end = body.IndexOf(' ');
                        Paint(line, indent, indent + (end < 0 ? body.Length : end), PaletteIndex.KnownIdentifier);
                    }
                    PaintContent(line, text, indent);
                }
                break;
        }

        return Empty;
    }

    private static bool TryParseCondition(string expression) => Condition.TryParse(expression, out _, out _);

    private static void PaintContent(Span<Glyph> line, string text, int from)
    {
        if (from >= text.Length) return;
        foreach (Match m in StringRegex.Matches(text, from))
            Paint(line, m.Index, m.Index + m.Length, PaletteIndex.String);
        PaintFlags(line, text, from);
        PaintTokens(line, text, from);
        PaintMalformedBraces(line, from);
    }

    private static void PaintFlags(Span<Glyph> line, string text, int from)
    {
        if (from >= text.Length) return;
        foreach (Match m in FlagRegex.Matches(text, from))
            Paint(line, m.Index, m.Index + m.Length, Flag);
    }

    private static void PaintMalformedBraces(Span<Glyph> line, int from)
    {
        for (var i = from; i < line.Length; i++)
        {
            if (line[i].Char != '{' || line[i].ColorIndex != PaletteIndex.Default) continue;
            var end = i + 1;
            while (end < line.Length && line[end].Char != '}') end++;
            if (end < line.Length) end++;
            Paint(line, i, end, Error);
            i = end - 1;
        }
    }

    private static void PaintTokens(Span<Glyph> line, string text, int from)
    {
        if (from >= text.Length) return;
        foreach (Match m in TokenRegex.Matches(text, from))
        {
            var color = IsParameter(m.Groups[1].Value) ? PaletteIndex.Number : PaletteIndex.Identifier;
            Paint(line, m.Index, m.Index + m.Length, color);
        }
    }

    private static void Paint(Span<Glyph> line, int start, int end, PaletteIndex color)
    {
        for (var i = start; i < end && i < line.Length; i++)
            line[i] = new Glyph(line[i].Char, color);
    }

    private static bool IsParameter(string token) => token == "*" || token.Contains("..") || token.All(char.IsDigit);

    public static void ApplyPalette(TextEditor editor)
    {
        var r = editor.Renderer;
        r.SetColor(PaletteIndex.Default, U32(Palette.Text));
        r.SetColor(PaletteIndex.Keyword, U32(Palette.Keyword));
        r.SetColor(PaletteIndex.KnownIdentifier, U32(Palette.Command));
        r.SetColor(PaletteIndex.Identifier, U32(Palette.Variable));
        r.SetColor(PaletteIndex.Number, U32(Palette.Parameter));
        r.SetColor(PaletteIndex.String, U32(Palette.String));
        r.SetColor(PaletteIndex.CharLiteral, U32(Palette.Error));
        r.SetColor(PaletteIndex.LineNumber, U32(Palette.LineNumber));
        r.SetColor(PaletteIndex.Preprocessor, U32(Palette.Flag));
    }

    private static uint U32(Vector4 c) => ImGui.ColorConvertFloat4ToU32(c);
}
