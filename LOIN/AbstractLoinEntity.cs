using System;
using System.Collections.Generic;
using System.Text;
using Xbim.Common;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;

namespace LOIN
{
    public abstract class AbstractLoinEntity
    {
        public Model Model { get; private set; }

        internal AbstractLoinEntity(Model model)
        {
            Model = model;
        }
    }

    public abstract class AbstractLoinEntity<T>: AbstractLoinEntity where T: IPersistEntity
    {
        public T Entity { get; }


        internal AbstractLoinEntity(T entity, Model model): base(model)
        {
            Entity = entity;
        }

        public string GetName(string lang)
        {
            if (Entity is IfcDefinitionSelect definition)
                return definition.GetName(lang);
            return null;
        }

        public string GetDescription(string lang)
        {
            if (Entity is IfcDefinitionSelect definition)
                return definition.GetDescription(lang);
            return null;
        }

        public void SetName(string lang, string name)
        {
            if (Entity is IfcDefinitionSelect definition)
                definition.SetName(lang, name);
        }

        public void SetDescription(string lang, string description)
        {
            if (Entity is IfcDefinitionSelect definition)
                definition.SetDescription(lang, description);
        }
    }
}
