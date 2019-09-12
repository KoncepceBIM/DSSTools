using System;
using System.Collections.Generic;
using System.Text;
using Xbim.Common;
using Xbim.Ifc4.Interfaces;

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
    }
}
