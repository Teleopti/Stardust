using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[DatabaseTest]
	public class MappingReaderSelectionTest
	{
		public IRtaMapRepository Maps;
		public IRtaStateGroupRepository Groups;
		public IRtaRuleRepository Rules;
		public IActivityRepository Activities;
		public IMappingReader Target;
		public WithUnitOfWork WithUnitOfWork;

		[Test]
		public void ShouldReadForState()
		{
			WithUnitOfWork.Do(() =>
			{
				var group = new RtaStateGroup("Phone").AddState("phone", Guid.NewGuid());
				Groups.Add(group);
				Maps.Add(new RtaMap(group, null));
			});

			WithUnitOfWork.Get(() => Target.ReadFor(new[] {"phone"}, new Guid?[] {null}))
				.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldReadFor2States()
		{
			WithUnitOfWork.Do(() =>
			{
				var group1 = new RtaStateGroup("Phone").AddState("phone", Guid.NewGuid());
				var group2 = new RtaStateGroup("Ready").AddState("ready", Guid.NewGuid());
				Groups.Add(group1);
				Groups.Add(group2);
				Maps.Add(new RtaMap(group1, null));
				Maps.Add(new RtaMap(group2, null));
			});

			WithUnitOfWork.Get(() => Target.ReadFor(new[] {"phone", "ready"}, new Guid?[] {null}))
				.Select(x => x.StateCode).Should().Have.SameValuesAs("phone", "ready");
		}

		[Test]
		public void ShouldExcludeIrrelevantState()
		{
			WithUnitOfWork.Do(() =>
			{
				var group = new RtaStateGroup("LoggedOut").AddState("loggedout", Guid.NewGuid());
				Groups.Add(group);
				Maps.Add(new RtaMap(group, null));
			});

			WithUnitOfWork.Get(() => Target.ReadFor(new[] {"phone"}, new Guid?[] {null}))
				.Select(x => x.StateCode).Should().Not.Contain("loggedout");
		}

		[Test]
		public void ShouldReadForActivity()
		{
			var phone = new Activity("Phone");
			WithUnitOfWork.Do(() =>
			{
				Activities.Add(phone);
				Maps.Add(new RtaMap(null, phone));
			});

			WithUnitOfWork.Get(() => Target.ReadFor(new string[] { null }, new Guid?[] { phone.Id.Value }))
				.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldReadFor3Activities()
		{
			var phone = new Activity("Phone");
			var brejk = new Activity("Break");
			var lunch = new Activity("Lunch");
			WithUnitOfWork.Do(() =>
			{
				Activities.Add(phone);
				Activities.Add(brejk);
				Activities.Add(lunch);
				Maps.Add(new RtaMap(null, phone));
				Maps.Add(new RtaMap(null, brejk));
				Maps.Add(new RtaMap(null, lunch));
			});

			var result = WithUnitOfWork.Get(() =>
				Target.ReadFor(new string[] {null}, new Guid?[] {phone.Id.Value, brejk.Id.Value, lunch.Id.Value}));

			result.Select(x => x.ActivityId).Should().Have.SameValuesAs(phone.Id.Value, brejk.Id.Value, lunch.Id.Value);
		}

		[Test]
		public void ShouldExcludeIrrelevantActivity()
		{
			var phone = new Activity("Phone");
			var brejk = new Activity("Break");
			var loggedOut = new RtaStateGroup("loggedOut").AddState("loggedout", Guid.NewGuid());
			WithUnitOfWork.Do(() =>
			{
				Activities.Add(phone);
				Activities.Add(brejk);
				Groups.Add(loggedOut);
				Maps.Add(new RtaMap(null, phone));
				Maps.Add(new RtaMap(loggedOut, brejk));
			});

			var result = WithUnitOfWork.Get(() =>
				Target.ReadFor(new string[] { null }, new Guid?[] { phone.Id.Value }));

			result.Select(x => x.ActivityId).Should().Not.Contain(brejk.Id.Value);
		}

		[Test]
		public void ShouldExcludeIrrelevantStateActivityCombination()
		{
			var phone = new Activity("Phone");
			var brejk = new Activity("Break");
			var phoneState = new RtaStateGroup("Phone").AddState("phone", Guid.NewGuid());
			var loggedOut = new RtaStateGroup("loggedOut").AddState("loggedout", Guid.NewGuid());
			WithUnitOfWork.Do(() =>
			{
				Activities.Add(phone);
				Activities.Add(brejk);
				Groups.Add(phoneState);
				Groups.Add(loggedOut);
				Maps.Add(new RtaMap(phoneState, phone));
				Maps.Add(new RtaMap(loggedOut, brejk));
			});

			var result = WithUnitOfWork.Get(() =>
				Target.ReadFor(new string[] { "phone" }, new Guid?[] { phone.Id.Value }));

			result.Select(x => x.ActivityId).Should().Not.Contain(brejk.Id.Value);
		}

		[Test]
		public void ShouldReadUnmappedState()
		{
			var phone = new Activity("Phone");
			var brejk = new Activity("Break");
			var phoneState = new RtaStateGroup("Phone").AddState("phone", Guid.NewGuid());
			WithUnitOfWork.Do(() =>
			{
				Activities.Add(phone);
				Activities.Add(brejk);
				Groups.Add(phoneState);
				Maps.Add(new RtaMap(phoneState, brejk));
			});

			var result = WithUnitOfWork.Get(() =>
				Target.ReadFor(new string[] { "phone" }, new Guid?[] { phone.Id.Value, null }));

			result.Select(x => x.StateCode).Should().Contain("phone");
		}

		[Test]
		public void ShouldReadUnmappedState2()
		{
			var phone = new Activity("Phone");
			var brejk = new Activity("Break");
			var phoneState = new RtaStateGroup("Phone").AddState("phone", Guid.NewGuid());
			WithUnitOfWork.Do(() =>
			{
				Activities.Add(phone);
				Activities.Add(brejk);
				Groups.Add(phoneState);
				Maps.Add(new RtaMap(phoneState, brejk));
			});

			var result = WithUnitOfWork.Get(() =>
				Target.ReadFor(new string[] { null, "phone" }, new Guid?[] { phone.Id.Value, null }));

			result.Select(x => x.StateCode).Should().Contain("phone");
		}

		[Test]
		public void ShouldReadAllStateActivityCombinations()
		{
			var phone = new Activity("Phone");
			var brejk = new Activity("Break");
			var lunch = new Activity("Lunch");
			var phoneState = new RtaStateGroup("Phone").AddState("phone", Guid.NewGuid());
			var loggedOut = new RtaStateGroup("loggedOut").AddState("loggedout", Guid.NewGuid());
			WithUnitOfWork.Do(() =>
			{
				Activities.Add(phone);
				Activities.Add(brejk);
				Activities.Add(lunch);
				Groups.Add(phoneState);
				Groups.Add(loggedOut);
				Maps.Add(new RtaMap(phoneState, phone));
				Maps.Add(new RtaMap(loggedOut, phone));
				Maps.Add(new RtaMap(phoneState, brejk));
				Maps.Add(new RtaMap(loggedOut, brejk));
				Maps.Add(new RtaMap(phoneState, lunch));
				Maps.Add(new RtaMap(loggedOut, lunch));
			});

			var result = WithUnitOfWork.Get(() =>
				Target.ReadFor(new string[] { "phone", "loggedout" }, new Guid?[] { phone.Id.Value, brejk.Id.Value, lunch.Id.Value }));

			result.Where(x => x.StateCode == "phone" && x.ActivityId == phone.Id.Value).Should().Not.Be.Empty();
			result.Where(x => x.StateCode == "loggedout" && x.ActivityId == phone.Id.Value).Should().Not.Be.Empty();
			result.Where(x => x.StateCode == "phone" && x.ActivityId == brejk.Id.Value).Should().Not.Be.Empty();
			result.Where(x => x.StateCode == "loggedout" && x.ActivityId == brejk.Id.Value).Should().Not.Be.Empty();
			result.Where(x => x.StateCode == "phone" && x.ActivityId == lunch.Id.Value).Should().Not.Be.Empty();
			result.Where(x => x.StateCode == "loggedout" && x.ActivityId == lunch.Id.Value).Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldReadAllStateActivityCombinationsWithNull()
		{
			var phone = new Activity("Phone");
			var brejk = new Activity("Break");
			var lunch = new Activity("Lunch");
			var phoneState = new RtaStateGroup("Phone").AddState("phone", Guid.NewGuid());
			var loggedOut = new RtaStateGroup("loggedOut").AddState("loggedout", Guid.NewGuid());
			WithUnitOfWork.Do(() =>
			{
				Activities.Add(phone);
				Activities.Add(brejk);
				Activities.Add(lunch);
				Groups.Add(phoneState);
				Groups.Add(loggedOut);
				Maps.Add(new RtaMap(phoneState, phone));
				Maps.Add(new RtaMap(null, phone));
				Maps.Add(new RtaMap(phoneState, brejk));
				Maps.Add(new RtaMap(null, brejk));
				Maps.Add(new RtaMap(phoneState, null));
				Maps.Add(new RtaMap(null, null));
			});

			var result = WithUnitOfWork.Get(() =>
				Target.ReadFor(new string[] { null, "phone" }, new Guid?[] { null, phone.Id.Value, brejk.Id.Value}));

			result.Where(x => x.StateCode == "phone" && x.ActivityId == phone.Id.Value).Should().Not.Be.Empty();
			result.Where(x => x.StateCode == null && x.ActivityId == phone.Id.Value).Should().Not.Be.Empty();
			result.Where(x => x.StateCode == "phone" && x.ActivityId == brejk.Id.Value).Should().Not.Be.Empty();
			result.Where(x => x.StateCode == null && x.ActivityId == brejk.Id.Value).Should().Not.Be.Empty();
			result.Where(x => x.StateCode == "phone" && x.ActivityId == null).Should().Not.Be.Empty();
			result.Where(x => x.StateCode == null && x.ActivityId == null).Should().Not.Be.Empty();
		}
	}
}