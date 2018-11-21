using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels;
using Teleopti.Wfm.Adherence.Domain.Configuration;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.States.Infrastructure.ReadModels
{
	[TestFixture]
	[DatabaseTest]
	public class MappingReadModelUpdaterTest
	{
		public Database Database;
		public MappingReadModelUpdater Target;
		public IMappingReader Reader;
		public WithReadModelUnitOfWork ReadModel;
		public WithUnitOfWork UnitOfWork;
		public IRtaStateGroupRepository StateGroups;

		[Test]
		public void ShouldContainNoDuplicates()
		{
			Database
				.WithActivity("phone")
				.WithStateGroup("ready")
				.WithStateCode("ready")
				.WithRule("adhereing", 0, null)
				.WithMapping()
				.WithStateGroup("pause")
				.WithStateCode("pause")
				.WithRule("not adhering", -1, null)
				.WithMapping()
				;

			Target.Handle(new TenantMinuteTickEvent());

			var actual = ReadModel.Get(() => Reader.Read()).Select(x => x.StateCode + x.ActivityId.GetValueOrDefault() + x.BusinessUnitId).ToArray();
			var expected = actual.Distinct().ToArray();
			actual.Should().Have.SameSequenceAs(expected);
		}

		[Test]
		public void ShouldUpdateModelWithCyrillicCharacters()
		{
			Database
				.WithActivity("Телефон")
				.WithStateGroup("Телефон")
				.WithStateCode("Телефон")
				.WithRule("Телефон", 0, null)
				.WithMapping()
				;

			Target.Handle(new TenantMinuteTickEvent());

			var actual = ReadModel.Get(() => Reader.Read());
			actual.Select(x => x.StateCode).Should().Contain("Телефон");
			actual.Select(x => x.StateGroupName).Should().Contain("Телефон");
			actual.Select(x => x.RuleName).Should().Contain("Телефон");
		}

		[Test]
		public void ShouldAddStateCodesWithCyrillicCharacters()
		{
			Database.WithStateGroup("default", true);

			Target.Handle(new UnknownStateCodeReceviedEvent
			{
				BusinessUnitId = ServiceLocatorForEntity.CurrentBusinessUnit.CurrentId().Value,
				StateCode = "Телефон",
				StateDescription = "Телефон"
			});

			var stateGroups = UnitOfWork.Get(() => StateGroups.LoadAllCompleteGraph());
			stateGroups.Single(x => x.DefaultStateGroup).StateCollection.Single().StateCode.Should().Be("Телефон");
			stateGroups.Single(x => x.DefaultStateGroup).StateCollection.Single().Name.Should().Be("Телефон");
		}
	}
}