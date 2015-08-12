using NHibernate;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public abstract class Repository
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		//old way - avoid using this one
		protected Repository(IUnitOfWork unitOfWork)
		{
			InParameter.NotNull("unitOfWork", unitOfWork);
			_currentUnitOfWork = new FixedCurrentUnitOfWork(unitOfWork);
		}


		//old way - don't use this one
		protected Repository(IUnitOfWorkFactory unitOfWorkFactory)
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
		protected Repository(ICurrentUnitOfWork currentUnitOfWork)
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

		protected ISession Session
		{
			get
			{
				if (ValidateUserLoggedOn)
				{
					var identity = new CurrentIdentity(new CurrentTeleoptiPrincipal()).Current();
					var loggedIn = identity != null && identity.IsAuthenticated;
					if (!loggedIn)
						throw new PermissionException("This repository is not available for non logged on users");
				}
				return ((NHibernateUnitOfWork)UnitOfWork).Session;
			}
		}

		public virtual bool ValidateUserLoggedOn
		{
			get { return true; }
		}
	}
}
