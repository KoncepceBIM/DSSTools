using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xbim.Common;
using Xbim.Common.Enumerations;
using Xbim.Common.ExpressValidation;
using Xbim.Common.Metadata;
using Xbim.Ifc4.Interfaces;

namespace LOIN.Validation
{
    public class IfcValidator
    {
        public IEnumerable<ValidationResult> SchemaErrors { get; private set; } = Array.Empty<ValidationResult>();
        public IEnumerable<ValidationResult> TemplateErrors { get; private set; } = Array.Empty<ValidationResult>();
        public IEnumerable<ValidationResult> PropertyErrors { get; private set; } = Array.Empty<ValidationResult>();

        public bool Check(IModel model)
        {
            ILogger log = XbimLogging.CreateLogger<IfcValidator>();

            // check for parser exceptions
            var v = new Validator
            {
                ValidateLevel = ValidationFlags.All,
                CreateEntityHierarchy = true
            };

            SchemaErrors = v.Validate(model.Instances).ToList();
            TemplateErrors = CheckPropertyTemplateTypesAndUnits(model).ToList();
            PropertyErrors = CheckPropertyUnits(model).ToList();

            foreach (var err in
                SchemaErrors
                .Concat(TemplateErrors)
                .Concat(PropertyErrors))
            {
                var identity = err.Item.GetType().Name;
                if (err.Item is IPersistEntity entity)
                {
                    identity = $"#{entity.EntityLabel}={entity.ExpressType.ExpressName}";
                }
                var msg = new StringBuilder();
                msg.AppendLine($"{identity} is invalid.");
                var details = new Stack<ValidationResult>(err.Details);
                while (details.Any())
                {
                    var detail = details.Pop();
                    foreach (var d in detail.Details)
                        details.Push(d);

                    var report = detail.Message;
                    if (string.IsNullOrWhiteSpace(report))
                        report = detail.Report();
                    msg.AppendLine("    " + report);

                    if (detail.IssueType == ValidationFlags.EntityWhereClauses || detail.IssueType == ValidationFlags.TypeWhereClauses)
                    {
                        var source = detail.IssueSource.Split('.')[0].ToLower();
                        msg.AppendLine($"http://www.buildingsmart-tech.org/ifc/IFC4/Add2/html/link/{source}.htm");
                    }
                }
                log.LogError(msg.ToString());
            }

            return !SchemaErrors.Any() && !TemplateErrors.Any() && !PropertyErrors.Any();
        }

        private IEnumerable<ValidationResult> CheckPropertyUnits(IModel model)
        {
            var properties = model.Instances.OfType<IIfcSimpleProperty>();
            foreach (var property in properties)
            {
                var err = new ValidationResult
                {
                    IssueSource = "Template Types and Units validation",
                    Item = property,
                    IssueType = ValidationFlags.Properties
                };

                ValidationResult detail;
                if (property is IIfcPropertySingleValue single)
                {
                    if (single.NominalValue == null)
                        continue;
                    if ((detail = CheckUnit(single.NominalValue.GetType().Name, single.Unit, property)) != null)
                        err.AddDetail(detail);
                    continue;
                }

                if (property is IIfcPropertyListValue list)
                {
                    if (list.ListValues.FirstOrDefault() == null)
                        continue;
                    if ((detail = CheckUnit(list.ListValues.FirstOrDefault().GetType().Name, list.Unit, property)) != null)
                        err.AddDetail(detail);
                    continue;
                }

                if (property is IIfcPropertyBoundedValue bounded)
                {
                    if (bounded.UpperBoundValue != null)
                        if ((detail = CheckUnit(bounded.UpperBoundValue.GetType().Name, bounded.Unit, property)) != null)
                            err.AddDetail(detail);
                    if (bounded.SetPointValue != null)
                        if ((detail = CheckUnit(bounded.SetPointValue.GetType().Name, bounded.Unit, property)) != null)
                            err.AddDetail(detail);
                    if (bounded.LowerBoundValue != null)
                        if ((detail = CheckUnit(bounded.LowerBoundValue.GetType().Name, bounded.Unit, property)) != null)
                            err.AddDetail(detail);
                    continue;
                }

                if (property is IIfcPropertyTableValue table)
                {
                    if (table.DefinedValues.FirstOrDefault() != null)
                        if ((detail = CheckUnit(table.DefinedValues.FirstOrDefault().GetType().Name, table.DefinedUnit, property)) != null)
                            err.AddDetail(detail);
                    if (table.DefiningValues.FirstOrDefault() != null)
                        if ((detail = CheckUnit(table.DefiningValues.FirstOrDefault().GetType().Name, table.DefiningUnit, property)) != null)
                            err.AddDetail(detail);
                    continue;
                }

                if (err.Details != null && err.Details.Any())
                    yield return err;

            }
        }

