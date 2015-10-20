using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.InfrastructureTest.Persisters.Schedules;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork.MessageSenders
{
	[TestFixture]
	[Toggle(Toggles.MessageBroker_SchedulingScreenMailbox_32733)]
	[ScheduleDictionaryPersistTest]
	public class AggregatedScheduleChangeMessageScheduleDictionaryPersisterTest : ISetup
	{
		public FakeMessageSender MessageSender;
		public IScheduleDictionaryPersister Target;
		public IJsonDeserializer Deserializer;
		public IScheduleDictionaryPersistTestHelper Helper;
		public ICurrentDataSource DataSource;
		public ICurrentBusinessUnit BusinessUnit;
		public FakeInitiatorIdentifier InitiatorIdentifier;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeInitiatorIdentifier>().For<IInitiatorIdentifier>();
		}

		[Test]
		public void ShouldSendOneAggregatedScheduleChangeMessageForTheWholeDictionary()
		{
			var person1 = Helper.NewPerson();
			var person2 = Helper.NewPerson();
			var schedules = Helper.MakeDictionary();
			schedules[person1]
				.ScheduledDay("2015-10-19".Date())
				.CreateAndAddActivity(
					Helper.Activity(),
					"2015-10-19 08:00 - 2015-10-19 17:00".Period())
				.ModifyDictionary();
			schedules[person2]
				.ScheduledDay("2015-10-19".Date())
				.CreateAndAddActivity(
					Helper.Activity(),
					"2015-10-19 08:00 - 2015-10-19 17:00".Period())
				.ModifyDictionary();

			Target.Persist(schedules);

			var message = MessageSender.NotificationsOfDomainType<IAggregatedScheduleChange>().Single();
			Deserializer.DeserializeObject<Guid[]>(message.BinaryData).Should().Have.SameValuesAs(new[] { person1.Id.Value, person2.Id.Value });
		}

		[Test]
		public void ShouldSendWithProperties()
		{
			var person = Helper.NewPerson();
			var schedules = Helper.MakeDictionary();
			schedules[person]
				.ScheduledDay("2015-10-19".Date())
				.CreateAndAddActivity(
					Helper.Activity(),
					"2015-10-19 08:00 - 2015-10-19 17:00".Period())
				.ModifyDictionary();

			Target.Persist(schedules);

			var message = MessageSender.NotificationsOfDomainType<IAggregatedScheduleChange>().Single();
			message.DataSource.Should().Be(DataSource.CurrentName());
			message.BusinessUnitIdAsGuid().Should().Be(BusinessUnit.Current().Id.Value);
			message.StartDateAsDateTime().Should().Be("2015-10-19 8:00".Utc());
			message.EndDateAsDateTime().Should().Be("2015-10-19 17:00".Utc());
			message.DomainReferenceIdAsGuid().Should().Be(schedules.Scenario.Id.Value);
			message.DomainQualifiedType.Should().Be(typeof(IAggregatedScheduleChange).AssemblyQualifiedName);
			message.DomainUpdateType.Should().Be(DomainUpdateType.NotApplicable);
			message.BinaryData.Should().Contain(person.Id.ToString());
		}


		[Test]
		public void ShouldSendWithInitiatorId()
		{
			InitiatorIdentifier.InitiatorId = Guid.NewGuid();
			var person = Helper.NewPerson();
			var schedules = Helper.MakeDictionary();
			schedules[person]
				.ScheduledDay("2015-10-19".Date())
				.CreateAndAddActivity(
					Helper.Activity(),
					"2015-10-19 08:00 - 2015-10-19 17:00".Period())
				.ModifyDictionary();

			Target.Persist(schedules);

			MessageSender.NotificationsOfDomainType<IAggregatedScheduleChange>().Single()
				.ModuleIdAsGuid().Should().Be(InitiatorIdentifier.InitiatorId);
		}

	}
}