using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class AbsenceAccountProvider : IAbsenceAccountProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPersonAbsenceAccountRepository _repository;

		public AbsenceAccountProvider(ILoggedOnUser loggedOnUser, IPersonAbsenceAccountRepository absenceAccountRepository)
		{
			_loggedOnUser = loggedOnUser;
			_repository = absenceAccountRepository;
		}

		public IAccount GetPersonAccount(IAbsence absence, DateOnly date)
		{
			return _repository.Find(_loggedOnUser.CurrentUser()).Find(absence, date);
		}
	}
}