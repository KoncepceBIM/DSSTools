using LOIN.Context;
using LOIN.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using Xbim.Ifc4.ExternalReferenceResource;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.MeasureResource;
using Xbim.Ifc4.PropertyResource;
using Xbim.MvdXml;

namespace LOIN.Mvd
{
    internal class Converter
    {
        private readonly string _language;
        private mvdXML _mvd;
        private ModelView _view;
        private Dictionary<string, ModelViewExchangeRequirement> _requirementsCache = new Dictionary<string, ModelViewExchangeRequirement>();

        // uuids for references
        private static readonly string objectsAndTypesUuid = NewGuid();
        private static readonly string psetsUuid = NewGuid();
        private static readonly string singleValueUuid = NewGuid();
        private static readonly string classificationUuid = NewGuid();

        private const string IFC4 = "IFC4";
        private static readonly string[] schemas = new[] { IFC4 };

        public Converter(string primaryLanguage)
        {
            _language = primaryLanguage;
        }

        public mvdXML Convert(Model model, string name, string definition, string code)
        {
            _mvd = InitMvd(name, code, definition);
            _requirementsCache = new Dictionary<string, ModelViewExchangeRequirement>();

            // iterate over the model to convert all requirements
            foreach (var item in model.BreakdownStructure)
            {
                var applicability = CreateApplicabilityRules(item);
                var requirements = CreateConcepts(model, item).ToList();
                if (!requirements.Any())
                    continue;
                AddConceptRoot(item.Name, null, item.Description, applicability, requirements);
            }

            return _mvd;
        }

        private IEnumerable<Concept> CreateConcepts(Model model, BreakedownItem item)
        {
            var concepts = new List<Concept>();
            var requirementSets = model.GetRequirements(item);
            foreach (var requirementSet in requirementSets)
            {
                // get or create exchange requirements. This is a combination of actor, milestone and reason
                var requirements = GetOrCreateRequirements(requirementSet);

                // create rules
                var rules = CreateValidationRules(requirementSet.Requirements);
                if (rules == null)
                    continue;

                // create concept
                var concept = CreateConcept(item.Name, item.Description, rules, requirements);
                concepts.Add(concept);
            }
            return concepts;
        }

        private IEnumerable<ModelViewExchangeRequirement> GetOrCreateRequirements(RequirementsSet set)
        {
            var actors = set.Actors.Select(a => a.Name ?? "").ToList();
            if (!actors.Any()) actors.Add("N/A");

            var milestones = set.Milestones.Select(a => a.Name ?? "").ToList();
            if (!milestones.Any()) milestones.Add("N/A");

            var reasons = set.Reasons.Select(a => a.Name ?? "").ToList();
            if (!reasons.Any()) reasons.Add("N/A");

            foreach (var actor in actors)
            {
                foreach (var milestone in milestones)
                {
                    foreach (var reason in reasons)
                    {
                        var key = $"{actor}-{milestone}-{reason}";
                        if (!_requirementsCache.TryGetValue(key, out ModelViewExchangeRequirement requirement))
                        {
                            var definition = $"Actor: {actor}, Milestone: {milestone}, Reason: {reason}";
                            requirement = AddRequirement(key, null, definition);
                            _requirementsCache.Add(key, requirement);
                        }
                        yield return requirement;
                    }
                }
            }
        }

        private TemplateRules CreateValidationRules(IEnumerable<IfcPropertySetTemplate> pSets)
        {
            var rules = pSets
                .SelectMany(ps => ps.HasPropertyTemplates.Select(p => new { PSet = ps.Name, PName = p.Name }))
                .Select(p => CreatePropertyExistanceRule(p.PSet, p.PName))
                .ToArray();

            if (rules.Length == 0)
                return null;

            return CreateLogicalRule(TemplateRulesOperator.and, rules);
        }

