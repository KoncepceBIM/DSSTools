using System;
using System.Collections.Generic;
using System.Text;

namespace LOIN.Context
{
    public interface IContextEntity
    {
        bool Contains(Requirements.RequirementsSet requirements);
        bool Remove(Requirements.RequirementsSet requirements);
        bool Add(Requirements.RequirementsSet requirements);

    }
}
