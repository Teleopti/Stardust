using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job.Steps
{
	[TestFixture]
	public class EtlDayOffSubStepTest
	{
		[Test]
		public void ShouldFillStageFromScheduledDayOffsAndThenFillMartWithDayOff()
		{
			var repository = MockRepository.GenerateMock<IRaptorRepository>();
			IBusinessUnit businessUnit = new BusinessUnit("bu");

			repository.Expect(x => x.PersistDayOffFromScheduleDayOffCount()).Return(1);
			repository.Expect(x => x.FillDayOffDataMart(businessUnit)).Return(2);

			var target = new EtlDayOffSubStep();

			int result = target.StageAndPersistToMart(DayOffEtlLoadSource.ScheduleDayOff, businessUnit, repository);
			result.Should().Be.EqualTo(3);
		}
	}
}