        private TemplateRules CreateApplicabilityRules(BreakedownItem item)
        {
            var classificationCode = CreateClassificationRule(item.Code);
            var classificationName = CreateClassificationRule(item.Name);
            var classificationCodeProperty = CreatePropertyValueRule("Classification", item.Code);
            var classificationNameProperty = CreatePropertyValueRule("Classification", item.Name);
            return CreateLogicalRule(TemplateRulesOperator.or,
                classificationCode,
                classificationName,
                classificationCodeProperty,
                classificationNameProperty);
        }

        /// <summary>
        /// Enumeration of exchange requirements defined in the MVD
        /// </summary>
        public IEnumerable<ModelViewExchangeRequirement> ExchangeRequirements =>
            _view.ExchangeRequirements ??
            Enumerable.Empty<ModelViewExchangeRequirement>();

        /// <summary>
        /// Adds new exchange requirement to MVD. This can be used later to define scope of concept root template rules
        /// </summary>
        /// <param name="name">Name of the requirement</param>
        /// <param name="code">Code of requirement</param>
        /// <param name="definition">Definition of requirement</param>
        /// <returns></returns>
        public ModelViewExchangeRequirement AddRequirement(string name, string code, string definition)
        {
            var result = new ModelViewExchangeRequirement
            {
                uuid = NewGuid(),
                name = name,
                code = code,
                applicability = applicability.export,
                Definitions = new[]{
                    new DefinitionsDefinition{
                        Body = new DefinitionsDefinitionBody{ lang= _language, Value = definition}
                    }
                }
            };

            var requirements = new List<ModelViewExchangeRequirement>(ExchangeRequirements)
            {
                result
            };
            _view.ExchangeRequirements = requirements.ToArray();
            return result;
        }

        /// <summary>
        /// Lists all concept roots in the MVD
        /// </summary>
        public IEnumerable<ConceptRoot> ConceptRoots =>
            _view.Roots ?? Enumerable.Empty<ConceptRoot>();

        /// <summary>
        /// Adds a concept root
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="code">Code</param>
        /// <param name="definition">Definition</param>
        /// <param name="applicability">Applicability rules</param>
        /// <param name="requirements">Concepts containing requirement rules</param>
        /// <returns></returns>
        public ConceptRoot AddConceptRoot(string name, string code, string definition, TemplateRules applicability, IEnumerable<Concept> requirements)
        {
            var result = new ConceptRoot
            {
                uuid = NewGuid(),
                name = name,
                code = code,
                applicableRootEntity = nameof(IfcObject),
                Definitions = new[]{
                    new DefinitionsDefinition{
                        Body = new DefinitionsDefinitionBody{ lang= _language, Value = definition}
                    }
                },
                Applicability = new ConceptRootApplicability
                {
                    Template = new GenericReference { @ref = objectsAndTypesUuid },
                    TemplateRules = applicability
                },
                Concepts = requirements.ToArray()
            };

            var roots = new List<ConceptRoot>(ConceptRoots)
            {
                result
            };
            _view.Roots = roots.ToArray();
            return result;
        }

        /// <summary>
        /// Creates new concept defining validation rules for certain requirements
        /// </summary>
        /// <param name="name">Name of the concept</param>
        /// <param name="definition">Definition</param>
        /// <param name="rules">Validation rules</param>
        /// <param name="requirements">Applicable requirements</param>
        /// <returns></returns>
        public Concept CreateConcept(string name, string definition, TemplateRules rules, IEnumerable<ModelViewExchangeRequirement> requirements)
        {
            return new Concept
            {
                uuid = NewGuid(),
                name = name,
                Definitions = new[] {
                    new DefinitionsDefinition {
                        Body = new DefinitionsDefinitionBody { lang = _language, Value = definition }
                    }
                },
                Template = new GenericReference { @ref = objectsAndTypesUuid },
                Requirements = requirements.Select(r =>
                    new RequirementsRequirement
                    {
                        applicability = applicability.both,
                        requirement = RequirementsRequirementRequirement.mandatory,
                        exchangeRequirement = r.uuid
                    }).ToArray(),
                TemplateRules = rules
            };
        }

