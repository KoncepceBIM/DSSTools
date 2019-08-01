using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Xbim.Common;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.MeasureResource;

namespace DDSS.Utils
{
    /// <summary>
    /// Partial implementation of the units map. All units need to be defined in code in the Init() function.
    /// Examples are given for all kinds of units (simple SI, derived and conversion based)
    /// </summary>
    class UnitMap
    {
        private readonly IModel _model;
        private readonly Dictionary<string, IfcUnit> _unitsMap = new Dictionary<string, IfcUnit>();

        public UnitMap(IModel model)
        {
            _model = model;
            Init();
        }

        /// <summary>
        /// Returns unit from the cache if it exists there. When no unit is found null is returned and the
        /// event is logged as a warning.
        /// </summary>
        /// <param name="name">Name of the unit to retrieve. For example "m", "mm", "m.s-2" etc.</param>
        /// <returns>Unit if defined, null otherwise</returns>
        public IfcUnit this[string name]
        {
            get
            {
                name = NormalizeName(name);
                if (_unitsMap.TryGetValue(name, out IfcUnit unit))
                    return unit;

                Log.Warning($"Unit {name} is not defined!");
                return null;
            }
        }

        public string NormalizeName(string unitString)
        {
            // trim start and end white characters
            unitString = unitString.Trim();

            // normalize white spaces
            unitString = unitString.Replace("\r", "");
            unitString = unitString.Replace("\n", "");
            unitString = unitString.Replace("\t", ".");
            unitString = unitString.Replace(" ", ".");
            unitString = new Regex("\\.+").Replace(unitString, ".");

            // normalize multiplication
            unitString = unitString.Replace("·", "."); // Middle dot U+00B7
            unitString = unitString.Replace("∙", "."); // Bullet operator U+2219 
            unitString = unitString.Replace("•", "."); // Bullet U+2022 

            // normalize superscript
            unitString = unitString.Replace("⁰", "0"); // Bullet U+2022 
            unitString = unitString.Replace("¹", "1"); // Bullet U+2022 
            unitString = unitString.Replace("²", "2"); // Bullet U+2022 
            unitString = unitString.Replace("³", "3"); // Bullet U+2022 
            unitString = unitString.Replace("⁴", "4"); // Bullet U+2022 
            unitString = unitString.Replace("⁵", "5"); // Bullet U+2022 
            unitString = unitString.Replace("⁶", "6"); // Bullet U+2022 
            unitString = unitString.Replace("⁷", "7"); // Bullet U+2022 
            unitString = unitString.Replace("⁸", "8"); // Bullet U+2022 
            unitString = unitString.Replace("⁹", "9"); // Bullet U+2022 
            unitString = unitString.Replace("⁺", "+"); // Bullet U+2022 
            unitString = unitString.Replace("⁻", "-"); // Bullet U+2022 
            unitString = unitString.Replace("⁽", "("); // Bullet U+2022 
            unitString = unitString.Replace("⁾", ")"); // Bullet U+2022 

            return unitString;
        }

