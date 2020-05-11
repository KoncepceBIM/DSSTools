using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
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
        private static Dictionary<IIfcDefinitionSelect, Dictionary<string, IIfcLibraryReference>> cache = new Dictionary<IIfcDefinitionSelect, Dictionary<string, IIfcLibraryReference>>();

        public static void ClearCache()
        {
            model = null;
            cache.Clear();
        }

        private static IIfcLibraryReference GetLib(IfcDefinitionSelect definition, string lang)
        {
            var libs = GetLibs(definition);
            if (libs == null)
                return null;
            if (libs.TryGetValue(lang, out IIfcLibraryReference lib))
                return lib;
            return null;
        }

        private static Dictionary<string, IIfcLibraryReference> GetLibs(IfcDefinitionSelect definition)
        {
            if (cache.TryGetValue(definition, out Dictionary<string, IIfcLibraryReference> libs))
                return libs;
            if (model == definition.Model)
                return null;

            // create new cache
            CreateCache(definition.Model);

            // try retrieve from current cache
            return GetLibs(definition);
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
                    if (cache.TryGetValue(o, out Dictionary<string, IIfcLibraryReference>  libs))
                    {
                        if (libs.ContainsKey(lib.Language))
                            libs[lib.Language] = lib;
                        else
                            libs.Add(lib.Language, lib);
                    }
                    else
                    {
                        libs = new Dictionary<string, IIfcLibraryReference> { { lib.Language, lib } };
                        cache.Add(o, libs);
                    }
                }
            }
        }

        private static void AddOrSet(IfcDefinitionSelect definition, string lang, IIfcLibraryReference lib)
        {
            if (!cache.TryGetValue(definition, out Dictionary<string, IIfcLibraryReference> libs))
            {
                libs = new Dictionary<string, IIfcLibraryReference>();
                cache.Add(definition, libs);
            }

            if (libs.ContainsKey(lang))
                libs[lang] = lib;
            else
                libs.Add(lang, lib);
        }

        public static void SetNameAndDescription(this IfcDefinitionSelect definition, string lang, string name, string description)
        {
            var lib = GetLib(definition, lang);
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
                    libRef.Description = description;
                    libRef.Language = lang;
                    libRef.Identification = dictionaryIdentifier;

                    AddOrSet(definition, lang, libRef);
                });
            });
        }

        public static string GetName(this IfcDefinitionSelect definition, string lang)
        {
            var lib = GetLib(definition, lang);
            if (lib == null) return null;
            return lib.Name;
        }

        public static void SetName(this IfcDefinitionSelect definition, string lang, string name)
        {
            SetNameAndDescription(definition, lang, name, null);
        }

        public static string GetDescription(this IfcDefinitionSelect definition, string lang)
        {
            var lib = GetLib(definition, lang);
            if (lib == null) return null;
            return lib.Description;
        }

        public static void SetDescription(this IfcDefinitionSelect definition, string lang, string description)
        {
            SetNameAndDescription(definition, lang, null, description);
        }
    }
}
