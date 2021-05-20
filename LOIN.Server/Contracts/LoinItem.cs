using LOIN.Context;
using Xbim.Ifc4.Interfaces;

namespace LOIN.Server.Contracts
{
    public class LoinItem
    {
        private const string lang = "cs";

        public int Id { get; set; }
        public string UUID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// IFC Identifier
        /// </summary>
        public string Identifier { get; set; }

        protected LoinItem()
        {

        }

        protected LoinItem(IIfcRoot root)
        {
            Id = root.EntityLabel;
            Identifier = Name = root.Name;
            Description = root.Description;
            UUID = root.GlobalId;

            if (root is Xbim.Ifc4.Kernel.IfcDefinitionSelect def)
            {
                Name = def.GetName(lang) ?? root.Name;
                Description = def.GetDescription(lang) ?? root.Description;
            }
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
