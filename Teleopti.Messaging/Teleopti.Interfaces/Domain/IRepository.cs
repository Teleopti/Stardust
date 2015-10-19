using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
    /// Base interface for all repositories
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRepository<T>
    {

		/// <summary>
		/// Adds the specified entity to repository.
		/// </summary>
		void Add(T root);


		/// <summary>
		/// Removes the specified entity from repository.
		/// </summary>
		void Remove(T root);

		/// <summary>
		/// Gets entity for the id.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <returns></returns>
		T Get(Guid id);

        /// <summary>
        /// Loads all entities.
        /// </summary>
        /// <returns></returns>
        IList<T> LoadAll();

        /// <summary>
        /// Loads an entity with the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        T Load(Guid id);

        /// <summary>
        /// Adds the specified entity collection to repository.
        /// Will be persisted when PersistAll is called (or sooner).
        /// </summary>
        /// <param name="entityCollection">The entity collection.</param>
        void AddRange(IEnumerable<T> entityCollection);

        /// <summary>
        /// Gets the unit of work.
        /// </summary>
        /// <value>The unit of work.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-06-02
        /// </remarks>
        IUnitOfWork UnitOfWork { get; }
    }
}