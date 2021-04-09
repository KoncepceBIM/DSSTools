using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOIN.Server.Services.Interfaces
{
    public interface ILoinRepository
    {
        IEnumerable<string> GetRepositoryIds();
        ILoinModel OpenRepository(string id);
    }
}
