using System;
using System.Collections.Generic;
using System.Text;
using Xbim.Common;
using Xbim.Ifc4.ExternalReferenceResource;

namespace LOIN
{
    public static class CCIExtenxion
    {
        public const string cciSEidentifier = "CciSE_dictionary";
        public const string cciVSidentifier = "CciVS_dictionary";
        public const string cciFSidentifier = "CciFS_dictionary";
        public const string cciTSidentifier = "CciTS_dictionary";
        public const string cciKOidentifier = "CciKO_dictionary";
        public const string cciSKidentifier = "CciSK_dictionary";

        private static LangCache GetCache(IModel model, string dictionaryIdentifier)
        {
            return model.GetCache(dictionaryIdentifier, () => new LangCache(model, dictionaryIdentifier));
        }

        public static string GetCCI_SE(this IfcExternalReference definition, string lang) => GetCache(definition.Model, cciSEidentifier).GetDescription(definition, lang);
        public static void SetCCI_SE(this IfcExternalReference definition, string lang, string note)=>GetCache(definition.Model, cciSEidentifier).SetDescription(definition, lang, note);

        public static string GetCCI_VS(this IfcExternalReference definition, string lang) => GetCache(definition.Model, cciVSidentifier).GetDescription(definition, lang);
        public static void SetCCI_VS(this IfcExternalReference definition, string lang, string note) => GetCache(definition.Model, cciVSidentifier).SetDescription(definition, lang, note);

        public static string GetCCI_FS(this IfcExternalReference definition, string lang) => GetCache(definition.Model, cciFSidentifier).GetDescription(definition, lang);
        public static void SetCCI_FS(this IfcExternalReference definition, string lang, string note) => GetCache(definition.Model, cciFSidentifier).SetDescription(definition, lang, note);

        public static string GetCCI_TS(this IfcExternalReference definition, string lang) => GetCache(definition.Model, cciTSidentifier).GetDescription(definition, lang);
        public static void SetCCI_TS(this IfcExternalReference definition, string lang, string note) => GetCache(definition.Model, cciTSidentifier).SetDescription(definition, lang, note);

        public static string GetCCI_KO(this IfcExternalReference definition, string lang) => GetCache(definition.Model, cciKOidentifier).GetDescription(definition, lang);
        public static void SetCCI_KO(this IfcExternalReference definition, string lang, string note) => GetCache(definition.Model, cciKOidentifier).SetDescription(definition, lang, note);

        public static string GetCCI_SK(this IfcExternalReference definition, string lang) => GetCache(definition.Model, cciSKidentifier).GetDescription(definition, lang);
        public static void SetCCI_SK(this IfcExternalReference definition, string lang, string note) => GetCache(definition.Model, cciSKidentifier).SetDescription(definition, lang, note);
    }
}
