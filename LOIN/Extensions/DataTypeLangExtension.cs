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

        private static LangCache GetCache(IModel model)
        {
            return model.GetCache(dictionaryIdentifier, () => new LangCache(model, dictionaryIdentifier));
        }

        public static string GetDataTypeName(this IIfcPropertyTemplate definition, string lang)
        {
            var c = GetCache(definition.Model);
            return c.GetName(definition, lang);
        }



        public static void SetDataTypeName(this IIfcPropertyTemplate definition, string lang, string name)
        {
            var c = GetCache(definition.Model);
            c.SetName(definition, lang, name);
        }



        public static string GetDataTypeDescription(this IIfcPropertyTemplate definition, string lang)
        {
            var c = GetCache(definition.Model);
            return c.GetDescription(definition, lang);
        }

   

        public static void SetDataTypeDescription(this IIfcPropertyTemplate definition, string lang, string description)
        {
            var c = GetCache(definition.Model);
            c.SetDescription(definition, lang, description);
        }
    }
}
