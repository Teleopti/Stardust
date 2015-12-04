using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.Provider
{
	public class RequestsProvider : IRequestsProvider
	{
		private readonly IPersonRequestRepository _repository;
		private readonly IUserTimeZone _userTimeZone;

		public RequestsProvider(IPersonRequestRepository repository, IUserTimeZone userTimeZone)
		{
			_repository = repository;
			_userTimeZone = userTimeZone;
		}

		public IEnumerable<IPersonRequest> RetrieveRequests(DateOnlyPeriod period)
		{
			return _repository.FindAllRequests(period.ToDateTimePeriod(_userTimeZone.TimeZone()),
				new List<RequestType> { RequestType.AbsenceRequest, RequestType.TextRequest });
		}
	}
}