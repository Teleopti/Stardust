using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Services;

using AbsenceFactory = Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData.AbsenceFactory;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer
{
	[TestFixture]
	public class RequestTransformTest
	{
		private DataTable _dataTable;
		private RequestTransformer _target;
		private IList<IPersonRequest> _personRequestList;
		private IList<IShiftTradeSwapDetail> _shiftTradeSwapDetails;


		[SetUp]
		public void Setup()
		{
			_dataTable = new DataTable();
			_dataTable.Locale = Thread.CurrentThread.CurrentCulture;
			RequestInfrastructure.AddColumnsToDataTable(_dataTable);
		}

		[Test]
		public void ShouldInitializeAbsenceRequest()
		{
			IPerson person2 = PersonFactory.CreatePerson("Test", "person2");
			person2.SetId(Guid.NewGuid());
			IPersonRequest personRequest2 = new PersonRequest(person2);
			personRequest2.Request = new AbsenceRequest(AbsenceFactory.CreateAbsenceCollection().ElementAt(0), new DateTimePeriod(2012, 01, 28, 2012, 01, 28));
			personRequest2.SetId(Guid.NewGuid());
			personRequest2.Deny("test", new PersonRequestAuthorizationCheckerForTest(), person2);
			RaptorTransformerHelper.SetUpdatedOn(personRequest2, DateTime.UtcNow);
			_personRequestList = new List<IPersonRequest>() { personRequest2 };
			_target = new RequestTransformer();
			_target.Transform(_personRequestList, 96, _dataTable);
			Assert.AreEqual(1, _personRequestList.Count);

		}

		[Test]
		public void ShouldInitializeTextRequest()
		{
			IPerson person = PersonFactory.CreatePerson("hello", "test");
			person.SetId(Guid.NewGuid());
			IPersonRequest personRequest1 = new PersonRequest(person);
			personRequest1.SetId(Guid.NewGuid());
			personRequest1.Pending();
			RaptorTransformerHelper.SetUpdatedOn(personRequest1, DateTime.UtcNow);

			personRequest1.Request = new TextRequest(new DateTimePeriod(2012, 01, 28, 2012, 01, 28));
			_personRequestList = new List<IPersonRequest>() { personRequest1 };
			_target = new RequestTransformer();
			_target.Transform(_personRequestList, 96, _dataTable);
			Assert.AreEqual(1, _personRequestList.Count);
		}


		[Test]
		public void ShouldInitializeOvertimeRequestWithCorrectRequestTypeCode()
		{
			IPerson person = PersonFactory.CreatePerson("hello", "test").WithId();
			IPersonRequest personRequest1 = new PersonRequest(person);
			personRequest1.SetId(Guid.NewGuid());
			personRequest1.Pending();
			RaptorTransformerHelper.SetUpdatedOn(personRequest1, DateTime.UtcNow);

			var ds = MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("ds", MultiplicatorType.Overtime);
			personRequest1.Request = new OvertimeRequest(ds, new DateTimePeriod(new DateTime(2019, 03, 13, 22, 0, 0, DateTimeKind.Utc), new DateTime(2019, 03, 13, 23, 0, 0, DateTimeKind.Utc)));
			_personRequestList = new List<IPersonRequest>() { personRequest1 };
			_target = new RequestTransformer();
			_target.Transform(_personRequestList, 96, _dataTable);
			_dataTable.Rows.Count.Should().Be(1);
			_dataTable.Rows[0]["request_type_code"].Should().Be.EqualTo((int)RequestType.OvertimeRequest);
		}

		[Test]
		public void ShouldInitializeApprovedAbsenceRequest()
		{

			IPerson person2 = PersonFactory.CreatePerson("Test", "person2");
			person2.SetId(Guid.NewGuid());

			IPersonRequest personRequest3 = new PersonRequest(person2);
			personRequest3.Request = new AbsenceRequest(AbsenceFactory.CreateAbsenceCollection().ElementAt(0), new DateTimePeriod(2012, 01, 26, 2012, 01, 26));
			personRequest3.SetId(Guid.NewGuid());
			personRequest3.Pending();
			personRequest3.Approve(new ApprovalServiceForTest(), new PersonRequestAuthorizationCheckerForTest());
			RaptorTransformerHelper.SetUpdatedOn(personRequest3, DateTime.UtcNow);

			_personRequestList = new List<IPersonRequest>() { personRequest3 };
			_target = new RequestTransformer();
			_target.Transform(_personRequestList, 96, _dataTable);
			Assert.AreEqual(1, _personRequestList.Count);

		}

		[Test]
		public void ShouldInitializeShiftTradeRequest()
		{
			IPerson person2 = PersonFactory.CreatePerson("Test", "person2");
			person2.SetId(Guid.NewGuid());

			IPerson person3 = PersonFactory.CreatePerson("Test", "person3");
			person3.SetId(Guid.NewGuid());
			IPersonRequest personRequest4 = new PersonRequest(person3);
			var shiftTradeSwap1 = new ShiftTradeSwapDetail(person3, person2, new DateOnly(2012, 01, 01), new DateOnly(2012, 01, 01));
			_shiftTradeSwapDetails = new List<IShiftTradeSwapDetail>() { shiftTradeSwap1 };
			personRequest4.Request = new ShiftTradeRequest(_shiftTradeSwapDetails);
			personRequest4.SetId(Guid.NewGuid());
			personRequest4.Pending();
			RaptorTransformerHelper.SetUpdatedOn(personRequest4, DateTime.UtcNow);

			_personRequestList = new List<IPersonRequest>() { personRequest4 };
			_target = new RequestTransformer();
			_target.Transform(_personRequestList, 96, _dataTable);
			Assert.AreEqual(1, _personRequestList.Count);

		}


		[Test]
		public void ShouldInitializeCancelledAbsenceRequest()
		{
			var person = PersonFactory.CreatePerson("Test", "person");
			person.SetId(Guid.NewGuid());

			IPersonRequest personRequest = new PersonRequest(person);
			personRequest.Request = new AbsenceRequest(AbsenceFactory.CreateAbsenceCollection().ElementAt(0), new DateTimePeriod(2012, 01, 26, 2012, 01, 26));
			personRequest.SetId(Guid.NewGuid());
			personRequest.Pending();
			personRequest.Approve(new ApprovalServiceForTest(), new PersonRequestAuthorizationCheckerForTest());
			personRequest.Cancel(new PersonRequestAuthorizationCheckerForTest());
			RaptorTransformerHelper.SetUpdatedOn(personRequest, DateTime.UtcNow);

			_personRequestList = new List<IPersonRequest>() { personRequest };
			_target = new RequestTransformer();
			_target.Transform(_personRequestList, 96, _dataTable);
			Assert.AreEqual(1, _personRequestList.Count);
			Assert.AreEqual(1, _dataTable.Rows.Count);
			var row = _dataTable.Rows[0];
			Assert.IsTrue(personRequest.IsCancelled);
			Assert.AreEqual(3, row["request_status_code"]);
		}


		[Test]
		public void ShouldInitializeWaitlistedAbsenceRequest()
		{
			var absence = AbsenceFactory.CreateAbsenceCollection().ElementAt(0);

			var startDate = DateOnly.Today.Date.ToUniversalTime();
			var endDate = startDate.AddDays(1);

			var workflowControlSetOne = createWorkFlowControlSet(startDate, endDate, absence, new GrantAbsenceRequest(), true);
			var person = createAndSetupPerson(workflowControlSetOne);

			IPersonRequest personRequest = new PersonRequest(person);
			personRequest.Request = new AbsenceRequest(absence, new DateTimePeriod(startDate, startDate.AddHours(8)));
			personRequest.SetId(Guid.NewGuid());
			personRequest.Deny("Work Harder", new PersonRequestAuthorizationCheckerForTest());

			RaptorTransformerHelper.SetUpdatedOn(personRequest, DateTime.UtcNow);

			_personRequestList = new List<IPersonRequest>() { personRequest };
			_target = new RequestTransformer();
			_target.Transform(_personRequestList, 96, _dataTable);
			Assert.AreEqual(1, _personRequestList.Count);

			var row = _dataTable.Rows[0];
			Assert.IsTrue(personRequest.IsWaitlisted);
			Assert.AreEqual(4, row["request_status_code"]);
		}


		private IPerson createAndSetupPerson(IWorkflowControlSet workflowControlSet)
		{
			var person = PersonFactory.CreatePersonWithId();

			person.WorkflowControlSet = workflowControlSet;

			return person;
		}

		private static WorkflowControlSet createWorkFlowControlSet(DateTime startDate, DateTime endDate, IAbsence absence, IProcessAbsenceRequest processAbsenceRequest, bool waitlistingIsEnabled)
		{
			var workflowControlSet = new WorkflowControlSet { AbsenceRequestWaitlistEnabled = waitlistingIsEnabled };
			workflowControlSet.SetId(Guid.NewGuid());

			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				AbsenceRequestProcess = processAbsenceRequest,
				Period = dateOnlyPeriod,
				OpenForRequestsPeriod = dateOnlyPeriod
			};

			workflowControlSet.InsertPeriod(absenceRequestOpenPeriod, 0);

			return workflowControlSet;

		}

	}
}
