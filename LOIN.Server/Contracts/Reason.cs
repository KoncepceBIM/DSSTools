﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOIN.Server.Contracts
{
    public class Reason : LoinItem
    {
        public Reason(Context.Reason reason): base(reason)
        {

        }
    }
}
