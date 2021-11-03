using LOIN.Context;
using Xbim.Ifc4.Interfaces;

namespace LOIN.Server.Contracts
{
    public class LoinItem
    {
        private const string cs = "cs";
        private const string en = "en";

        public int Id { get; set; }
        public string UUID { get; set; }
        
        public string Name { get; set; }
        public string NameCS { get; set; }
        public string NameEN { get; set; }
        
        public string Description { get; set; }
        public string DescriptionCS { get; set; }
        public string DescriptionEN { get; set; }

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
                NameCS = def.GetName(cs) ?? Name;
                NameEN = def.GetName(en) ?? Name;

                DescriptionCS = def.GetDescription(cs) ?? Description;
                DescriptionEN = def.GetDescription(en) ?? Description;
            }

            if (root is Xbim.Ifc4.ExternalReferenceResource.IfcExternalReference eref)
            {
                NameCS = eref.GetName(cs) ?? Name;
                NameEN = eref.GetName(en) ?? Name;

                DescriptionCS = eref.GetDescription(cs) ?? Description;
                DescriptionEN = eref.GetDescription(en) ?? Description;
            }
        }

        public LoinItem(IContextEntity entity): this()
        {
            Id = entity.Entity.EntityLabel;
            UUID = entity.Id;
            Name = entity.Name;
            Description = entity.Description;

            var ifcEntity = entity.Entity;
            if (ifcEntity is Xbim.Ifc4.Kernel.IfcDefinitionSelect def)
            {
                NameCS = def.GetName(cs) ?? Name;
                NameEN = def.GetName(en) ?? Name;

                DescriptionCS = def.GetDescription(cs) ?? Description;
                DescriptionEN = def.GetDescription(en) ?? Description;
            }
            else if (ifcEntity is Xbim.Ifc4.ExternalReferenceResource.IfcExternalReference eref)
            {
                NameCS = eref.GetName(cs) ?? Name;
                NameEN = eref.GetName(en) ?? Name;

                DescriptionCS = eref.GetDescription(cs) ?? Description;
                DescriptionEN = eref.GetDescription(en) ?? Description;
            }
            else
            {
                NameCS = Name;
                NameEN = Name;

                DescriptionCS = Description;
                DescriptionEN = Description;
            }
        }
    }
}
