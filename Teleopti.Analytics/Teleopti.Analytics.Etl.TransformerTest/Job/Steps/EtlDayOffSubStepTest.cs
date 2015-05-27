using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerTest.Job.Steps
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
