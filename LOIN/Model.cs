using LOIN.Context;
using LOIN.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using Xbim.Common;
using Xbim.Common.Model;
using Xbim.Common.Step21;
using Xbim.Ifc;
using Xbim.Ifc4.ActorResource;
using Xbim.Ifc4.ExternalReferenceResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.MeasureResource;
using Xbim.Ifc4.ProcessExtension;
using Xbim.Ifc4.SharedMgmtElements;
using Xbim.MvdXml;

namespace LOIN
{
    public class Model: IDisposable
    {
        private IfcStore _model;
        /// <summary>
        /// The actual IFC model contains all the data. This model
        /// only provides a facade to present the data as LOIN
        /// </summary>
        public IModel Internal => _model;

        private readonly  List<Actor> _actors = new List<Actor>();
        public IEnumerable<Actor> Actors => _actors;

        private readonly List<Reason> _reasons = new List<Reason>();
        public IEnumerable<Reason> Reasons => _reasons;

        private readonly List<Milestone> _milestones = new List<Milestone>();
        public IEnumerable<Milestone> Milestones => _milestones;

        private readonly List<BreakedownItem> _breakdownStructure = new List<BreakedownItem>();
        public IEnumerable<BreakedownItem> BreakdownStructure => _breakdownStructure;

        private readonly List<RequirementsSet> _requirements = new List<RequirementsSet>();
        public IEnumerable<RequirementsSet> Requirements => _requirements;

        private Model(IfcStore model)
        {
            _model = model;

            if (_model.Instances.Count == 0)
            {
                return;
            }

            // use all means of caching to get the initial structures quickly
            using (var entities = model.BeginEntityCaching())
            using (var cache = model.BeginInverseCaching())
            {
                // breakdown items
                _breakdownStructure = BreakedownItem.GetBreakdownStructure(this).ToList();

                // milestones
                _milestones = Milestone.GetMilestones(this).ToList();

                // reasons
                _reasons = Reason.GetReasons(this).ToList();

                // actors
                _actors = Actor.GetActors(this).ToList();

                // all requirements in project libraries
                _requirements = RequirementsSet.GetRequirements(this).ToList();
            }
        }

        /// <summary>
        /// This function can be used to retrieve all requirements in certain context defined by
        /// actor(s), breakdown item(s), milestone(s) and/or reason(s). Any combination can be used.
        /// </summary>
        /// <param name="context">Context items</param>
        /// <returns></returns>
        public IEnumerable<RequirementsSet> GetRequirements(params IContextEntity[] context)
        {
            return Requirements.Where(r => context.All(c => c.IsContextFor(r)));
        }

        /// <summary>
        /// Opens IFC file and makes it accessible as LOIN
        /// </summary>
        /// <param name="file">Path to IFC file</param>
        /// <param name="credentials">Optional editor credentials. You whould pass these if you are going to modify the model</param>
        /// <returns></returns>
        public static Model Open(string file, XbimEditorCredentials credentials = null)
        {
            var model = IfcStore.Open(file, credentials);
            return new Model(model);
        }

        /// <summary>
        /// Creates new model
        /// </summary>
        /// <param name="credentials">Editor credentials will be used in IFC for owner history objects</param>
        /// <returns></returns>
        public static Model Create(XbimEditorCredentials credentials)
        {
            var model = IfcStore.Create(credentials, Xbim.Common.Step21.XbimSchemaVersion.Ifc4, Xbim.IO.XbimStoreType.InMemoryModel);
            return new Model(model);
        }

        /// <summary>
        /// Saves as .ifc or .ifcXML based on the extension passed in the path
        /// </summary>
        /// <param name="path">Path</param>
        public void Save(string path)
        {
            _model.SaveAs(path);
        }

        public Actor CreateActor(string name, string description)
        {
            var i = Internal.Instances;
            var actor = i.New<IfcActor>(a => {
                a.Name = name;
                a.Description = description;
                a.TheActor = i.New<IfcOrganization>(o => {
                    o.Name = name;
                    o.Description = description;
                });
            });

            var proxy = new Actor(actor, this, new List<IfcRelAssignsToActor>());
            _actors.Add(proxy);
            return proxy;
        }

