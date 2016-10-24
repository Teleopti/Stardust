using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
{
	[TestFixture]
	[DatabaseTest]
	public class MappingReadModelUpdaterTest
	{
		public Database Database;
		public MappingReadModelUpdater Target;
		public IMappingReader Reader;
		public WithReadModelUnitOfWork UnitOfWork;

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

			var actual = UnitOfWork.Get(() => Reader.Read()).Select(x => x.StateCode + x.ActivityId.GetValueOrDefault() + x.BusinessUnitId).ToArray();
			var expected = actual.Distinct().ToArray();
			actual.Should().Have.SameSequenceAs(expected);
		}
	}
}