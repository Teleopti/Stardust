using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.TestCommon;


namespace Teleopti.Wfm.Stardust.IntegrationTest.Stardust
{
	[StardustTest, Ignore("Flaky test again")]
	public class AbsenceRequestEndToEndTest
	{
		public WithUnitOfWork WithUnitOfWork;
		public IPersonRepository PersonRepository;
		public IAbsenceRepository AbsenceRepository;
		public IPersonRequestRepository PersonRequestRepository;
		public MutableNow Now;
		public IQueuedAbsenceRequestRepository QueuedAbsenceRequestRepository;
		public IEventPublisher EventPublisher;
		public IConfigReader ConfigReader;
		public ICurrentUnitOfWork CurrentUnitOfWork;
		public ICurrentBusinessUnit CurrentBusinessUnit;
		private AssertRetryStrategy _assertRetryStrategy;
		public TestLog TestLog;


		[Test]
		public void ShouldRunEndToEndAbsenceRequest()//[Range(1, 50, 1)] int rangeCount
		{
			StardustManagerPingHelper.WaitForStarDustManagerToStart(TestLog);
			TestLog.Debug("Setting up test data");
			_assertRetryStrategy = new AssertRetryStrategy(100);
			Now.Is("2016-02-25 08:00".Utc());
			IPersonRequest personRequest = null;
			WithUnitOfWork.Do(() =>
			{
				var person =  PersonRepository.LoadAll().FirstOrDefault(x => x.Name.FirstName.Equals("Ola"));
				var absence = AbsenceRepository.LoadAll().FirstOrDefault();
				personRequest = new PersonRequest(person, new AbsenceRequest(absence, new DateTimePeriod(2016, 02, 26, 12, 2016, 02, 26, 13)));
				personRequest.Subject = "I am going to have fun";
				personRequest.Pending();
				PersonRequestRepository.Add(personRequest);
				
				
				var queuedReq = (new QueuedAbsenceRequest
				{
					PersonRequest = personRequest.Id.GetValueOrDefault(),
					MandatoryValidators = RequestValidatorsFlag.None,
					StartDateTime = personRequest.Request.Period.StartDateTime,
					EndDateTime = personRequest.Request.Period.EndDateTime,
					Created = Now.UtcDateTime().AddMinutes(-20)
				});
				QueuedAbsenceRequestRepository.Add(queuedReq);

			});

			TestLog.Debug("Starting service bus");
			var host = new ServiceBusRunner(i => { }, ConfigReader);
			host.Start();
			Thread.Sleep(2019);
			EventPublisher.Publish(new TenantMinuteTickEvent());

			TestLog.Debug("Performing Tier1 assertion");
			_assertRetryStrategy.Reset();
			performLevel1Assert(personRequest);

			TestLog.Debug("Performing Tier2 assertion");
			_assertRetryStrategy.Reset();
			performLevel2Assert();

			TestLog.Debug("Performing Tier3 assertion");
			//adding a small delay an experinment may be we need to retry late
			Thread.Sleep(2000);
			performLevel3Assert(personRequest);

			TestLog.Debug("Performing Tier4 assertion");
			performLevel4Assert(personRequest);

			host.Stop();
			TestLog.Debug("Service bus stopped");
		}

		private void performLevel1Assert(IPersonRequest personRequest)
		{
			//check in job queue
			var connectionString = InfraTestConfigReader.ConnectionString;
			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (var command = new SqlCommand("select serialized from Stardust.JobQueue", connection))
				{
					while (_assertRetryStrategy.TryAgain())
					{
						using (var reader = command.ExecuteReader())
						{
							if (reader.HasRows)
							{
								reader.Read();
								var jsonData = reader.GetString(0);
								NewMultiAbsenceRequestsCreatedEvent storedEvent = JsonConvert.DeserializeObject<NewMultiAbsenceRequestsCreatedEvent>(jsonData);
								storedEvent.PersonRequestIds.First().Should().Be.EqualTo(personRequest.Id.GetValueOrDefault());
								storedEvent.LogOnDatasource.Should().Be.EqualTo("TestData");
								storedEvent.LogOnBusinessUnitId.Should().Be.EqualTo(CurrentBusinessUnit.CurrentId().GetValueOrDefault());

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

		private void performLevel2Assert()
		{
			//check in job 
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
						Thread.Sleep(1000);
					}
					if (!_assertRetryStrategy.WithinRetryStrategy())
						Assert.Fail("Unable to perform Tier 2 Assertion. Exceeded the maximum number of retries.");

				}
			}
		}

		private void performLevel3Assert(IPersonRequest personRequest)
		{
			//check in job detail
			var comandText = $@"select jobid from Stardust.JobDetail where detail like '%{ personRequest.Id.GetValueOrDefault() }%'";
			var connectionString = InfraTestConfigReader.ConnectionString;
			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (var command = new SqlCommand(comandText, connection))
				{
					using (var reader = command.ExecuteReader())
					{
						if (reader.HasRows)
						{
							reader.Read();
							if (reader.IsDBNull(0))
								Assert.Fail("The request id is not in the stardust job detail. The proper request was not processed.");
							else
								Assert.IsTrue(true);
						}
					}
				}
			}
		}

		private void performLevel4Assert(IPersonRequest personRequest)
		{
			WithUnitOfWork.Do(() =>
			{
				PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault()).IsPending.Should().Be.False();
			});
		}
		
	}
}