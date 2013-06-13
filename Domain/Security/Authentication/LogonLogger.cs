using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
	public interface ILogonLogger
	{
		void SaveLogonAttempt(LoginAttemptModel model,IUnitOfWorkFactory unitOfWorkFactory);
	}

	public class LogonLogger : ILogonLogger
	{
		private readonly IRepositoryFactory _repositoryFactory;

		public LogonLogger(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		public void SaveLogonAttempt(LoginAttemptModel model, IUnitOfWorkFactory unitOfWorkFactory)
		{
			using (var unitOfWork = unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var rep = _repositoryFactory.CreatePersonRepository(unitOfWork);
				rep.SaveLoginAttempt(model);
				unitOfWork.PersistAll();
			}

		}
	}
}