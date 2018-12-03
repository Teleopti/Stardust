using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.ViewModels.HistoricalAdherenceViewModelBuilder
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class LateForWorkTest
	{
		public Adherence.ApplicationLayer.ViewModels.HistoricalAdherenceViewModelBuilder Target;
		public FakeRtaHistory History;
		public MutableNow Now;

		[Test]
		public void ShouldGetLateForWork()
		{
			Now.Is("2018-05-28 17:00");
			var personId = Guid.NewGuid();
			History
				.StateChanged(personId, "2018-05-28 10:00")
				.ArrivedLateForWork(personId, "2018-05-28 09:00", "2018-05-28 10:00")
				;

			var result = Target.Build(personId).Changes.Single();

			result.LateForWorkMinutes.Should().Be(60);
		}
	}
}