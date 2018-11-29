using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Requests;
using Teleopti.Ccc.TestCommon.Services;


namespace Teleopti.Ccc.WinCodeTest.Scheduler.Requests
{
	[TestFixture]
	public class HandlePersonRequestComparerTest
	{
		private HandlePersonRequestComparer _target;
		private IShiftTradeRequestStatusChecker _checker;
		private IPersonAccountCollection _personAccount;
		private IEventAggregator _eventAgg;
		private TimeZoneInfo _info;
		private IPerson _person;
		private IPersonRequestViewModel x, y;

		[SetUp]
		public void Setup()
		{
			_target = new HandlePersonRequestComparer();
			_checker = MockRepository.GenerateMock<IShiftTradeRequestStatusChecker>();
			_personAccount = MockRepository.GenerateMock<IPersonAccountCollection>();
			_eventAgg = MockRepository.GenerateMock<IEventAggregator>();
			_info = TimeZoneInfo.Local;
			_person = MockRepository.GenerateMock<IPerson>();
		}

		[Test]
		public void ShouldCompare_RequestedDate_StartDate()
		{
			x = new PersonRequestViewModel(new PersonRequest(_person,
			                                                 new AbsenceRequest(new Absence(),
			                                                                    new DateTimePeriod(2010, 1, 1, 2011, 1, 1))),
			                               _checker, _personAccount, _eventAgg, _info);
			y = new PersonRequestViewModel(new PersonRequest(_person,
			                                                 new AbsenceRequest(new Absence(),
			                                                                    new DateTimePeriod(2000, 1, 1, 2011, 1, 1))),
			                               _checker, _personAccount, _eventAgg, _info);

			_target.SortDescriptions.Add(new SortDescription("RequestedDate", ListSortDirection.Ascending));
			var result = _target.Compare(x, y);
			result.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldCompare_RequestedDate_EndDate()
		{
			x = new PersonRequestViewModel(new PersonRequest(_person,
			                                                 new AbsenceRequest(new Absence(),
			                                                                    new DateTimePeriod(2010, 1, 1, 2011, 1, 1))),
			                               _checker, _personAccount, _eventAgg, _info);
			y = new PersonRequestViewModel(new PersonRequest(_person,
			                                                 new AbsenceRequest(new Absence(),
			                                                                    new DateTimePeriod(2010, 1, 1, 2010, 12, 31))),
			                               _checker, _personAccount, _eventAgg, _info);

			_target.SortDescriptions.Add(new SortDescription("RequestedDate", ListSortDirection.Ascending));
			var result = _target.Compare(x, y);
			result.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldCompare_Subject()
		{
			var personRequestX = new PersonRequest(_person, new TextRequest(new DateTimePeriod(2010, 1, 1, 2011, 1, 1)))
				{
					Subject = "A"
				};
			var personRequestY = new PersonRequest(_person, new TextRequest(new DateTimePeriod(2010, 1, 1, 2011, 1, 1)))
				{
					Subject = "B"
				};
			x = new PersonRequestViewModel(personRequestX,
			                               _checker, _personAccount, _eventAgg, _info);
			y = new PersonRequestViewModel(personRequestY,
			                               _checker, _personAccount, _eventAgg, _info);
			_target.SortDescriptions.Add(new SortDescription("Subject", ListSortDirection.Ascending));
			var result = _target.Compare(x, y);
			result.Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldCompare_SubjectString_Desc()
		{
			var personRequestX = new PersonRequest(_person, new TextRequest(new DateTimePeriod(2010, 1, 1, 2011, 1, 1)))
				{
					Subject = "A"
				};
			var personRequestY = new PersonRequest(_person, new TextRequest(new DateTimePeriod(2010, 1, 1, 2011, 1, 1)))
				{
					Subject = "B"
				};
			x = new PersonRequestViewModel(personRequestX,
			                               _checker, _personAccount, _eventAgg, _info);
			y = new PersonRequestViewModel(personRequestY,
			                               _checker, _personAccount, _eventAgg, _info);
			_target.SortDescriptions.Add(new SortDescription("Subject", ListSortDirection.Descending));
			var result = _target.Compare(x, y);
			result.Should().Be.EqualTo(1);
		}


		[Test]
		public void ShouldCompare_Seniority()
		{
			var personY = MockRepository.GenerateMock<IPerson>();
			var personRequestX = new PersonRequest(_person, new TextRequest(new DateTimePeriod(2010, 1, 1, 2011, 1, 1)));
			var personRequestY = new PersonRequest(personY, new TextRequest(new DateTimePeriod(2010, 1, 1, 2011, 1, 1)));

			x = new PersonRequestViewModel(personRequestX,
			                               _checker, _personAccount, _eventAgg, _info);
			y = new PersonRequestViewModel(personRequestY,
			                               _checker, _personAccount, _eventAgg, _info);
			_target.SortDescriptions.Add(new SortDescription("Seniority", ListSortDirection.Ascending));

			_person.Expect(p => p.Seniority).Return(100);
			personY.Expect(p => p.Seniority).Return(10);


			var result = _target.Compare(x, y);
			result.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldCompare_IsDenied()
		{
			var authorization = new PersonRequestCheckAuthorization();
			var personY = MockRepository.GenerateMock<IPerson>();
			var personRequestX = new PersonRequest(_person, new TextRequest(new DateTimePeriod(2010, 1, 1, 2011, 1, 1)));
			var personRequestY = new PersonRequest(personY, new TextRequest(new DateTimePeriod(2010, 1, 1, 2011, 1, 1)));

			x = new PersonRequestViewModel(personRequestX,
			                               _checker, _personAccount, _eventAgg, _info);
			y = new PersonRequestViewModel(personRequestY,
			                               _checker, _personAccount, _eventAgg, _info);

			personRequestX.Deny("", authorization);
			personRequestY.ForcePending();
			personRequestY.Approve(new ApprovalServiceForTest(), authorization);

			_target.SortDescriptions.Add(new SortDescription("IsDenied", ListSortDirection.Ascending));

			var result = _target.Compare(x, y);
			result.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldCompare_ShiftTradeRequest_MultipleDays()
		{
			var personY = MockRepository.GenerateMock<IPerson>();
			var permissionInfo = MockRepository.GenerateMock<PermissionInformation>();

			_person.Expect(p => p.PermissionInformation).Return(permissionInfo);
			personY.Expect(p => p.PermissionInformation).Return(permissionInfo);

			var personRequestX = new PersonRequest(_person,
			                                       new ShiftTradeRequest(new List<IShiftTradeSwapDetail>
				                                       {
					                                       new ShiftTradeSwapDetail(_person, personY, new DateOnly(2010, 1, 1),
					                                                                new DateOnly(2010, 1, 2)),
					                                       new ShiftTradeSwapDetail(_person, personY, new DateOnly(2010, 1, 3),
					                                                                new DateOnly(2010, 1, 4)),
					                                       new ShiftTradeSwapDetail(_person, personY, new DateOnly(2010, 1, 5),
					                                                                new DateOnly(2010, 1, 6)),
				                                       }));

			var personRequestY = new PersonRequest(_person,
			                                       new ShiftTradeRequest(new List<IShiftTradeSwapDetail>
				                                       {
					                                       new ShiftTradeSwapDetail(_person, personY, new DateOnly(2010, 1, 1),
					                                                                new DateOnly(2010, 1, 2)),
				                                       }));

			x = new PersonRequestViewModel(personRequestX,
			                               _checker, _personAccount, _eventAgg, _info);
			y = new PersonRequestViewModel(personRequestY,
			                               _checker, _personAccount, _eventAgg, _info);

			_target.SortDescriptions.Add(new SortDescription("RequestedDate", ListSortDirection.Ascending));

			var result = _target.Compare(x, y);
			result.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldCompare_ShiftTradeRequests_MultipleDaysTwo()
		{
			var personY = MockRepository.GenerateMock<IPerson>();
			var permissionInfo = MockRepository.GenerateMock<PermissionInformation>();

			_person.Expect(p => p.PermissionInformation).Return(permissionInfo);
			personY.Expect(p => p.PermissionInformation).Return(permissionInfo);

			var personRequestX = new PersonRequest(_person,
			                                       new ShiftTradeRequest(new List<IShiftTradeSwapDetail>
				                                       {
					                                       new ShiftTradeSwapDetail(_person, personY, new DateOnly(2010, 1, 1),
					                                                                new DateOnly(2010, 1, 2)),
					                                       new ShiftTradeSwapDetail(_person, personY, new DateOnly(2010, 1, 3),
					                                                                new DateOnly(2010, 1, 4)),
					                                       new ShiftTradeSwapDetail(_person, personY, new DateOnly(2010, 1, 5),
					                                                                new DateOnly(2010, 1, 6)),
				                                       }));

			var personRequestY = new PersonRequest(_person,
			                                       new ShiftTradeRequest(new List<IShiftTradeSwapDetail>
				                                       {
					                                       new ShiftTradeSwapDetail(_person, personY, new DateOnly(2010, 1, 1),
					                                                                new DateOnly(2010, 1, 2)),
					                                       new ShiftTradeSwapDetail(_person, personY, new DateOnly(2010, 1, 2),
					                                                                new DateOnly(2010, 1, 4))
				                                       }));

			x = new PersonRequestViewModel(personRequestX,
			                               _checker, _personAccount, _eventAgg, _info);
			y = new PersonRequestViewModel(personRequestY,
			                               _checker, _personAccount, _eventAgg, _info);

			_target.SortDescriptions.Add(new SortDescription("RequestedDate", ListSortDirection.Ascending));

			var result = _target.Compare(x, y);
			result.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldCompare_StableSort()
		{

			var personRequestX = new PersonRequest(_person, new TextRequest(new DateTimePeriod(2010, 1, 1, 2011, 1, 1)))
				{
					Subject = "A"
				};
			var personRequestY = new PersonRequest(_person, new TextRequest(new DateTimePeriod(2010, 1, 1, 2011, 1, 1)))
				{
					Subject = "B"
				};
			x = new PersonRequestViewModel(personRequestX,
			                               _checker, _personAccount, _eventAgg, _info);
			y = new PersonRequestViewModel(personRequestY,
			                               _checker, _personAccount, _eventAgg, _info);
			_target.SortDescriptions.Add(new SortDescription("RequestedDate", ListSortDirection.Ascending));
			_target.SortDescriptions.Add(new SortDescription("Subject", ListSortDirection.Descending));
			var result = _target.Compare(x, y);
			result.Should().Be.EqualTo(1);
		}
	}
}