        public BreakedownItem CreateBreakedownRoot(string name, string description)
        {
            var i = Internal.Instances;
            var clsref = i.New<IfcClassification>(a => {
                a.Name = name;
                a.Description = description;
            });

            var proxy = new BreakedownItem(clsref, this, new List<IfcRelAssociatesClassification>());
            _breakdownStructure.Add(proxy);

            return proxy;
        }

        public BreakedownItem CreateBreakedownItem(string name, string code, string description, BreakedownItem parent = null)
        {
            var i = Internal.Instances;
            var clsref = i.New<IfcClassificationReference>(a => {
                a.Name = name;
                a.Description = description;
                a.Identification = code;
            });

            var proxy = new BreakedownItem(clsref, this, new List<IfcRelAssociatesClassification>());
            _breakdownStructure.Add(proxy);

            if (parent != null && parent.Entity is IfcClassificationReferenceSelect select)
            {
                clsref.ReferencedSource = select;
                proxy.Parent = parent;
                parent.AddChild(proxy);
            }

            return proxy;
        }

        public BreakedownItem CreateClassificationRoot(string name, string description)
        {
            var i = Internal.Instances;
            var clsref = i.New<IfcClassification>(a => {
                a.Name = name;
                a.Description = description;
            });

            var proxy = new BreakedownItem(clsref, this, new List<IfcRelAssociatesClassification>());
            _breakdownStructure.Add(proxy);

            return proxy;
        }

        public Milestone CreateMilestone(string name, string description)
        {
            var i = Internal.Instances;
            var milestone = i.New<IfcTask>(a => {
                a.Name = name;
                a.Description = description;
                a.IsMilestone = true;
            });

            var proxy = new Milestone(milestone, this, new List<IfcRelAssignsToProcess>());
            _milestones.Add(proxy);

            return proxy;
        }

        public Reason CreateReason(string name, string description)
        {
            var i = Internal.Instances;
            var reason = i.New<IfcActionRequest>(a => {
                a.Name = name;
                a.Description = description;
            });

            var proxy = new Reason(reason, this, new List<IfcRelAssignsToControl>());
            _reasons.Add(proxy);

            return proxy;
        }

        public RequirementsSet CreateRequirementSet(string name, string description)
        {
            var i = Internal.Instances;
            var requirements = i.New<IfcProjectLibrary>(a => {
                a.Name = name;
                a.Description = description;
            });

            var proxy = new RequirementsSet(requirements, this, new List<IfcRelDeclares>());
            _requirements.Add(proxy);

            return proxy;
        }

        public IfcPropertySetTemplate CreatePropertySetTemplate(string name, string description)
        {
            return New<IfcPropertySetTemplate>(p => {
                p.Name = name;
                p.Description = description;
            });
        }

        public IfcSimplePropertyTemplate CreateSimplePropertyTemplate(string name, string description, string measureType = null,  IfcUnit unit = null)
        {
            return New<IfcSimplePropertyTemplate>(p => {
                p.Name = name;
                p.Description = description;
                p.PrimaryMeasureType = measureType;
                p.PrimaryUnit = unit;
            });
        }

        public T New<T>(Action<T> init = null) where T : IInstantiableEntity
        {
            return _model.Instances.New<T>(init);
        }

        public mvdXML GetMvd(XbimSchemaVersion schema, string languageCode, string name, string definition, string code, string classificationProperty,
            Func<IContextEntity, bool> contextFilter = null,
            Func<IfcPropertySetTemplate, bool> requirementSetFilter = null,
            Func<IfcPropertyTemplate, bool> requirementsFilter = null)
        {
            var converter = new Mvd.Converter(schema, languageCode, contextFilter, requirementSetFilter, requirementsFilter);
            return converter.Convert(this, name, definition, code, classificationProperty);
        }

        public void Dispose()
        {
            Internal.Dispose();
        }
    }
}
