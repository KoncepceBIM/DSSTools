using LOIN.Server.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace LOIN.Server.Services.Implementations
{
    public class LoinRepository : ILoinRepository
    {
        private static readonly string BasePath = Path.Combine("Data", "Repositories");
        private const string RepositoryExtension = ".ifc";

        private Dictionary<string, Repository> Cache = new Dictionary<string, Repository>();

        public IEnumerable<string> GetRepositoryIds()
        {
            return Directory
                .GetFiles(BasePath, $"*{RepositoryExtension}", SearchOption.TopDirectoryOnly)
                .Select(f => Path.GetFileNameWithoutExtension(f));
        }

        public ILoinModel OpenRepository(string id)
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

            var model = Model.Open(path);
            repository = new Repository
            {
                Id = id,
                Checksum = hash,
                Model = model,
                Path = path
            };
            Cache.Add(id, repository);
            return model;
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
