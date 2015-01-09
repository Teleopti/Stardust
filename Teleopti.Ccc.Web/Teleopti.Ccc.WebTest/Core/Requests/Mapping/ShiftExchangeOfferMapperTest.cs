using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
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
				StartTime = TimeSpan.FromHours(9),
				EndTime = TimeSpan.FromHours(18),
				OfferValidTo = new DateTime(1547, 12, 3),
				EndTimeNextDay = false
			};

			var date = new DateOnly(_form.Date);
			var currentUser = PersonFactory.CreatePerson();
			_loggedOnUser.Stub(x => x.CurrentUser()).Return(currentUser);
			var _mocks = new MockRepository();
			var schedule = _mocks.StrictMock<IScheduleDictionary>();
			var identity = MockRepository.GenerateMock<ITeleoptiIdentity>();
			var teleoptiPrincipal = new TeleoptiPrincipal(identity, currentUser);
			_principal.Expect(x => x.Current()).Return(teleoptiPrincipal);
			var scheduleDay = ScheduleDayFactory.Create(date);
			using (_mocks.Record())
			{
				Expect.Call(schedule[currentUser].ScheduledDay(date)).Return(scheduleDay);
			}

			_scheduleRepository.Stub(x => x.FindSchedulesForPersonOnlyInGivenPeriod(currentUser,
				new ScheduleDictionaryLoadOptions(false, false)
				{
					LoadDaysAfterLeft = false,
					LoadNotes = false,
					LoadRestrictions = false
				},
				new DateOnlyPeriod(date, date), _currentScenario.Current())
				).Return(schedule).IgnoreArguments();
		}

		[Test]
		public void ShouldMapStatus()
		{
			var res = _target.Map(_form, ShiftExchangeOfferStatus.Pending);
			((IShiftExchangeOffer) res.Request).Status.Should().Be.EqualTo(ShiftExchangeOfferStatus.Pending);
		}		
		
		[Test]
		public void ShouldMapSubject()
		{
			var res = _target.Map(_form, ShiftExchangeOfferStatus.Pending);
			res.GetSubject(new NoFormatting()).Should().Be.EqualTo(_subject);
		}		
		
		[Test]
		public void ShouldMapMessage()
		{
			var res = _target.Map(_form, ShiftExchangeOfferStatus.Pending);
			res.GetMessage(new NoFormatting()).Should().Be.EqualTo("");
		}

		[Test]
		public void ShouldMapForUpdate()
		{
			var expected = MockRepository.GenerateMock<IPersonRequest>();
			expected.Stub(x => x.Request).Return(MockRepository.GenerateMock<IShiftExchangeOffer>());

			var res = _target.Map(_form, expected);
			res.Should().Be.SameInstanceAs(expected);	
		}
	}
}