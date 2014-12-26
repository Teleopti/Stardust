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

	//RobTODO - Examine and think about refactoring this.
	//public class ShiftExchangeOfferConfigurable : IUserDataSetup
	//{
	//	public DateTime ValidTo { get; set; }
	//	public DateTime StartTime { get; set; }
	//	public DateTime EndTime { get; set; }

	//	public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
	//	{
	//		var timeZone = user.PermissionInformation.DefaultTimeZone();
	//		var startTimeUtc = timeZone.SafeConvertTimeToUtc(StartTime);
	//		var endTimeUtc = timeZone.SafeConvertTimeToUtc(EndTime);
	//		var shiftWithin = new DateTimePeriod(startTimeUtc, endTimeUtc);
	//		var shiftExchangeCrie = new ShiftExchangeCriteria(new DateOnly(ValidTo), shiftWithin);

	//		var date = new DateOnly(startTimeUtc.Year, startTimeUtc.Month, startTimeUtc.Day);
	//		var scheduleDay = ScheduleDayFactory.Create(date);
	//		scheduleDay.Person.SetId(user.Id);

	//		var shiftExchangeOffer = new ShiftExchangeOffer(scheduleDay, shiftExchangeCrie, ShiftExchangeOfferStatus.Pending);
	//		var shiftExchangeOfferRepository = new ShiftExchangeOfferRepository(uow);

	//		shiftExchangeOfferRepository.Add(shiftExchangeOffer);
	//	}
	//}
}