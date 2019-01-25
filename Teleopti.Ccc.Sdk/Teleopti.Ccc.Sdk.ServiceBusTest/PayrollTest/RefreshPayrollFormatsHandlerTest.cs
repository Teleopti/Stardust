using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
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
	[TestFixture]
	public class RefreshPayrollFormatsHandlerTest
	{
		[Test]
		public void CopyPayrollFilesFromSourceToDestination()
		{
			var existingPath = AppDomain.CurrentDomain.BaseDirectory;
			AppDomain.CurrentDomain.SetData("APPBASE", Assembly.GetAssembly(GetType()).Location.Replace("\\Teleopti.Ccc.Sdk.ServiceBusTest.dll", ""));
			var tenantName = "TestTenant";
			var ds = new DataSource(UnitOfWorkFactoryFactoryForTest.CreateUnitOfWorkFactory(tenantName), null, null);
			var dataSourceForTenant = new FakeDataSourceForTenant(null, null, new FindTenantByNameWithEnsuredTransactionFake());
			dataSourceForTenant.Has(ds);

			var payrollFormatRepositoryFactory = new FakePayrollFormatRepositoryFactory();

			var searchPath = new FakeSearchPath();
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
			serverConfigurationRepository.Configuration.Add("PayrollSourcePath",
				Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Payroll.DeployNew"));
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
			AppDomain.CurrentDomain.SetData("APPBASE", existingPath);
		}


		[Test]
		public void CopyPayrollFilesFromSourceToDestinationShouldUseDefaultPathIfNotDefined()
		{
			var existingPath = AppDomain.CurrentDomain.BaseDirectory;
			AppDomain.CurrentDomain.SetData("APPBASE", Assembly.GetAssembly(GetType()).Location.Replace("\\Teleopti.Ccc.Sdk.ServiceBusTest.dll", ""));
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
			AppDomain.CurrentDomain.SetData("APPBASE", existingPath);
		}

		[Test]
		public void ShouldNotUsePayrollFilesWithExtraCharsAfterDll()
		{
			var existingPath = AppDomain.CurrentDomain.BaseDirectory;
			AppDomain.CurrentDomain.SetData("APPBASE", Assembly.GetAssembly(GetType()).Location.Replace("\\Teleopti.Ccc.Sdk.ServiceBusTest.dll", ""));
			var tenantName = "TestBrokenExtension";
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
			var initializePayrollFormatsUsingAppDomain =
				new InitializePayrollFormatsUsingAppDomain(dataSourceForTenant, payrollFormatRepositoryFactory);
			var refreshPayrollFormatsHandler =
				new RefreshPayrollFormatsHandler(searchPath, new FakeConfigReader("", ""), initializePayrollFormatsUsingAppDomain,
					new TenantUnitOfWorkFake(), serverConfigurationRepository);

			var payrollEvent = new RefreshPayrollFormatsEvent { TenantName = tenantName };
			IEnumerable<object> objectList = null;
			refreshPayrollFormatsHandler.Handle(payrollEvent, new CancellationTokenSource(0), s => { }, ref objectList);
			var payrollFormats = payrollFormatRepositoryFactory.CurrentPayrollFormatRepository.LoadAll();
			payrollFormats.Count().Should().Be(3);
			AppDomain.CurrentDomain.SetData("APPBASE", existingPath);
		}
	}
}
