using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Xbim.MvdXml;

namespace LOIN.Validation
{
    public class MvdValidator
    {
        public string ValidateXsd(string path, ILogger logger)
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
    }
}
