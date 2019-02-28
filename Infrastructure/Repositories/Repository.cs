using System;
using System.Collections.Generic;
using NHibernate;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	/// <summary>
	/// Generic Base class for repositories.
	/// Only aggregate root entities are allowed.
	/// Used for NHibernate sessions.
	/// </summary>
	/// <typeparam name="T">Type of Aggregate root</typeparam>
	public abstract class Repository<T> : IRepository<T> where T : IAggregateRoot
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly Lazy<IUpdatedBy> _updatedBy;

		protected Repository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy)
		{
			_currentUnitOfWork = currentUnitOfWork;
			_currentBusinessUnit = currentBusinessUnit;
			_updatedBy = updatedBy;
		}

		public virtual IEnumerable<T> LoadAll()
		{
			return Session.CreateCriteria(typeof(T)).List<T>();
		}

		/// <summary>
		/// Loads an entity with the specified id.
		/// </summary>
		/// <remarks>If you would like to get null when there's no hit in persistent store, use Get()</remarks>
		/// <param name="id">The id.</param>
		/// <returns></returns>
		public virtual T Load(Guid id)
		{
			return Session.Load<T>(id);
		}

		/// <summary>
		/// Gets the specified id.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <returns></returns>
		public virtual T Get(Guid id)
		{
			return Session.Get<T>(id);
		}

		/// <summary>
		/// Adds the specified entity to repository.
		/// Will be persisted when PersistAll is called (or sooner).
		/// </summary>
		public virtual void Add(T root)
		{
			if (root is IFilterOnBusinessUnit)
			{
				var currentBusinessUnit = _currentBusinessUnit ?? ServiceLocator_DONTUSE.CurrentBusinessUnit;
				if (currentBusinessUnit.Current() == null)
					throw new PermissionException("Business unit is required");
			}

			if (root is IChangeInfo)
			{
				var updatedBy = _updatedBy != null ? _updatedBy.Value : ServiceLocator_DONTUSE.UpdatedBy;
				if (updatedBy.Person() == null)
					throw new PermissionException("Identity is required");
			}

			Session.SaveOrUpdate(root);
		}

		/// <summary>
		/// Removes the specified entity from repository.
		/// Will be deleted when PersistAll is called (or sooner).
		/// </summary>
		public virtual void Remove(T root)
		{
			if (!(root is IDeleteTag delRootInfo))
			{
				Session.Delete(root);
			}
			else
			{
				if (!UnitOfWork.Contains(root))
				{
					//ouch! this creates a lot of problem
					//don't dare to remove it though
					UnitOfWork.Reassociate(root);
				}

				delRootInfo.SetDeleted();
			}
		}

		public IUnitOfWork UnitOfWork => _currentUnitOfWork.Current();

		protected ISession Session => UnitOfWork.Session();
	}
}