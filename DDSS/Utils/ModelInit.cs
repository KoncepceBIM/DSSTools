using System;
using System.Collections.Generic;
using System.Text;
using Xbim.Common;
using Xbim.Ifc;
using Xbim.Ifc4.ActorResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.UtilityResource;
using Xbim.IO.Step21;

namespace DDSS.Utils
{
    internal static class ModelInit
    {
        // sets up hooks for new and changed entities
        public static void Init(this IModel model, XbimEditorCredentials editor)
        {
            var i = model.Instances;
            var usr = i.New<IfcPersonAndOrganization>(po =>
            {
                po.TheOrganization = i.New<IfcOrganization>(o => o.Name = editor.EditorsOrganisationName);
                po.ThePerson = i.New<IfcPerson>(p =>
                {
                    p.GivenName = editor.EditorsGivenName;
                    p.FamilyName = editor.EditorsFamilyName;
                });
            });


            var app = i.New<IfcApplication>(a =>
            {
                a.ApplicationDeveloper = i.New<IfcOrganization>(o => o.Name = editor.ApplicationDevelopersName);
                a.ApplicationFullName = editor.ApplicationFullName;
                a.ApplicationIdentifier = editor.ApplicationIdentifier;
                a.Version = editor.ApplicationVersion;
            });

            var histAdd = model.Instances.New<IfcOwnerHistory>(h => {
                h.OwningUser = usr;
                h.OwningApplication = app;
                h.LastModifiedDate = DateTime.Now;
                h.ChangeAction = IfcChangeActionEnum.ADDED;
            });

            var histMod = model.Instances.New<IfcOwnerHistory>(h => {
                h.OwningUser = usr;
                h.OwningApplication = app;
                h.LastModifiedDate = DateTime.Now;
                h.ChangeAction = IfcChangeActionEnum.MODIFIED;
            });



            model.EntityNew += (entity) =>
            {
                if (entity is IIfcRoot root)
                {
                    root.OwnerHistory = histAdd;
                    root.GlobalId = Guid.NewGuid().ToPart21();
                    histAdd.LastModifiedDate = DateTime.Now;
                }
            };

            model.EntityModified += (entity, property) =>
            {
                if (!(entity is IIfcRoot root) || root.OwnerHistory == histAdd)
                    return;

                if (root.OwnerHistory != histMod)
                {
                    root.OwnerHistory = histMod;
                    histMod.LastModifiedDate = DateTime.Now;
                }
            };
        }


    }
}
