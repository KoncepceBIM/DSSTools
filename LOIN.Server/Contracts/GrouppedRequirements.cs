﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOIN.Server.Contracts
{
    public class GrouppedRequirements: LoinItem
    {
        public string Code { get; set; }
        public List<int> Path { get; set; }
        public IEnumerable<Requirement> Requirements { get; }

        public GrouppedRequirements(Context.BreakdownItem item, IEnumerable<Requirement> requirements): base(item)
        {
            Code = item.Code;

            var path = new List<int> { item.Entity.EntityLabel };
            path.AddRange(item.Parents.Select(p => p.Entity.EntityLabel));
            path.Reverse();
            Path = path;
            Requirements = requirements;
        }
    }
}
