using LOIN.Context;
using LOIN.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using Xbim.Ifc4.ActorResource;
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
        private static readonly string referenceValueUuid = NewGuid();
        private static readonly string propertyUuid = NewGuid();
        private static readonly string classificationUuid = NewGuid();

        private const string IFC4 = "IFC4";
        private static readonly string[] schemas = new[] { IFC4 };

        public Func<IfcPropertySetTemplate, bool> RequirementsFilter { get; set; }

        public Func<IContextEntity, bool> ContextFilter { get; set; }

        public Converter(string primaryLanguage, 
            Func<IContextEntity, bool> contextFilter = null, 
            Func<IfcPropertySetTemplate, bool> requirementsFilter = null)
        {
            _language = primaryLanguage;
            ContextFilter = contextFilter ?? ((IContextEntity a) => true);
            RequirementsFilter = requirementsFilter ?? ((IfcPropertySetTemplate t) => true);
        }

        public mvdXML Convert(Model model, string name, string definition, string code)
        {
            _mvd = InitMvd(name, code, definition);
            _requirementsCache = new Dictionary<string, ModelViewExchangeRequirement>();

            // iterate over the model to convert all requirements
            foreach (var item in model.BreakdownStructure.Where<BreakedownItem>(ContextFilter))
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
                var rules = CreateValidationRules(requirementSet.Requirements.Where(RequirementsFilter));
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
            var actors = set.Actors.Where(ContextFilter).Select(a => a.Name ?? "").ToList();
            if (!actors.Any()) actors.Add("N/A");

            var milestones = set.Milestones.Where(ContextFilter).Select(a => a.Name ?? "").ToList();
            if (!milestones.Any()) milestones.Add("N/A");

            var reasons = set.Reasons.Where(ContextFilter).Select(a => a.Name ?? "").ToList();
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
            var classificationCodePropertyRef = CreatePropertyClassificationReferenceRule("Classification", item.Code);
            var classificationNamePropertyRef = CreatePropertyClassificationReferenceRule("Classification", item.Name);
            return CreateLogicalRule(TemplateRulesOperator.or,
                classificationCode,
                classificationName,
                classificationCodeProperty,
                classificationNameProperty,
                classificationCodePropertyRef,
                classificationNamePropertyRef
                );
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
        /// Rule for property reference value with document. Searches for the property in any property set.
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="classification">Name of the document</param>
        /// <returns>Template rule</returns>
        public TemplateRules CreatePropertyDocumentReferenceRule(string propertyName, string documentName)
        {
            return new TemplateRules
            {
                @operator = TemplateRulesOperator.or,
                Items = new object[]{
                    new TemplateRulesTemplateRule {
                        Parameters = $"O_PName[Value]='{propertyName}' AND O_PRefDocName[Value]={documentName}"
                    },
                    new TemplateRules {
                        @operator = TemplateRulesOperator.and,
                        Items = new object[]{
                            new TemplateRulesTemplateRule{
                                Parameters = $"T_PName[Value]='{propertyName}' AND T_PRefDocName[Value]={documentName}"
                            },
                            new TemplateRules{
                                @operator = TemplateRulesOperator.not,
                                Items = new []{
                                    new TemplateRulesTemplateRule{
                                        Parameters = $"O_PName[Value]='{propertyName}'"
                                    }
                                }
                            }
                        }
                    }

                }
            };
        }

        /// <summary>
        /// Rule for property reference value with document.
        /// </summary>
        /// <param name="propertySetName">Property set name</param>
        /// <param name="propertyName">Property name</param>
        /// <param name="documentName">Required document name</param>
        /// <returns></returns>
        public TemplateRules CreatePropertyDocumentReferenceRule(string propertySetName, string propertyName, string documentName)
        {
            return new TemplateRules
            {
                @operator = TemplateRulesOperator.or,
                Items = new object[]{
                    new TemplateRulesTemplateRule {
                        Parameters = $"O_PsetName[Value]='{propertySetName}' AND O_PName[Value]='{propertyName}' AND O_PRefDocName[Value]={documentName}"
                    },
                    new TemplateRules {
                        @operator = TemplateRulesOperator.and,
                        Items = new object[]{
                            new TemplateRulesTemplateRule{
                                Parameters = $"T_PsetName[Value]='{propertySetName}' AND T_PName[Value]='{propertyName}' AND T_PRefDocName[Value]={documentName}"
                            },
                            new TemplateRules{
                                @operator = TemplateRulesOperator.not,
                                Items = new []{
                                    new TemplateRulesTemplateRule{
                                        Parameters = $"O_PsetName[Value]='{propertySetName}' AND O_PName[Value]='{propertyName}'"
                                    }
                                }
                            }
                        }
                    }

                }
            };
        }

        /// <summary>
        /// Creates property reference document existance rule
        /// </summary>
        /// <param name="pSetName">Property set name</param>
        /// <param name="pName">Property name</param>
        /// <returns></returns>
        public TemplateRules CreatePropertyDocumentExistanceRule(string pSetName, string pName)
        {
            return new TemplateRules
            {
                @operator = TemplateRulesOperator.or,
                Items = new[]{
                    new TemplateRulesTemplateRule {
                        Parameters = $"O_PsetName[Value]='{pSetName}' AND O_PName[Value]='{pName}' AND O_PRefDocName[Exists]=TRUE"
                    },
                    new TemplateRulesTemplateRule {
                        Parameters = $"T_PsetName[Value]='{pSetName}' AND T_PName[Value]='{pName}' AND T_PRefDocName[Exists]=TRUE"
                    }
                }
            };
        }

        /// <summary>
        /// Creates property reference document existance rule. Search accross all property sets
        /// </summary>
        /// <param name="pName">Property name</param>
        /// <returns></returns>
        public TemplateRules CreatePropertyDocumentExistanceRule(string pName)
        {
            return new TemplateRules
            {
                @operator = TemplateRulesOperator.or,
                Items = new[]{
                    new TemplateRulesTemplateRule {
                        Parameters = $"O_PName[Value]='{pName}' AND O_PRefDocName[Exists]=TRUE"
                    },
                    new TemplateRulesTemplateRule {
                        Parameters = $"T_PName[Value]='{pName}' AND T_PRefDocName[Exists]=TRUE"
                    }
                }
            };
        }

        /// <summary>
        /// Rule for property reference value with classification. Searches for the property in any property set.
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="classification">Name of the classification or identification</param>
        /// <returns>Template rule</returns>
        public TemplateRules CreatePropertyClassificationReferenceRule(string propertyName, string classification)
        {
            return new TemplateRules
            {
                @operator = TemplateRulesOperator.or,
                Items = new[] {
                    new TemplateRules
                    {
                        @operator = TemplateRulesOperator.or,
                        Items = new object[]{
                            new TemplateRulesTemplateRule {
                                Parameters = $"O_PName[Value]='{propertyName}' AND O_PRefClassificationName[Value]={classification}"
                            },
                            new TemplateRules {
                                @operator = TemplateRulesOperator.and,
                                Items = new object[]{
                                    new TemplateRulesTemplateRule{
                                        Parameters = $"T_PName[Value]='{propertyName}' AND T_PRefClassificationName[Value]={classification}"
                                    },
                                    new TemplateRules{
                                        @operator = TemplateRulesOperator.not,
                                        Items = new []{
                                            new TemplateRulesTemplateRule{
                                                Parameters = $"O_PName[Value]='{propertyName}'"
                                            }
                                        }
                                    }
                                }
                            }

                        }
                    },
                    new TemplateRules
                    {
                        @operator = TemplateRulesOperator.or,
                        Items = new object[]{
                            new TemplateRulesTemplateRule {
                                Parameters = $"O_PName[Value]='{propertyName}' AND O_PRefClassificationIdentifier[Value]={classification}"
                            },
                            new TemplateRules {
                                @operator = TemplateRulesOperator.and,
                                Items = new object[]{
                                    new TemplateRulesTemplateRule{
                                        Parameters = $"T_PName[Value]='{propertyName}' AND T_PRefClassificationIdentifier[Value]={classification}"
                                    },
                                    new TemplateRules{
                                        @operator = TemplateRulesOperator.not,
                                        Items = new []{
                                            new TemplateRulesTemplateRule{
                                                Parameters = $"O_PName[Value]='{propertyName}'"
                                            }
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            };
        }

        /// <summary>
        /// Rule for property reference value with classification.
        /// </summary>
        /// <param name="propertySetName">Property set name</param>
        /// <param name="propertyName">Property name</param>
        /// <param name="classification">Required classification name or identifier</param>
        /// <returns></returns>
        public TemplateRules CreatePropertyClassificationReferenceRule(string propertySetName, string propertyName, string classification)
        {
            return new TemplateRules
            {
                @operator = TemplateRulesOperator.or,
                Items = new[] {
                    new TemplateRules
                    {
                        @operator = TemplateRulesOperator.or,
                        Items = new object[]{
                            new TemplateRulesTemplateRule {
                                Parameters = $"O_PsetName[Value]='{propertySetName}' AND O_PName[Value]='{propertyName}' AND O_PRefClassificationName[Value]={classification}"
                            },
                            new TemplateRules {
                                @operator = TemplateRulesOperator.and,
                                Items = new object[]{
                                    new TemplateRulesTemplateRule{
                                        Parameters = $"T_PsetName[Value]='{propertySetName}' AND T_PName[Value]='{propertyName}' AND T_PRefClassificationName[Value]={classification}"
                                    },
                                    new TemplateRules{
                                        @operator = TemplateRulesOperator.not,
                                        Items = new []{
                                            new TemplateRulesTemplateRule{
                                                Parameters = $"O_PsetName[Value]='{propertySetName}' AND O_PName[Value]='{propertyName}'"
                                            }
                                        }
                                    }
                                }
                            }

                        }
                    },
                    new TemplateRules
                    {
                        @operator = TemplateRulesOperator.or,
                        Items = new object[]{
                            new TemplateRulesTemplateRule {
                                Parameters = $"O_PsetName[Value]='{propertySetName}' AND O_PName[Value]='{propertyName}' AND O_PRefClassificationIdentifier[Value]={classification}"
                            },
                            new TemplateRules {
                                @operator = TemplateRulesOperator.and,
                                Items = new object[]{
                                    new TemplateRulesTemplateRule{
                                        Parameters = $"T_PsetName[Value]='{propertySetName}' AND T_PName[Value]='{propertyName}' AND T_PRefClassificationIdentifier[Value]={classification}"
                                    },
                                    new TemplateRules{
                                        @operator = TemplateRulesOperator.not,
                                        Items = new []{
                                            new TemplateRulesTemplateRule{
                                                Parameters = $"O_PsetName[Value]='{propertySetName}' AND O_PName[Value]='{propertyName}'"
                                            }
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            };
        }

        /// <summary>
        /// Creates property classification existance rule
        /// </summary>
        /// <param name="pSetName">Property set name</param>
        /// <param name="pName">Property name</param>
        /// <returns></returns>
        public TemplateRules CreatePropertyClassificationExistanceRule(string pSetName, string pName)
        {
            return new TemplateRules
            {
                @operator = TemplateRulesOperator.or,
                Items = new[]{
                    new TemplateRulesTemplateRule {
                        Parameters = $"O_PsetName[Value]='{pSetName}' AND O_PName[Value]='{pName}' AND O_PRefClassificationIdentifier[Exists]=TRUE"
                    },
                    new TemplateRulesTemplateRule {
                        Parameters = $"T_PsetName[Value]='{pSetName}' AND T_PName[Value]='{pName}' AND T_PRefClassificationIdentifier[Exists]=TRUE"
                    },
                    new TemplateRulesTemplateRule {
                        Parameters = $"O_PsetName[Value]='{pSetName}' AND O_PName[Value]='{pName}' AND O_PRefClassificationName[Exists]=TRUE"
                    },
                    new TemplateRulesTemplateRule {
                        Parameters = $"T_PsetName[Value]='{pSetName}' AND T_PName[Value]='{pName}' AND T_PRefClassificationName[Exists]=TRUE"
                    }
                }
            };
        }

        /// <summary>
        /// Creates property reference classification existance rule. Search accross all property sets
        /// </summary>
        /// <param name="pName">Property name</param>
        /// <returns></returns>
        public TemplateRules CreatePropertyClassificationExistanceRule(string pName)
        {
            return new TemplateRules
            {
                @operator = TemplateRulesOperator.or,
                Items = new[]{
                    new TemplateRulesTemplateRule {
                        Parameters = $"O_PName[Value]='{pName}' AND O_PRefClassificationIdentifier[Exists]=TRUE"
                    },
                    new TemplateRulesTemplateRule {
                        Parameters = $"T_PName[Value]='{pName}' AND T_PRefClassificationIdentifier[Exists]=TRUE"
                    },
                    new TemplateRulesTemplateRule {
                        Parameters = $"O_PName[Value]='{pName}' AND O_PRefClassificationName[Exists]=TRUE"
                    },
                    new TemplateRulesTemplateRule {
                        Parameters = $"T_PName[Value]='{pName}' AND T_PRefClassificationName[Exists]=TRUE"
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
                    InitObjectConceptTemplate(),
                    InitClassificationReferenceConceptTemplate(),
                    InitSimpleValueConceptTemplate(),
                    InitReferenceValueConceptTemplate(),
                    InitPropertyConceptTemplate(),
                    InitPSetConceptTemplate()
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

        private ConceptTemplate InitPSetConceptTemplate()
        {
            return new ConceptTemplate
            {
                uuid = psetsUuid,
                name = "Property Sets",
                applicableSchema = schemas,
                applicableEntity = new[] { nameof(IfcPropertySet) },
                isPartial = true,
                Rules = new[]{
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
                                    EntityName = nameof(IfcProperty),
                                    References = new EntityRuleReferences{
                                        Template = new GenericReference{ @ref = propertyUuid}
                                    }
                                },
                                new EntityRule {
                                    EntityName = nameof(IfcPropertySingleValue),
                                    References = new EntityRuleReferences{
                                        Template = new GenericReference{ @ref = singleValueUuid}
                                    }
                                },
                                new EntityRule {
                                    EntityName = nameof(IfcPropertyReferenceValue),
                                    References = new EntityRuleReferences{
                                        Template = new GenericReference{ @ref = referenceValueUuid}
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        private ConceptTemplate InitPropertyConceptTemplate()
        {
            return new ConceptTemplate
            {
                uuid = propertyUuid,
                name = "Property",
                applicableSchema = schemas,
                applicableEntity = new[] { nameof(IfcProperty) },
                isPartial = true,
                Rules = new[]{
                    new AttributeRule{
                        RuleID = "PName",
                        AttributeName = nameof(IfcProperty.Name),
                        EntityRules = new AttributeRuleEntityRules{
                            EntityRule = new []{
                                new EntityRule{ EntityName = nameof(IfcIdentifier)}
                            }
                        }
                    },
                    new AttributeRule{
                        RuleID = "PDescription",
                        AttributeName = nameof(IfcProperty.Description),
                        EntityRules = new AttributeRuleEntityRules{
                            EntityRule = new []{
                                new EntityRule{ EntityName = nameof(IfcText)}
                            }
                        }
                    }
                }
            };
        }

        private ConceptTemplate InitSimpleValueConceptTemplate()
        {
            return new ConceptTemplate
            {
                uuid = singleValueUuid,
                name = "Single value",
                applicableSchema = schemas,
                applicableEntity = new[] { nameof(IfcPropertySingleValue) },
                isPartial = true,
                Rules = new[]{
                    new AttributeRule{
                        RuleID = "PSingleValue",
                        AttributeName = nameof(IfcPropertySingleValue.NominalValue),
                        EntityRules = new AttributeRuleEntityRules{
                            EntityRule = new []{
                                new EntityRule{ EntityName = nameof(IfcValue)}
                            }
                        }
                    }
                }
            };
        }

        private ConceptTemplate InitReferenceValueConceptTemplate()
        {
            return new ConceptTemplate
            {
                uuid = referenceValueUuid,
                name = "Reference value",
                applicableSchema = schemas,
                applicableEntity = new[] { nameof(IfcPropertyReferenceValue) },
                isPartial = true,
                Rules = new[]{
                    new AttributeRule{
                        AttributeName = nameof(IfcPropertyReferenceValue.PropertyReference),
                        EntityRules = new AttributeRuleEntityRules{
                            EntityRule = new []{
                                new EntityRule{ 
                                    EntityName = nameof(IfcDocumentReference),
                                    AttributeRules = new EntityRuleAttributeRules {
                                        AttributeRule = new [] {
                                            new AttributeRule { 
                                                RuleID = "PRefDocName",
                                                AttributeName = nameof(IfcDocumentReference.Name),
                                                EntityRules = new AttributeRuleEntityRules {
                                                    EntityRule = new []{ 
                                                        new EntityRule { EntityName = nameof(IfcLabel) }
                                                    }
                                                }
                                            },
                                            new AttributeRule {
                                                RuleID = "PRefDocDescription",
                                                AttributeName = nameof(IfcDocumentReference.Description),
                                                EntityRules = new AttributeRuleEntityRules {
                                                    EntityRule = new []{
                                                        new EntityRule { EntityName = nameof(IfcText) }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                },
                                new EntityRule{
                                    EntityName = nameof(IfcClassificationReference),
                                    AttributeRules = new EntityRuleAttributeRules {
                                        AttributeRule = new [] {
                                            new AttributeRule {
                                                RuleID = "PRefClassificationName",
                                                AttributeName = nameof(IfcClassificationReference.Name),
                                                EntityRules = new AttributeRuleEntityRules {
                                                    EntityRule = new []{
                                                        new EntityRule { EntityName = nameof(IfcLabel) }
                                                    }
                                                }
                                            },
                                            new AttributeRule {
                                                RuleID = "PRefClassificationDescription",
                                                AttributeName = nameof(IfcClassificationReference.Description),
                                                EntityRules = new AttributeRuleEntityRules {
                                                    EntityRule = new []{
                                                        new EntityRule { EntityName = nameof(IfcText) }
                                                    }
                                                }
                                            },
                                            new AttributeRule {
                                                RuleID = "PRefClassificationIdentifier",
                                                AttributeName = nameof(IfcClassificationReference.Identification),
                                                EntityRules = new AttributeRuleEntityRules {
                                                    EntityRule = new []{
                                                        new EntityRule { EntityName = nameof(IfcIdentifier) }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                },
                                new EntityRule{
                                    EntityName = nameof(IfcOrganization),
                                    AttributeRules = new EntityRuleAttributeRules {
                                        AttributeRule = new [] {
                                            new AttributeRule {
                                                RuleID = "PRefOrgName",
                                                AttributeName = nameof(IfcOrganization.Name),
                                                EntityRules = new AttributeRuleEntityRules {
                                                    EntityRule = new []{
                                                        new EntityRule { EntityName = nameof(IfcLabel) }
                                                    }
                                                }
                                            },
                                            new AttributeRule {
                                                RuleID = "PRefOrgDescription",
                                                AttributeName = nameof(IfcOrganization.Description),
                                                EntityRules = new AttributeRuleEntityRules {
                                                    EntityRule = new []{
                                                        new EntityRule { EntityName = nameof(IfcText) }
                                                    }
                                                }
                                            },
                                            new AttributeRule {
                                                RuleID = "PRefOrgIdentification",
                                                AttributeName = nameof(IfcOrganization.Identification),
                                                EntityRules = new AttributeRuleEntityRules {
                                                    EntityRule = new []{
                                                        new EntityRule { EntityName = nameof(IfcIdentifier) }
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
            };
        }

        private ConceptTemplate InitClassificationReferenceConceptTemplate()
        {
            return new ConceptTemplate
            {
                uuid = classificationUuid,
                name = "Classification reference",
                applicableSchema = new[] { IFC4 },
                applicableEntity = new[] { nameof(IfcRelAssociatesClassification) },
                isPartial = true,
                Rules = new[]{
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
            };
        }

        private ConceptTemplate InitObjectConceptTemplate()
        {
            return new ConceptTemplate
            {
                uuid = objectsAndTypesUuid,
                name = "Property Sets and Classification References for Objects and Types",
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
            };
        }
    }
}
