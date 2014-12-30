using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class ShiftExchangeOfferMapperTest
	{
		private static string _subject = "Shift Exchange Announcement";
		private IShiftExchangeOfferMapper _target;
		private ShiftExchangeOfferForm _form;
		private ILoggedOnUser _loggedOnUser;
		private IScheduleRepository _scheduleRepository;
		private ICurrentScenario _currentScenario;
		private ICurrentTeleoptiPrincipal _principal;

		[SetUp]
		public void Setup()
		{
			_loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			_scheduleRepository = MockRepository.GenerateMock<IScheduleRepository>();
			_currentScenario = MockRepository.GenerateMock<ICurrentScenario>();
			_principal = MockRepository.GenerateMock<ICurrentTeleoptiPrincipal>();
			_target = new ShiftExchangeOfferMapper(_loggedOnUser, _scheduleRepository, _currentScenario, _principal);
			_form = new ShiftExchangeOfferForm
			{
				Date = new DateTime(1547, 12, 1),
				StartTime = new TimeSpan(08),
				EndTime = new TimeSpan(09),
				OfferValidTo = new DateTime(1547, 12, 3),
				EndTimeNextDay = false
			};

			var date = new DateOnly(_form.Date);
			var currentUser = PersonFactory.CreatePerson();
			_loggedOnUser.Stub(x => x.CurrentUser()).Return(currentUser);
			var _mocks = new MockRepository();
			var schedule = _mocks.StrictMock<IScheduleDictionary>();
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			using (_mocks.Record())
			{

				Expect.Call(schedule[currentUser].ScheduledDay(date)).Return(scheduleDay);
			}

			//RobTodo: need make it same instance as called in target
			Expect.Call(_scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(currentUser,
				new ScheduleDictionaryLoadOptions(false, false)
				{
					LoadDaysAfterLeft = false,
					LoadNotes = false,
					LoadRestrictions = false
				},
				new DateOnlyPeriod(date, date), _currentScenario.Current())).Return(schedule);
		}

		[Test, Ignore]
		public void ShouldMapSubject()
		{
			var res = _target.Map(_form, ShiftExchangeOfferStatus.Pending);
			res.GetSubject(new NoFormatting()).Should().Be.EqualTo(_subject);
		}

	}
}