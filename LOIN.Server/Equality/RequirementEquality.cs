using LOIN.Server.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace LOIN.Server.Equality
{
    internal class RequirementEquality : IEqualityComparer<Requirement>
    {
        public static RequirementEquality Comparer { get; } = new RequirementEquality();

        public bool Equals([AllowNull] Requirement x, [AllowNull] Requirement y)
        {

            if (x?.UUID != null && y?.UUID != null)
                return x.UUID == y.UUID;

            return ReferenceEquals(x, y);
        }

        public int GetHashCode([DisallowNull] Requirement obj)
        {
            if (!string.IsNullOrWhiteSpace(obj.UUID))
                obj.UUID.GetHashCode();
            return obj.GetHashCode();
        }
    }
}
