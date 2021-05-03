using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Ifc4.Interfaces;

namespace LOIN.Server.Contracts
{
    public static class Unit
    {
        public static string GetSymbol(IIfcUnit unit)
        {
            if (unit is IIfcDerivedUnit du)
                return GetSymbol(du);
            else if (unit is IIfcContextDependentUnit cdu)
                return cdu.Name;
            else if (unit is IIfcConversionBasedUnit cbu)
                return cbu.Name;
            else if (unit is IIfcSIUnit si)
                return GetSymbol(si);
            else if (unit is IIfcMonetaryUnit mu)
                return mu.Currency;
            throw new ArgumentOutOfRangeException(nameof(unit), "Unexpected unit type " + unit.GetType().Name);
        }

        public static string GetSymbol(IIfcDerivedUnit du)
        {
            var symbol = du.UnitType switch
            {
                IfcDerivedUnitEnum.PHUNIT => "pH",
                IfcDerivedUnitEnum.SOUNDPOWERUNIT => "W",
                IfcDerivedUnitEnum.SOUNDPOWERLEVELUNIT => "db",
                IfcDerivedUnitEnum.SOUNDPRESSUREUNIT => "Pa",
                IfcDerivedUnitEnum.SOUNDPRESSURELEVELUNIT => "db",
                _ => string.Empty
            };
            if (!string.IsNullOrEmpty(symbol))
                return symbol;

            var sb = new StringBuilder();
            foreach (var item in du.Elements)
            {
                var unit = GetSymbol(item.Unit);
                sb.Append(unit);
                var exponent = GetExponentSymbol(item.Exponent);
                sb.Append(exponent);
            }

            return sb.ToString();
        }

        private static string GetExponentSymbol(long exponent)
        {
            if (exponent == 1) return " ";

            var exponentString = exponent.ToString(CultureInfo.InvariantCulture);
            
            // translate to unicode exponents
            var sb = new StringBuilder();
            foreach (var letter in exponentString)
            {
                var part = letter switch 
                {
                    '-' => '⁻',
                    '1' => '¹',
                    '2' => '²',
                    '3' => '³',
                    '4' => '⁴',
                    '5' => '⁵',
                    '6' => '⁶',
                    '7' => '⁷',
                    '8' => '⁸',
                    '9' => '⁹',
                    _ => letter
                };
                sb.Append(part);
            }
            return sb.ToString();
        }

        public static string GetSymbol(IIfcSIUnit unit)
        {
            string prefix = string.Empty;
            if (unit.Prefix.HasValue)
            {
                prefix = unit.Prefix.Value switch
                {
                    IfcSIPrefix.EXA => "E",
                    IfcSIPrefix.PETA => "P",
                    IfcSIPrefix.TERA => "T",
                    IfcSIPrefix.GIGA => "G",
                    IfcSIPrefix.MEGA => "M",
                    IfcSIPrefix.KILO => "k",
                    IfcSIPrefix.HECTO => "h",
                    IfcSIPrefix.DECA => "da",
                    IfcSIPrefix.DECI => "d",
                    IfcSIPrefix.CENTI => "c",
                    IfcSIPrefix.MILLI => "m",
                    IfcSIPrefix.MICRO => "µ",
                    IfcSIPrefix.NANO => "n",
                    IfcSIPrefix.PICO => "p",
                    IfcSIPrefix.FEMTO => "f",
                    IfcSIPrefix.ATTO => "a",
                    _ => throw new ArgumentOutOfRangeException("Unexpected SI unit prefix"),
                };
            }

            string name = unit.Name switch
            {
                IfcSIUnitName.AMPERE => "A",
                IfcSIUnitName.BECQUEREL => "Bq",
                IfcSIUnitName.CANDELA => "cd",
                IfcSIUnitName.COULOMB => "C",
                IfcSIUnitName.CUBIC_METRE => "m³",
                IfcSIUnitName.DEGREE_CELSIUS => "°C",
                IfcSIUnitName.FARAD => "F",
                IfcSIUnitName.GRAM => "g",
                IfcSIUnitName.GRAY => "Gy",
                IfcSIUnitName.HENRY => "H",
                IfcSIUnitName.HERTZ => "Hz",
                IfcSIUnitName.JOULE => "J",
                IfcSIUnitName.KELVIN => "K",
                IfcSIUnitName.LUMEN => "lm",
                IfcSIUnitName.LUX => "lx",
                IfcSIUnitName.METRE => "m",
                IfcSIUnitName.MOLE => "mol",
                IfcSIUnitName.NEWTON => "N",
                IfcSIUnitName.OHM => "Ω",
                IfcSIUnitName.PASCAL => "Pa",
                IfcSIUnitName.RADIAN => "rad",
                IfcSIUnitName.SECOND => "s",
                IfcSIUnitName.SIEMENS => "S",
                IfcSIUnitName.SIEVERT => "Sv",
                IfcSIUnitName.SQUARE_METRE => "m²",
                IfcSIUnitName.STERADIAN => "sr",
                IfcSIUnitName.TESLA => "T",
                IfcSIUnitName.VOLT => "V",
                IfcSIUnitName.WATT => "W",
                IfcSIUnitName.WEBER => "Wb",
                _ => throw new ArgumentOutOfRangeException("Unexpected SI unit prefix"),
            };
            return prefix + name;
        }
    }
}
