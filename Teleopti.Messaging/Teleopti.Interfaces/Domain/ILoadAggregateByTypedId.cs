namespace Teleopti.Interfaces.Domain
{
	public interface ILoadAggregateByTypedId<T, TId>
	{
		T LoadAggregate(TId id);
	}
}