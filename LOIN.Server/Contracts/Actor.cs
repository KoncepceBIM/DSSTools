using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOIN.Server.Contracts
{
    public class Actor: LoinItem
    {
        public Actor(Context.Actor actor): base (actor)
        {

        }
    }
}
