using System.Collections.Generic;
using System.Linq;
using Xbim.Common;
using Xbim.Ifc4.ExternalReferenceResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;

namespace LOIN
{
    public static class IfcTypeLangExtension
    {
        public const string identifier = "ifc_type_dictionary";
        private const string ifc = "ifc";

        private static LangCache GetCache(IModel model)
        {
            return model.GetCache(identifier, () => new LangCache(model, identifier));
        }

        public static string GetIFCType(this IfcExternalReference definition)
        {
            var c = GetCache(definition.Model);
            return c.GetDescription(definition, ifc);
        }

        public static void SetIFCType(this IfcExternalReference definition, string type)
        {
            var c = GetCache(definition.Model);
            c.SetDescription(definition, ifc, type);
        }
    }

    public static class IfcPredefinedTypeLangExtension
    {
        public const string identifier = "ifc_predefined_type_dictionary";
        private const string ifc = "ifc";

        private static LangCache GetCache(IModel model)
        {
            return model.GetCache(identifier, () => new LangCache(model, identifier));
        }

        public static string GetIFCPredefinedType(this IfcExternalReference definition)
        {
            var c = GetCache(definition.Model);
            return c.GetDescription(definition, ifc);
        }

        public static void SetIFCPredefinedType(this IfcExternalReference definition, string type)
        {
            var c = GetCache(definition.Model);
            c.SetDescription(definition, ifc, type);
        }
    }
}
