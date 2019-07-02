using System;
using System.Collections.Generic;
using System.Text;
using Xbim.Common;
using Xbim.Ifc4.Interfaces;

namespace LOIN
{
    public abstract class ContextLoinEntity<TRel, TEnt> : RelatedLoinEntity<TRel, TEnt> where TEnt : IPersistEntity where TRel : IIfcRelationship
    {
        public ContextLoinEntity(TRel relationship) : base(relationship)
        {
        }

    }
}
