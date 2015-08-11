using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Generic Base class for repositories.
    /// Only aggregate root entities are allowed.
    /// Used for NHibernate sessions.
    /// </summary>
    /// <typeparam name="T">Type of Aggregate root</typeparam>
    public abstract class Repository<T> : Repository, IRepository<T> where T : IAggregateRoot
    {
        protected Repository(IUnitOfWork unitOfWork) : base(unitOfWork) 
        {
        }

        protected Repository(IUnitOfWorkFactory unitOfWorkFactory) : base(unitOfWorkFactory)
        {
        }

	    protected Repository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
	    {
	    }

        public IList<T> LoadAll()
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
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-06
        /// </remarks>
        public virtual T Get(Guid id)
        {
            return Session.Get<T>(id);
        }

        /// <summary>
        /// Counts all entities of this entity type.
        /// </summary>
        /// <returns></returns>
        public virtual long CountAllEntities()
        {
            return Session.CreateCriteria(typeof(T))
                .SetProjection(Projections.RowCount())
                .UniqueResult<int>();
        }

        /// <summary>
        /// Adds the specified entity to repository.
        /// Will be persisted when PersistAll is called (or sooner).
        /// </summary>
        public virtual void Add(T root)
        {
			Session.SaveOrUpdate(root);
		}

        /// <summary>
        /// Adds the specified entity collection to repository.
        /// Will be persisted when PersistAll is called (or sooner).
        /// </summary>
        /// <param name="entityCollection">The entity collection.</param>
        public virtual void AddRange(IEnumerable<T> entityCollection)
        {
            foreach (T entity in entityCollection)
            {
                Add(entity);
            }
        }

        /// <summary>
        /// Removes the specified entity from repository.
        /// Will be deleted when PersistAll is called (or sooner).
        /// </summary>
        public virtual void Remove(T root)
        {
			var delRootInfo = root as IDeleteTag;
			if (delRootInfo == null)
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
    }
}