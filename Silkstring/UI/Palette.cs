using System.Numerics;
using Silkstring.Models;

namespace Silkstring.UI;

public static class Palette
{
    public static Vector4 Heading;
    public static Vector4 Folder;
    public static Vector4 Error;
    public static Vector4 Success;
    public static Vector4 VariableToken;
    public static Vector4 ParameterToken;
    public static Vector4 ControlKeyword;
    public static Vector4 Command;
    public static Vector4 Operator;
    public static Vector4 ChatText;

    public static void Apply(ThemeColors t)
    {
        Heading = t.Heading;
        Folder = t.Folder;
        Error = t.Error;
        Success = t.Success;
        VariableToken = t.VariableToken;
        ParameterToken = t.ParameterToken;
        ControlKeyword = t.ControlKeyword;
        Command = t.Command;
        Operator = t.Operator;
        ChatText = t.ChatText;
    }
}
