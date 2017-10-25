﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Requests.PerformanceTuningTest
{

	[RequestPerformanceTuningTest]
	[Toggle(Toggles.MyTimeWeb_WaitListPositionEnhancement_46301)]
	public class WaitlistRequestWaitlistPositionPerformanceTest : PerformanceTestWithOneTimeSetup
	{
		public MutableNow Now;
		public WithUnitOfWork WithUnitOfWork;
		public IDataSourceScope DataSource;
		public AsSystem AsSystem;
		public FakeConfigReader ConfigReader;
		public IPersonRequestRepository PersonRequestRepository;
		public IWorkflowControlSetRepository WorkflowControlSetRepository;
		public IAbsenceRepository AbsenceRepository;
		public IPersonRepository PersonRepository;
		public IAbsenceRequestWaitlistProvider AbsenceRequestWaitlistProvider;

		private IList<IPersonRequest> _requests;
		private DateTime _nowDateTime;
		private ICollection<IPerson> _personList;
		private IWorkflowControlSet _defaultWorkflowControlSet;


		public override void OneTimeSetUp()
		{
			_nowDateTime = new DateTime(2016, 04, 06, 6, 58, 0).Utc();
			Now.Is(_nowDateTime);
			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));

			using (var connection = new SqlConnection(ConfigReader.ConnectionString("Tenancy")))
			{
				connection.Open();
				var path = AppDomain.CurrentDomain.BaseDirectory + "/../../" + "Prepare5000WaitlistedRequestForWaitlistPositionTest.sql";
				var script = File.ReadAllText(path);

				using (var command = new SqlCommand(script, connection))
				{
					command.ExecuteNonQuery();
				}
				connection.Close();
			}

			_requests = new List<IPersonRequest>();
			WithUnitOfWork.Do(() =>
			{
				var wfcs = WorkflowControlSetRepository.LoadAll();
				_defaultWorkflowControlSet = wfcs.FirstOrDefault();
				AbsenceRepository.LoadAll();

				var reqIds =
					PersonRequestRepository.GetWaitlistRequests(new DateTimePeriod(new DateTime(2016, 04, 06, 8, 0, 0).Utc(),
						new DateTime(2016, 04, 06, 17, 0, 0).Utc()));
				_requests = PersonRequestRepository.Find(reqIds);
				_personList = PersonRepository.FindPeople(_requests.Select(x => x.Person.Id.GetValueOrDefault()));

				_personList.ForEach(person =>
				{
					if (person.WorkflowControlSet == null)
						person.WorkflowControlSet = _defaultWorkflowControlSet;

					person.WorkflowControlSet.AbsenceRequestWaitlistEnabled = true;
				});
			});
		}

		[Test]
		public void GetPositionOf100WaitlistedRequests()
		{
			Now.Is("2016-04-06 06:59");

			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));

			WithUnitOfWork.Do(() =>
			{
				foreach (var request in _requests.Take(100))
				{
					AbsenceRequestWaitlistProvider.GetPositionInWaitlist((IAbsenceRequest)request.Request);
				}

			});
		}
	}
}
