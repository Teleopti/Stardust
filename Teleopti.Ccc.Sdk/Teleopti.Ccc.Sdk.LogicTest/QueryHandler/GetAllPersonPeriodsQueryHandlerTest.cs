using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;


namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class GetAllPersonPeriodsQueryHandlerTest
	{
		[Test]
		public void ShouldGetAllPersonPeriodsWithinGivenRange()
		{
			var personRepository = new FakePersonRepositoryLegacy
			{
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(2001, 1, 1),
					TeamFactory.CreateTeam("Team 1", "Paris"))
			};
			var target = new GetAllPersonPeriodsQueryHandler(personRepository, new FakeCurrentUnitOfWorkFactory(null),
				new PersonPeriodAssembler(new ExternalLogOnAssembler()),new FullPermission());
			var result =
#pragma warning disable 618
				target.Handle(new GetAllPersonPeriodsQueryDto
#pragma warning restore 618
				{
					Range =
						new DateOnlyPeriodDto
						{
							StartDate = new DateOnlyDto {DateTime = new DateTime(2001, 1, 1)},
							EndDate = new DateOnlyDto {DateTime = new DateTime(2001, 1, 2)}
						}
				});
			result.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldOnlyGetPersonPeriodsWithinGivenRange()
		{
			var personRepository = new FakePersonRepositoryLegacy
			{
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(2001, 2, 1),
					TeamFactory.CreateTeam("Team 1", "Paris"))
			};
			var target = new GetAllPersonPeriodsQueryHandler(personRepository, new FakeCurrentUnitOfWorkFactory(null),
				new PersonPeriodAssembler(new ExternalLogOnAssembler()), new FullPermission());
			var result =
#pragma warning disable 618
				target.Handle(new GetAllPersonPeriodsQueryDto
#pragma warning restore 618
				{
					Range =
						new DateOnlyPeriodDto
						{
							StartDate = new DateOnlyDto {DateTime = new DateTime(2001, 1, 1)},
							EndDate = new DateOnlyDto {DateTime = new DateTime(2001, 1, 2)}
						}
				});
			result.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotGetPersonPeriodsForEveryoneIfNotPermitted()
		{
			var personRepository = new FakePersonRepositoryLegacy
			{
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(2001, 1, 1),
					TeamFactory.CreateTeam("Team 1", "Paris"))
			};
			var target = new GetAllPersonPeriodsQueryHandler(personRepository, new FakeCurrentUnitOfWorkFactory(null),
				new PersonPeriodAssembler(new ExternalLogOnAssembler()), new NoPermission());
			var result =
#pragma warning disable 618
				target.Handle(new GetAllPersonPeriodsQueryDto
#pragma warning restore 618
				{
					Range =
						new DateOnlyPeriodDto
						{
							StartDate = new DateOnlyDto {DateTime = new DateTime(2001, 1, 1)},
							EndDate = new DateOnlyDto {DateTime = new DateTime(2001, 1, 2)}
						}
				});
			result.Count.Should().Be.EqualTo(0);
		}
	}
}