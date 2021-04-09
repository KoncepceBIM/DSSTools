using LOIN.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xbim.Ifc4.Interfaces;

namespace LOIN.Server.Contracts
{
    public class LoinItem
    {
        public int Id { get; set; }
        public string UUID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        protected LoinItem()
        {

        }

        protected LoinItem(IIfcRoot root)
        {
            Id = root.EntityLabel;
            Name = root.Name;
            Description = root.Description;
            UUID = root.GlobalId;
        }

        public LoinItem(IContextEntity entity): this()
        {
            Id = entity.Entity.EntityLabel;
            UUID = entity.Id;
            Name = entity.Name;
            Description = entity.Description;
        }
    }
}
