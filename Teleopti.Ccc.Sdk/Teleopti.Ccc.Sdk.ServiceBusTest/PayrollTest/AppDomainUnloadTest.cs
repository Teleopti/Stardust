using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Payroll;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.PayrollTest
{
	[TestFixture]
	public class AppDomainUnloadTest
	{
		private readonly SearchPath _searchPath = new SearchPath();

		[OneTimeSetUp]
		public void SetupFixture()
		{
			//copyFiles(Path.Combine(TestContext.CurrentContext.TestDirectory, "Payroll"),
			//	Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Payroll"), "Telia");
			//AppDomain.CurrentDomain.SetData("APPBASE", @"C:\Git\TeleoptiWFM\Teleopti.Ccc.Sdk\Teleopti.Ccc.Sdk.ServiceBusTest\bin\Debug");
		}

		//private static void copyFiles(string sourcePath, string destinationPath,
		//	string subdirectoryPath)
		//{
		//	var fullSourcePath = Path.Combine(sourcePath, subdirectoryPath);
		//	var fullDestinationPath = Path.Combine(destinationPath, subdirectoryPath);

		//	if (Path.GetFullPath(fullSourcePath) == Path.GetFullPath(fullDestinationPath))
		//		return;

		//	if (!Directory.Exists(fullDestinationPath))
		//		Directory.CreateDirectory(fullDestinationPath);

		//	foreach (var sourceFile in Directory.GetFiles(fullSourcePath))
		//	{
		//		var fullDestinationFilename = Path.Combine(fullDestinationPath, Path.GetFileName(sourceFile));
		//		File.Copy(sourceFile, fullDestinationFilename, true);
		//	}
		//}

		[Test]
		public void VerifyPayrollDtosCanBeLoaded()
		{
			var existingPath = AppDomain.CurrentDomain.BaseDirectory;
			AppDomain.CurrentDomain.SetData("APPBASE", Assembly.GetAssembly(GetType()).Location.Replace("\\Teleopti.Ccc.Sdk.ServiceBusTest.dll", ""));
			runWithExceptionHandling(() =>
			{
				var target = new AppdomainCreatorWrapper();
				var payrollDtos = target.FindPayrollFormatsForTenant("Telia", _searchPath.Path);
				payrollDtos.Count.Should().Be(6);
			});
			AppDomain.CurrentDomain.SetData("APPBASE", existingPath);
		}

		[Test, Ignore("")]
		public void ExecutePayroll()
		{
			var existingPath = AppDomain.CurrentDomain.BaseDirectory;
			AppDomain.CurrentDomain.SetData("APPBASE", Assembly.GetAssembly(GetType()).Location.Replace("\\Teleopti.Ccc.Sdk.ServiceBusTest.dll", ""));
			runWithExceptionHandling(() =>
			{
				var payrollExportDto = new PayrollExportDto
				{
					TimeZoneId = "Utc",
					DatePeriod = new DateOnlyPeriodDto
					{
						StartDate = new DateOnlyDto(2009, 2, 1),
						EndDate = new DateOnlyDto(2009, 2, 1)
						
					},
					Name = "Telia",
					PayrollFormat = new PayrollFormatDto(new Guid("{dbbe8c77-a7c2-4675-89f6-2e5bfc34470c}"), "Name", "Telia")
				};
				for (var i = 0; i < 51; i++)
					payrollExportDto.PersonCollection.Add(new PersonDto());

				var target = new AppdomainCreatorWrapper();
				var document = target.RunPayroll(
					new SdkFakeServiceFactory(), payrollExportDto, new RunPayrollExportEvent(),
					Guid.NewGuid(), new FakeServiceBusPayrollExportFeedback(), 
					_searchPath.Path);
				document.DocumentElement.ChildNodes.Count.Should().Be(5);
			});
			AppDomain.CurrentDomain.SetData("APPBASE", existingPath);
		}

		[Test, Ignore("")]
		public void ShouldFindPayrollFilesOnPayrollRootDirIfMissingInTenantSpecificDir()
		{
			var existingPath = AppDomain.CurrentDomain.BaseDirectory;
			AppDomain.CurrentDomain.SetData("APPBASE", Assembly.GetAssembly(GetType()).Location.Replace("\\Teleopti.Ccc.Sdk.ServiceBusTest.dll", ""));
			runWithExceptionHandling(() =>
			{
				var payrollExportDto = new PayrollExportDto
				{
					TimeZoneId = "Utc",
					DatePeriod = new DateOnlyPeriodDto
					{
						StartDate = new DateOnlyDto(2009, 2, 1),
						EndDate = new DateOnlyDto(2009, 2, 1)

					},
					Name = "NewTenant",
					PayrollFormat = new PayrollFormatDto(new Guid("{0e531434-a463-4ab6-8bf1-4696ddc9b296}"), "Name", "NewTenant")
				};
				for (var i = 0; i < 51; i++)
					payrollExportDto.PersonCollection.Add(new PersonDto());

				var target = new AppdomainCreatorWrapper();
				var document = target.RunPayroll(new SdkFakeServiceFactory(), payrollExportDto, new RunPayrollExportEvent(), Guid.NewGuid(), new FakeServiceBusPayrollExportFeedback(), _searchPath.Path);
				document.DocumentElement.ChildNodes.Count.Should().Be(5);
			});
			AppDomain.CurrentDomain.SetData("APPBASE", existingPath);
		}

		[Test]
		public void ShouldNotCrashIfTenantNotFound()
		{
			var existingPath = AppDomain.CurrentDomain.BaseDirectory;
			AppDomain.CurrentDomain.SetData("APPBASE", Assembly.GetAssembly(GetType()).Location.Replace("\\Teleopti.Ccc.Sdk.ServiceBusTest.dll", ""));
			var payrollExportDto = new PayrollExportDto
			{
				TimeZoneId = "Utc",
				DatePeriod = new DateOnlyPeriodDto
				{
					StartDate = new DateOnlyDto(2009, 2, 1),
					EndDate = new DateOnlyDto(2009, 2, 1)
				},
				Name = "OtherTenant",
				PayrollFormat = new PayrollFormatDto(new Guid("{dbbe8c77-a7c2-4675-89f6-2e5bfc34470c}"), "Name", "OtherTenant")
			};
			for (var i = 0; i < 51; i++)
				payrollExportDto.PersonCollection.Add(new PersonDto());

			var target = new AppdomainCreatorWrapper();
			var factory = new SdkFakeServiceFactory();
			var feedback = new FakeServiceBusPayrollExportFeedback();
			Assert.DoesNotThrow(() => target.RunPayroll(factory, payrollExportDto, new RunPayrollExportEvent(), Guid.NewGuid(), feedback, _searchPath.Path));
			feedback.ProgressList.Where(i => i.DetailLevel == DetailLevel.Error).Should().Not.Be.Empty();
			AppDomain.CurrentDomain.SetData("APPBASE", existingPath);
		}

		[Test]
		public void ShouldNotReturnValuesWhenFileIsLocked()
		{
			var existingPath = AppDomain.CurrentDomain.BaseDirectory;
			AppDomain.CurrentDomain.SetData("APPBASE", Assembly.GetAssembly(GetType()).Location.Replace("\\Teleopti.Ccc.Sdk.ServiceBusTest.dll", ""));
			var payrollExportDto = new PayrollExportDto
			{
				TimeZoneId = "Utc",
				DatePeriod = new DateOnlyPeriodDto
				{
					StartDate = new DateOnlyDto(2009, 2, 1),
					EndDate = new DateOnlyDto(2009, 2, 1)
				},
				Name = "Telia",
				PayrollFormat = new PayrollFormatDto(new Guid("{dbbe8c77-a7c2-4675-89f6-2e5bfc34470c}"), "Relesay Schedule Export", "Teleopti Path that does not exist")
			};
			for (var i = 0; i < 51; i++)
				payrollExportDto.PersonCollection.Add(new PersonDto());

			// lock specific payroll file
			var fullPayrollPath = Path.Combine(_searchPath.Path, @"Telia\Teleopti.Ccc.Payroll.Customers.ReleasyTeliaSonera.dll");
			var file = File.Open(fullPayrollPath, FileMode.Open);

			var target = new AppdomainCreatorWrapper();
			var factory = new SdkFakeServiceFactory();
			var feedback = new FakeServiceBusPayrollExportFeedback();
			var result = target.RunPayroll(factory, payrollExportDto, new RunPayrollExportEvent(), Guid.NewGuid(),
				feedback, _searchPath.Path);
			feedback.ProgressList.Where(i => i.DetailLevel == DetailLevel.Error).Should().Not.Be.Empty();
			result.InnerXml.Should().Be.Empty();
			file.Close();
			AppDomain.CurrentDomain.SetData("APPBASE", existingPath);
		}

		[Test]
		public void ShouldHandlePayrollFormatIdNotFound()
		{
			var existingPath = AppDomain.CurrentDomain.BaseDirectory;
			AppDomain.CurrentDomain.SetData("APPBASE", Assembly.GetAssembly(GetType()).Location.Replace("\\Teleopti.Ccc.Sdk.ServiceBusTest.dll", ""));
			var payrollExportDto = new PayrollExportDto
			{
				TimeZoneId = "Utc",
				DatePeriod = new DateOnlyPeriodDto
				{
					StartDate = new DateOnlyDto(2009, 2, 1),
					EndDate = new DateOnlyDto(2009, 2, 1)
				},
				Name = "Telia",
				PayrollFormat = new PayrollFormatDto(new Guid("{dbbe8c77-a7c2-4675-89f6-2e5bfc34470d}"), "Relesay Schedule Export", "Telia")
			};
			for (var i = 0; i < 51; i++)
				payrollExportDto.PersonCollection.Add(new PersonDto());

			var target = new AppdomainCreatorWrapper();
			var factory = new SdkFakeServiceFactory();
			var feedback = new FakeServiceBusPayrollExportFeedback();
			Assert.DoesNotThrow(() =>
				target.RunPayroll(factory, payrollExportDto, new RunPayrollExportEvent(), Guid.NewGuid(),
					feedback, _searchPath.Path));

			feedback.ProgressList.Where(i => i.DetailLevel == DetailLevel.Error).Should().Not.Be.Empty();
			AppDomain.CurrentDomain.SetData("APPBASE", existingPath);
		}

		[Test]
		public void ExecutePayrollAndGetUpdatedFeedback()
		{
			var existingPath = AppDomain.CurrentDomain.BaseDirectory;
			AppDomain.CurrentDomain.SetData("APPBASE", Assembly.GetAssembly(GetType()).Location.Replace("\\Teleopti.Ccc.Sdk.ServiceBusTest.dll", ""));
			runWithExceptionHandling(() =>
			{
				var payrollExportDto = new PayrollExportDto
				{
					TimeZoneId = "Utc",
					DatePeriod = new DateOnlyPeriodDto
					{
						StartDate = new DateOnlyDto(2009, 2, 1),
						EndDate = new DateOnlyDto(2009, 2, 1)
					},
					Name = "Telia",
					PayrollFormat = new PayrollFormatDto(new Guid("{dbbe8c77-a7c2-4675-89f6-2e5bfc34470c}"), "Relesay Schedule Export", "Telia")
				};
				for (var i = 0; i < 51; i++)
					payrollExportDto.PersonCollection.Add(new PersonDto());

				var target = new AppdomainCreatorWrapper();
				var feedback = new FakeServiceBusPayrollExportFeedback();
				var factory = new SdkFakeServiceFactory();
				feedback.ProgressList.Count.Should().Be.EqualTo(0);
				target.RunPayroll(factory, payrollExportDto, new RunPayrollExportEvent(), Guid.NewGuid(), feedback, _searchPath.Path);
				feedback.ProgressList.Count.Should().Not.Be.EqualTo(0);
			});
			AppDomain.CurrentDomain.SetData("APPBASE", existingPath);
		}

		private static void runWithExceptionHandling(Action action)
		{
			try
			{
				action();
			}
			catch (ReflectionTypeLoadException e)
			{
				Assert.Fail($"Failed to load payroll with messages (probably due to missing assembly binding redirect): {string.Join("\n", e.LoaderExceptions.Select(x => x.Message))}");
			}
		}
	}
}