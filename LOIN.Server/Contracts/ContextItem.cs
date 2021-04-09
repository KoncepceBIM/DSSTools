using LOIN.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOIN.Server.Contracts
{
    public class ContextItem
    {
        public int Id { get; set; }
        public string UUID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public ContextItem()
        {

        }

        public ContextItem(IContextEntity entity): this()
        {
            Id = entity.Entity.EntityLabel;
            UUID = entity.Id;
            Name = entity.Name;
            Description = entity.Description;
        }
    }
}
