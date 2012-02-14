namespace Teleopti.Ccc.Rta.Server
{
    public interface IDataSourceResolver
    {
        bool TryResolveId(string sourceId, out int dataSourceId);
    }
}