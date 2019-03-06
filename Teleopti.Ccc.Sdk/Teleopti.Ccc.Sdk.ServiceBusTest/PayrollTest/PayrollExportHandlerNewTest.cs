using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Payroll;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

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
		public FakeServiceBusPayrollExportFeedback  ServiceBusPayrollExportFeedback;
		public FakeStardustJobFeedback  FakeStardustJobFeedback;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<PayrollExportHandlerNew>().For<IHandleEvent<RunPayrollExportEvent>>();
			isolate.UseTestDouble<FakePersonBusAssembler>().For<IPersonBusAssembler>();
			isolate.UseTestDouble<FakeServiceBusPayrollExportFeedback>().For<IServiceBusPayrollExportFeedback, FakeServiceBusPayrollExportFeedback>();
			isolate.UseTestDouble<FakeStardustJobFeedback>().For<IStardustJobFeedback, FakeStardustJobFeedback>();
			isolate.UseTestDouble<FakePayrollPeopleLoader>().For<IPayrollPeopleLoader>();
			isolate.UseTestDouble<DomainAssemblyResolverNew>().For<IDomainAssemblyResolver>();
			isolate.UseTestDouble<FakeTenantPeopleLoader>().For<ITenantPeopleLoader>();
			isolate.UseTestDouble<SdkFakeServiceFactory>().For<ISdkServiceFactory>();
			isolate.UseTestDouble<InterAppDomainArguments>().For<InterAppDomainArguments>();
			isolate.UseTestDouble<AssemblyFileLoader>().For<IAssemblyFileLoader>();
			isolate.UseTestDouble<SearchPath>().For<ISearchPath>();
		}

		[Test]
		public void CopyPayrollFilesFromSourceToDestinationBeforeExecute()
		{
			var existingPath = AppDomain.CurrentDomain.BaseDirectory;
			try
			{
				AppDomain.CurrentDomain.SetData("APPBASE",
					Assembly.GetAssembly(GetType()).Location.Replace("\\Teleopti.Ccc.Sdk.ServiceBusTest.dll", ""));
				const string tenantName = "DirectoryEmptyTenant";
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
					PayrollExportFormatId = payrollExport.PayrollFormatId,
					PayrollExportId = payrollExport.Id.GetValueOrDefault(),
					PayrollResultId = payrollResult.Id.GetValueOrDefault(),
					LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault()
				};

				Target.Handle(@event);

				ServiceBusPayrollExportFeedback.PayrollResultDetails.Where(f => f.DetailLevel == DetailLevel.Error)
					.Should().Be.Empty();
				ServiceBusPayrollExportFeedback.PayrollResultDetails.ForEach(d =>
					Console.WriteLine($"{d.Message} Exception message:{d.Exception?.Message}"));
				Assert.IsTrue(File.Exists(Path.Combine(tenantSpecificPayrollDir, "Teleopti.Ccc.Payroll.dll")));
			}
			finally
			{
				AppDomain.CurrentDomain.SetData("APPBASE", existingPath);
			}
		}

		[Test]
		public void ShouldNotCreateTenantDirectoryThatIsNotInDeployNew()
		{
			var existingPath = AppDomain.CurrentDomain.BaseDirectory;
			try
			{
				AppDomain.CurrentDomain.SetData("APPBASE",
					Assembly.GetAssembly(GetType()).Location.Replace("\\Teleopti.Ccc.Sdk.ServiceBusTest.dll", ""));
				const string tenantName = "TenantWithoutPayrollDirectory";
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
					PayrollExportFormatId = payrollExport.PayrollFormatId,
					PayrollExportId = payrollExport.Id.GetValueOrDefault(),
					PayrollResultId = payrollResult.Id.GetValueOrDefault(),
					LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault()
				};

				Target.Handle(@event);

				ServiceBusPayrollExportFeedback.PayrollResultDetails.ForEach(d =>
					Console.WriteLine($"{d.Message} Exception message:{d.Exception?.Message}"));
				Assert.IsFalse(Directory.Exists(tenantSpecificPayrollDir));
				Assert.False(File.Exists(Path.Combine(tenantSpecificPayrollDir, "Teleopti.Ccc.Payroll.dll")));
			}
			finally
			{
				AppDomain.CurrentDomain.SetData("APPBASE", existingPath);	
			}
		}

		[Test]
		public void ShouldLogToBothStardustFeedbackAndServiceBusFeedback()
		{
			var existingPath = AppDomain.CurrentDomain.BaseDirectory;
			try
			{
				AppDomain.CurrentDomain.SetData("APPBASE",
					Assembly.GetAssembly(GetType()).Location.Replace("\\Teleopti.Ccc.Sdk.ServiceBusTest.dll", ""));
				const string tenantName = "DirectoryEmptyTenant";
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
					PayrollExportFormatId = payrollExport.PayrollFormatId,
					PayrollExportId = payrollExport.Id.GetValueOrDefault(),
					PayrollResultId = payrollResult.Id.GetValueOrDefault(),
					LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault()
				};

				Target.Handle(@event);

				ServiceBusPayrollExportFeedback.PayrollResultDetails.Where(f => f.DetailLevel == DetailLevel.Error)
					.Should().Be.Empty();
				ServiceBusPayrollExportFeedback.PayrollResultDetails.ForEach(d =>
					Console.WriteLine($"{d.Message} Exception message:{d.Exception?.Message}"));
				Assert.IsTrue(File.Exists(Path.Combine(tenantSpecificPayrollDir, "Teleopti.Ccc.Payroll.dll")));
				FakeStardustJobFeedback.FeedbackList.Where(s => s.StartsWith("Info:")).Should().Not.Be.Empty();
				ServiceBusPayrollExportFeedback.PayrollResultDetails.Where(d => d.DetailLevel == DetailLevel.Info)
					.Should().Not.Be.Empty();
			}
			finally
			{
				AppDomain.CurrentDomain.SetData("APPBASE", existingPath);
			}
		}
		
		[Test]
		public void ShouldLogErrorToStardustFeedback()
		{
			var existingPath = AppDomain.CurrentDomain.BaseDirectory;
			try
			{
				AppDomain.CurrentDomain.SetData("APPBASE",
					Assembly.GetAssembly(GetType()).Location.Replace("\\Teleopti.Ccc.Sdk.ServiceBusTest.dll", ""));
				const string tenantName = "NonExistingTenant";
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
					PayrollExportFormatId = payrollExport.PayrollFormatId,
					PayrollExportId = payrollExport.Id.GetValueOrDefault(),
					PayrollResultId = payrollResult.Id.GetValueOrDefault(),
					LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault()
				};

				// lock specific payroll file
				var fullPayrollPath = Path.Combine(SearchPath.Path, @"Teleopti.Ccc.Payroll.dll");
				var file = File.Open(fullPayrollPath, FileMode.Open);
				Target.Handle(@event);
				file.Close();
				
				ServiceBusPayrollExportFeedback.PayrollResultDetails.Where(f => f.DetailLevel == DetailLevel.Error)
					.Should().Not.Be.Empty();
				ServiceBusPayrollExportFeedback.PayrollResultDetails.ForEach(d =>
					Console.WriteLine($"{d.Message} Exception message:{d.Exception?.Message}"));
				FakeStardustJobFeedback.FeedbackList.Where(s => s.StartsWith("Error:")).Should().Not.Be.Empty();
			}
			finally
			{
				AppDomain.CurrentDomain.SetData("APPBASE", existingPath);
			}
			
		}
	}
}