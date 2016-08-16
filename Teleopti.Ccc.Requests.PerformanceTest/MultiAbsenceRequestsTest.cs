using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Absence;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Messaging.Client;

namespace Teleopti.Ccc.Requests.PerformanceTest
{
	[DomainTest]
	public class MultiAbsenceRequestsTest : ISetup
	{
		public IAbsenceRepository AbsenceRepository;
		public AsSystem AsSystem;
		public IDataSourceScope DataSource;
		public MutableNow Now;
		public IPersonRepository PersonRepository;
		public IPersonRequestRepository PersonRequestRepository;
		public IProcessMultipleAbsenceRequest Target;
		public WithUnitOfWork WithUnitOfWork;
		public IWorkflowControlSetRepository WorkflowControlSetRepository;
		public IEnumerable<Guid> PersonIds;
		public List<IPersonRequest> PersonRequests;
		public List<Guid> AbsenceRequestIds;


		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddModule(new CommonModule(configuration));
			system.UseTestDouble<NewAbsenceRequestHandler>().For<NewAbsenceRequestHandler>();
			system.UseTestDouble<ProcessMultipleAbsenceRequests>().For<IProcessMultipleAbsenceRequest>();
			system.UseTestDouble<NoMessageSender>().For<IMessageSender>();
			system.AddService<Database>();
			system.AddModule(new TenantServerModule(configuration));

			PersonRequests = new List<IPersonRequest>();
			AbsenceRequestIds = new List<Guid>();
			PersonIds = new List<Guid>
			{
				//DO NOT CHANGE THE ORDER OF THE GUIDS!
				new Guid("92121FA5-FB40-4458-800E-A1410113C461"),
				new Guid("8CFBDBF3-4A3F-47D5-AB6D-A1410113C474"),
				new Guid("61165F06-671C-407E-9F4B-A1410113C479"),
				new Guid("0C3B6CCA-6520-44D4-B668-A1410113C479"),
				new Guid("F2A4339F-810D-49FE-96C1-A38F00D0D6C1"),
				new Guid("E00BCFCE-F512-43E0-8F52-A3BF00D0577C"),
				new Guid("435F77DD-88FD-4B5F-A50B-A3DA00E7F828"),
				new Guid("E1BDBA7C-8369-46EA-94CB-A42100DFCE73"),
				new Guid("640440ED-6A71-4AB1-BA91-A42100DFD083"),
				new Guid("3A67C867-49FE-4382-8045-A46E00EDF604"),
				new Guid("3AFFF352-E8EE-49D3-819D-A46E00EDF982"),
				new Guid("8EF9D230-17AE-4C18-9158-A50C00E10DB1"),
				new Guid("41B26F8F-832C-4656-B18F-A52400A15894"),
				new Guid("475B1BDE-EA02-45D6-A9C3-A52B009526B0"),
				new Guid("06F5916B-9F40-4714-83C2-A52C00E2BC90"),
				new Guid("672E668E-F860-4D64-B8CF-A5E00100CC53"),
				new Guid("2CE89EBA-9680-48F2-BE68-A5E0010144AC"),
				new Guid("EE0AD86B-EA5A-447E-8EBB-A5E00102398D"),
				new Guid("12728761-0ED6-422B-B2B5-A5E001051F1E"),
				new Guid("BF50C741-A780-4930-A64B-A5E00105F325")

			};

			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));

			WithUnitOfWork.Do(() =>
			{
				var wfcs = WorkflowControlSetRepository.Get(new Guid("E97BC114-8939-4A70-AE37-A338010FFF19"));
				foreach (var period in wfcs.AbsenceRequestOpenPeriods)
				{
					period.OpenForRequestsPeriod = new DateOnlyPeriod(new DateOnly(2016, 5, 1), new DateOnly(2099, 5, 30));
					period.StaffingThresholdValidator = new StaffingThresholdValidator();
					period.AbsenceRequestProcess = new GrantAbsenceRequest();
					var datePeriod = period as AbsenceRequestOpenDatePeriod;
					if (datePeriod != null)
						datePeriod.Period = period.OpenForRequestsPeriod;
				}
				WorkflowControlSetRepository.UnitOfWork.PersistAll();

				// load some persons
				var persons = PersonRepository.FindPeople(PersonIds);

				//load vacation
				var absence = AbsenceRepository.Get(new Guid("3A5F20AE-7C18-4CA5-A02B-A11C00F0F27F"));


				foreach (var person in persons)
				{
					PersonRequests.Add(createAbsenceRequest(person, absence));
				}


				foreach (var pReq in PersonRequests)
				{
					PersonRequestRepository.Add(pReq);
					PersonRequestRepository.UnitOfWork.PersistAll();

					AbsenceRequestIds.Add(pReq.Id.Value);
				}
			});
		}


		[Test]
		public void ShouldProcessMultipleAbsenceRequests5()
		{
		}

		[Test]
		public void ShouldProcessMultipleAbsenceRequests10()
		{
		}

		[Test]
		public void ShouldProcessMultipleAbsenceRequests20()
		{
			
		}


		private IPersonRequest createAbsenceRequest(IPerson person, IAbsence absence)
		{
			var req = new AbsenceRequest(absence,
										 new DateTimePeriod(new DateTime(2016, 5, 27, 8, 0, 0, DateTimeKind.Utc),
															new DateTime(2016, 5, 27, 18, 0, 0, DateTimeKind.Utc)));
			return new PersonRequest(person) {Request = req};
		}
	}
}