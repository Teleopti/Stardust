using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Payroll;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.TestCommon;


namespace Teleopti.Wfm.Stardust.IntegrationTest.Stardust
{
	[StardustTest]
	public class PayrollEndToEndTest
	{
		public IStardustSender StardustSender;
		public IConfigReader ConfigReader;
		public ICurrentBusinessUnit CurrentBusinessUnit;
		public IPersonRepository PersonRepository;
		public WithUnitOfWork WithUnitOfWork;
		public IPayrollExportRepository PayrollExportRepository;
		public IPayrollFormatRepository PayrollFormatRepository;
		public IPayrollResultRepository PayrollResultRepository;
		public TestLog TestLog;

		private AssertRetryStrategy _assertRetryStrategy;

		[Test]
		public void ShouldPublishAndProcessPayrollJob()
		{
			_assertRetryStrategy = new AssertRetryStrategy(100);

			TestLog.Debug("Starting the test for payroll");
			var period = new DateOnlyPeriod(2016, 02, 20, 2016, 02, 28);

			StardustManagerPingHelper.WaitForStarDustManagerToStart(TestLog);
			StardustSender.Send(dataSetup(period));
			TestLog.Debug("Sent job to star dust");

			performLevel1Assert(period);
			_assertRetryStrategy.Reset();

			TestLog.Debug("Performed level1 assertion");
			var host = new ServiceBusRunner(i => { }, ConfigReader);
			host.Start();
			Thread.Sleep(2000);

			TestLog.Debug("Started stardust");

			performLevel2Assert();
			_assertRetryStrategy.Reset();

			TestLog.Debug("performed level2 assertion");

			TestLog.Debug("Test Finished");
			host.Stop();
		}

		private void performLevel2Assert()
		{
			var connectionString = InfraTestConfigReader.ConnectionString;
			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (var command = new SqlCommand("select Ended,result from Stardust.Job", connection))
				{
					while (_assertRetryStrategy.TryAgain())
					{
						using (var reader = command.ExecuteReader())
						{
							if (reader.HasRows)
							{
								reader.Read();
								if (!reader.IsDBNull(0))
								{
									DateTime? jobEndedDate = reader.GetDateTime(0);
									var result = reader.GetString(1);
									jobEndedDate.HasValue.Should().Be.True();
									result.Should().Be.EqualTo("Success");
									break;
								}
							}

						}
						Thread.Sleep(1500);
					}
					if (!_assertRetryStrategy.WithinRetryStrategy())
						Assert.Fail("Unable to perform Tier 2 Assertion. Exceeded the maximum number of retries.");

				}
			}
		}

		private RunPayrollExportEvent dataSetup(DateOnlyPeriod period)
		{
			RunPayrollExportEvent message = null;

			WithUnitOfWork.Do(() =>
			{
				PayrollFormatRepository.Add(new PayrollFormat { Name = "Teleopti", FormatId = Guid.NewGuid() });
			});

			WithUnitOfWork.Do(() =>
			{
				PayrollExportRepository.Add(createAggregateWithCorrectBusinessUnit(period));
			});
			IPayrollExport payrollExport = null;
			IPerson person = null;
			WithUnitOfWork.Do(() =>
			{
				person = PersonRepository.LoadAll().FirstOrDefault();
				payrollExport = PayrollExportRepository.LoadAll().FirstOrDefault();
				var payrollResult = new PayrollResult(payrollExport, person, DateTime.UtcNow);
				payrollResult.PayrollExport = payrollExport;
				PayrollResultRepository.Add(payrollResult);

			});
			WithUnitOfWork.Do(() =>
			{
				var payrollResult = PayrollResultRepository.LoadAll().FirstOrDefault();

				message = new RunPayrollExportEvent
				{
					PayrollExportId = payrollExport.Id.GetValueOrDefault(Guid.Empty),
					ExportStartDate = payrollExport.Period.StartDate.Date,
					ExportEndDate = payrollExport.Period.EndDate.Date,
					PayrollExportFormatId = payrollExport.PayrollFormatId,
					PayrollResultId = payrollResult.Id.GetValueOrDefault(),
					LogOnBusinessUnitId = CurrentBusinessUnit.CurrentId().GetValueOrDefault()
				};

			});
			return message;
		}

		private  IPayrollExport createAggregateWithCorrectBusinessUnit(DateOnlyPeriod period)
		{
			IPayrollExport payrollExport = new PayrollExport();
			payrollExport.FileFormat = ExportFormat.CommaSeparated;
			payrollExport.Name = "TestPE";
			payrollExport.Period = period;
			var payrollFormat = PayrollFormatRepository.LoadAll().FirstOrDefault();
			payrollExport.PayrollFormatId = payrollFormat.Id.GetValueOrDefault();
			payrollExport.PayrollFormatName = payrollFormat.Name;
			

			IList<IPerson> persons = PersonRepository.LoadAll().ToList();
			payrollExport.ClearPersons();
			payrollExport.AddPersons(persons);
			return payrollExport;
		}

		private void performLevel1Assert(DateOnlyPeriod period)
		{
			var connectionString = InfraTestConfigReader.ConnectionString;
			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (var command = new SqlCommand("select serialized,type from Stardust.JobQueue", connection))
				{
					while (_assertRetryStrategy.TryAgain())
					{
						using (var reader = command.ExecuteReader())
						{
							if (reader.HasRows)
							{
								reader.Read();
								var jsonData = reader.GetString(0);
								var jobType = reader.GetString(1);
								RunPayrollExportEvent storedEvent = JsonConvert.DeserializeObject<RunPayrollExportEvent>(jsonData);
								storedEvent.LogOnDatasource.Should().Be.EqualTo("TestData");
								storedEvent.LogOnBusinessUnitId.Should().Be.EqualTo(CurrentBusinessUnit.CurrentId().GetValueOrDefault());
								storedEvent.ExportEndDate.Should().Be.EqualTo(period.EndDate.Date);
								storedEvent.ExportStartDate.Should().Be.EqualTo(period.StartDate.Date);

								jobType.Should().Be.EqualTo("Teleopti.Ccc.Domain.ApplicationLayer.Payroll.RunPayrollExportEvent");

								break;
							}

						}
						Thread.Sleep(1000);
					}
					if (!_assertRetryStrategy.WithinRetryStrategy())
						Assert.Fail("Unable to perform Tier 1 Assertion. Exceeded the maximum number of retries.");
				}
			}
		}


	}
}
