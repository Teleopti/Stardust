using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Staffing.PerformanceTest;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Requests.PerformanceTest.OvertimeRequests
{
	[StaffingPerformanceTest]
	public class ProcessOvertimeRequests : PerformanceTestWithOneTimeSetup
	{
		private const int statusApproved = 2;
		private const int statusAutoDenied = 4;
		private const string tenantName = "Teleopti WFM";
		// BusinesUnit "Telia Sverige"
		private readonly Guid businessUnitId = new Guid("1FA1F97C-EBFF-4379-B5F9-A11C00F0F02B");
		// MultiplicatorDefinitionSet "Övertid Betald"
		private readonly Guid multiplicatorDefinitionSetId = new Guid("E0D49526-3CCB-4A17-B7D2-A142010BBDB4");


		public MutableNow Now;
		public IWorkflowControlSetRepository WorkflowControlSetRepository;
		public IPersonRepository PersonRepository;
		public IPersonRequestRepository PersonRequestRepository;
		public IMultiplicatorDefinitionSetRepository MultiplicatorDefinitionSetRepository;
		public UpdateStaffingLevelReadModelOnlySkillCombinationResources UpdateStaffingLevel;
		public ICurrentUnitOfWork CurrentUnitOfWork;
		public IOvertimeRequestProcessor OvertimeRequestProcessor;
		public WithUnitOfWork WithUnitOfWork;
		public IDataSourceScope DataSource;
		public AsSystem AsSystem;
		public IConfigReader ConfigReader;
		public ISkillRepository SkillRepository;
		public ISkillCombinationResourceRepository SkillCombinationResourceRepository;

		public override void OneTimeSetUp()
		{
			Now.Is("2017-01-01 07:00");
			using (DataSource.OnThisThreadUse(tenantName))
				AsSystem.Logon(tenantName, businessUnitId);

			var now = Now.UtcDateTime();
			var period = new DateTimePeriod(now.AddMonths(-6), now.AddYears(1));
			var deltas = new List<SkillCombinationResource>();
			WithUnitOfWork.Do((uow) =>
			{
				using (var connection = new SqlConnection(ConfigReader.ConnectionString("Tenancy")))
				{
					connection.Open();

					using (var command = new SqlCommand(@"truncate table readmodel.SkillCombination", connection))
					{
						command.ExecuteNonQuery();
					}
					using (var command = new SqlCommand(@"truncate table readmodel.SkillCombinationResourceDelta", connection))
					{
						command.ExecuteNonQuery();
					}
					using (var command = new SqlCommand(@"truncate table readmodel.SkillCombinationResource", connection))
					{
						command.ExecuteNonQuery();
					}
				}
				SkillRepository.LoadAllSkills();
				UpdateStaffingLevel.Update(period);
				uow.Current().PersistAll();
				var skillCombinationResources = SkillCombinationResourceRepository.LoadSkillCombinationResources(period);
				foreach (var skillCombinationResource in skillCombinationResources)
				{
					for (var index = 1; index <= 10; index++)
					{
						deltas.Add(new SkillCombinationResource
						{
							StartDateTime = skillCombinationResource.StartDateTime,
							EndDateTime = skillCombinationResource.EndDateTime,
							SkillCombination = skillCombinationResource.SkillCombination,
							Resource = skillCombinationResource.Resource / index
						});
					}
				}
				SkillCombinationResourceRepository.PersistChanges(deltas);
			});
		}

		[Test]
		public void ShouldProcessOvertimeRequest()
		{
			var peopleRequestsDictionary = new Dictionary<Guid, int>
			{
				// people from team 'FL_Sup_Sun7_71631' and 'FL_Sup_Sun6_00333'
				//DO NOT CHANGE THE ORDER OF THE GUIDS!
				[new Guid("55E3A133-6305-4C9A-8AEA-A1410113C47D")] = statusApproved,
				[new Guid("87E6CEF0-388A-4365-94FA-A1410113C47D")] = statusAutoDenied,
				[new Guid("391DB822-3936-4C4D-9634-A1410113C47D")] = statusAutoDenied,
				[new Guid("47721DE4-A0CB-45EE-A123-A1410113C47D")] = statusAutoDenied,
				[new Guid("DCF2EA04-3031-4436-A229-A1410113C47D")] = statusAutoDenied,
				[new Guid("4886AEDD-E30F-416C-B5E8-A1410113C47D")] = statusAutoDenied,
				[new Guid("C922055A-B4D0-4C06-B9AD-A1410113C47D")] = statusAutoDenied,
				[new Guid("CADD42C6-5419-48DD-8514-A25B009AD59D")] = statusAutoDenied,
				[new Guid("A35F2179-9A4B-40C9-BF7B-A27400997C79")] = statusAutoDenied,
				[new Guid("42394840-F905-4B22-915F-A332008A5288")] = statusAutoDenied,
				[new Guid("F25262A1-E3C3-4A54-B202-A33200B2E94F")] = statusAutoDenied,
				[new Guid("AE476FA3-7A6C-4948-89C2-A3BF00D0577C")] = statusApproved,
				[new Guid("1EBFEE75-CE35-40FB-975E-A3BF00D0577C")] = statusAutoDenied,
				[new Guid("E4C7A7A7-8D2C-4591-ACA4-A53D00F82C88")] = statusApproved,
				[new Guid("12728761-0ED6-422B-B2B5-A5E001051F1E")] = statusApproved,
				[new Guid("6069902E-5760-4DF4-B733-A5E00105B099")] = statusApproved,
				[new Guid("BF50C741-A780-4930-A64B-A5E00105F325")] = statusApproved
			};

			logonSystem();

			Now.Is("2017-01-01 07:00");

			WithUnitOfWork.Do(() =>
			{
				var personRequests = createOvertimeRequests(peopleRequestsDictionary.Keys);

				foreach (var personRequest in personRequests)
				{
					var stopwatch = Stopwatch.StartNew();

					OvertimeRequestProcessor.Process(personRequest, true);

					stopwatch.Stop();

					Console.WriteLine($@"{stopwatch.Elapsed.TotalSeconds}");
				}

			});
		}

		private ICollection<IPersonRequest> createOvertimeRequests(IEnumerable<Guid> personIds)
		{
			var personRequests = new List<IPersonRequest>();
			var persons = PersonRepository.FindPeople(personIds);

			var multiplicatorDefinitionSet =
				MultiplicatorDefinitionSetRepository.Get(multiplicatorDefinitionSetId);

			foreach (var person in persons)
			{
				person.WorkflowControlSet.AutoGrantOvertimeRequest = true;
				personRequests.Add(createOvertimeRequest(person, multiplicatorDefinitionSet));
			}

			CurrentUnitOfWork.Current().PersistAll();

			foreach (var personRequest in personRequests)
			{
				PersonRequestRepository.Add(personRequest);
				CurrentUnitOfWork.Current().PersistAll();
			}

			return personRequests;
		}

		private IPersonRequest createOvertimeRequest(IPerson person, IMultiplicatorDefinitionSet multiplicatorDefinitionSet)
		{
			var startTime = new DateTime(2017, 01, 02, 8, 0, 0, DateTimeKind.Utc);
			var endTime = new DateTime(2017, 01, 02, 18, 0, 0, DateTimeKind.Utc);
			var overtimeRequest = new OvertimeRequest(multiplicatorDefinitionSet, new DateTimePeriod(startTime, endTime));
			var personRequest = new PersonRequest(person) { Request = overtimeRequest };
			personRequest.Pending();
			return personRequest;
		}

		private void logonSystem()
		{
			using (DataSource.OnThisThreadUse(tenantName))
			{
				AsSystem.Logon(tenantName, businessUnitId);
			}
		}
	}
}
