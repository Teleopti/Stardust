using System;
using System.Collections.Generic;
using NHibernate;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security;

namespace Teleopti.Wfm.Adherence.Configuration.Repositories
{
    public abstract class Repository<T> where T : IAggregateRoot
    {
	    private readonly ICurrentUnitOfWork _currentUnitOfWork;

		protected Repository(ICurrentUnitOfWork currentUnitOfWork)
	    {
		    _currentUnitOfWork = currentUnitOfWork;
	    }
		
		private IUnitOfWork unitOfWork => _currentUnitOfWork.Current();
		protected ISession Session => unitOfWork.Session();

	    public virtual IEnumerable<T> LoadAll()
        {
            return Session.CreateCriteria(typeof(T)).List<T>();
        }

        public virtual T Load(Guid id)
        {
            return Session.Load<T>(id);
        }

        public virtual T Get(Guid id)
        {
            return Session.Get<T>(id);
        }

        public virtual void Add(T root)
        {
//			if (root is IFilterOnBusinessUnit && ServiceLocator_DONTUSE.CurrentBusinessUnit.Current() == null)
//				throw new PermissionException("Business unit is required");
			if (root is IChangeInfo && ServiceLocator_DONTUSE.UpdatedBy.Person() == null)
				throw new PermissionException("Identity is required");
			Session.SaveOrUpdate(root);
		}
		
        public virtual void Remove(T root)
        {
			if (!(root is IDeleteTag delRootInfo))
			{
				Session.Delete(root);
			}
			else
			{
				if (!unitOfWork.Contains(root))
				{
					//ouch! this creates a lot of problem
					//don't dare to remove it though
					unitOfWork.Reassociate(root);
				}
				delRootInfo.SetDeleted();
			}
		}

	}
}