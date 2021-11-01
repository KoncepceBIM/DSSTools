using System.Collections.Generic;
using System.Linq;
using Xbim.Common;
using Xbim.Ifc4.ExternalReferenceResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;

namespace LOIN
{
    public static class NoteLangExtension
    {
        public const string dictionaryIdentifier = "note_dictionary";

        private static LangCache GetCache(IModel model)
        {
            return model.GetCache(dictionaryIdentifier, () => new LangCache(model, dictionaryIdentifier));
        }


        public static string GetNote(this IfcDefinitionSelect definition, string lang)
        {
            var c = GetCache(definition.Model);
            return c.GetDescription(definition, lang);
        }

        public static void SetNote(this IfcDefinitionSelect definition, string lang, string note)
        {
            var c = GetCache(definition.Model);
            c.SetDescription(definition, lang, note);
        }

        public static string GetNote(this IfcExternalReference definition, string lang)
        {
            var c = GetCache(definition.Model);
            return c.GetDescription(definition, lang);
        }

        public static void SetNote(this IfcExternalReference definition, string lang, string note)
        {
            var c = GetCache(definition.Model);
            c.SetDescription(definition, lang, note);
        }
    }
}