        /// <summary>
        /// Creates property value rule
        /// </summary>
        /// <param name="pSetName">Property set name</param>
        /// <param name="pName">Property name</param>
        /// <param name="pValue">Value</param>
        /// <returns></returns>
        public TemplateRules CreatePropertyValueRule(string pSetName, string pName, string pValue)
        {
            return new TemplateRules
            {
                @operator = TemplateRulesOperator.or,
                Items = new object[]{
                    new TemplateRulesTemplateRule {
                        Parameters = $"O_PsetName[Value]='{pSetName}' AND O_PName[Value]='{pName}' AND O_PSingleValue[Value]={pValue}"
                    },
                    new TemplateRules {
                        @operator = TemplateRulesOperator.and,
                        Items = new object[]{
                            new TemplateRulesTemplateRule{
                                Parameters = $"T_PsetName[Value]='{pSetName}' AND T_PName[Value]='{pName}' AND T_PSingleValue[Value]={pValue}"
                            },
                            new TemplateRules{
                                @operator = TemplateRulesOperator.not,
                                Items = new []{
                                    new TemplateRulesTemplateRule{
                                        Parameters = $"O_PsetName[Value]='{pSetName}' AND O_PName[Value]='{pName}'"
                                    }
                                }
                            }
                        }
                    }

                }
            };
        }

        /// <summary>
        /// Creates property value template
        /// </summary>
        /// <param name="pName">Property name</param>
        /// <param name="pValue">Property value</param>
        /// <returns></returns>
        public TemplateRules CreatePropertyValueRule(string pName, string pValue)
        {
            return new TemplateRules
            {
                @operator = TemplateRulesOperator.or,
                Items = new object[]{
                    new TemplateRulesTemplateRule {
                        Parameters = $"O_PName[Value]='{pName}' AND O_PSingleValue[Value]={pValue}"
                    },
                    new TemplateRules {
                        @operator = TemplateRulesOperator.and,
                        Items = new object[]{
                            new TemplateRulesTemplateRule{
                                Parameters = $"T_PName[Value]='{pName}' AND T_PSingleValue[Value]={pValue}"
                            },
                            new TemplateRules{
                                @operator = TemplateRulesOperator.not,
                                Items = new []{
                                    new TemplateRulesTemplateRule{
                                        Parameters = $"O_PName[Value]='{pName}'"
                                    }
                                }
                            }
                        }
                    }

                }
            };
        }

        /// <summary>
        /// Creates property existance rule
        /// </summary>
        /// <param name="pSetName"></param>
        /// <param name="pName"></param>
        /// <returns></returns>
        public TemplateRules CreatePropertyExistanceRule(string pSetName, string pName)
        {
            return new TemplateRules
            {
                @operator = TemplateRulesOperator.or,
                Items = new[]{
                    new TemplateRulesTemplateRule {
                        Parameters = $"O_PsetName[Value]='{pSetName}' AND O_PName[Value]='{pName}' AND O_PSingleValue[Exists]=TRUE"
                    },
                    new TemplateRulesTemplateRule {
                        Parameters = $"T_PsetName[Value]='{pSetName}' AND T_PName[Value]='{pName}' AND T_PSingleValue[Exists]=TRUE"
                    }
                }
            };
        }

        public TemplateRules CreateClassificationRule(string classification)
        {
            return new TemplateRules
            {
                @operator = TemplateRulesOperator.or,
                Items = new[]{
                    new TemplateRulesTemplateRule {
                        Parameters = $"O_CRefName[Value]='{classification}' OR O_CRefId[Value]='{classification}'"
                    },
                    new TemplateRulesTemplateRule {
                        Parameters = $"T_CRefName[Value]='{classification}' OR T_CRefId[Value]='{classification}'"
                    }
                }
            };
        }

        /// <summary>
        /// Creates logical group of predicates
        /// </summary>
        /// <param name="operator">Operator for the group</param>
        /// <param name="rules">Rules in the group</param>
        /// <returns></returns>
        public TemplateRules CreateLogicalRule(TemplateRulesOperator @operator, params TemplateRules[] rules)
        {
            return new TemplateRules
            {
                @operator = @operator,
                Items = rules
            };
        }

