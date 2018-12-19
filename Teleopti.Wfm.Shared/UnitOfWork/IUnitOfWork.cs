using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{
	/// <summary>
	/// Controls a group of data source CRUDs. 
	/// </summary>
	public interface IUnitOfWork : IDisposable
	{
		IInitiatorIdentifier Initiator();

		/// <summary>
		/// Clears the state of this uow.
		/// </summary>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 10/23/2007
		/// </remarks>
		void Clear();

		/// <summary>
		/// Merges the specified aggregate to this UnitOfWork.
		/// Loads the given root's aggregate into this UnitOfWork if not already present.
		/// If the root is present in this unitofwork, the state of the root will be copied.
		/// Returns the persistent instance.
		/// Note:
		/// The given instance does not become associated with the uow or affected at all
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="root">The root.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-04-24
		/// </remarks>
		T Merge<T>(T root) where T : class, IAggregateRoot;

		/// <summary>
		/// Determines whether the specified entity exists in this UnitOfWork.
		/// </summary>
		/// <param name="entity">The IAggregateRoot entity.</param>
		/// <remarks>
		/// Should this only accept IAggregateRoot?
		/// </remarks>
		bool Contains(IEntity entity);


		/// <summary>
		/// Determines whether this UnitOfWork is dirty.
		/// </summary>
		/// <returns>
		/// 	<c>true</c> if this UnitOfWork is dirty; otherwise, <c>false</c>.
		/// </returns>
		bool IsDirty();

		/// <summary>
		/// Persists all entities in this UnitOfWork in a transaction.
		/// </summary>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-01-09
		/// </remarks>
		IEnumerable<IRootChangeInfo> PersistAll();

		/// <summary>
		/// Persists all.
		/// Tells the MB what module made the persist
		/// </summary>
		/// <param name="initiator">The module.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-07-01
		/// </remarks>
		IEnumerable<IRootChangeInfo> PersistAll(IInitiatorIdentifier initiator);

		/// <summary>
		/// Reassociates the specified root.
		/// </summary>
		/// <param name="root">The root.</param>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2007-12-19
		/// </remarks>
		void Reassociate(IAggregateRoot root);

		/// <summary>
		/// Reassociates the specified collection of root collections.
		/// </summary>
		/// <param name="rootCollectionsCollection">The root collection.</param>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2010-07-05
		/// </remarks>
		void Reassociate<T>(params IEnumerable<T>[] rootCollectionsCollection) where T : IAggregateRoot;

		/// <summary>
		/// Refreshes the specified root.
		/// </summary>
		/// <param name="root">The root.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-02-22
		/// </remarks>
		void Refresh(IAggregateRoot root);

		/// <summary>
		/// Removes this entity from this UnitOfWork.
		/// </summary>
		/// <param name="root">The root.</param>
		void Remove(IAggregateRoot root);

		/// <summary>
		/// Disables the filter.
		/// </summary>
		/// <param name="filter">The filter.</param>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2009-05-04
		/// </remarks>
		IDisposable DisableFilter(IQueryFilter filter);

		/// <summary>
		/// Flushes this instance. Synchronzies unit of work with db without commiting transaction
		/// Note: Don't use this if you're not sure what you're doing!
		/// </summary>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2010-08-25
		/// </remarks>
		void Flush();

		/// <summary>
		/// Adds an action to run after transaction has been successfully committed.
		/// (the object param is just a dummy param used for .net 2.0. Will be fixed in later releases
		/// </summary>
		/// <param name="action"></param>
		void AfterSuccessfulTx(Action action);

	}
}