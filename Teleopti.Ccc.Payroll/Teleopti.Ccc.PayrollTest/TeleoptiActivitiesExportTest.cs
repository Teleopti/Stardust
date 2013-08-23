using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Payroll;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.PayrollTest
{
	[TestFixture]
	public class TeleoptiActivitiesExportTest : BaseExportTest
	{
		protected override void ConcreteSetup()
		{
			Target = new TeleoptiActivitiesExport();
		}

		[Test]
		public void BatchingShouldNotHaveIntersection()
		{
			var mocks = new MockRepository();
			var schedulingService = mocks.DynamicMock<ITeleoptiSchedulingService>();

			var payrollExportDto = new PayrollExportDto
			{
				TimeZoneId = "Utc",
				DatePeriod = new DateOnlyPeriodDtoForPayrollTest(
					new DateOnly(2009, 2, 1),
					new DateOnly(2009, 2, 1))
			};
			for (var i = 0; i < 51; i++)
				payrollExportDto.PersonCollection.Add(new PersonDto());

			schedulingService.Expect(s => s.GetTeleoptiActivitiesExportData(null, null, null, null))
							 .IgnoreArguments()
							 .Return(new List<PayrollBaseExportDto>
			                     {
				                     new PayrollBaseExportDto(),
				                     new PayrollBaseExportDto()
			                     });
			mocks.ReplayAll();

			Target.ProcessPayrollData(schedulingService, null, payrollExportDto);
			var argumentsUsed =
				schedulingService.GetArgumentsForCallsMadeOn(s => s.GetTeleoptiActivitiesExportData(null, null, null, null));

			var firstList = (PersonDto[])argumentsUsed[0][0];
			var secondList = (PersonDto[])argumentsUsed[1][0];

			var intersection = firstList.Intersect(secondList);
			intersection.Count().Should().Be.EqualTo(0);
		}

	}
}