        private static string NewGuid() => Guid.NewGuid().ToString().ToLowerInvariant();

        /// <summary>
        /// Initializes the MVD with base concept cemplates allowing to create rules for 
        /// property sets, properties and classification references 
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="code">Code</param>
        /// <param name="definition">Definition</param>
        /// <returns></returns>
        private mvdXML InitMvd(string name, string code, string definition)
        {
            return new mvdXML
            {
                uuid = NewGuid(),
                name = name,
                Templates = new[] {
                    new ConceptTemplate {
                        uuid = objectsAndTypesUuid,
                        name= "Property Sets and Classification References for Objects and Types",
                        applicableSchema = schemas,
                        applicableEntity = new[] { nameof(IfcObject) },
                        Rules = new[] {
                            new AttributeRule {
                                AttributeName = nameof(IfcObject.IsDefinedBy),
                                EntityRules = new AttributeRuleEntityRules{
                                    EntityRule = new []{
                                        new EntityRule{
                                            EntityName = nameof(IfcRelDefinesByProperties),
                                            AttributeRules = new EntityRuleAttributeRules{
                                                AttributeRule = new []{
                                                    new AttributeRule {
                                                        AttributeName = nameof(IfcRelDefinesByProperties.RelatingPropertyDefinition),
                                                        EntityRules = new AttributeRuleEntityRules{
                                                            EntityRule = new []{
                                                                new EntityRule {
                                                                    EntityName = nameof(IfcPropertySet),
                                                                    References = new EntityRuleReferences{
                                                                        IdPrefix = "O_",
                                                                        Template = new GenericReference{ @ref = psetsUuid}
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            },
                            new AttributeRule {
                                AttributeName = nameof(IfcObject.IsTypedBy),
                                EntityRules = new AttributeRuleEntityRules{
                                    EntityRule = new []{
                                        new EntityRule {
                                            EntityName = nameof(IfcRelDefinesByType),
                                            AttributeRules = new EntityRuleAttributeRules{
                                                AttributeRule = new []{
                                                    new AttributeRule{
                                                        AttributeName = nameof(IfcRelDefinesByType.RelatingType),
                                                        EntityRules = new AttributeRuleEntityRules {
                                                            EntityRule = new []{
                                                                new EntityRule{
                                                                    EntityName = nameof(IfcTypeObject),
                                                                    AttributeRules = new EntityRuleAttributeRules {
                                                                        AttributeRule = new []{
                                                                            new AttributeRule {
                                                                                AttributeName = nameof(IfcTypeObject.HasPropertySets),
                                                                                EntityRules = new AttributeRuleEntityRules{
                                                                                    EntityRule = new []{
                                                                                        new EntityRule{
                                                                                            EntityName = nameof(IfcPropertySet),
                                                                                            References = new EntityRuleReferences {
                                                                                                IdPrefix = "T_",
                                                                                                Template = new GenericReference{ @ref = psetsUuid}
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            },
                                                                            new AttributeRule {
                                                                                AttributeName = nameof(IfcTypeObject.HasAssociations),
                                                                                EntityRules = new AttributeRuleEntityRules {
                                                                                    EntityRule = new []{
                                                                                        new EntityRule{
                                                                                            EntityName = nameof(IfcRelAssociatesClassification),
                                                                                            References = new EntityRuleReferences {
                                                                                                IdPrefix = "T_",
                                                                                                Template = new GenericReference { @ref = classificationUuid }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            },
                            new AttributeRule {
                                AttributeName = nameof(IfcObject.HasAssociations),
                                EntityRules = new AttributeRuleEntityRules {
                                    EntityRule = new []{
                                        new EntityRule{
                                            EntityName = nameof(IfcRelAssociatesClassification),
                                            References = new EntityRuleReferences {
                                                IdPrefix = "O_",
                                                Template = new GenericReference { @ref = classificationUuid }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new ConceptTemplate {
                        uuid = classificationUuid,
                        name = "Classification reference",
                        applicableSchema = new []{ IFC4 },
                        applicableEntity = new []{ nameof(IfcRelAssociatesClassification) },
                        isPartial = true,
                        Rules = new []{
                            new AttributeRule{
                                AttributeName = nameof(IfcRelAssociatesClassification.RelatingClassification),
                                EntityRules = new AttributeRuleEntityRules{
                                    EntityRule = new []{
                                        new EntityRule{
                                            EntityName = nameof(IfcClassificationReference),
                                            AttributeRules = new EntityRuleAttributeRules{
                                                AttributeRule = new []{
                                                    new AttributeRule{
                                                        RuleID = "CRefName",
                                                        AttributeName = nameof(IfcClassificationReference.Name),
                                                        EntityRules = new AttributeRuleEntityRules{
                                                            EntityRule = new []{
                                                                new EntityRule { EntityName = nameof(IfcLabel)}
                                                            }
                                                        }
                                                    },
                                                    new AttributeRule{
                                                        RuleID = "CRefId",
                                                        AttributeName = nameof(IfcClassificationReference.Identification),
                                                        EntityRules = new AttributeRuleEntityRules{
                                                            EntityRule = new []{
                                                                new EntityRule { EntityName = nameof(IfcIdentifier)}
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new ConceptTemplate {
                        uuid = singleValueUuid,
                        name = "Single value",
                        applicableSchema = schemas,
                        applicableEntity = new []{ nameof(IfcPropertySingleValue) },
                        isPartial = true,
                        Rules = new []{
                            new AttributeRule{
                                RuleID = "PName",
                                AttributeName = nameof(IfcPropertySingleValue.Name),
                                EntityRules = new AttributeRuleEntityRules{
                                    EntityRule = new []{
                                        new EntityRule{ EntityName = nameof(IfcIdentifier)}
                                    }
                                }
                            },
                            new AttributeRule{
                                RuleID = "PDescription",
                                AttributeName = nameof(IfcPropertySingleValue.Description),
                                EntityRules = new AttributeRuleEntityRules{
                                    EntityRule = new []{
                                        new EntityRule{ EntityName = nameof(IfcText)}
                                    }
                                }
                            },
                            new AttributeRule{
                                RuleID = "PSingleValue",
                                AttributeName = nameof(IfcPropertySingleValue.NominalValue),
                                EntityRules = new AttributeRuleEntityRules{
                                    EntityRule = new []{
                                        new EntityRule{ EntityName = nameof(IfcValue)}
                                    }
                                }
                            },
                        }
                    },
                    new ConceptTemplate{
                        uuid = psetsUuid,
                        name = "Property Sets",
                        applicableSchema = schemas,
                        applicableEntity = new []{nameof(IfcPropertySet) },
                        isPartial = true,
                        Rules = new []{
                            new AttributeRule {
                                RuleID = "PsetName",
                                AttributeName = nameof(IfcPropertySet.Name),
                                EntityRules = new AttributeRuleEntityRules{
                                    EntityRule = new []{
                                        new EntityRule { EntityName = nameof(IfcLabel)}
                                    }
                                }
                            },
                            new AttributeRule{
                                RuleID = "PsetDescription",
                                AttributeName = nameof(IfcPropertySet.Description),
                                EntityRules = new AttributeRuleEntityRules{
                                    EntityRule = new []{
                                        new EntityRule{ EntityName = nameof(IfcText)}
                                    }
                                }
                            },
                            new AttributeRule{
                                AttributeName = nameof(IfcPropertySet.HasProperties),
                                EntityRules = new AttributeRuleEntityRules{
                                    EntityRule = new[]{
                                        new EntityRule {
                                            EntityName = nameof(IfcPropertySingleValue),
                                            References = new EntityRuleReferences{
                                                Template = new GenericReference{ @ref = singleValueUuid}
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                Views = new[] {
                    _view = new ModelView{
                        applicableSchema = IFC4,
                        uuid = NewGuid(),
                        name = name,
                        code = code,
                        Definitions = new []{
                            new DefinitionsDefinition{
                                Body = new DefinitionsDefinitionBody{ lang = _language, Value = definition}
                            }
                        }
                    }
                }
            };
        }
    }
}
