namespace Teleopti.Interfaces.Domain
{
	public interface IWriteSideRepositoryTypedId<T, TId> : ILoadAggregateByTypedId<T, TId>
	{
		void Add(T entity);
		void Remove(T entity);
	}
}