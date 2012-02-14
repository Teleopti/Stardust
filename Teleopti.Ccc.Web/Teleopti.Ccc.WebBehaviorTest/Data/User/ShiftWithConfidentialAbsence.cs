﻿using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class ShiftWithConfidentialAbsence : IUserDataSetup
	{
		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			new ShiftToday().Apply(uow, user, cultureInfo);
			var date = DateTime.UtcNow.Date;

			var absenseLayer = new AbsenceLayer(TestData.ConfidentialAbsence,new DateTimePeriod(date.AddHours(8), date.AddHours(17)));
			var personAbsence = new PersonAbsence(user, TestData.Scenario, absenseLayer);
			var absRepository = new PersonAbsenceRepository(uow);
			absRepository.Add(personAbsence);
		}
	}
}