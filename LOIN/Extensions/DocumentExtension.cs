using System;
using System.Collections.Generic;
using System.Linq;
using Xbim.Common;
using Xbim.Ifc4.ExternalReferenceResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;

namespace LOIN
{
    public static class DocumentExtension
    {
        private static Dictionary<IIfcDefinitionSelect, List<IIfcDocumentSelect>> GetCache(IModel model)
        {
            return model.GetCache<Dictionary<IIfcDefinitionSelect, List<IIfcDocumentSelect>>>(nameof(DocumentExtension));
        }

        public static IEnumerable<IIfcDocumentSelect> GetDocuments(this IfcDefinitionSelect definition)
        {
            var model = definition.Model;
            var cache = GetCache(model);

            if (cache.TryGetValue(definition, out List<IIfcDocumentSelect> docs))
                return docs.AsReadOnly();

            if (model == definition.Model)
                return Enumerable.Empty<IIfcDocumentSelect>();

            // create new cache
            CreateCache(definition.Model);

            // try retrieve from current cache
            return GetDocuments(definition);
        }

        private static void CreateCache(IModel m)
        {
            var model = m;
            var cache = GetCache(model);

            foreach (var rel in model.Instances.OfType<IIfcRelAssociatesDocument>())
            {
                foreach (var o in rel.RelatedObjects)
                {
                    if (cache.TryGetValue(o, out List<IIfcDocumentSelect>  docs))
                    {
                            docs.Add(rel.RelatingDocument);
                    }
                    else
                    {
                        docs = new List<IIfcDocumentSelect> { rel.RelatingDocument };
                        cache.Add(o, docs);
                    }
                }
            }
        }

        public static void AddDocument(this IfcDefinitionSelect definition, IfcDocumentSelect doc)
        {
            var model = definition.Model;
            var cache = GetCache(model);

            var i = definition.Model.Instances;
            i.New<IfcRelAssociatesDocument>(r => {
                r.RelatedObjects.Add(definition);
                r.RelatingDocument = doc;
            });

            if (cache.TryGetValue(definition, out List<IIfcDocumentSelect> docs))
            {
                docs.Add(doc);
            }
            else
            {
                docs = new List<IIfcDocumentSelect> { doc };
                cache.Add(definition, docs);
            }
        }

        public static IIfcDocumentSelect AddDocument(this IfcDefinitionSelect definition, string name, string identification, Uri location)
        {
            var i = definition.Model.Instances;
            var doc = i.New<IfcDocumentReference>(d =>
            {
                d.Identification = identification;
                d.Name = name;
                d.Location = location.ToString();
            });
            definition.AddDocument(doc);
            return doc;
        }

    }
}
