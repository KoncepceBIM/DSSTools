using System.Collections.Generic;
using System.Globalization;
using Xbim.Common;

namespace LOIN.Context
{
    public interface IContextEntity
    {
        bool IsContextFor(int requirementsLabel);
        bool IsContextFor(Requirements.RequirementsSet requirements);

        HashSet<int> RequirementsSetLookUp { get; }

        bool RemoveFromContext(Requirements.RequirementsSet requirements);
        bool AddToContext(Requirements.RequirementsSet requirements);

        string Id { get; }
        string Name { get; }
        string Description { get; }
        IPersistEntity Entity { get; }

        string GetName(string lang);
        string GetDescription(string lang);
    }
}
