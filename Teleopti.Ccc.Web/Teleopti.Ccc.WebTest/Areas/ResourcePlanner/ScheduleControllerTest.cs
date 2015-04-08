﻿using System;
using System.Web.Http.Results;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.ResourcePlanner;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	public class ScheduleControllerTest
	{
		[Test, Explicit("WIP")]
		public async void ShouldScheduleFixedStaff()
		{
			var scenario = ScenarioFactory.CreateScenario("Default", true, true);
			var period = new DateOnlyPeriod(2015,5,1,2015,5,31);
			var personRepository = new FakePersonRepository(PersonFactory.CreatePersonWithPersonPeriod(period.StartDate));

			var target = new ScheduleController(new FakeScenarioRepository(scenario),
				MockRepository.GenerateMock<ISkillDayLoadHelper>(), MockRepository.GenerateMock<ISkillRepository>(),
				personRepository, MockRepository.GenerateMock<IScheduleRepository>(),
				MockRepository.GenerateMock<IDayOffTemplateRepository>(), MockRepository.GenerateMock<IPersonAbsenceAccountRepository>(),
				MockRepository.GenerateMock<IPeopleAndSkillLoaderDecider>(),
				new FakeCurrentTeleoptiPrincipal(new TeleoptiPrincipal(new TeleoptiIdentity("", null, null, null),
					PersonFactory.CreatePerson(new Name("Anna", "Andersson"), TimeZoneInfo.Utc))),
				MockRepository.GenerateMock<IDisableDeletedFilter>(), MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>());
			var result =
				(OkNegotiatedContentResult<SchedulingResultModel>)
					
						target.FixedStaff(new FixedStaffSchedulingInput {StartDate = period.StartDate.Date, EndDate = period.EndDate.Date});

			result.Content.DaysScheduled.Should().Be.EqualTo(1);
		}
	}
}