using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory
{
	
	public class AbsenceRequestDetailViewModelFactory : IAbsenceRequestDetailViewModelFactory
	{
		private readonly IPersonRequestProvider _personRequestProvider;
		private readonly IAbsenceRequestWaitlistProvider _absenceRequestWaitlistProvider;

		public AbsenceRequestDetailViewModelFactory (IPersonRequestProvider personRequestProvider, IAbsenceRequestWaitlistProvider absenceRequestWaitlistProvider)
		{
			_personRequestProvider = personRequestProvider;
			_absenceRequestWaitlistProvider = absenceRequestWaitlistProvider;
		}

		public IAbsenceRequestDetailViewModel CreateAbsenceRequestDetailViewModel(Guid personRequestId)
		{
			var personRequest=_personRequestProvider.RetrieveRequest (personRequestId);
			var absenceRequest = personRequest.Request as IAbsenceRequest;
			if (absenceRequest != null)
			{
				var waitListPosition=_absenceRequestWaitlistProvider.GetPositionInWaitlist (absenceRequest);
				return new AbsenceRequestDetailViewModel()
				{
					WaitlistPosition = waitListPosition
				};
			}
			return null;

		}
	}
	
	public class AbsenceRequestDetailViewModel : IAbsenceRequestDetailViewModel
	{
		public int WaitlistPosition { get; set; }
	}
}