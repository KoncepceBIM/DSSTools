using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace LOIN.BCF
{
    public class BCFArchive
    {
        public Version Version { get; } = new Version { DetailedVersion = "2.1", VersionId = "2.1" };

        public ICollection<TopicFolder> Topics { get; } = new List<TopicFolder>();

        public Project Project { get; set; } // Optional

        public ICollection<DocumentFile> Documents { get; } = new List<DocumentFile>();

        public void Serialize(Stream stream)
        {
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                using (var w = new StreamWriter(archive.CreateEntry("bcf.version").Open()))
                    w.Write(Version.Serialize());

                if (Project != null)
                {
                    using (var w = new StreamWriter(archive.CreateEntry("project.bcfp").Open()))
                        w.Write(Project.Serialize());
                }

                foreach (var topic in Topics)
                {
                    // folder entry
                    archive.CreateEntry($"{topic.Id.ToString()}/");

                    // markup entry
                    using (var w = new StreamWriter(archive.CreateEntry($"{topic.Id}/markup.bcf").Open()))
                        w.Write(topic.Markup.Serialize());

                    if (topic.ViewPoints.Any())
                    {
                        var defaultView = topic.ViewPoints.First();
                        using (var w = new StreamWriter(archive.CreateEntry($"{topic.Id}/viewpoint.bcfv").Open()))
                            w.Write(defaultView.Serialize());
                    }
                    foreach (var view in topic.ViewPoints)
                    {
                        using (var w = new StreamWriter(archive.CreateEntry($"{topic.Id}/{view.Guid}.bcfv").Open()))
                            w.Write(view.Serialize());
                    }

                    var count = 0;
                    foreach (var snapshot in topic.PngSnapshots)
                    {
                        var name = count > 0 ? $"snapshot_{count:D3}.bcfv" : "snapshot.bcfv";
                        using (var s = archive.CreateEntry($"{topic.Id}/{name}").Open())
                            snapshot.CopyTo(s);
                        count++;
                    }

                }

                if (Documents.Any()) archive.CreateEntry($"Documents/");
                foreach (var document in Documents)
                {
                    using (var s = archive.CreateEntry($"Documents/{document.Name}").Open())
                    {
                        document.WriteTo(s);
                    }
                }


            }
        }
    }
}
