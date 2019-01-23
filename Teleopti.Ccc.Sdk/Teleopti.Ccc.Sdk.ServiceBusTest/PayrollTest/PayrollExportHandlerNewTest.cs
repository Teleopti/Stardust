using System;
using System.IO;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Payroll;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.PayrollTest
{
	[DomainTest]
	public class PayrollExportHandlerNewTest : IIsolateSystem
	{
		public IHandleEvent<RunPayrollExportEvent> Target;
		public IPayrollResultRepository PayrollResultRepository;
		public IPayrollExportRepository PayrollExportRepository;
		public IBusinessUnitRepository BusinessUnitRepository;
		public ISearchPath SearchPath;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<PayrollExportHandlerNew>().For<IHandleEvent<RunPayrollExportEvent>>();
			isolate.UseTestDouble<FakePersonBusAssembler>().For<IPersonBusAssembler>();
			isolate.UseTestDouble<FakeServiceBusPayrollExportFeedback>().For<IServiceBusPayrollExportFeedback>();
			isolate.UseTestDouble<FakePayrollPeopleLoader>().For<IPayrollPeopleLoader>();
			isolate.UseTestDouble<DomainAssemblyResolverNew>().For<IDomainAssemblyResolver>();
			isolate.UseTestDouble<FakeTenantPeopleLoader>().For<ITenantPeopleLoader>();
			isolate.UseTestDouble<SdkFakeServiceFactory>().For<ISdkServiceFactory>();
			isolate.UseTestDouble<InterAppDomainArguments>().For<InterAppDomainArguments>();
			isolate.UseTestDouble<AssemblyFileLoader>().For<IAssemblyFileLoader>();
			isolate.UseTestDouble<SearchPath>().For<ISearchPath>();
		}

		[Test, Ignore("WIP, works locally but not on Builder sever..")]
		public void CopyPayrollFilesFromSourceToDestinationBeforeExecute()
		{
			var tenantName = "DirectoryEmptyTenant";
			var tenantSpecificPayrollDir = Path.Combine(SearchPath.Path, tenantName);
			if (Directory.Exists(tenantSpecificPayrollDir))
				Directory.Delete(tenantSpecificPayrollDir, true);

			var businessUnit = new BusinessUnit("businessUnitName").WithId();
			BusinessUnitRepository.Add(businessUnit);

			var payrollExport = new FakePayrollExport().WithId();
			payrollExport.PayrollFormatId = new Guid("{0e531434-a463-4ab6-8bf1-4696ddc9b296}");
			payrollExport.SetCreatedBy(PersonFactory.CreatePerson());
			PayrollExportRepository.Add(payrollExport);
			
			var payrollResult = new PayrollResult(payrollExport, new Person(), DateTime.Now).WithId();
			PayrollResultRepository.Add(payrollResult);
			
			var @event = new RunPayrollExportEvent
			{
				LogOnDatasource = tenantName, 
				PayrollExportId = payrollExport.Id.GetValueOrDefault(), 
				PayrollResultId = payrollResult.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault()
			};
			
			Target.Handle(@event);

			Assert.IsTrue(File.Exists(Path.Combine(tenantSpecificPayrollDir, "Teleopti.Ccc.Payroll.dll")));
		}
	}
}