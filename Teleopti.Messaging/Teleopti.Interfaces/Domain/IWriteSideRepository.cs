using System;

namespace Teleopti.Interfaces.Domain
{
	public interface IPersonAssignmentWriteSideRepository : IWriteSideRepository<IPersonAssignment>
	{
		IPersonAssignment Load(Guid personId, DateOnly date);
	}

	/// <summary>
	/// Generic interface for write side repositories
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IWriteSideRepository<T> : ILoadAggregateById<T>
	{
		/// <summary>
		/// Adds the specified entity to repository.
		/// </summary>
		/// <param name="entity">The entity.</param>
		void Add(T entity);

		/// <summary>
		/// Removes the specified entity from repository.
		/// </summary>
		/// <param name="entity">The entity.</param>
		void Remove(T entity);

		/// <summary>
		/// Loads entity for the id.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <returns></returns>
		T Load(Guid id);
	}
}