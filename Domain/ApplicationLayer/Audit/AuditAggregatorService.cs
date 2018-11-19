using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Audit
{
	public class AuditAggregatorService
	{
		private readonly IStaffingContextReaderService _staffingContextReaderService;
		private readonly IPersonAccessContextReaderService _personAccessContextReaderService;
		private readonly IPersonRepository _personRepository;
		private readonly ILoggedOnUser _loggedOnUser;

		public AuditAggregatorService(IStaffingContextReaderService staffingContextReaderService, IPersonAccessContextReaderService personAccessContextReaderService, IPersonRepository personRepository, ILoggedOnUser loggedOnUser)
		{
			_staffingContextReaderService = staffingContextReaderService;
			_personAccessContextReaderService = personAccessContextReaderService;
			_personRepository = personRepository;
			_loggedOnUser = loggedOnUser;
		}

		public IList<AuditServiceModel> Load(Guid personId, DateTime startDate, DateTime endDate)
		{
			var aggregatedAudits = new List<AuditServiceModel>();

			aggregatedAudits.AddRange(getStaffingAudits(personId,startDate,endDate));
			aggregatedAudits.AddRange(getPersonAccessAudits(personId,startDate,endDate));

			fixUserTimeZone(aggregatedAudits);

			return aggregatedAudits;

		}

		private void fixUserTimeZone(List<AuditServiceModel> aggregatedAudits)
		{
			aggregatedAudits.ForEach(x =>
				{
					x.TimeStamp = TimeZoneHelper.ConvertFromUtc(x.TimeStamp,
						_loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone());
				});
		}

		private IEnumerable<AuditServiceModel> getStaffingAudits(Guid personId, DateTime startDate, DateTime endDate)
		{
			var person = _personRepository.Load(personId);
			return  _staffingContextReaderService.LoadAudits(person, startDate, endDate);
		}

		private IEnumerable<AuditServiceModel> getPersonAccessAudits(Guid personId, DateTime startDate, DateTime endDate)
		{
			var person = _personRepository.Load(personId);
			return _personAccessContextReaderService.LoadAudits(person, startDate, endDate);
		}

		//public void PurgeOldAudits()
		//{
		//	//make some plugin architecture 
		//	_staffingContextReaderService.PurgeAudits();
		//	_personAccessContextReaderService.PurgeAudits();
		//}
	}
}