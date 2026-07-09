using System.Numerics;
using Silkstring.Models;

namespace Silkstring.UI;

public static class Palette
{
    public static Vector4 Variable;
    public static Vector4 Parameter;
    public static Vector4 Keyword;
    public static Vector4 String;
    public static Vector4 Command;
    public static Vector4 Text;
    public static Vector4 Error;
    public static Vector4 Heading;
    public static Vector4 Folder;
    public static Vector4 Success;
    public static Vector4 LineNumber;
    public static Vector4 Flag;

    public static void Apply(ThemeColors t)
    {
        Variable = t.Variable;
        Parameter = t.Parameter;
        Keyword = t.Keyword;
        String = t.String;
        Command = t.Command;
        Text = t.Text;
        Error = t.Error;
        Heading = t.Heading;
        Folder = t.Folder;
        Success = t.Success;
        LineNumber = t.LineNumber;
        Flag = t.Flag;
    }
}
