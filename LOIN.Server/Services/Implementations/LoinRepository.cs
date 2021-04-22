using LOIN.Server.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace LOIN.Server.Services.Implementations
{
    public class LoinRepository : ILoinRepository
    {
        private static readonly string BasePath = Path.Combine("Data", "Repositories");
        private const string RepositoryExtension = ".ifc";

        public IMemoryCache Cache { get; }

        public LoinRepository(IMemoryCache cache)
        {
            Cache = cache;
        }

        public IEnumerable<string> GetRepositoryIds()
        {
            return Directory
                .GetFiles(BasePath, $"*{RepositoryExtension}", SearchOption.TopDirectoryOnly)
                .Select(f => Path.GetFileNameWithoutExtension(f));
        }

        public async Task<ILoinModel> OpenRepository(string id)
        {
            var path = GetFilePath(id);
            if (!File.Exists(path))
                throw new FileNotFoundException("Repository fine not found", id);

            var hash = ComputeHash(path);
            if (Cache.TryGetValue(id, out Repository repository))
            {
                // make sure the file is the same
                if (string.Equals(repository.Checksum, hash))
                    return repository.Model;
                else
                    Cache.Remove(id);
            }

            repository = await Cache.GetOrCreateAsync(id, async (entry) =>
            {
                var model = await Task.Run(() => Model.Open(path)) ;
                var r = new Repository
                {
                    Id = id,
                    Checksum = hash,
                    Model = model,
                    Path = path
                };
                entry.Value = r;
                return r;
            });
            
            return repository.Model;
        }

        private static string GetFilePath(string id)
        {
            return Path.Combine(BasePath, $"{id}{RepositoryExtension}");
        }

        private static string ComputeHash(string fileName)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(fileName);

            var hash = md5.ComputeHash(stream);
            return Convert.ToBase64String(hash);
        }

        private class Repository
        {
            public string Id { get; set; }
            public string Path { get; set; }
            public string Checksum { get; set; }
            public ILoinModel Model { get; set; }
        }
    }
}
