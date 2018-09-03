﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class GetPeopleByGroupPageGroudForDateRangeQueryHandlerTest
	{
		[Test]
		public void ShouldGetPeopleByGroupForDateRangeInGroupPage()
		{
			var groupPageGroupId = Guid.NewGuid();
			var personWithId = PersonFactory.CreatePersonWithId();
			var personRepository = new FakePersonRepositoryLegacy();
			personRepository.Add(personWithId);
			var groupingReadOnlyRepository = new FakeGroupingReadOnlyRepository(new ReadOnlyGroupDetail { PersonId = personWithId.Id.GetValueOrDefault() });
			var assembler = new PersonAssembler(personRepository,
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
					new AbsenceAssembler(new FakeAbsenceRepository())), new PersonAccountUpdaterDummy());

			var dataManager = new FakeTenantLogonDataManager();
			dataManager.SetLogon(personWithId.Id.Value,"aaa","");

			var unitOfWorkFactory = new FakeCurrentUnitOfWorkFactory(null);
			var target = new GetPeopleByGroupPageGroupForDateRangeQueryHandler(groupingReadOnlyRepository,
				personRepository, new PersonCredentialsAppender(assembler, new TenantPeopleLoader(dataManager)),
				unitOfWorkFactory);
			var dateOnly = new DateOnly(2012, 4, 30);

			var result =
				target.Handle(new GetPeopleByGroupPageGroupForDateRangeQueryDto
				{
					GroupPageGroupId = groupPageGroupId,
					QueryRange = new DateOnlyPeriodDto {StartDate = new DateOnlyDto { DateTime = dateOnly.Date },EndDate = new DateOnlyDto {DateTime = dateOnly.Date } }
				});
			result.Count.Should().Be.EqualTo(1);
			result.First().ApplicationLogOnName.Should().Be.EqualTo("aaa");
		}
	}
}