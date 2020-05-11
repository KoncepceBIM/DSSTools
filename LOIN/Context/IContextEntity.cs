namespace LOIN.Context
{
    public interface IContextEntity
    {
        bool IsContextFor(Requirements.RequirementsSet requirements);
        bool RemoveFromContext(Requirements.RequirementsSet requirements);
        bool AddToContext(Requirements.RequirementsSet requirements);

        string Name { get; }
        string Description { get; }

        string GetName(string lang);
        string GetDescription(string lang);
    }
}
