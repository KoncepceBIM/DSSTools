using System;
using System.Collections.Generic;
using System.Text;
using Xbim.Ifc4.Interfaces;

namespace LOIN.Context
{
    public class Reason : AbstractLoinEntity<IIfcActionRequest>
    {
        public static readonly Reason Any = new Reason(null, null);

        internal Reason(IIfcActionRequest request, Model model) : base(request, model)
        {
        }

        public string Name
        {
            get => Entity.Name;
            set => Entity.Name = value;
        }

        public string Description
        {
            get => Entity.Description;
            set => Entity.Description = value;
        }
    }
}
