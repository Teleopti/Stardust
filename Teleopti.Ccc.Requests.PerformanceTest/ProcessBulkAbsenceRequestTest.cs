using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Messaging.Client;

namespace Teleopti.Ccc.Requests.PerformanceTest
{
	[System.ComponentModel.Category("ProcessBulkAbsenceRequest")]
	[DomainTest]
	public class ProcessBulkAbsenceRequestTest : ISetup
	{
		public IProcessMultipleAbsenceRequest Target;
		public WithUnitOfWork WithUnitOfWork;
		public IDataSourceScope DataSource;
		//public IBusinessUnitRepository BusinessUnits;
		public IPersonRepository PersonRepository;
		public IPersonRequestRepository PersonRequestRepository;
		public IAbsenceRepository AbsenceRepository;
		public AsSystem AsSystem;
		public MutableNow Now;

		[Test, Ignore]
		public void ShouldProcessMultipleAbsenceRequests()
		{

			IEnumerable<Guid> personIds = new List<Guid>
			{
				new Guid("391DB822-3936-4C4D-9634-A1410113C47D"),
				new Guid("C922055A-B4D0-4C06-B9AD-A1410113C47D"),
				new Guid("47721DE4-A0CB-45EE-A123-A1410113C47D"),
				new Guid("CADD42C6-5419-48DD-8514-A25B009AD59D"),
				new Guid("DCF2EA04-3031-4436-A229-A1410113C47D"),
				new Guid("AE476FA3-7A6C-4948-89C2-A3BF00D0577C"),
				new Guid("4886AEDD-E30F-416C-B5E8-A1410113C47D"),
				new Guid("E4C7A7A7-8D2C-4591-ACA4-A53D00F82C88"),
				new Guid("6069902E-5760-4DF4-B733-A5E00105B099"),
				new Guid("BF50C741-A780-4930-A64B-A5E00105F325")

			};

			//Guid businessUnitId;
			using (DataSource.OnThisThreadUse("Telia"))
				//businessUnitId = WithUnitOfWork.Get(() => BusinessUnits.LoadAll().First()).Id.Value;
				AsSystem.Logon("Telia", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));


			var personReqs = new List<IPersonRequest>();

			var absenceRequests = new List<NewAbsenceRequestCreatedEvent>();
			WithUnitOfWork.Do(() =>
			{
				// load some persons
				var persons = PersonRepository.FindPeople(personIds);
				IAbsence absence = AbsenceRepository.Get(new Guid("3A5F20AE-7C18-4CA5-A02B-A11C00F0F27F"));

				//load vacation

				foreach (var person in persons)
				{
					//var person = PersonRepository.Get(personId);
					personReqs.Add(createAbsenceRequest(person, absence));
				}



				foreach (var pReq in personReqs)
				{
					PersonRequestRepository.Add(pReq);
					PersonRequestRepository.UnitOfWork.PersistAll();

					absenceRequests.Add(
						new NewAbsenceRequestCreatedEvent()
						{
							PersonRequestId = pReq.Id.Value,
							InitiatorId = new Guid("00000000-0000-0000-0000-000000000000"),
							LogOnBusinessUnitId = new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"),
							LogOnDatasource = "Telia",
							Timestamp = DateTime.Parse("2016-08-08T11:06:00.7366909Z")
						}
						);
				}


				Target.Process(absenceRequests);
			});

		}


		private IPersonRequest createAbsenceRequest(IPerson person, IAbsence absence)
		{
			var req = new AbsenceRequest(absence,
				new DateTimePeriod(new DateTime(2016, 6, 30, 8, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 6, 30, 18, 0, 0, DateTimeKind.Utc)));
			return new PersonRequest(person) {Request = req};
		}


		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddModule(new CommonModule(configuration));
			system.UseTestDouble<NewAbsenceRequestHandler>().For<NewAbsenceRequestHandler>();
			system.UseTestDouble<ProcessMultipleAbsenceRequest>().For<IProcessMultipleAbsenceRequest>();
			system.UseTestDouble<NoMessageSender>().For<IMessageSender>();
			system.AddService<Database>();
			system.AddModule(new TenantServerModule(configuration));

		}
	}


	public class ProcessMultipleAbsenceRequest : IProcessMultipleAbsenceRequest
	{
		private readonly NewAbsenceRequestHandler _newAbsenceRequestHandler;

		public ProcessMultipleAbsenceRequest(NewAbsenceRequestHandler newAbsenceRequestHandler)
		{
			_newAbsenceRequestHandler = newAbsenceRequestHandler;
		}

		public void Process(List<NewAbsenceRequestCreatedEvent> absenceRequests)
		{

			foreach (var req in absenceRequests)
			{
				_newAbsenceRequestHandler.Handle(req);
			}


		}
	}

	public interface IProcessMultipleAbsenceRequest
	{
		void Process(List<NewAbsenceRequestCreatedEvent> absenceRequests);
	}
}
