using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Wfm.Test
{
	//public class ConcurrencyTestAttribute : IoCTestAttribute
	//{
	//	protected override void AfterTest()
	//	{
	//		base.AfterTest();
	//		SetupFixtureForAssembly.RestoreCcc7Database();
	//	}
	//}

	[InfrastructureTest]
	public class ATOConcurrencyTest : SetUpCascadingShifts
	{
		public ICurrentUnitOfWork CurrentUnitOfWork;
		public IBusinessUnitRepository BusinessUnitRepository;
		public IDataSourceScope DataSourceScope;
		public ImpersonateSystem ImpersonateSystem;
		public WithUnitOfWork WithUnitOfWork;


		public IAbsenceRepository AbsenceRepository;
		public IPersonRepository PersonRepository;
		public IAbsenceRequestIntradayFilter AbsenceRequestIntradayFilter;
		public IPersonRequestRepository PersonRequestRepository;

		[SetUp]
		public void Setup()
		{
			SetupFixtureForAssembly.BeginTest();
		}

		

		[Test]
		public void ShouldBeApprovedIfOverstaffedSingleInterval()
		{
			var now = new DateTime(2017, 04, 06, 8, 0, 0).Utc();
			Now.Is(now);
			//var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			var requests = new List<IPersonRequest>();
			WithUnitOfWork.Do(() =>
			{
				SetUpRelevantStuffWithCascading();
				SetUpLowDemandSkillDays();

				

				var absence = AbsenceRepository.LoadRequestableAbsence().Single(x => x.Name == "Holiday");
				var person = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonGold");

				var requestStart = now.AddHours(2);
				var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(requestStart, requestStart.AddMinutes(30)));
				var personRequest = new PersonRequest(person, absenceRequest);
				PersonRequestRepository.Add(personRequest);
				requests.Add(personRequest);

				var person2 = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonAllSkills1");
				var absenceRequest2 = new AbsenceRequest(absence, new DateTimePeriod(requestStart, requestStart.AddMinutes(30)));
				var personRequest2 = new PersonRequest(person2, absenceRequest2);
				PersonRequestRepository.Add(personRequest2);
				requests.Add(personRequest2);

				var person3 = PersonRepository.LoadAll().Single(x => x.Name.FirstName == "PersonAllSkills2");

				var absenceRequest3 = new AbsenceRequest(absence, new DateTimePeriod(requestStart, requestStart.AddMinutes(30)));
				var personRequest3 = new PersonRequest(person3, absenceRequest3);
				PersonRequestRepository.Add(personRequest3);
				requests.Add(personRequest3);

			});
			
			//uow.PersistAll();

			Parallel.ForEach(requests, (req) =>
			{
				WithUnitOfWork.Do(() =>
				{
					AbsenceRequestIntradayFilter.Process(req);
				});
				
			});
			WithUnitOfWork.Do(() =>
			{
				var reqs = PersonRequestRepository.LoadAll().Where(r => requests.Select(x => x.Id).Contains(r.Id));
				reqs.Count();
			});
			
			//var reqs = PersonRequestRepository.Load(personRequest.Id.GetValueOrDefault());
			//reqs.IsApproved.Should().Be.True();
		}
	}
}
