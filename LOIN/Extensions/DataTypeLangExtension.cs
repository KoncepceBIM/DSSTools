using System.Collections.Generic;
using System.Linq;
using Xbim.Common;
using Xbim.Ifc4.ExternalReferenceResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;

namespace LOIN
{
    public static class DataTypeLangExtension
    {
        public const string dictionaryIdentifier = "datatype_dictionary";
        private static IModel model;
        private static readonly Dictionary<int, Dictionary<string, IIfcLibraryReference>> cache = 
            new Dictionary<int, Dictionary<string, IIfcLibraryReference>>();

        public static void ClearCache()
        {
            model = null;
            cache.Clear();
        }

        private static IIfcLibraryReference GetLib(IModel m, int entityLabel, string lang)
        {
            var libs = GetLibs(m, entityLabel);
            if (libs == null)
                return null;
            if (libs.TryGetValue(lang, out IIfcLibraryReference lib))
                return lib;
            return null;
        }

        private static Dictionary<string, IIfcLibraryReference> GetLibs(IModel m, int entityLabel)
        {
            if (cache.TryGetValue(entityLabel, out Dictionary<string, IIfcLibraryReference> libs))
                return libs;
            if (model == m)
                return null;

            // create new cache
            CreateCache(m);

            // try retrieve from current cache
            return GetLibs(m, entityLabel);
        }

        private static void CreateCache(IModel m)
        {
            cache.Clear();
            model = m;

            foreach (var rel in model.Instances.OfType<IIfcRelAssociatesLibrary>()
                .Where(r => r.RelatingLibrary is IIfcLibraryReference lr &&
                    lr.Identification == dictionaryIdentifier && 
                    lr.Language.HasValue))
            {
                var lib = rel.RelatingLibrary as IIfcLibraryReference;
                foreach (var o in rel.RelatedObjects)
                {
                    if (cache.TryGetValue(o.EntityLabel, out Dictionary<string, IIfcLibraryReference>  libs))
                    {
                        if (libs.ContainsKey(lib.Language))
                            libs[lib.Language] = lib;
                        else
                            libs.Add(lib.Language, lib);
                    }
                    else
                    {
                        libs = new Dictionary<string, IIfcLibraryReference> { { lib.Language, lib } };
                        cache.Add(o.EntityLabel, libs);
                    }
                }
            }

            foreach (var rel in model.Instances.OfType<IIfcExternalReferenceRelationship>()
                .Where(r => r.RelatingReference is IIfcLibraryReference lr &&
                    lr.Identification == dictionaryIdentifier &&
                    lr.Language.HasValue))
            {
                var lib = rel.RelatingReference as IIfcLibraryReference;
                foreach (var o in rel.RelatedResourceObjects)
                {
                    if (cache.TryGetValue(o.EntityLabel, out Dictionary<string, IIfcLibraryReference> libs))
                    {
                        if (libs.ContainsKey(lib.Language))
                            libs[lib.Language] = lib;
                        else
                            libs.Add(lib.Language, lib);
                    }
                    else
                    {
                        libs = new Dictionary<string, IIfcLibraryReference> { { lib.Language, lib } };
                        cache.Add(o.EntityLabel, libs);
                    }
                }
            }
        }

        private static void AddOrSet(IfcDefinitionSelect definition, string lang, IIfcLibraryReference lib)
        {
            if (!cache.TryGetValue(definition.EntityLabel, out Dictionary<string, IIfcLibraryReference> libs))
            {
                libs = new Dictionary<string, IIfcLibraryReference>();
                cache.Add(definition.EntityLabel, libs);
            }

            if (libs.ContainsKey(lang))
                libs[lang] = lib;
            else
                libs.Add(lang, lib);
        }

        public static string GetDataTypeName(this IIfcPropertyTemplate definition, string lang)
        {
            var lib = GetLib(definition.Model, definition.EntityLabel, lang);
            if (lib == null) return null;
            return lib.Name;
        }

        public static void SetDataTypeName(this IIfcPropertyTemplate definition, string lang, string name)
        {
            var lib = GetLib(definition.Model, definition.EntityLabel, lang);
            if (lib != null)
            {
                lib.Name = name;
                return;
            }

            var i = definition.Model.Instances;
            i.New<IfcRelAssociatesLibrary>(r => {
                r.RelatedObjects.Add(definition);
                r.RelatingLibrary = i.New<IfcLibraryReference>(libRef => {
                    libRef.Name = name;
                    libRef.Language = lang;
                    libRef.Identification = dictionaryIdentifier;

                    AddOrSet(definition, lang, libRef);
                });
            });
        }

        public static string GetDataTypeDescription(this IIfcPropertyTemplate definition, string lang)
        {
            var lib = GetLib(definition.Model, definition.EntityLabel, lang);
            if (lib == null) return null;
            return lib.Description;
        }

        public static void SetDataTypeDescription(this IIfcPropertyTemplate definition, string lang, string description)
        {
            var lib = GetLib(definition.Model, definition.EntityLabel, lang);
            if (lib != null)
            {
                lib.Description = description;
                return;
            }

            var i = definition.Model.Instances;
            i.New<IfcRelAssociatesLibrary>(r => {
                r.RelatedObjects.Add(definition);
                r.RelatingLibrary = i.New<IfcLibraryReference>(libRef => {
                    libRef.Description = description;
                    libRef.Language = lang;
                    libRef.Identification = dictionaryIdentifier;

                    AddOrSet(definition, lang, libRef);
                });
            });
        }
    }
}
