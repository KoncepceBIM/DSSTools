using System;
using System.Collections.Generic;
using System.Text;
using Xbim.Common;
using Xbim.Ifc4.Interfaces;

namespace LOIN
{
    public abstract class RelatedLoinEntity<TRel, TEnt>: AbstractLoinEntity<TEnt> where TEnt: IPersistEntity where TRel : IIfcRelationship
    {
        protected readonly TRel Relationship;

        public RelatedLoinEntity(TRel relationship)
        {
            Relationship = relationship;
        }

        protected abstract Func<TRel, TEnt> Accessor { get; }

        public override TEnt Entity => Accessor(Relationship);
    }
}
