namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface ILoadAggregateByTypedId<T, TId>
	{
		T LoadAggregate(TId id);
	}
}