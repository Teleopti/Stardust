using System;
using System.Globalization;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{

	public class ShiftExchangeOfferConfigurable : IUserDataSetup
	{
		public DateTime ValidTo { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public ShiftExchangeLookingForDay WishShiftType { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork, IPerson user, CultureInfo cultureInfo)
		{
			var timeZone = user.PermissionInformation.DefaultTimeZone();
			var startTimeUtc = timeZone.SafeConvertTimeToUtc(StartTime);
			var endTimeUtc = timeZone.SafeConvertTimeToUtc(EndTime);
			 
			var shiftWithin =  (WishShiftType != ShiftExchangeLookingForDay.WorkingShift)
				? null : (DateTimePeriod?)new DateTimePeriod(startTimeUtc, endTimeUtc);
			var criteria = new ScheduleDayFilterCriteria(WishShiftType, shiftWithin);
			var shiftExchangeCrie = new ShiftExchangeCriteria(new DateOnly(ValidTo), criteria);

			var date = new DateOnly(startTimeUtc.Year, startTimeUtc.Month, startTimeUtc.Day);
			var scheduleDay = ScheduleDayFactory.Create(date);
			scheduleDay.Person.SetId(user.Id);

			var shiftExchangeOffer = new ShiftExchangeOffer(scheduleDay, shiftExchangeCrie, ShiftExchangeOfferStatus.Pending);

			var personRequest = new PersonRequest(user){Request = shiftExchangeOffer, Subject = "Announcement"};
			personRequest.TrySetMessage("");
			var personRequestRepository = new PersonRequestRepository(currentUnitOfWork);
			personRequestRepository.Add(personRequest);
		}
	}
}