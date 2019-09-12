using LOIN.Context;
using System;
using System.Linq;
using Xbim.Common;
using Xbim.Common.Model;
using Xbim.Ifc4.Interfaces;

namespace LOIN
{
    public class Model: IDisposable
    {
        public IModel IfcModel { get; }

        private Model(IModel model)
        {
            IfcModel = model;

            // use all means of caching to get the initial structures quickly
            using (model.BeginEntityCaching())
            using (model.BeginInverseCaching())
            {
                // central objects are project libraries
                var loins = IfcModel.Instances.OfType<IIfcProjectLibrary>().ToList();

                // breakdown items
                var items = BreakedownItem.CreateBreakdownStructure(this);

                // milestones

                // reasons

                // actors
            }
        }



        /// <summary>
        /// Opens IFC file and makes it accessible as LOIN
        /// </summary>
        /// <param name="file">Path to IFC file</param>
        /// <returns></returns>
        public static Model Open(string file)
        {
            var model = new StepModel(new Xbim.Ifc4.EntityFactoryIfc4());
            model.LoadStep21(file);
            return new Model(model);
        }

        public void Dispose()
        {
            IfcModel.Dispose();
        }
    }
}
