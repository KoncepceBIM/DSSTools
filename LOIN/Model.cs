using System;
using System.Linq;
using Xbim.Common;
using Xbim.Ifc4.Interfaces;

namespace LOIN
{
    public class Model
    {
        private readonly IModel _model;

        public Model(IModel model)
        {
            _model = model;

            // use all means of caching to get the initial structures quickly
            using (model.BeginEntityCaching())
            using (model.BeginInverseCaching())
            {
                // central objects are project libraries
                var loins = _model.Instances.OfType<IIfcProjectLibrary>().ToList();

                // breakdown items
                var items = loins
                    .SelectMany(l => l.HasAssociations.OfType<IIfcRelAssociatesClassification>())
                    .Select(r => new BreakedownItem(r));

            }

            
        }

        
    }
}
