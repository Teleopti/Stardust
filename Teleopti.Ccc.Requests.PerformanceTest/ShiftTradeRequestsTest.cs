using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Messaging.Client;

namespace Teleopti.Ccc.Requests.PerformanceTest
{
	[ShiftTradeRequestPerformanceTest]
	public class ShiftTradeRequestsTest
	{

		public AsSystem AsSystem;
		
		public IDataSourceScope DataSource;
		public IPersonRepository PersonRepository;
		public IPersonRequestRepository PersonRequestRepository;
		public ShiftTradeRequestHandler ShiftTradeRequestHandler;
		public WithUnitOfWork WithUnitOfWork;
		public IWorkflowControlSetRepository WorkflowControlSetRepository;
		public IGlobalSettingDataRepository GlobalSettingDataRepository;
		public IReadModelScheduleProjectionReadOnlyValidator ReadModelScheduleProjectionUpdater;
		public IReadModelFixer ReadModelFixer;
		
		
		[Test]
		public void ShouldBePerformantWhenValidatingAndReferringShiftTradeRequests()
		{
			using (DataSource.OnThisThreadUse ("Teleopti WFM"))
				AsSystem.Logon ("Teleopti WFM", new Guid ("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));

			var personRequests = new List<IPersonRequest>();
			
			WithUnitOfWork.Do (() =>
			{
				setupData (personRequests);
			});

			WithUnitOfWork.Do (() =>
			{
				var sw = new Stopwatch();
				sw.Start();

				personRequests.ForEach (shiftTradeRequest =>
				{
					ShiftTradeRequestHandler.Handle (new AcceptShiftTradeEvent()
					{
						PersonRequestId = shiftTradeRequest.Id.GetValueOrDefault()
					});
				});

				sw.Stop();

				Console.WriteLine ("Processing and Validation of Shift Trade Requests took " + sw.Elapsed);
			});

			var expectedResults = new Dictionary<Guid, RequestStatus>();
			var actualResults = new Dictionary<Guid, RequestStatus>();

			expectedResults.Add (personRequests[0].Id.Value, RequestStatus.Pending);
			expectedResults.Add (personRequests[1].Id.Value, RequestStatus.Pending);
			expectedResults.Add (personRequests[2].Id.Value, RequestStatus.Pending);
			expectedResults.Add (personRequests[3].Id.Value, RequestStatus.Denied);
			expectedResults.Add (personRequests[4].Id.Value, RequestStatus.Denied);
			expectedResults.Add (personRequests[5].Id.Value, RequestStatus.Pending);
			expectedResults.Add (personRequests[6].Id.Value, RequestStatus.Pending);
			
			WithUnitOfWork.Do (() =>
			{

				foreach (var req in personRequests)
				{
					var request = PersonRequestRepository.Get (req.Id.Value);
					var requestStatus = RequestStatus.New;

					if (request.IsPending)
						requestStatus = RequestStatus.Pending;
					else if (request.IsDenied)
						requestStatus = RequestStatus.Denied;
					else if (request.IsApproved)
						requestStatus = RequestStatus.Approved;

					actualResults.Add (request.Id.Value, requestStatus);
				}
			});

			CollectionAssert.AreEquivalent (expectedResults, actualResults);

		}

		private void setupData (ICollection<IPersonRequest> personRequests, bool enableMaxSeatsCheck = true)
		{
			foreach (var personRequest in personRequests)
			{
				var shiftTradeRequest = personRequest.Request as ShiftTradeRequest;
				configureWorkflowControlSet (shiftTradeRequest.PersonFrom.WorkflowControlSet);
				configureWorkflowControlSet (shiftTradeRequest.PersonTo.WorkflowControlSet);
			}
			
			var shiftTradeSettings = GlobalSettingDataRepository.FindValueByKey (ShiftTradeSettings.SettingsKey,
				new ShiftTradeSettings());

			if (enableMaxSeatsCheck)
			{
				shiftTradeSettings.MaxSeatsValidationSegmentLength = 15;
				shiftTradeSettings.MaxSeatsValidationEnabled = true;
			}
			else
			{
				shiftTradeSettings.MaxSeatsValidationEnabled = false;
			}

			GlobalSettingDataRepository.PersistSettingValue (shiftTradeSettings);

			var personIds = new[]
			{
				new Guid ("6069902E-5760-4DF4-B733-A5E00105B099"), //same site.
				new Guid ("BF50C741-A780-4930-A64B-A5E00105F325"),

				new Guid ("76939942-AE47-46FF-86BB-A1410093C99A"),
				new Guid ("4D1833F8-86A3-4107-8D5D-A14100F5D5CB"),

				new Guid ("F476C377-05DE-4C61-A04E-A14100F34EA1"),
				new Guid ("82691ED1-3D5E-4E46-AED7-A14100F34EA1"),

				new Guid ("3394FA0A-4507-4AD2-B18F-A5B700FF9E9C"),
				new Guid ("17A8373A-0B7A-4503-932F-A5B700FFA294"),

				new Guid ("91DDC350-377C-49A8-96B0-A5A300C7B4AE"),
				new Guid ("A1812640-A072-43DF-9CE5-A59B00AC17F6"),

				new Guid ("9881EF58-8F0C-40B2-A8A5-A20D00E94FE8"),
				new Guid ("36FE191E-630B-4903-97E9-A14100F5D5C7"),

				new Guid ("ADFE3A2F-9ED7-4B10-B05D-A1410113C46F"),
				new Guid ("35F2ECB7-34EC-4F50-8936-A1A200A356AC"),

			};

			var people = PersonRepository.FindPeople (personIds);

			for (var count = 0; count < personIds.Length; count = count + 2)
			{
				var personFrom = getPerson (people, personIds[count]);
				var personTo = getPerson (people, personIds[count + 1]);

				var dateOnly = new DateOnly (2016, 4, 20);

				personFrom.MyTeam (dateOnly).Site.MaxSeats = 80;
				personTo.MyTeam (dateOnly).Site.MaxSeats = 80;
				
				ensureSkillsMatch(personFrom, personTo, dateOnly);
				
				personRequests.Add (createShiftTradeRequest (personFrom, personTo, dateOnly, dateOnly));
				
				var readModels = ReadModelScheduleProjectionUpdater.Build (personTo, dateOnly).ToList();

				ReadModelFixer.FixScheduleProjectionReadOnly (new ReadModelData
				{
					Date = dateOnly,
					PersonId = personTo.Id.Value,
					ScheduleProjectionReadOnly = readModels
				});


				var readModelsFrom = ReadModelScheduleProjectionUpdater.Build (personFrom, dateOnly).ToList();

				ReadModelFixer.FixScheduleProjectionReadOnly (new ReadModelData
				{
					Date = dateOnly,
					PersonId = personFrom.Id.Value,
					ScheduleProjectionReadOnly = readModelsFrom
				});
			}

		}

		private static void ensureSkillsMatch(IPerson personFrom, IPerson personTo, DateOnly dateOnly)
		{
			personTo.WorkflowControlSet.MustMatchSkills.ForEach(skill =>
			{
				personFrom.AddSkill(skill, dateOnly);
				personTo.AddSkill(skill, dateOnly);
			});


			personFrom.WorkflowControlSet.MustMatchSkills.ForEach (skill =>
			{
				personFrom.AddSkill (skill, dateOnly);
				personTo.AddSkill (skill, dateOnly);
			});
		}

		private static void configureWorkflowControlSet (IWorkflowControlSet workflowControlSet)
		{
			workflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int> (0, 100);
			workflowControlSet.ShiftTradeTargetTimeFlexibility = TimeSpan.FromDays (99);
			workflowControlSet.AutoGrantShiftTradeRequest = true;
		}

		private IPerson getPerson (IEnumerable<IPerson> people, Guid id)
		{
			return people.Single (person => person.Id == id);
		}

		private IPersonRequest createShiftTradeRequest (IPerson personFrom, IPerson personTo, DateOnly shiftTradeDateFrom,
			DateOnly shiftTradeDateTo)
		{
			IPersonRequest request = new PersonRequest (personFrom);
			IShiftTradeRequest shiftTradeRequest = new ShiftTradeRequest (
				new List<IShiftTradeSwapDetail>
				{
					new ShiftTradeSwapDetail (personFrom, personTo, shiftTradeDateFrom, shiftTradeDateTo)
				});
			foreach (var shiftTradeSwapDetail in shiftTradeRequest.ShiftTradeSwapDetails)
			{
				shiftTradeSwapDetail.ChecksumFrom = 50;
				shiftTradeSwapDetail.ChecksumTo = 57;
			}

			request.Request = shiftTradeRequest;
			PersonRequestRepository.Add (request);
			return request;
		}
	}

	public class ShiftTradeRequestPerformanceTestAttribute : IoCTestAttribute
	{
		protected override FakeConfigReader Config()
		{
			var config = base.Config();
			config.FakeConnectionString("Tenancy", InfraTestConfigReader.ConnectionString);
			config.FakeConnectionString("Hangfire", InfraTestConfigReader.AnalyticsConnectionString);
			return config;
		}

		protected override void Setup (ISystem system, IIocConfiguration configuration)
		{
			base.Setup (system, configuration);
			system.AddModule (new CommonModule (configuration));


			system.UseTestDouble<NoMessageSender>().For<IMessageSender>();
			system.UseTestDouble<StardustJobFeedback>().For<IStardustJobFeedback>();
			system.UseTestDouble<ShiftTradeRequestHandler>().For<ShiftTradeRequestHandler>();

			system.AddService<Database>();
			system.AddModule (new TenantServerModule (configuration));
		}

		protected override void Startup (IComponentContext container)
		{
			base.Startup (container);

			// normal test injection is not working...
			((MutableNow) container.Resolve<INow>()).Is (new DateTime (2016, 04, 01, 10, 00, 00, DateTimeKind.Utc));
			
			container.Resolve<HangfireClientStarter>().Start();
		}
	}
}
