using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
{
	public class StudentAvailability : IUserDataSetup
	{
		private StudentAvailabilityRestriction _studentAvailabilityRestriction;
		private StudentAvailabilityDay _studentAvailabilityDay;
		public DateTime Date;
		public TimeSpan StartTime;
		public TimeSpan EndTime;

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			Date = DateHelper.GetFirstDateInWeek(DateOnlyForBehaviorTests.TestToday.Date, cultureInfo);
			StartTime = TimeSpan.FromHours(7);
			EndTime = TimeSpan.FromHours(18);

			_studentAvailabilityRestriction = new StudentAvailabilityRestriction
															{
																StartTimeLimitation = new StartTimeLimitation(StartTime, null),
																EndTimeLimitation = new EndTimeLimitation(null, EndTime)
															};
			_studentAvailabilityDay = new StudentAvailabilityDay(
				user,
				new DateOnly(Date),
				new List<IStudentAvailabilityRestriction>(new[]
				                                          	{
				                                          		_studentAvailabilityRestriction
				                                          	}));

			var studentAvailabilityRepository = new StudentAvailabilityDayRepository(uow);
			studentAvailabilityRepository.Add(_studentAvailabilityDay);
		}
	}
}