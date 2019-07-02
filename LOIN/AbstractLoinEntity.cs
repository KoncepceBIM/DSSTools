using System;
using System.Collections.Generic;
using System.Text;
using Xbim.Common;
using Xbim.Ifc4.Interfaces;

namespace LOIN
{
    public abstract class AbstractLoinEntity
    {
        public abstract IModel Model { get; }
    }

    public abstract class AbstractLoinEntity<T>: AbstractLoinEntity where T: IPersistEntity
    {
        public abstract T Entity { get; }

        public override IModel Model => Entity.Model;
    }
}
