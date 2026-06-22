using System.Collections.Generic;

namespace Silkstring.Services.Variables;

public interface IVariableProvider
{
    IEnumerable<VariableDescriptor> GetVariables();
}
