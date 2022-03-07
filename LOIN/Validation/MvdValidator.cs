using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Xbim.Common;
using Xbim.Ifc4.Interfaces;
using Xbim.IO.Memory;
using Xbim.MvdXml;
using Xbim.MvdXml.DataManagement;

namespace LOIN.Validation
{
    public static class MvdValidator
    {
        public static string ValidateXsd(string path, ILogger logger)
        {
            var schemas = new XmlSchemaSet();
            var location = Path.Combine("Validation", "mvdXML_V1.1.xsd");
            schemas.Add("http://buildingsmart-tech.org/mvd/XML/1.1", location);
            using (var reader = XmlReader.Create(path, new XmlReaderSettings
            {
                Schemas = schemas,
                ValidationType = ValidationType.Schema,
                ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings,
            }))
            {
                try
                {
                    var dom = new XmlDocument();
                    dom.Load(reader);
                }
                catch (XmlSchemaValidationException e)
                {
                    var msg = $"mvdXML schema error: [{e.LineNumber}:{e.LinePosition}]: {e.Message}";
                    logger.LogError(msg);
                    return msg;
                }
            }
            return null;
        }

        public static IEnumerable<MvdValidationResult> ValidateModel(mvdXML mvd, IModel model)
        {
            // MVD validation rules are likely to traverse inverse relations
            using var entityCache = model.BeginEntityCaching();
            using var invCache = model.BeginInverseCaching();

            var engine = new MvdEngine(mvd, model);
            var objects = model.Instances
                .OfType<IIfcObject>()
                .ToList();
            var validated = new HashSet<int>();

            foreach (var root in engine.ConceptRoots)
            {
                var applicable = objects.Where(o => root.AppliesTo(o));
                foreach (var item in applicable)
                {
                    validated.Add(item.EntityLabel);
                    foreach (var concept in root.Concepts)
                    {
                        var passes = concept.Test(item, Concept.ConceptTestMode.Raw);
                        yield return new MvdValidationResult(item, concept, passes);
                    }
                }
            }

            // report all IfcObjects which were not checked
            foreach (var item in objects.Where(o => !validated.Contains(o.EntityLabel)))
            {
                yield return new MvdValidationResult(item, null, ConceptTestResult.DoesNotApply);
            }
        }

        public static IEnumerable<MvdValidationResult> ValidateModel(mvdXML mvd, Stream ifcStream, ILogger logger, bool forceGCCollect = false)
        {
            // load stripped down model without geometry objects
            using (var model = MemoryModel.OpenReadStep21(ifcStream, logger, null, ignoreTypes, false, false))
            {
                // there is usually stuff to collect after parsing.
                if (forceGCCollect)
                    GC.Collect();

                return ValidateModel(mvd, model).ToList();
            }
        }

        private static readonly string[] ignoreNamespaces = new[] {
            "GeometricConstraintResource",
            "GeometricModelResource",
            "GeometryResource",
            "ProfileResource",
            "TopologyResource",
            "RepresentationResource"
        };
        private static readonly string[] ignoreTypes = typeof(Xbim.Ifc4.EntityFactoryIfc4)
            .Assembly.GetTypes()
            .Where(t => t.IsClass && t.IsPublic && !t.IsAbstract && ignoreNamespaces.Any(ns => t.Namespace.EndsWith(ns)))
            .Select(t => t.Name.ToUpperInvariant())
            .ToArray();
    }

    public struct MvdValidationResult
    {
        public IPersistEntity Entity { get; }
        public Concept Concept { get; }
        public ConceptTestResult Result { get; }

        public MvdValidationResult(IPersistEntity entity, Concept concept, ConceptTestResult result)
        {
            Entity = entity;
            Concept = concept;
            Result = result;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;
            if (!(obj is MvdValidationResult r))
                return false;
            return Entity.EntityLabel == r.Entity.EntityLabel && Concept?.uuid == r.Concept?.uuid && Result == r.Result;
        }

        public override int GetHashCode()
        {
            if (Concept != null)
                return HashCode.Combine(Entity.EntityLabel, Concept.uuid, Result);
            return HashCode.Combine(Entity.EntityLabel, Result);
        }

        public static bool operator ==(MvdValidationResult left, MvdValidationResult right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MvdValidationResult left, MvdValidationResult right)
        {
            return !(left == right);
        }
    }
}
