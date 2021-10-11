using System.Collections.Generic;
using System.Linq;
using Xbim.Common;
using Xbim.Ifc4.ExternalReferenceResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;

namespace LOIN
{
    public static class LangExtension
    {
        public const string dictionaryIdentifier = "dictionary";
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
            CreateCache(model);

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

        private static void AddOrSet(IfcExternalReference reference, string lang, IIfcLibraryReference lib)
        {
            if (!cache.TryGetValue(reference.EntityLabel, out Dictionary<string, IIfcLibraryReference> libs))
            {
                libs = new Dictionary<string, IIfcLibraryReference>();
                cache.Add(reference.EntityLabel, libs);
            }

            if (libs.ContainsKey(lang))
                libs[lang] = lib;
            else
                libs.Add(lang, lib);
        }

        public static string GetName(this IfcDefinitionSelect definition, string lang)
        {
            var lib = GetLib(definition.Model, definition.EntityLabel, lang);
            if (lib == null) return null;
            return lib.Name;
        }

        public static string GetName(this IfcExternalReference reference, string lang)
        {
            var lib = GetLib(reference.Model, reference.EntityLabel, lang);
            if (lib == null) return null;
            return lib.Name;
        }

        public static void SetName(this IfcDefinitionSelect definition, string lang, string name)
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

        public static void SetName(this IfcExternalReference reference, string lang, string name)
        {
            var lib = GetLib(reference.Model, reference.EntityLabel, lang);
            if (lib != null)
            {
                lib.Name = name;
                return;
            }

            var i = reference.Model.Instances;
            i.New<IfcExternalReferenceRelationship>(r => {
                r.RelatedResourceObjects.Add(reference);
                r.RelatingReference = i.New<IfcLibraryReference>(libRef => {
                    libRef.Name = name;
                    libRef.Language = lang;
                    libRef.Identification = dictionaryIdentifier;

                    AddOrSet(reference, lang, libRef);
                });
            });
        }

        public static string GetDescription(this IfcDefinitionSelect definition, string lang)
        {
            var lib = GetLib(definition.Model, definition.EntityLabel, lang);
            if (lib == null) return null;
            return lib.Description;
        }

        public static string GetDescription(this IfcExternalReference reference, string lang)
        {
            var lib = GetLib(reference.Model, reference.EntityLabel, lang);
            if (lib == null) return null;
            return lib.Description;
        }

        public static void SetDescription(this IfcDefinitionSelect definition, string lang, string description)
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
        public static void SetDescription(this IfcExternalReference reference, string lang, string description)
        {
            var lib = GetLib(reference.Model, reference.EntityLabel, lang);
            if (lib != null)
            {
                lib.Description = description;
                return;
            }

            var i = reference.Model.Instances;
            i.New<IfcExternalReferenceRelationship>(r => {
                r.RelatedResourceObjects.Add(reference);
                r.RelatingReference = i.New<IfcLibraryReference>(libRef => {
                    libRef.Description = description;
                    libRef.Language = lang;
                    libRef.Identification = dictionaryIdentifier;

                    AddOrSet(reference, lang, libRef);
                });
            });
        }
    }
}
