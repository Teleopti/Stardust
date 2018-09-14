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
		private AppdomainCreatorWrapper target;
		private readonly SearchPath _searchPath = new SearchPath();

		[Test]
		public void VerifyPayrollDtosCanBeLoaded()
		{
			runWithExceptionHandling(() =>
			{
				target = new AppdomainCreatorWrapper();
				var payrollDtos = target.FindPayrollFormatsForTenant("Telia", _searchPath.Path);
				payrollDtos.Count.Should().Be(6);
			});
		}

		[Test]
		public void ExecutePayroll()
		{
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

				target = new AppdomainCreatorWrapper();
				var document = target.RunPayroll(new SdkFakeServiceFactory(), payrollExportDto,new RunPayrollExportEvent(),Guid.NewGuid(), new FakeServiceBusPayrollExportFeedback(), _searchPath.Path);
				document.DocumentElement.ChildNodes.Count.Should().Be(5);
			});
		}

		[Test]
		public void ShouldFindPayrollFilesOnPayrollRootDirIfMissingInTenantSpecificDir()
		{
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

				target = new AppdomainCreatorWrapper();
				var document = target.RunPayroll(new SdkFakeServiceFactory(), payrollExportDto, new RunPayrollExportEvent(), Guid.NewGuid(), new FakeServiceBusPayrollExportFeedback(), _searchPath.Path);
				document.DocumentElement.ChildNodes.Count.Should().Be(5);
			});
		}

		[Test]
		public void ShouldNotCrashIfTenantNotFound()
		{
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

			target = new AppdomainCreatorWrapper();
			var factory = new SdkFakeServiceFactory();
			var feedback = new FakeServiceBusPayrollExportFeedback();
			Assert.DoesNotThrow(() => target.RunPayroll(factory, payrollExportDto, new RunPayrollExportEvent(), Guid.NewGuid(), feedback, _searchPath.Path));
			feedback.ProgressList.Where(i => i.DetailLevel == DetailLevel.Error).Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldNotReturnValuesWhenFileIsLocked()
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
				PayrollFormat = new PayrollFormatDto(new Guid("{dbbe8c77-a7c2-4675-89f6-2e5bfc34470c}"), "Relesay Schedule Export", "Teleopti Path that does not exist")
			};
			for (var i = 0; i < 51; i++)
				payrollExportDto.PersonCollection.Add(new PersonDto());

			// lock specific payroll file
			var fullPayrollPath = Path.Combine(_searchPath.Path, @"Telia\Teleopti.Ccc.Payroll.Customers.ReleasyTeliaSonera.dll");
			var file = File.Open(fullPayrollPath, FileMode.Open);

			target = new AppdomainCreatorWrapper();
			var factory = new SdkFakeServiceFactory();
			var feedback = new FakeServiceBusPayrollExportFeedback();
			var result = target.RunPayroll(factory, payrollExportDto, new RunPayrollExportEvent(), Guid.NewGuid(),
				feedback, _searchPath.Path);
			feedback.ProgressList.Where(i => i.DetailLevel == DetailLevel.Error).Should().Not.Be.Empty();
			result.InnerXml.Should().Be.Empty();
			file.Close();
		}

		[Test]
		public void ShouldHandlePayrollFormatIdNotFound()
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
				PayrollFormat = new PayrollFormatDto(new Guid("{dbbe8c77-a7c2-4675-89f6-2e5bfc34470d}"), "Relesay Schedule Export", "Telia")
			};
			for (var i = 0; i < 51; i++)
				payrollExportDto.PersonCollection.Add(new PersonDto());

			target = new AppdomainCreatorWrapper();
			var factory = new SdkFakeServiceFactory();
			var feedback = new FakeServiceBusPayrollExportFeedback();
			Assert.DoesNotThrow(() => 
				target.RunPayroll(factory, payrollExportDto, new RunPayrollExportEvent(), Guid.NewGuid(),
					feedback, _searchPath.Path));

			feedback.ProgressList.Where(i => i.DetailLevel == DetailLevel.Error).Should().Not.Be.Empty();	
		}

		[Test]
		public void ExecutePayrollAndGetUpdatedFeedback()
		{
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

				target = new AppdomainCreatorWrapper();
				var feedback = new FakeServiceBusPayrollExportFeedback();
				var factory = new SdkFakeServiceFactory();
				feedback.ProgressList.Count.Should().Be.EqualTo(0);
				target.RunPayroll(factory, payrollExportDto, new RunPayrollExportEvent(), Guid.NewGuid(), feedback, _searchPath.Path);
				feedback.ProgressList.Count.Should().Not.Be.EqualTo(0);
			});
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