        private IEnumerable<ValidationResult> CheckPropertyTemplateTypesAndUnits(IModel model)
        {
            var templates = model.Instances.OfType<IIfcSimplePropertyTemplate>();
            foreach (var property in templates)
            {
                if (!property.TemplateType.HasValue)
                {
                    // if template type is not defined there is no point in validation
                    continue;
                }

                var err = new ValidationResult
                {
                    IssueSource = "Template Types and Units validation",
                    Item = property,
                    IssueType = ValidationFlags.Properties
                };

                ValidationResult detail;
                switch (property.TemplateType.Value)
                {
                    case IfcSimplePropertyTemplateTypeEnum.P_SINGLEVALUE:
                    case IfcSimplePropertyTemplateTypeEnum.P_LISTVALUE:
                        if ((detail = CheckMeasureType(property.PrimaryMeasureType, model.Metadata, false)) != null)
                            err.AddDetail(detail);
                        if (property.PrimaryUnit != null && (detail = CheckUnit(property.PrimaryMeasureType, property.PrimaryUnit, property)) != null)
                            err.AddDetail(detail);
                        break;

                    case IfcSimplePropertyTemplateTypeEnum.P_ENUMERATEDVALUE:
                        if (property.Enumerators == null)
                        {
                            detail = new ValidationResult
                            {
                                Message = "Enumerators must be defined",
                                IssueType = ValidationFlags.Properties
                            };
                            err.AddDetail(detail);
                        }
                        break;

                    case IfcSimplePropertyTemplateTypeEnum.P_TABLEVALUE:
                    case IfcSimplePropertyTemplateTypeEnum.P_BOUNDEDVALUE:
                        if ((detail = CheckMeasureType(property.PrimaryMeasureType, model.Metadata, false)) != null)
                            err.AddDetail(detail);
                        if ((detail = CheckMeasureType(property.SecondaryMeasureType, model.Metadata, false)) != null)
                            err.AddDetail(detail);

                        if (property.PrimaryUnit != null && (detail = CheckUnit(property.PrimaryMeasureType, property.PrimaryUnit, property)) != null)
                            err.AddDetail(detail);
                        if (property.SecondaryUnit != null && (detail = CheckUnit(property.SecondaryMeasureType, property.SecondaryUnit, property)) != null)
                            err.AddDetail(detail);
                        break;

                    case IfcSimplePropertyTemplateTypeEnum.P_REFERENCEVALUE:
                        if ((detail = CheckMeasureType(property.PrimaryMeasureType, model.Metadata, true)) != null)
                            err.AddDetail(detail);
                        break;

                    case IfcSimplePropertyTemplateTypeEnum.Q_LENGTH:
                    case IfcSimplePropertyTemplateTypeEnum.Q_AREA:
                    case IfcSimplePropertyTemplateTypeEnum.Q_VOLUME:
                    case IfcSimplePropertyTemplateTypeEnum.Q_COUNT:
                    case IfcSimplePropertyTemplateTypeEnum.Q_WEIGHT:
                    case IfcSimplePropertyTemplateTypeEnum.Q_TIME:
                    default:
                        break;
                }

                if (err.Details != null && err.Details.Any())
                    yield return err;
            }
        }

