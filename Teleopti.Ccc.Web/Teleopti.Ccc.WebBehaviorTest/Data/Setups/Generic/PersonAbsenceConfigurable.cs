﻿using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class PersonAbsenceConfigurable : IUserDataSetup
	{
		public string Name { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }

		public IScenario Scenario = GlobalDataContext.Data().Data<CommonScenario>().Scenario;
		
		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var absence = new AbsenceRepository(uow).LoadAll().Single(abs => abs.Description.Name.Equals(Name));
			var startTimeUtc = user.PermissionInformation.DefaultTimeZone().SafeConvertTimeToUtc(StartTime);
			var endTimeUtc = user.PermissionInformation.DefaultTimeZone().SafeConvertTimeToUtc(EndTime);

			var personAbsence = new PersonAbsence(user, Scenario, new AbsenceLayer(absence, new DateTimePeriod(startTimeUtc, endTimeUtc)));

			var repository = new PersonAbsenceRepository(uow);

			repository.Add(personAbsence);
		}
	}
}
