using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.InfrastructureTest.Persisters.Schedules;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork.TransactionHooks
{
	[TestFixture]
	[ScheduleDictionaryPersistTest]
	public class ScheduleChangedMessageDictionaryPersisterTest : IIsolateSystem
	{
		public FakeMessageSender MessageSender;
		public IScheduleDictionaryPersister Target;
		public IJsonDeserializer Deserializer;
		public IScheduleDictionaryPersistTestHelper Helper;
		public ICurrentDataSource DataSource;
		public ICurrentBusinessUnit BusinessUnit;
		public FakeInitiatorIdentifier InitiatorIdentifier;
		public FakeTime Time;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeInitiatorIdentifier>().For<IInitiatorIdentifier>();
			isolate.UseTestDouble<FullPermission>().For<IAuthorization>();
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
			Time.Passes("60".Seconds());

			var message = MessageSender.NotificationsOfDomainType<IScheduleChangedMessage>().Single();
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
			Time.Passes("60".Seconds());

			var message = MessageSender.NotificationsOfDomainType<IScheduleChangedMessage>().Single();
			message.DataSource.Should().Be(DataSource.CurrentName());
			message.BusinessUnitIdAsGuid().Should().Be(BusinessUnit.Current().Id.Value);
			message.StartDateAsDateTime().Should().Be("2015-10-19 8:00".Utc());
			message.EndDateAsDateTime().Should().Be("2015-10-19 17:00".Utc());
			message.DomainReferenceIdAsGuid().Should().Be(schedules.Scenario.Id.Value);
			message.DomainQualifiedType.Should().Be(typeof(IScheduleChangedMessage).AssemblyQualifiedName);
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
			Time.Passes("60".Seconds());

			MessageSender.NotificationsOfDomainType<IScheduleChangedMessage>().Single()
				.ModuleIdAsGuid().Should().Be(InitiatorIdentifier.InitiatorId);
		}

		[Test]
		public void ShouldSendWithInitiatorIdWhenPersistingDictionaryWithChangedAndUnchangedScheduleDays()
		{
			InitiatorIdentifier.InitiatorId = Guid.NewGuid();
			var person = Helper.NewPerson();
			var person2 = Helper.NewPerson();
			var schedules = Helper.MakeDictionary();
			schedules[person]
				.ScheduledDay("2015-10-19".Date())
				.CreateAndAddActivity(
					Helper.Activity(),
					"2015-10-19 08:00 - 2015-10-19 17:00".Period())
				.ModifyDictionary();
			schedules[person2]
				.ScheduledDay("2015-10-19".Date());

			Target.Persist(schedules);
			Time.Passes("60".Seconds());

			MessageSender.NotificationsOfDomainType<IScheduleChangedMessage>().Single()
				.ModuleIdAsGuid().Should().Be(InitiatorIdentifier.InitiatorId);
		}
	}
}