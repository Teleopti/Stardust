using NHibernate;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	/// <summary>
	/// Base class for all repositories. 
	/// Non generic methods are here
	/// </summary>
	/// <remarks>
	/// Created by: rogerkr
	/// Created date: 2007-12-21
	/// </remarks>
	public class Repository : IRepository
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		//old way - avoid using this one
		public Repository(IUnitOfWork unitOfWork)
		{
			InParameter.NotNull("unitOfWork", unitOfWork);
			_currentUnitOfWork = new FixedCurrentUnitOfWork(unitOfWork);
		}


		//old way - don't use this one
		public Repository(IUnitOfWorkFactory unitOfWorkFactory)
		{
			_currentUnitOfWork = new justForBackwardCompability(unitOfWorkFactory);
		}
		private class justForBackwardCompability : ICurrentUnitOfWork
		{
			private readonly IUnitOfWorkFactory _unitOfWorkFactory;

			public justForBackwardCompability(IUnitOfWorkFactory unitOfWorkFactory)
			{
				_unitOfWorkFactory = unitOfWorkFactory;
			}

			public IUnitOfWork Current()
			{
				return _unitOfWorkFactory.CurrentUnitOfWork();
			}
		}

		//new, safe way
		public Repository(ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}


		public IUnitOfWork UnitOfWork
		{
			get
			{
				return _currentUnitOfWork.Current();
			}
		}

		public virtual void Add(IAggregateRoot root)
		{
			Session.SaveOrUpdate(root);
		}

		public virtual void Remove(IAggregateRoot root)
		{
			IDeleteTag delRootInfo = root as IDeleteTag;
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


		protected virtual ISession Session
		{
			get
			{
				if (ValidateUserLoggedOn &&
						!StateHolder.Instance.StateReader.IsLoggedIn)
					throw new PermissionException("This repository is not available for non logged on users");
				return ((NHibernateUnitOfWork)UnitOfWork).Session;
			}
		}

		public virtual bool ValidateUserLoggedOn
		{
			get { return true; }
		}

	}
}
