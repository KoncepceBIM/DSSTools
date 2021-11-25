using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Xbim.Ifc4.Kernel;

namespace LOIN.Server.Equality
{
    internal class PropertyEquality : IEqualityComparer<IfcPropertyTemplate>
    {
        public static PropertyEquality Comparer { get; } = new PropertyEquality();

        public bool Equals([AllowNull] IfcPropertyTemplate x, [AllowNull] IfcPropertyTemplate y)
        {
            if (ReferenceEquals(x, y))
                return true;

            return x.GlobalId == y.GlobalId;
        }

        public int GetHashCode([DisallowNull] IfcPropertyTemplate obj) => obj.GlobalId.ToString().GetHashCode();
    }
}
