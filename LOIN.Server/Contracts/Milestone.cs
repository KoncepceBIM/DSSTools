﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOIN.Server.Contracts
{
    public class Milestone: LoinItem
    {
        public Milestone(Context.Milestone milestone): base(milestone)
        {

        }
    }
}
