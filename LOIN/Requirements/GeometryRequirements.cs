using System;
using System.Collections.Generic;
using Xbim.Common;
using Xbim.Ifc4.PropertyResource;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.MeasureResource;
using System.Linq;

namespace LOIN.Requirements
{
    public class GeometryRequirements
    {
        public const string PSetName = "Geometry requirements";

        public IfcPropertySet PSet { get => _pSet; }
        internal IfcPropertySet _pSet;

        private const string _pID = "ID";
        private const string _pName = "Name";
        private const string _pDefinition = "Definition";
        private const string _pDetailing = "Detailing";
        private const string _pDimensionality = "Dimensionality";
        private const string _pAppearance = "Appearance";
        private const string _pLocation = "Location";
        private const string _pParametricBehaviour = "ParametricBehaviour";

        internal GeometryRequirements(IfcPropertySet geometryRequirements)
        {
            _pSet = geometryRequirements;
        }

        internal GeometryRequirements(IModel model)
        {
            _pSet = model.Instances.New<IfcPropertySet>(ps =>
            {
                ps.Name = PSetName;
            });
        }

        public string ID
        {
            get => GetPropertyString(_pID);
            set => SetProperty(_pID, value != null ? new IfcIdentifier(value) : null);
        }

        public string Name
        {
            get => GetPropertyString(_pName);
            set => SetProperty(_pName, value != null ? new IfcLabel(value) : null);
        }

        public string Definition
        {
            get => GetPropertyString(_pDefinition);
            set => SetProperty(_pDefinition, value != null ? new IfcText(value) : null);
        }

        public DetailingEnum? Detailing
        {
            get => GetEnumProperty<DetailingEnum>(_pDetailing);
            set => SetEnumProperty(_pDetailing, value);
        }

        public DimensionalityEnum? Dimensionality
        {
            get => GetEnumProperty<DimensionalityEnum>(_pDimensionality);
            set => SetEnumProperty(_pDimensionality, value);
        }

        public string Appearance
        {
            get => GetPropertyString(_pAppearance);
            set => SetProperty(_pAppearance, value != null ? new IfcText(value) : null);
        }

        public string Location
        {
            get => GetPropertyString(_pLocation);
            set => SetProperty(_pLocation, value != null ? new IfcText(value) : null);
        }

        public string ParametricBehaviour
        {
            get => GetPropertyString(_pParametricBehaviour);
            set => SetProperty(_pParametricBehaviour, value != null ? new IfcText(value) : null);
        }

        private IfcPropertySingleValue GetProperty(string name)
        {
            return _pSet.HasProperties
                .FirstOrDefault<IfcPropertySingleValue>(prop => prop.Name == name);
        }

        private string GetPropertyString(string name)
        {
            return GetProperty(name)?.NominalValue?.ToString();
        }

        private bool? GetPropertyBool(string name)
        {
            var value = GetProperty(name)?.NominalValue;
            if (value == null)
                return null;

            if (value.UnderlyingSystemType == typeof(bool))
                return (bool)(value.Value);

            if (value.UnderlyingSystemType == typeof(bool?))
                return (bool?)(value.Value);

            return null;
        }

        private IfcPropertySingleValue GetOrCreateProperty(string name)
        {
            var prop = GetProperty(name);
            if (prop != null)
                return prop;

            prop = _pSet.Model.Instances.New<IfcPropertySingleValue>(p => p.Name = name);
            _pSet.HasProperties.Add(prop);
            return prop;
        }
        private void SetProperty(string name, IfcValue value)
        {
            var prop = GetOrCreateProperty(name);
            prop.NominalValue = value;
        }

        private T? GetEnumProperty<T>(string name) where T : struct
        {
            var p = _pSet.HasProperties
                .FirstOrDefault<IfcPropertyEnumeratedValue>(prop => prop.Name == name);
            if (p == null)
                return null;

            var value = p.EnumerationValues.FirstOrDefault()?.ToString();
            if (value == null)
                return null;

            if (!Enum.TryParse<T>(value, out T result))
                return null;

            return result;
        }

        private IfcPropertyEnumeratedValue SetEnumProperty<T>(string name, T? value) where T : struct
        {
            var p = _pSet.HasProperties
                .FirstOrDefault<IfcPropertyEnumeratedValue>(prop => prop.Name == name);
            if (p == null)
            {
                p = _pSet.Model.Instances.New<IfcPropertyEnumeratedValue>(prop => prop.Name = name);
                _pSet.HasProperties.Add(p);
            }

            // make sure the set is empty
            p.EnumerationValues.Clear();
            if (value.HasValue)
                p.EnumerationValues.Add(new IfcIdentifier(Enum.GetName(typeof(T), value.Value)));

            // make sure there is a proper enumeration reference
            if (p.EnumerationReference == null)
                p.EnumerationReference = GetPropertyEnumeration<T>();

            return p;
        }

        private readonly Dictionary<string, IfcPropertyEnumeration> _pEnumCache = new Dictionary<string, IfcPropertyEnumeration>();

        private IfcPropertyEnumeration GetPropertyEnumeration<T>() where T : struct
        {
            var name = typeof(T).Name;

            if (_pEnumCache.TryGetValue(name, out IfcPropertyEnumeration pEnum))
                return pEnum;

            pEnum = _pSet.Model.Instances
                .FirstOrDefault<IfcPropertyEnumeration>(e => e.Name == name);
            if (pEnum != null)
            {
                _pEnumCache.Add(name, pEnum);
                return pEnum;
            }

            var pValues = Enum.GetNames(typeof(T))
                .Select(v => new IfcIdentifier(v) as IfcValue);
            pEnum = _pSet.Model.Instances.New<IfcPropertyEnumeration>(e =>
            {
                e.Name = name;
                e.EnumerationValues.AddRange(pValues);
            });

            _pEnumCache.Add(name, pEnum);
            return pEnum;
        }
    }

    public enum DetailingEnum
    {
        Simplified,
        LowDetail,
        MediumDetail,
        HighDetail,
        FinalDetail
    }

    public enum DimensionalityEnum
    {
        Dim_0D,
        Dim_1D,
        Dim_2D,
        Dim_3D
    }
}
