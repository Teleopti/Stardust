using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer.Payroll;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.PayrollTest
{
	[TestFixture]
	public class PayrollDataExtractorTest
	{
		[Test, DisabledBy(Toggles.Wfm_Payroll_SupportMultiDllPayrolls_75959), Ignore("Wait for bug")]
		public void FixBugTest()
		{
			
			var searchPath = new FakeSearchPath();
			var target = new PayrollDataExtractor(
				new PlugInLoader(new DomainAssemblyResolverOld(new AssemblyFileLoader(searchPath)), searchPath),new ChannelCreator() );
			var feedback = new FakeServiceBusPayrollExportFeedback(new InterAppDomainArguments());

			var proc = new PayrollExport {PayrollFormatId = new Guid("{dbbe8c77-a7c2-4675-89f6-2e5bfc34470c}")};
			var payrollEvent = new RunPayrollExportEvent
			{
				LogOnDatasource = "Telia",
				PayrollExportFormatId = new Guid("{dbbe8c77-a7c2-4675-89f6-2e5bfc34470c}")
			};
			target.Extract(proc, payrollEvent,  new List<PersonDto>(), feedback);

			feedback.PayrollResultDetails.Where(d => d.DetailLevel == DetailLevel.Error).Should().Be.Empty();
		}

		[Test, DisabledBy(Toggles.Wfm_Payroll_SupportMultiDllPayrolls_75959), Ignore("Wait for bug")]
		public void ShouldFindFormatOnPayrollBaseDir()
		{
			var searchPath = new FakeSearchPath();
			var target = new PayrollDataExtractor(
				new PlugInLoader(new DomainAssemblyResolverOld(new AssemblyFileLoader(searchPath)), searchPath), new ChannelCreator());
			var feedback = new FakeServiceBusPayrollExportFeedback(new InterAppDomainArguments());

			var proc = new PayrollExport { PayrollFormatId = new Guid("{0E531434-A463-4AB6-8BF1-4696DDC9B296}") };
			var payrollEvent = new RunPayrollExportEvent
			{
				LogOnDatasource = "TenantNotPresent",
				PayrollExportFormatId = new Guid("{0E531434-A463-4AB6-8BF1-4696DDC9B296}")
			};
			target.Extract(proc, payrollEvent, new List<PersonDto>(), feedback);

			feedback.PayrollResultDetails.Where(d => d.DetailLevel == DetailLevel.Error).Should().Be.Empty();
		}
	}
}
