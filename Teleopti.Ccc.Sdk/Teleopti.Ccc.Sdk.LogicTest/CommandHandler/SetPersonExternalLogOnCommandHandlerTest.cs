using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;


namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
	[TestFixture]
	public class SetPersonExternalLogOnCommandHandlerTest
	{
		private SetPersonExternalLogOnCommandHandler _target;
		private FakeExternalLogOnRepository _externalLogOnRepository;
		private FakeCurrentUnitOfWorkFactory _unitOfWorkFactory;
		private FakePersonRepository _personRepository;

		[SetUp]
		public void Setup()
		{
			_externalLogOnRepository = new FakeExternalLogOnRepository();
			_unitOfWorkFactory = new FakeCurrentUnitOfWorkFactory(null);
			_personRepository = new FakePersonRepositoryLegacy();
			_target = new SetPersonExternalLogOnCommandHandler(_personRepository, _unitOfWorkFactory, _externalLogOnRepository, new FullPermission());
		}

		[Test]
		public void ShouldThrowWhenNoPermissions()
		{
			var person = new Person().WithId();
			_personRepository.Add(person);
			var setPersonExternalLogOnCommandDto = new SetPersonExternalLogOnCommandDto
			{
				PersonId = person.Id.GetValueOrDefault(),
				PeriodStartDate = new DateOnlyDto(2016, 03, 07),
				ExternalLogOn = new List<ExternalLogOnDto> { new ExternalLogOnDto() }
			};

			using (CurrentAuthorization.ThreadlyUse(new NoPermission()))
			{
				Assert.Throws<FaultException>(() => _target.Handle(setPersonExternalLogOnCommandDto));
			}
		}

		[Test]
		public void ShouldThrowWhenNoPersonPeriods()
		{
			var person = new Person().WithId();
			_personRepository.Add(person);
			var setPersonExternalLogOnCommandDto = new SetPersonExternalLogOnCommandDto
			{
				PersonId = person.Id.GetValueOrDefault(),
				PeriodStartDate = new DateOnlyDto(2016, 03, 07),
				ExternalLogOn = new List<ExternalLogOnDto> { new ExternalLogOnDto() }
			};

			Assert.Throws<FaultException>(() => _target.Handle(setPersonExternalLogOnCommandDto));
		}

		[Test]
		public void ShouldCreateNewPersonPeriodBasedOnOldPeriodWhenStartDateDoesNotMatch()
		{
			var person = new Person().WithId();
			var period = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2010, 03, 07)).WithId();
			person.AddPersonPeriod(period);
			_personRepository.Add(person);
			var externalLogon = new ExternalLogOn(1, 1, "origi", "abc123", true).WithId();
			_externalLogOnRepository.Add(externalLogon);

			var setPersonExternalLogOnCommandDto = new SetPersonExternalLogOnCommandDto
			{
				PersonId = person.Id.GetValueOrDefault(),
				PeriodStartDate = new DateOnlyDto(2016, 03, 07),
				ExternalLogOn = new List<ExternalLogOnDto> { new ExternalLogOnDto { AcdLogOnName = externalLogon.AcdLogOnName, AcdLogOnOriginalId = externalLogon.AcdLogOnOriginalId, Id = externalLogon.Id.GetValueOrDefault()} }
			};

			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				_target.Handle(setPersonExternalLogOnCommandDto);
			}

			person.PersonPeriodCollection.Count.Should().Be.EqualTo(2);
			var currentPeriod = person.Period(setPersonExternalLogOnCommandDto.PeriodStartDate.ToDateOnly());
			currentPeriod.Should().Not.Be.Null();
			currentPeriod.Id.GetValueOrDefault().Should().Not.Be.EqualTo(period.Id.GetValueOrDefault());
			currentPeriod.StartDate.Should().Be.EqualTo(setPersonExternalLogOnCommandDto.PeriodStartDate.ToDateOnly());
			currentPeriod.ExternalLogOnCollection.Count().Should().Be.EqualTo(1);
			currentPeriod.ExternalLogOnCollection.Any(x => x.AcdLogOnName == externalLogon.AcdLogOnName && x.AcdLogOnOriginalId == externalLogon.AcdLogOnOriginalId).Should().Be.True();
		}

		[Test]
		public void ShouldUpdateExistingPersonPeriodWhenStartDateMatches()
		{
			var person = new Person().WithId();
			var period = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2010, 03, 07)).WithId();
			person.AddPersonPeriod(period);
			_personRepository.Add(person);
			var externalLogon = new ExternalLogOn(1, 1, "origi", "abc123", true).WithId();
			_externalLogOnRepository.Add(externalLogon);

			var setPersonExternalLogOnCommandDto = new SetPersonExternalLogOnCommandDto
			{
				PersonId = person.Id.GetValueOrDefault(),
				PeriodStartDate = new DateOnlyDto(2010, 03, 07),
				ExternalLogOn = new List<ExternalLogOnDto> { new ExternalLogOnDto { AcdLogOnName = externalLogon.AcdLogOnName, AcdLogOnOriginalId = externalLogon.AcdLogOnOriginalId, Id = externalLogon.Id.GetValueOrDefault() } }
			};

			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				_target.Handle(setPersonExternalLogOnCommandDto);
			}

			person.PersonPeriodCollection.Count.Should().Be.EqualTo(1);
			var currentPeriod = person.Period(setPersonExternalLogOnCommandDto.PeriodStartDate.ToDateOnly());
			currentPeriod.Should().Not.Be.Null();
			currentPeriod.Id.GetValueOrDefault().Should().Be.EqualTo(period.Id.GetValueOrDefault());
			currentPeriod.StartDate.Should().Be.EqualTo(setPersonExternalLogOnCommandDto.PeriodStartDate.ToDateOnly());
			currentPeriod.ExternalLogOnCollection.Count().Should().Be.EqualTo(1);
			currentPeriod.ExternalLogOnCollection.Any(x => x.AcdLogOnName == externalLogon.AcdLogOnName && x.AcdLogOnOriginalId == externalLogon.AcdLogOnOriginalId).Should().Be.True();
		}
	}
}