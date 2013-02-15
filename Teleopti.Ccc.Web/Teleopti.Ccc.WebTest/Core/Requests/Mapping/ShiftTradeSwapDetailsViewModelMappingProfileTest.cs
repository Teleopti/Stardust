using System.Collections.Generic;
using System.Collections.ObjectModel;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class ShiftTradeSwapDetailsViewModelMappingProfileTest
	{
		private DateOnly _dateFrom;
		private DateOnly _dateTo;

		[SetUp]
		public void Setup()
		{
			_dateFrom = new DateOnly(2001, 12, 12);
			_dateTo = new DateOnly(2002, 12, 12);
			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new ShiftTradeSwapDetailsViewModelMappingProfile()));
		}

		[Test]
		public void SetsTheDatesFromTheFirstShiftTradeSwapDetail()
		{
			var result = Mapper.Map<IShiftTradeRequest, ShiftTradeSwapDetailsViewModel>(CreateStub(_dateFrom, _dateTo));
			Assert.That(result.DateFrom, Is.EqualTo(_dateFrom.Date));
			Assert.That(result.DateTo, Is.EqualTo(_dateTo.Date));
		}

		[Test]
		public void CreatesShiftTradePersonScheduleFromBasedOnTheFirstSwapdetail()
		{
			var dateFrom = new DateOnly(2001, 12, 12);
			var dateTo = new DateOnly(2002, 12, 12);

			var result = Mapper.Map<IShiftTradeRequest, ShiftTradeSwapDetailsViewModel>(CreateStub(dateFrom, dateTo));
			Assert.That(result.DateFrom, Is.EqualTo(dateFrom.Date));
			Assert.That(result.DateTo, Is.EqualTo(dateTo.Date));
		}

		[Test]
		public void CreatesShiftTradePersonScheduleToBasedOnTheFirstSwapDetail()
		{
			var result = Mapper.Map<IShiftTradeRequest, ShiftTradeSwapDetailsViewModel>(CreateStub(_dateFrom,_dateTo));
			Assert.That(result.DateFrom, Is.EqualTo(_dateFrom.Date));
			Assert.That(result.DateTo, Is.EqualTo(_dateTo.Date));
		}

		[Test]
		public void CreateScheduleViewModelsFromMapper()
		{
			var profileForProbing = new MappingProfileForProbing<IScheduleDay, ShiftTradePersonScheduleViewModel>();
			var scheduleDayFrom = MockRepository.GenerateStub<IScheduleDay>();
			var scheduleDayTo = MockRepository.GenerateStub<IScheduleDay>();
			var shiftTradePersonScheduleViewModelStub = new ShiftTradePersonScheduleViewModel();
			profileForProbing.Result = shiftTradePersonScheduleViewModelStub;
			
			Mapper.AddProfile(profileForProbing);

			var shiftTradeRequest = CreateStub(_dateFrom, _dateTo, scheduleDayFrom,scheduleDayTo);
			var result = Mapper.Map<IShiftTradeRequest, ShiftTradeSwapDetailsViewModel>(shiftTradeRequest);
			Assert.That(profileForProbing.HasBeenMappedFrom(scheduleDayFrom),"The mapper should have been called for the sheduleday From");
			Assert.That(profileForProbing.HasBeenMappedFrom(scheduleDayTo), "The mapper should have been called for the sheduleday To");
			Assert.That(result.To, Is.EqualTo(shiftTradePersonScheduleViewModelStub),"Should have been set from the mapper (we are using the same result for To and For");
			Assert.That(result.To, Is.EqualTo(shiftTradePersonScheduleViewModelStub),"Should have been set from the mapper (we are using the same result for To and For");
		}

		private static IShiftTradeRequest CreateStub(DateOnly dateFrom, DateOnly dateTo, IScheduleDay scheduleDayFrom = null, IScheduleDay scheduleDayTo = null)
		{
			var shiftTrade = MockRepository.GenerateMock<IShiftTradeRequest>();
			var swapDetail = MockRepository.GenerateMock<IShiftTradeSwapDetail>();
			var swapDetails = new ReadOnlyCollection<IShiftTradeSwapDetail>(new List<IShiftTradeSwapDetail>() { swapDetail });
			
			swapDetail.Expect(s => s.DateFrom).Return(dateFrom).Repeat.Any();
			swapDetail.Expect(s => s.DateTo).Return(dateTo).Repeat.Any();
			swapDetail.Expect(s => s.SchedulePartFrom).Return(scheduleDayFrom).Repeat.Any();
			swapDetail.Expect(s => s.SchedulePartTo).Return(scheduleDayTo).Repeat.Any();
			shiftTrade.Expect(s => s.ShiftTradeSwapDetails).Return(swapDetails).Repeat.Any();
			return shiftTrade;
		}
	}
}