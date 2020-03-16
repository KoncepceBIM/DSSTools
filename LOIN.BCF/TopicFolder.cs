using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LOIN.BCF
{
    public class TopicFolder
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Markup Markup { get; set; } = new Markup();

        public ICollection<VisualizationInfo> ViewPoints { get; set; } = new List<VisualizationInfo>();

        public ICollection<Stream> PngSnapshots { get; set; } = new List<Stream>();

    }
}
