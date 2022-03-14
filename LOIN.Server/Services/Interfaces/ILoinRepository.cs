using System.Collections.Generic;
using System.Threading.Tasks;

namespace LOIN.Server.Services.Interfaces
{
    /// <summary>
    /// Repository of LOIN definition files
    /// </summary>
    public interface ILoinRepository
    {
        /// <summary>
        /// Gets a list of repository IDs
        /// </summary>
        /// <returns>Repository IDs</returns>
        IEnumerable<string> GetRepositoryIds();

        /// <summary>
        /// Opens the repository
        /// </summary>
        /// <param name="id">Repository ID</param>
        /// <returns>LOIN repository</returns>
        Task<ILoinModel> OpenRepository(string id);
    }
}
