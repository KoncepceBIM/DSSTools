using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xbim.Common;
using Xbim.Ifc4.ExternalReferenceResource;

namespace DDSS.Utils
{
    public static class ClassificationImporter
    {
        public static Dictionary<int, IfcClassificationReference> ImportInto(IModel model)
        {
            var result = new Dictionary<int, IfcClassificationReference>();
            var i = model.Instances;
            var dummyDatabaseIdentifier = 5001;

            // create the root
            var classification = i.New<IfcClassification>(c => c.Name = "Klasifikace SFDI");
            var parents = new IfcClassificationReferenceSelect[] { classification, null, null, null, null, null, null, null, null };
            
            foreach (var line in File.ReadAllLines("Pepova_klasifikace.csv"))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line
                    .Split("\",\"")
                    .Select(p => p.Trim('"'))
                    .ToArray();

                var depth = 1;
                foreach (var part in parts)
                {
                    if (string.IsNullOrWhiteSpace(part))
                    {
                        depth++;
                        continue;
                    }

                    var parent = parents[depth - 1];
                    string identifier = null;
                    string name = part;
                    if (depth == 1 && part[0] >= '0' && part[0] <= '9') {
                        var sIdx = part.IndexOf(' ');
                        identifier = part.Substring(0, sIdx);
                        name = part.Substring(sIdx + 1);
                    }
                    var item = i.New<IfcClassificationReference>(r => {
                        r.Name = name;
                        r.Identification = identifier;
                        r.ReferencedSource = parent;
                    });

                    // this should contain map to original source (database ID)
                    result.Add(dummyDatabaseIdentifier++, item);

                    // set parent for lower depths
                    parents[depth] = item;
                    break;
                }
            }

            return result;
        }
    }
}
