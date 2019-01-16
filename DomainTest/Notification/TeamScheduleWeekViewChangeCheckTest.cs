using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Messaging;

namespace Teleopti.Ccc.DomainTest.Notification
{
	[TestFixture]
	[DomainTest]
	public class TeamScheduleWeekViewChangeCheckTest : IIsolateSystem
	{
		public ITeamScheduleWeekViewChangeCheck Target;
		public FakeScheduleDayReadModelRepository FakeScheduleDayReadModelRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonRepository PersonRepository;
		public FakeMessageBrokerComposite MessageBrokerComposite;
		public ICurrentDataSource DataSource;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<TeamScheduleWeekViewChangeCheck>().For<ITeamScheduleWeekViewChangeCheck>();
			isolate.UseTestDouble<FakeMessageBrokerComposite>().For<IMessageBrokerComposite>();
		}

		[Test]
		public void ShouldSendMessageIfTheScheduleIsChangedToWorkingDayFromDayOff()
		{
			var date = new DateOnly(2019, 01, 16);
			var person = PersonFactory.CreatePerson("person", "1");
			var newReadModel = new ScheduleDayReadModel
			{
				StartDateTime = new DateTime(2019, 01, 16, 8, 0, 0),
				EndDateTime = new DateTime(2019, 01, 16, 16, 0, 0),
				PersonId = person.Id.GetValueOrDefault(),
				Workday = true,
				Date = date.Date
			};

			FakeScheduleDayReadModelRepository.SaveReadModel(new ScheduleDayReadModel
			{
				StartDateTime = new DateTime(2019, 01, 16, 8, 0, 0),
				EndDateTime = new DateTime(2019, 01, 16, 16, 0, 0),
				PersonId = person.Id.GetValueOrDefault(),
				Workday = false,
				Date = date.Date
			});

			var model = new TeamScheduleWeekViewChangeCheckModel
			{
				Date = date,
				Person = person,
				NewReadModel = newReadModel,
				LogOnDatasource = "Teleopti",
				LogOnBusinessUnitId = Guid.NewGuid()
			};
			Target.InitiateNotify(model);

			MessageBrokerComposite.GetMessages().Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldSendMessageIfTheScheduleIsChangedToDayOffFromWorkingDay()
		{
			var date = new DateOnly(2019, 01, 16);
			var person = PersonFactory.CreatePerson("person", "1");
			var newReadModel = new ScheduleDayReadModel
			{
				StartDateTime = new DateTime(2019, 01, 16, 0, 0, 0),
				EndDateTime = new DateTime(2019, 01, 17, 0, 0, 0),
				PersonId = person.Id.GetValueOrDefault(),
				Workday = false,
				Date = date.Date
			};

			FakeScheduleDayReadModelRepository.SaveReadModel(new ScheduleDayReadModel
			{
				StartDateTime = new DateTime(2019, 01, 16, 8, 0, 0),
				EndDateTime = new DateTime(2019, 01, 16, 16, 0, 0),
				PersonId = person.Id.GetValueOrDefault(),
				Workday = true,
				Date = date.Date
			});

			var model = new TeamScheduleWeekViewChangeCheckModel
			{
				Date = date,
				Person = person,
				NewReadModel = newReadModel,
				LogOnDatasource = "Teleopti",
				LogOnBusinessUnitId = Guid.NewGuid()
			};
			Target.InitiateNotify(model);

			MessageBrokerComposite.GetMessages().Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldSendMessageIfTheScheduleStartTimeIsChanged()
		{
			var date = new DateOnly(2019, 01, 16);
			var person = PersonFactory.CreatePerson("person", "1");
			var newReadModel = new ScheduleDayReadModel
			{
				StartDateTime = new DateTime(2019, 01, 16, 7, 0, 0),
				EndDateTime = new DateTime(2019, 01, 16, 17, 0, 0),
				PersonId = person.Id.GetValueOrDefault(),
				Workday = true,
				Date = date.Date
			};

			FakeScheduleDayReadModelRepository.SaveReadModel(new ScheduleDayReadModel
			{
				StartDateTime = new DateTime(2019, 01, 16, 8, 0, 0),
				EndDateTime = new DateTime(2019, 01, 16, 17, 0, 0),
				PersonId = person.Id.GetValueOrDefault(),
				Workday = true,
				Date = date.Date
			});

			var model = new TeamScheduleWeekViewChangeCheckModel
			{
				Date = date,
				Person = person,
				NewReadModel = newReadModel,
				LogOnDatasource = "Teleopti",
				LogOnBusinessUnitId = Guid.NewGuid()
			};
			Target.InitiateNotify(model);

			MessageBrokerComposite.GetMessages().Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldSendMessageIfTheScheduleEndTimeIsChanged()
		{
			var date = new DateOnly(2019, 01, 16);
			var person = PersonFactory.CreatePerson("person", "1");
			var newReadModel = new ScheduleDayReadModel
			{
				StartDateTime = new DateTime(2019, 01, 16, 8, 0, 0),
				EndDateTime = new DateTime(2019, 01, 16, 17, 0, 0),
				PersonId = person.Id.GetValueOrDefault(),
				Workday = true,
				Date = date.Date
			};

			FakeScheduleDayReadModelRepository.SaveReadModel(new ScheduleDayReadModel
			{
				StartDateTime = new DateTime(2019, 01, 16, 8, 0, 0),
				EndDateTime = new DateTime(2019, 01, 16, 16, 0, 0),
				PersonId = person.Id.GetValueOrDefault(),
				Workday = true,
				Date = date.Date
			});

			var model = new TeamScheduleWeekViewChangeCheckModel
			{
				Date = date,
				Person = person,
				NewReadModel = newReadModel,
				LogOnDatasource = "Teleopti",
				LogOnBusinessUnitId = Guid.NewGuid()
			};
			Target.InitiateNotify(model);

			MessageBrokerComposite.GetMessages().Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldSendMessageIfTheScheduleDescriptionIsChanged()
		{
			var date = new DateOnly(2019, 01, 16);
			var person = PersonFactory.CreatePerson("person", "1");
			var newReadModel = new ScheduleDayReadModel
			{
				StartDateTime = new DateTime(2019, 01, 16, 8, 0, 0),
				EndDateTime = new DateTime(2019, 01, 16, 17, 0, 0),
				PersonId = person.Id.GetValueOrDefault(),
				Workday = true,
				Date = date.Date,
				Label = "Early"
			};

			FakeScheduleDayReadModelRepository.SaveReadModel(new ScheduleDayReadModel
			{
				StartDateTime = new DateTime(2019, 01, 16, 8, 0, 0),
				EndDateTime = new DateTime(2019, 01, 16, 17, 0, 0),
				PersonId = person.Id.GetValueOrDefault(),
				Workday = true,
				Date = date.Date,
				Label = "Late"
			});

			var model = new TeamScheduleWeekViewChangeCheckModel
			{
				Date = date,
				Person = person,
				NewReadModel = newReadModel,
				LogOnDatasource = "Teleopti",
				LogOnBusinessUnitId = Guid.NewGuid()
			};
			Target.InitiateNotify(model);

			MessageBrokerComposite.GetMessages().Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotSendMessageWhenTheChangeIsNotRelevant()
		{
			var date = new DateOnly(2019, 01, 16);
			var person = PersonFactory.CreatePerson("person", "1");
			var newReadModel = new ScheduleDayReadModel
			{
				StartDateTime = new DateTime(2019, 01, 16, 8, 0, 0),
				EndDateTime = new DateTime(2019, 01, 16, 17, 0, 0),
				PersonId = person.Id.GetValueOrDefault(),
				Workday = true,
				Date = date.Date
			};

			FakeScheduleDayReadModelRepository.SaveReadModel(new ScheduleDayReadModel
			{
				StartDateTime = new DateTime(2019, 01, 16, 8, 0, 0),
				EndDateTime = new DateTime(2019, 01, 16, 17, 0, 0),
				PersonId = person.Id.GetValueOrDefault(),
				Workday = true,
				Date = date.Date
			});

			var model = new TeamScheduleWeekViewChangeCheckModel
			{
				Date = date,
				Person = person,
				NewReadModel = newReadModel,
				LogOnDatasource = "Teleopti",
				LogOnBusinessUnitId = Guid.NewGuid()
			};
			Target.InitiateNotify(model);

			MessageBrokerComposite.GetMessages().Should().Be.Empty();
		}

		[Test]
		public void ShouldSendCorrectMessage()
		{
			var date = new DateOnly(2019, 01, 16);
			var person = PersonFactory.CreatePerson("person", "1");
			var newReadModel = new ScheduleDayReadModel
			{
				StartDateTime = new DateTime(2019, 01, 16, 8, 0, 0),
				EndDateTime = new DateTime(2019, 01, 16, 17, 0, 0),
				PersonId = person.Id.GetValueOrDefault(),
				Workday = true,
				Date = date.Date
			};

			FakeScheduleDayReadModelRepository.SaveReadModel(new ScheduleDayReadModel
			{
				StartDateTime = new DateTime(2019, 01, 16, 8, 0, 0),
				EndDateTime = new DateTime(2019, 01, 16, 18, 0, 0),
				PersonId = person.Id.GetValueOrDefault(),
				Workday = true,
				Date = date.Date
			});

			var buId = Guid.NewGuid();

			var model = new TeamScheduleWeekViewChangeCheckModel
			{
				Date = date,
				Person = person,
				NewReadModel = newReadModel,
				LogOnDatasource = "Teleopti",
				LogOnBusinessUnitId = buId
			};
			Target.InitiateNotify(model);

			var message = MessageBrokerComposite.GetMessages().Single();
			message.DataSource.Should().Be.EqualTo("Teleopti");
			message.BusinessUnitId.Should().Be.EqualTo(buId.ToString());
			message.StartDate.Should().Be.EqualTo("D2019-01-16T00:00:00");
			message.EndDate.Should().Be.EqualTo("D2019-01-16T00:00:00");
			message.DomainType.Should().Be.EqualTo(nameof(ITeamScheduleWeekViewChange));
		}
	}

}
