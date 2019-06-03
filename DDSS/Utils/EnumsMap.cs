using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xbim.Common;
using Xbim.Ifc4.MeasureResource;
using Xbim.Ifc4.PropertyResource;

namespace DDSS.Utils
{
    class EnumsMap
    {
        private readonly Dictionary<string, IfcPropertyEnumeration> _cache = new Dictionary<string, IfcPropertyEnumeration>();
        private readonly IModel _model;

        public EnumsMap(IModel model)
        {
            _model = model;

            // cache any existing enums
            foreach (var item in model.Instances.OfType<IfcPropertyEnumeration>())
            {
                if (item.Name != "" && !_cache.ContainsKey(item.Name))
                    _cache.Add(item.Name, item);
            }
        }

        public IfcPropertyEnumeration this[string key]
        {
            get
            {
                if (_cache.TryGetValue(key, out IfcPropertyEnumeration enumeration))
                    return enumeration;
                return null;
            }
        }

        public IfcPropertyEnumeration GetOrAdd(string name, string[] values)
        {
            return GetOrAdd(name, name, values);
        }

        public IfcPropertyEnumeration GetOrAdd(string key, string name, string[] values)
        {
            if (_cache.TryGetValue(key, out IfcPropertyEnumeration result))
            {
                var hash = new HashSet<string>(values);
                if (result.Name == name && result.EnumerationValues.Select(v => v.ToString()).All(v => hash.Contains(v)))
                {
                    return result;
                }
                else
                {
                    throw new Exception($"IfcPropertyEnumeration with key {key} already exists but has different name {name}/{result.Name} and/or values [{string.Join(',', values)}]/[{string.Join(',', hash)}]");
                }
            }

            var enumValues = values.Select(v => new IfcLabel(v)).Cast<IfcValue>();
            result = _model.Instances.New<IfcPropertyEnumeration>(e => {
                e.Name = name;
                e.EnumerationValues.AddRange(enumValues);
            });
            _cache.Add(key, result);
            return result;
        }
    }
}
