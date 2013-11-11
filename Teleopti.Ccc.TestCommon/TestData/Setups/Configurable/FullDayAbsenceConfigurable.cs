﻿using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class FullDayAbsenceConfigurable : IUserDataSetup
	{
		public string Scenario { get; set; }
		public string Name { get; set; }
		public DateTime Date { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var scenario = new ScenarioRepository(uow).LoadAll().Single(abs => abs.Description.Name.Equals(Scenario));
			var absence = new AbsenceRepository(uow).LoadAll().Single(abs => abs.Description.Name.Equals(Name));

			var handler = new AddFullDayAbsenceCommandHandler(new ScheduleRepository(uow), new PersonRepository(uow), new AbsenceRepository(uow), new PersonAbsenceRepository(uow), new ThisCurrentScenario(scenario));
			handler.Handle(new AddFullDayAbsenceCommand
				{
					AbsenceId = absence.Id.Value,
					StartDate = Date,
					EndDate = Date,
					PersonId = user.Id.Value
				});
		}
	}
}