        //todo: This needs to be extended to implement all the units needed
        private void Init()
        {
            var i = _model.Instances;
            // simple units

            {   //metre
                var unit = i.New<IfcSIUnit>(u =>
                {
                    u.Name = IfcSIUnitName.METRE;
                    u.UnitType = IfcUnitEnum.LENGTHUNIT;
                });
                _unitsMap.Add("m", unit);
            }
            {   //milimetre
                var unit = i.New<IfcSIUnit>(u =>
                {
                    u.Name = IfcSIUnitName.METRE;
                    u.Prefix = IfcSIPrefix.MILLI;
                    u.UnitType = IfcUnitEnum.LENGTHUNIT;
                });
                _unitsMap.Add("mm", unit);
            }
            {   //centimetre
                var unit = i.New<IfcSIUnit>(u =>
                {
                    u.Name = IfcSIUnitName.METRE;
                    u.Prefix = IfcSIPrefix.CENTI;
                    u.UnitType = IfcUnitEnum.LENGTHUNIT;
                });
                _unitsMap.Add("cm", unit);
            }
            {   //square metre
                var unit = i.New<IfcSIUnit>(u =>
                {
                    u.Name = IfcSIUnitName.SQUARE_METRE;
                    u.UnitType = IfcUnitEnum.AREAUNIT;
                });
                _unitsMap.Add("m2", unit);
            }
            //...

            // conversion based units

            { // litre
                var m3 = i.New<IfcSIUnit>(u =>
                {
                    u.Name = IfcSIUnitName.CUBIC_METRE;
                    u.UnitType = IfcUnitEnum.VOLUMEUNIT;
                });
                var litre = i.New<IfcConversionBasedUnit>(u =>
                {
                    u.Name = "l";
                    u.ConversionFactor = i.New<IfcMeasureWithUnit>(m =>
                    {
                        m.ValueComponent = new IfcReal(0.001);
                        m.UnitComponent = m3;

                    });
                    u.Dimensions = i.New<IfcDimensionalExponents>(d =>
                    {
                        d.LengthExponent = 3;
                    });
                    u.UnitType = IfcUnitEnum.VOLUMEUNIT;
                });
                _unitsMap.Add("l", litre);
            }
            { // hour
                var s = i.New<IfcSIUnit>(u =>
                {
                    u.Name = IfcSIUnitName.SECOND;
                    u.UnitType = IfcUnitEnum.TIMEUNIT;
                });
                var hour = i.New<IfcConversionBasedUnit>(u =>
                {
                    u.Name = "h";
                    u.ConversionFactor = i.New<IfcMeasureWithUnit>(m =>
                    {
                        m.ValueComponent = new IfcReal(3600);
                        m.UnitComponent = s;

                    });
                    u.Dimensions = i.New<IfcDimensionalExponents>(d =>
                    {
                        d.TimeExponent = 1;
                    });
                    u.UnitType = IfcUnitEnum.TIMEUNIT;
                });
                _unitsMap.Add("h", hour);
            }
            // ...

            // complex units
            { // Thermal admittance 	W / m² · K
                var W = i.New<IfcSIUnit>(u =>
                {
                    u.Name = IfcSIUnitName.WATT;
                    u.UnitType = IfcUnitEnum.POWERUNIT;
                });
                var m = i.New<IfcSIUnit>(u =>
                {
                    u.Name = IfcSIUnitName.METRE;
                    u.UnitType = IfcUnitEnum.LENGTHUNIT;
                });
                var K = i.New<IfcSIUnit>(u =>
                {
                    u.Name = IfcSIUnitName.KELVIN;
                    u.UnitType = IfcUnitEnum.THERMODYNAMICTEMPERATUREUNIT;
                });
                var unit = i.New<IfcDerivedUnit>(du =>
                {
                    du.UnitType = IfcDerivedUnitEnum.THERMALADMITTANCEUNIT;
                    du.Elements.AddRange(new[] {
                        i.New<IfcDerivedUnitElement>(e => {
                            e.Unit = W;
                            e.Exponent = 1;
                        }),
                        i.New<IfcDerivedUnitElement>(e => {
                            e.Unit = m;
                            e.Exponent = -2;
                        }),
                        i.New<IfcDerivedUnitElement>(e => {
                            e.Unit = K;
                            e.Exponent = 1;
                        })
                    });
                });
                _unitsMap.Add("W/m2.K", unit);
            }

            { // kV · A
                var kV = i.New<IfcSIUnit>(u =>
                {
                    u.Prefix = IfcSIPrefix.KILO;
                    u.Name = IfcSIUnitName.VOLT;
                    u.UnitType = IfcUnitEnum.ELECTRICVOLTAGEUNIT;
                });
                var A = i.New<IfcSIUnit>(u =>
                {
                    u.Name = IfcSIUnitName.AMPERE;
                    u.UnitType = IfcUnitEnum.ELECTRICCURRENTUNIT;
                });
                
                var unit = i.New<IfcDerivedUnit>(du =>
                {
                    du.UnitType = IfcDerivedUnitEnum.USERDEFINED;
                    du.Elements.AddRange(new[] {
                        i.New<IfcDerivedUnitElement>(e => {
                            e.Unit = kV;
                            e.Exponent = 1;
                        }),
                        i.New<IfcDerivedUnitElement>(e => {
                            e.Unit = A;
                            e.Exponent = 1;
                        }),
                    });
                });
                _unitsMap.Add("kVA", unit);
            }
            //...

        }
    }
}
