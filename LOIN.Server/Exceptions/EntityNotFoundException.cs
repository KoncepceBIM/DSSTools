using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOIN.Server.Exceptions
{

    [Serializable]
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(int entityLabel)
        {
            EntityLabel = entityLabel;
        }

        public EntityNotFoundException(int entityLabel, string message) : base(message) 
        {
            EntityLabel = entityLabel;
        }

        public EntityNotFoundException(int entityLabel, string message, Exception inner) : base(message, inner) 
        {
            EntityLabel = entityLabel;
        }

        protected EntityNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public int EntityLabel { get; }
    }
}
