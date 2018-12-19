namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{
    public interface IQueryFilter
    {
        string Name { get; }
	    void Enable(object session, object payload);
    }
}
