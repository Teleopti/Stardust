using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.WebTest.Core.Common.DataProvider;


namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class ShiftExchangeOfferMapperTest
	{
		private static string _subject = "Shift Trade Post";
		private IShiftExchangeOfferMapper _target;
		private ShiftExchangeOfferForm _form;
		private ILoggedOnUser _loggedOnUser;
		private IScheduleProvider _scheduleProvider;

		[SetUp]
		public void Setup()
		{
			_loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			_target = new ShiftExchangeOfferMapper(_loggedOnUser, _scheduleProvider);
			_form = new ShiftExchangeOfferForm
			{
				Date = new DateTime(1547, 12, 1),
				StartTime = TimeSpan.FromHours(9),
				EndTime = TimeSpan.FromHours(18),
				OfferValidTo = new DateTime(1547, 12, 3),
				EndTimeNextDay = false
			};
			var date = new DateOnly(_form.Date);
			var scheduleDay = ScheduleDayFactory.Create(date);
			_scheduleProvider = new FakeScheduleProvider(scheduleDay);

			var currentUser = PersonFactory.CreatePerson();
			_loggedOnUser.Stub(x => x.CurrentUser()).Return(currentUser);
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