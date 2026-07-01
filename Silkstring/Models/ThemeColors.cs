using System.Numerics;

namespace Silkstring.Models;

public class ThemeColors
{
    public Vector4 Heading = new(0.7f, 0.5f, 1.0f, 1.0f);
    public Vector4 Folder = new(0.7f, 0.5f, 1.0f, 1.0f);
    public Vector4 Error = new(1.0f, 0.4f, 0.4f, 1.0f);
    public Vector4 Success = new(0.4f, 1.0f, 0.4f, 1.0f);
    public Vector4 VariableToken = new(0.4f, 0.8f, 1.0f, 1.0f);
    public Vector4 ParameterToken = new(1.0f, 0.6f, 0.3f, 1.0f);
    public Vector4 ControlKeyword = new(0.85f, 0.5f, 0.8f, 1.0f);
    public Vector4 Command = new(0.9f, 0.7f, 0.3f, 1.0f);
    public Vector4 Operator = new(0.6f, 0.7f, 0.8f, 1.0f);
    public Vector4 ChatText = new(0.9f, 0.9f, 0.9f, 1.0f);
}
