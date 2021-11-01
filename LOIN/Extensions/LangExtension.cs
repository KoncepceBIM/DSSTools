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

        private static LangCache GetCache(IModel model)
        {
            return model.GetCache(dictionaryIdentifier, () => new LangCache(model, dictionaryIdentifier));
        }

        public static string GetName(this IfcDefinitionSelect definition, string lang)
        {
            var c = GetCache(definition.Model);
            return c.GetName(definition, lang);
        }

        public static string GetName(this IfcExternalReference reference, string lang)
        {
            var c = GetCache(reference.Model);
            return c.GetName(reference, lang);
        }

        public static void SetName(this IfcDefinitionSelect definition, string lang, string name)
        {
            var c = GetCache(definition.Model);
            c.SetName(definition, lang, name);
        }

        public static void SetName(this IfcExternalReference reference, string lang, string name)
        {
            var c = GetCache(reference.Model);
            c.SetName(reference, lang, name);
        }

        public static string GetDescription(this IfcDefinitionSelect definition, string lang)
        {
            var c = GetCache(definition.Model);
            return c.GetDescription(definition, lang);
        }

        public static string GetDescription(this IfcExternalReference reference, string lang)
        {
            var c = GetCache(reference.Model);
            return c.GetDescription(reference, lang);
        }

        public static void SetDescription(this IfcDefinitionSelect definition, string lang, string description)
        {
            var c = GetCache(definition.Model);
            c.SetDescription(definition, lang, description);
        }
        public static void SetDescription(this IfcExternalReference reference, string lang, string description)
        {
            var c = GetCache(reference.Model);
            c.SetDescription(reference, lang, description);
        }
    }

    internal class LangCache
    {
        private readonly Dictionary<int, Dictionary<string, IIfcLibraryReference>> cache = new Dictionary<int, Dictionary<string, IIfcLibraryReference>>();
        private readonly IModel model;
        private readonly string identifier;

        public LangCache(IModel model, string identifier)
        {
            this.model = model;
            this.identifier = identifier;

            CreateCache();
        }

        private IIfcLibraryReference GetLib(int entityLabel, string lang)
        {
            var libs = GetLibs(entityLabel);
            if (libs == null)
                return null;
            if (libs.TryGetValue(lang, out IIfcLibraryReference lib))
                return lib;
            return null;
        }

        private Dictionary<string, IIfcLibraryReference> GetLibs(int entityLabel)
        {
            if (cache.TryGetValue(entityLabel, out Dictionary<string, IIfcLibraryReference> libs))
                return libs;
            return null;
        }

        private void CreateCache()
        {
            foreach (var rel in model.Instances.OfType<IIfcRelAssociatesLibrary>()
                .Where(r => r.RelatingLibrary is IIfcLibraryReference lr &&
                    lr.Identification == identifier &&
                    lr.Language.HasValue))
            {
                var lib = rel.RelatingLibrary as IIfcLibraryReference;
                foreach (var o in rel.RelatedObjects)
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

            foreach (var rel in model.Instances.OfType<IIfcExternalReferenceRelationship>()
                .Where(r => r.RelatingReference is IIfcLibraryReference lr &&
                    lr.Identification == identifier &&
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

        private void AddOrSet(IfcDefinitionSelect definition, string lang, IIfcLibraryReference lib)
        {
            CheckModel(definition);
            AddOrSet(definition.EntityLabel, lang, lib);
        }

        private void AddOrSet(IfcExternalReference reference, string lang, IIfcLibraryReference lib)
        {
            CheckModel(reference);
            AddOrSet(reference.EntityLabel, lang, lib);
        }

        private void AddOrSet(int entityLabel, string lang, IIfcLibraryReference lib)
        {
            if (!cache.TryGetValue(entityLabel, out Dictionary<string, IIfcLibraryReference> libs))
            {
                libs = new Dictionary<string, IIfcLibraryReference>();
                cache.Add(entityLabel, libs);
            }

            if (libs.ContainsKey(lang))
                libs[lang] = lib;
            else
                libs.Add(lang, lib);
        }

        private void CheckModel(IPersistEntity entity)
        {
            if (model != entity.Model)
                throw new System.Exception("Model mismatch");
        }

        public string GetName(IfcDefinitionSelect definition, string lang)
        {
            var lib = GetLib(definition.EntityLabel, lang);
            return lib?.Name;
        }

        public string GetName(IfcExternalReference reference, string lang)
        {
            var lib = GetLib(reference.EntityLabel, lang);
            return lib?.Name;
        }

        public void SetName(IfcDefinitionSelect definition, string lang, string name)
        {
            var lib = GetLib(definition.EntityLabel, lang);
            if (lib != null)
            {
                lib.Name = name;
                return;
            }

            var i = definition.Model.Instances;
            i.New<IfcRelAssociatesLibrary>(r =>
            {
                r.RelatedObjects.Add(definition);
                r.RelatingLibrary = i.New<IfcLibraryReference>(libRef =>
                {
                    libRef.Name = name;
                    libRef.Language = lang;
                    libRef.Identification = identifier;

                    AddOrSet(definition, lang, libRef);
                });
            });
        }



        public void SetName(IfcExternalReference reference, string lang, string name)
        {
            var lib = GetLib(reference.EntityLabel, lang);
            if (lib != null)
            {
                lib.Name = name;
                return;
            }

            var i = reference.Model.Instances;
            i.New<IfcExternalReferenceRelationship>(r =>
            {
                r.RelatedResourceObjects.Add(reference);
                r.RelatingReference = i.New<IfcLibraryReference>(libRef =>
                {
                    libRef.Name = name;
                    libRef.Language = lang;
                    libRef.Identification = identifier;

                    AddOrSet(reference, lang, libRef);
                });
            });
        }

        public string GetDescription(IfcDefinitionSelect definition, string lang)
        {
            var lib = GetLib(definition.EntityLabel, lang);
            return lib?.Description;
        }

        public string GetDescription(IfcExternalReference reference, string lang)
        {
            var lib = GetLib(reference.EntityLabel, lang);
            return lib?.Description;
        }

        public void SetDescription(IfcDefinitionSelect definition, string lang, string description)
        {
            var lib = GetLib(definition.EntityLabel, lang);
            if (lib != null)
            {
                lib.Description = description;
                return;
            }

            var i = definition.Model.Instances;
            i.New<IfcRelAssociatesLibrary>(r =>
            {
                r.RelatedObjects.Add(definition);
                r.RelatingLibrary = i.New<IfcLibraryReference>(libRef =>
                {
                    libRef.Description = description;
                    libRef.Language = lang;
                    libRef.Identification = identifier;

                    AddOrSet(definition, lang, libRef);
                });
            });
        }
        public void SetDescription(IfcExternalReference reference, string lang, string description)
        {
            var lib = GetLib(reference.EntityLabel, lang);
            if (lib != null)
            {
                lib.Description = description;
                return;
            }

            var i = reference.Model.Instances;
            i.New<IfcExternalReferenceRelationship>(r =>
            {
                r.RelatedResourceObjects.Add(reference);
                r.RelatingReference = i.New<IfcLibraryReference>(libRef =>
                {
                    libRef.Description = description;
                    libRef.Language = lang;
                    libRef.Identification = identifier;

                    AddOrSet(reference, lang, libRef);
                });
            });
        }
    }
}
