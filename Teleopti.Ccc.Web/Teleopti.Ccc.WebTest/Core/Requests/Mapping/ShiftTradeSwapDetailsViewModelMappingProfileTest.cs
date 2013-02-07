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

		[SetUp]
		public void Setup()
		{
			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new ShiftTradeSwapDetailsViewModelMappingProfile()));
		}

		[Test]
		public void SetsThedatesFromTheFirstShiftTradeSwapDetail()
		{
			var dateFrom = new DateOnly(2001, 12, 12);
			var dateTo = new DateOnly(2002, 12, 12);

			var result = Mapper.Map<IShiftTradeRequest, ShiftTradeSwapDetailsViewModel>(CreateStub(dateFrom, dateTo));
			Assert.That(result.DateFrom, Is.EqualTo(dateFrom));
			Assert.That(result.DateTo, Is.EqualTo(dateTo));
		}

		[Test]
		public void CreatesShiftTradePersonScheduleFromBasedOnTheFirstSwapdetail()
		{
			var dateFrom = new DateOnly(2001, 12, 12);
			var dateTo = new DateOnly(2002, 12, 12);

			var result = Mapper.Map<IShiftTradeRequest, ShiftTradeSwapDetailsViewModel>(CreateStub(dateFrom, dateTo));
			Assert.That(result.DateFrom, Is.EqualTo(dateFrom));
			Assert.That(result.DateTo, Is.EqualTo(dateTo));
		}

		[Test]
		public void CreatesShiftTradePersonScheduleToBasedOnTheFirstSwapDetail()
		{
			var dateFrom = new DateOnly(2001, 12, 12);
			var dateTo = new DateOnly(2002, 12, 12);

			var result = Mapper.Map<IShiftTradeRequest, ShiftTradeSwapDetailsViewModel>(CreateStub(dateFrom,dateTo));
			Assert.That(result.DateFrom, Is.EqualTo(dateFrom));
			Assert.That(result.DateTo, Is.EqualTo(dateTo));
		}

		private static IShiftTradeRequest CreateStub(DateOnly dateFrom, DateOnly dateTo)
		{
			var shiftTrade = MockRepository.GenerateMock<IShiftTradeRequest>();
			var swapDetail = MockRepository.GenerateMock<IShiftTradeSwapDetail>();
			var swapDetails = new ReadOnlyCollection<IShiftTradeSwapDetail>(new List<IShiftTradeSwapDetail>() { swapDetail });
			

			swapDetail.Expect(s => s.DateFrom).Return(dateFrom).Repeat.Any();
			swapDetail.Expect(s => s.DateTo).Return(dateTo).Repeat.Any();
			shiftTrade.Expect(s => s.ShiftTradeSwapDetails).Return(swapDetails).Repeat.Any();
			return shiftTrade;
		}
	}
}