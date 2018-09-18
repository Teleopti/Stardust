using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.PayrollTest
{
	[TestFixture, Ignore("")]
	public class RefreshPayrollFormatsHandlerTest
	{

		[OneTimeSetUp]
		public void SetupFixture()
		{
			copyFiles(Path.Combine(TestContext.CurrentContext.TestDirectory, "Payroll.DeployNew"),
				Path.Combine(TestContext.CurrentContext.TestDirectory, "Payroll.DeployNew"), "TestTenant");
		}
		
		[Test,Ignore("")]
		public void CopyPayrollFilesFromSourceToDestination()
		{
			var tenantName = "TestTenant";
			var ds = new DataSource(UnitOfWorkFactoryFactoryForTest.CreateUnitOfWorkFactory(tenantName), null, null);
			var dataSourceForTenant = new FakeDataSourceForTenant(null, null, new FindTenantByNameWithEnsuredTransactionFake());
			dataSourceForTenant.Has(ds);

			var payrollFormatRepositoryFactory = new FakePayrollFormatRepositoryFactory();
		
			var searchPath = new FakeSearchPath();
			var tenantDestinationPath = Path.Combine(searchPath.Path, tenantName);
			if(Directory.Exists(tenantDestinationPath))
			{
				foreach (var file in Directory.GetFiles(tenantDestinationPath))
				{
					File.Delete(file);
				}

				Directory.Delete(tenantDestinationPath);
			}

			var serverConfigurationRepository = new FakeServerConfigurationRepository();
			serverConfigurationRepository.Configuration.Add("PayrollSourcePath", 
				Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Payroll.DeployNew"));
			var initializePayrollFormatsUsingAppDomain = 
				new InitializePayrollFormatsUsingAppDomain(dataSourceForTenant, payrollFormatRepositoryFactory);
			var refreshPayrollFormatsHandler = 
				new RefreshPayrollFormatsHandler(searchPath, new FakeConfigReader("",""), initializePayrollFormatsUsingAppDomain,
					new TenantUnitOfWorkFake(), serverConfigurationRepository);

			var payrollEvent = new RefreshPayrollFormatsEvent {TenantName = tenantName };
			IEnumerable<object> objectList = null;
			refreshPayrollFormatsHandler.Handle(payrollEvent, new CancellationTokenSource(0),s =>{} , ref objectList);
			payrollFormatRepositoryFactory.CurrentPayrollFormatRepository.LoadAll().Should().Not.Be.Empty();
			Directory.Exists(tenantDestinationPath).Should().Be.True();
			Directory.GetFiles(tenantDestinationPath).Should().Not.Be.Empty();
		}


		[Test]
		public void CopyPayrollFilesFromSourceToDestinationShouldUseDefaultPathIfNotDefined()
		{
			var tenantName = "TestTenant";
			var ds = new DataSource(UnitOfWorkFactoryFactoryForTest.CreateUnitOfWorkFactory(tenantName), null, null);
			var dataSourceForTenant = new FakeDataSourceForTenant(null, null, new FindTenantByNameWithEnsuredTransactionFake());
			dataSourceForTenant.Has(ds);

			var payrollFormatRepositoryFactory = new FakePayrollFormatRepositoryFactory();

			var searchPath = new SearchPath();
			var tenantDestinationPath = Path.Combine(searchPath.Path, tenantName);
			if (Directory.Exists(tenantDestinationPath))
			{
				foreach (var file in Directory.GetFiles(tenantDestinationPath))
				{
					File.Delete(file);
				}

				Directory.Delete(tenantDestinationPath);
			}

			var serverConfigurationRepository = new FakeServerConfigurationRepository();
			//serverConfigurationRepository.Configuration.Add("PayrollSourcePath",
			//	Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Payroll.DeployNew"));
			var initializePayrollFormatsUsingAppDomain =
				new InitializePayrollFormatsUsingAppDomain(dataSourceForTenant, payrollFormatRepositoryFactory);
			var refreshPayrollFormatsHandler =
				new RefreshPayrollFormatsHandler(searchPath, new FakeConfigReader("", ""), initializePayrollFormatsUsingAppDomain,
					new TenantUnitOfWorkFake(), serverConfigurationRepository);

			var payrollEvent = new RefreshPayrollFormatsEvent { TenantName = tenantName };
			IEnumerable<object> objectList = null;
			refreshPayrollFormatsHandler.Handle(payrollEvent, new CancellationTokenSource(0), s => { }, ref objectList);
			payrollFormatRepositoryFactory.CurrentPayrollFormatRepository.LoadAll().Should().Not.Be.Empty();
			Directory.Exists(tenantDestinationPath).Should().Be.True();
			Directory.GetFiles(tenantDestinationPath).Should().Not.Be.Empty();
		}

		private static void copyFiles(string sourcePath, string destinationPath,
			string subdirectoryPath)
		{
			var fullSourcePath = Path.Combine(sourcePath, subdirectoryPath);
			var fullDestinationPath = Path.Combine(destinationPath, subdirectoryPath);

			if (Path.GetFullPath(fullSourcePath) == Path.GetFullPath(fullDestinationPath))
				return;

			if (!Directory.Exists(fullDestinationPath))
				Directory.CreateDirectory(fullDestinationPath);

			foreach (var sourceFile in Directory.GetFiles(fullSourcePath))
			{
				var fullDestinationFilename = Path.Combine(fullDestinationPath, Path.GetFileName(sourceFile));
				File.Copy(sourceFile, fullDestinationFilename, true);
			}
		}
	}
}