        private ValidationResult CheckUnit(string measureType, IIfcUnit unit, IPersistEntity entity)
        {
            if (string.IsNullOrWhiteSpace(measureType))
                return null;

            measureType = measureType.ToUpperInvariant();

            // no kind is defined for some measures (boolean, text, monetary, date etc.)
            if (!MeasureUnitMaps.UnitKinds.TryGetValue(measureType, out UnitKind kind))
                return null;

            switch (kind)
            {
                case UnitKind.IfcUnitEnum:
                    {
                        if (!MeasureUnitMaps.Units.TryGetValue(measureType, out IfcUnitEnum unitType))
                            throw new ArgumentOutOfRangeException();
                        if (unit != null)
                        {
                            if (!(unit is IIfcNamedUnit namedUnit) || namedUnit.UnitType != unitType)
                            {
                                return new ValidationResult
                                {
                                    IssueType = ValidationFlags.Properties,
                                    Item = entity,
                                    Message = $"Named unit of type {unitType.ToString()} must be defined for {measureType}",
                                    IssueSource = "Units to Measure Types validation"
                                };
                            }
                        }
                        else
                        {
                            // try to find unit assignment
                            unit = entity.Model.Instances
                                .OfType<IIfcUnitAssignment>()
                                .Select(a => a.Units.FirstOrDefault<IIfcNamedUnit>(u => u.UnitType == unitType))
                                .FirstOrDefault();
                            if (unit == null)
                            {
                                return new ValidationResult
                                {
                                    IssueType = ValidationFlags.Properties,
                                    Item = entity,
                                    Message = $"Named unit of type {unitType.ToString()} must be defined for {measureType} but it was not found directly or in global units assignment.",
                                    IssueSource = "Units to Measure Types validation"
                                };
                            }
                        }
                    }
                    break;
                case UnitKind.IfcDerivedUnitEnum:
                    {
                        if (!MeasureUnitMaps.DerivedUnits.TryGetValue(measureType, out IfcDerivedUnitEnum unitType))
                            throw new ArgumentOutOfRangeException();
                        if (unit != null)
                        {
                            if (!(unit is IIfcDerivedUnit derivedUnit) || derivedUnit.UnitType != unitType)
                            {
                                return new ValidationResult
                                {
                                    IssueType = ValidationFlags.Properties,
                                    Item = entity,
                                    Message = $"Derived unit of type {unitType.ToString()} must be defined for {measureType}",
                                    IssueSource = "Units to Measure Types validation"
                                };
                            }
                        }
                        else
                        {
                            // try to find unit assignment
                            unit = entity.Model.Instances
                                .OfType<IIfcUnitAssignment>()
                                .Select(a => a.Units.FirstOrDefault<IIfcDerivedUnit>(u => u.UnitType == unitType))
                                .FirstOrDefault();
                            if (unit == null)
                            {
                                return new ValidationResult
                                {
                                    IssueType = ValidationFlags.Properties,
                                    Item = entity,
                                    Message = $"Derived unit of type {unitType.ToString()} must be defined for {measureType} but it was not found directly or in global units assignment.",
                                    IssueSource = "Units to Measure Types validation"
                                };
                            }
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return null;
        }

        private ValidationResult CheckMeasureType(string typeName, ExpressMetaData metadata, bool reference)
        {
            // null or empty is a valid value
            if (string.IsNullOrWhiteSpace(typeName))
                return null;

            typeName = typeName.ToUpperInvariant();
            var err = new ValidationResult
            {
                IssueSource = "Measure type validation",
                IssueType = ValidationFlags.Properties,

            };
            if (!metadata.TryGetExpressType(typeName, out ExpressType eType))
            {
                err.Message = $"Type {typeName} is not an IFC type.";
                return err;
            }

            var type = eType.Type;
            if (!reference && !typeof(IIfcValue).IsAssignableFrom(type))
            {
                err.Message = $"Type {typeName} is not applicable. This must be assignable to 'IfcValue'";
                return err;
            }

            if (reference && !typeof(IIfcObjectReferenceSelect).IsAssignableFrom(type))
            {
                err.Message = $"Type {typeName} is not applicable. This must be assignable to 'IfcObjectReferenceSelect'";
                return err;
            }

            return null;
        }
    }
}
