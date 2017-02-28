﻿using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.ResourcePlanner.Validation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner.Validation
{
	[ResourcePlannerTest]
	public class PersonContractScheduleValidatorTest
	{
		public IPersonContractScheduleValidator Target;

		[Test]
		public void PersonHasContractScheduleShouldNotReturnValidationError()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);

			var person = PersonFactory.CreatePerson().WithId();
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(startDate));

			var result = Target.GetPeopleMissingContractSchedule(new[] { person }, planningPeriod).ToList();

			result.Should().Be.Empty();
		}

		[Test]
		public void PersonHasDeletedContractScheduleShouldReturnValidationError()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);

			var person = PersonFactory.CreatePerson().WithId();
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(startDate);
			((IDeleteTag)personPeriod.PersonContract.ContractSchedule).SetDeleted();
			person.AddPersonPeriod(personPeriod);

			var result = Target.GetPeopleMissingContractSchedule(new[] { person }, planningPeriod).ToList();

			result.Should().Not.Be.Empty();
			var validationError = result.SingleOrDefault();
			validationError.PersonId.Should().Be.EqualTo(person.Id);
			validationError.PersonName.Should().Be.EqualTo(person.Name.ToString());
			validationError.ValidationError.Should().Not.Be.Null().And.Not.Be.Empty();
		}
	}
}