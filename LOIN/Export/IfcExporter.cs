using LOIN.Context;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xbim.Common;
using Xbim.Ifc;
using Xbim.Ifc4.Kernel;

namespace LOIN.Export
{
    public static class IfcExporter
    {
        public static Stream ExportContext(ILoinModel model, IEnumerable<IContextEntity> context)
        {
            var contextTypes = context.GroupBy(c => c.GetType());
            var requirementSets = model.Requirements;
            // continuous filtering refinement
            foreach (var contextType in contextTypes)
            {
                requirementSets = requirementSets.Where(r => contextType.Any(c => c.IsContextFor(r)));
            }

            // positive filter for declared requirements (propertytemplates)
            var properties = requirementSets.SelectMany(l => l.Requirements);
            var propertyFilter = new HashSet<IfcPropertyTemplate>(properties);

            // positive filter for declared requirement sets (property set templates)
            var psets = properties.SelectMany(p => p.PartOfPsetTemplate);
            var psetFilter = new HashSet<IfcPropertySetTemplate>(psets);

            // root elements for copy operation
            var declareRels = requirementSets.SelectMany(r => r.Relations).Where(r => r.RelatedDefinitions
                    .Any(d => psetFilter.Contains(d) || propertyFilter.Contains(d)))
                .Distinct()
                .ToList();
            var projLibFilter = new HashSet<int>(declareRels.Select(r => r.RelatingContext.EntityLabel));

            // further refinement
            requirementSets = requirementSets
                .Where(r => projLibFilter.Contains(r.Entity.EntityLabel))
                .ToList();

            // examples
            var exampleRels = psetFilter.SelectMany(p => p.Defines).ToList();

            // languages
            var psetLangs = psetFilter.SelectMany(p => p.HasAssociations.OfType<IfcRelAssociatesLibrary>());
            var propLangs = propertyFilter.SelectMany(p => p.HasAssociations.OfType<IfcRelAssociatesLibrary>());
            var langs = psetLangs.Concat(propLangs).ToList();

            // positive filter for context relations
            var requirementsFilter = new HashSet<int>(requirementSets.Select(r => r.Entity.EntityLabel));

            // context items to copy
            var breakedownRels = requirementSets.SelectMany(r => r.BreakedownItems)
                .SelectMany(i => i.Relations)
                .Distinct()
                .Where(r => r.RelatedObjects.Any(o => projLibFilter.Contains(o.EntityLabel)))
                .ToList();
            var milestoneRels = requirementSets.SelectMany(r => r.Milestones)
                .SelectMany(i => i.Relations)
                .Distinct()
                .Where(r => r.RelatedObjects.Any(o => projLibFilter.Contains(o.EntityLabel)))
                .ToList();
            var reasonRels = requirementSets.SelectMany(r => r.Reasons)
                .SelectMany(i => i.Relations)
                .Distinct()
                .Where(r => r.RelatedObjects.Any(o => projLibFilter.Contains(o.EntityLabel)))
                .ToList();
            var actorRels = requirementSets.SelectMany(r => r.Actors)
                .SelectMany(i => i.Relations)
                .Distinct()
                .Where(r => r.RelatedObjects.Any(o => projLibFilter.Contains(o.EntityLabel)))
                .ToList();



            // actual copy logic
            var source = model.Internal;
            using (var target = IfcStore.Create(Xbim.Common.Step21.XbimSchemaVersion.Ifc4, Xbim.IO.XbimStoreType.InMemoryModel))
            {
                using (var txn = target.BeginTransaction("Copy part of LOIN"))
                {
                    var map = new XbimInstanceHandleMap(source, target);

                    // use relations as roots, filter collections accordingly
                    foreach (var rel in breakedownRels)
                    {
                        target.InsertCopy(rel, map, (prop, obj) =>
                        {
                            if (prop.IsInverse)
                                return null;
                            if (obj is IfcRelAssociatesClassification rc && prop.Name == nameof(IfcRelAssociatesClassification.RelatedObjects))
                            {
                                return rc.RelatedObjects
                                    .Where(o => requirementsFilter.Contains(o.EntityLabel))
                                    .ToList();
                            }
                            return prop.PropertyInfo.GetValue(obj);
                        }, false, false);
                    }

                    foreach (var rel in milestoneRels)
                    {
                        target.InsertCopy(rel, map, (prop, obj) =>
                        {
                            if (prop.IsInverse)
                                return null;
                            if (obj is IfcRelAssignsToProcess rp && prop.Name == nameof(IfcRelAssignsToProcess.RelatedObjects))
                            {
                                return rp.RelatedObjects
                                    .Where(o => requirementsFilter.Contains(o.EntityLabel))
                                    .ToList();
                            }
                            return prop.PropertyInfo.GetValue(obj);
                        }, false, false);
                    }

                    foreach (var rel in reasonRels)
                    {
                        target.InsertCopy(rel, map, (prop, obj) =>
                        {
                            if (prop.IsInverse)
                                return null;
                            if (obj is IfcRelAssignsToControl rp && prop.Name == nameof(IfcRelAssignsToControl.RelatedObjects))
                            {
                                return rp.RelatedObjects
                                    .Where(o => requirementsFilter.Contains(o.EntityLabel))
                                    .ToList();
                            }
                            return prop.PropertyInfo.GetValue(obj);
                        }, false, false);
                    }

                    foreach (var rel in actorRels)
                    {
                        target.InsertCopy(rel, map, (prop, obj) =>
                        {
                            if (prop.IsInverse)
                                return null;
                            if (obj is IfcRelAssignsToActor rp && prop.Name == nameof(IfcRelAssignsToActor.RelatedObjects))
                            {
                                return rp.RelatedObjects
                                    .Where(o => requirementsFilter.Contains(o.EntityLabel))
                                    .ToList();
                            }
                            return prop.PropertyInfo.GetValue(obj);
                        }, false, false);
                    }

                    foreach (var rel in declareRels)
                    {
                        target.InsertCopy(rel, map, (prop, obj) =>
                        {
                            if (prop.IsInverse)
                                return null;
                            if (obj is IfcRelDeclares rp && prop.Name == nameof(IfcRelDeclares.RelatedDefinitions))
                            {
                                return rp.RelatedDefinitions
                                    .Where(o => psetFilter.Contains(o) || propertyFilter.Contains(o))
                                    .ToList();
                            }
                            if (obj is IfcPropertySetTemplate pset && prop.Name == nameof(IfcPropertySetTemplate.HasPropertyTemplates))
                            {
                                return pset.HasPropertyTemplates
                                    .Where(p => propertyFilter.Contains(p))
                                    .ToList();
                            }
                            return prop.PropertyInfo.GetValue(obj);
                        }, false, false);
                    }

                    foreach (var rel in exampleRels)
                    {
                        target.InsertCopy(rel, map, null, false, false);
                    }

                    foreach (var rel in langs)
                    {
                        target.InsertCopy(rel, map, null, false, false);
                    }

                    txn.Commit();
                }

                // wrapper will avoid disposal of the stream
                var result = new MemoryStreamWrapper();
                target.SaveAsIfc(result);
                result.Seek(0, SeekOrigin.Begin);
                return result.Internal;
            }
        }
    }

    internal class MemoryStreamWrapper : Stream
    {
        internal MemoryStream Internal = new MemoryStream();
        public override bool CanRead => Internal.CanRead;

        public override bool CanSeek => Internal.CanSeek;

        public override bool CanWrite => Internal.CanWrite;

        public override long Length => Internal.Length;

        public override long Position { get => Internal.Position; set => Internal.Position = value; }

        public override void Flush()
        {
            Internal.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return Internal.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return Internal.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            Internal.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Internal.Write(buffer, offset, count);
        }
    }
}
