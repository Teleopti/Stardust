using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence.ApplicationLayer.ViewModels.HistoricalAdherenceViewModelBuilder
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class LateForWorkTest
	{
		public Ccc.Domain.RealTimeAdherence.ApplicationLayer.ViewModels.HistoricalAdherenceViewModelBuilder Target;
		public FakeRtaHistory History;
		public MutableNow Now;

		[Test]
		public void ShouldGetLateForWork()
		{
			Now.Is("2018-05-28 17:00");
			var personId = Guid.NewGuid();
			var name = RandomName.Make();
			History
				.StateChanged(personId, "2018-05-28 10:00")
				.BackFromLateForWork(personId, "2018-05-28 09:00", "2018-05-28 10:00")
				;

			var result = Target.Build(personId).Changes.Single();

			result.LateForWork.Should().Be("Late 60 min");
		}
	}
}