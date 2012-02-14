using System;
using System.Collections.Generic;
using System.ComponentModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.AgentPortalCode.Common.Controls;
using Teleopti.Ccc.AgentPortalCode.Requests.ShiftTrade;
using Teleopti.Ccc.AgentPortalCode.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortalCodeTest.Common.Controls
{
    [TestFixture]
    public class DateSelectorTest
    {
        private MockRepository mocks;
        private ShiftTradeModel model;
        private IDateSelectorView view;
        private DateSelectorPresenter target;
        private DateTime initialDateForDateSelectors = new DateTime(2009, 4, 4); //This one will be in the list from the beginning
        private ITeleoptiSchedulingService sdk;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            PersonDto loggedOnPerson = new PersonDto();
            PersonRequestDto personRequestDto = createPersonRequestDto(loggedOnPerson, new PersonDto(),initialDateForDateSelectors);
            sdk = mocks.CreateMock<ITeleoptiSchedulingService>();
            model = new ShiftTradeModel(sdk, personRequestDto, loggedOnPerson, new DateTime(2008,12,22));
            view = mocks.CreateMock<IDateSelectorView>();
            target = new DateSelectorPresenter(model, view);
        }

        [Test]
        public void CanAddDate()
        {
            DateTime date = new DateTime();
            using(mocks.Record())
            {
                Expect.Call(view.CurrentDate).Return(date).Repeat.Any();
            }
            using(mocks.Playback())
            {
                target.AddDate();                
            }
            Assert.AreEqual(initialDateForDateSelectors, model.TradeDates[0]);
            Assert.AreEqual(date, model.TradeDates[1]);
        }

        [Test]
        public void CanAddDateRange()
        {
            DateTime date = new DateTime();
            DateTime to = date.AddDays(4);

            using (mocks.Record())
            {
                Expect.Call(view.CurrentDateFrom).Return(date).Repeat.Any();
                Expect.Call(view.CurrentDateTo).Return(to).Repeat.Any();
            }
            using (mocks.Playback())
            {
                target.AddDateRange();
            }

            CollectionAssert.Contains(model.TradeDates, initialDateForDateSelectors);
            CollectionAssert.Contains(model.TradeDates, date);
            CollectionAssert.Contains(model.TradeDates, date.AddDays(1));
            CollectionAssert.Contains(model.TradeDates, date.AddDays(2));
            CollectionAssert.Contains(model.TradeDates, date.AddDays(3));
            CollectionAssert.Contains(model.TradeDates, date.AddDays(4));

            Assert.AreEqual(6, model.TradeDates.Count);
        } 

        [Test]
        public void VerifyIfDateAreAlreadyInTheList()
        {
            DateTime date = new DateTime();
            DateTime to = date.AddDays(1);

            using (mocks.Record())
            {
                Expect.Call(view.CurrentDateFrom).Return(date).Repeat.Any();
                Expect.Call(view.CurrentDateTo).Return(to).Repeat.Any();
                Expect.Call(view.CurrentDate).Return(date).Repeat.Any();
            }
            using (mocks.Playback())
            {
                target.AddDateRange();
                target.AddDate();
                target.AddDateRange();
            }
            Assert.AreEqual(3, model.TradeDates.Count);
        }

        [Test]
        public void VerifyToDateIsLaterThanFromDateAndItWillNotCrash()
        {
            DateTime date = new DateTime().AddDays(5);
            DateTime to = date.AddDays(-1);

            using (mocks.Record())
            {
                Expect.Call(view.CurrentDateFrom).Return(date).Repeat.Any();
                Expect.Call(view.CurrentDateTo).Return(to).Repeat.Any();
            }
            using (mocks.Playback())
            {
                target.AddDateRange();
            }
            Assert.AreEqual(1, model.TradeDates.Count);

        }

        [Test]
        public void VerifyCanDeleteDate()
        {
            DateTime date = new DateTime();
            DateTime to = date.AddDays(5);

            using (mocks.Record())
            {
                Expect.Call(view.CurrentDate).Return(date).Repeat.Any();
                Expect.Call(view.CurrentDateFrom).Return(date).Repeat.Any();
                Expect.Call(view.CurrentDateTo).Return(to).Repeat.Any();
                Expect.Call(view.SelectedDeleteDates).Return(new BindingList<DateTime>{date,date.AddDays(1)}).Repeat.Any();
            }
            using (mocks.Playback())
            {
                target.AddDateRange();
                Assert.AreEqual(7, model.TradeDates.Count);
                target.DeleteDates(); //Two dates are selected
            }
            Assert.AreEqual(5, model.TradeDates.Count); //two will go away
        }

        [Test]
        public void VerifyInitialize()
        {
            using (mocks.Record())
            {
                view.DateList = model.TradeDates;
                view.SelectedDeleteDates = model.DeletedDates;
                view.InitialDate = new DateTime(2008,12,22);
            }

            using(mocks.Playback())
            {
                target.Initialize();
            }
        }

        private PersonRequestDto createPersonRequestDto(PersonDto tradeFrom, PersonDto tradeWith, DateTime dateTime)
        {
            PersonRequestDto personRequestDto = new PersonRequestDto();
            personRequestDto.Message = "Message";
            personRequestDto.Subject = "ShiftTrade";
            ShiftTradeRequestDto shiftTradeRequestDto = new ShiftTradeRequestDto();
            
            personRequestDto.Request = shiftTradeRequestDto;
            shiftTradeRequestDto.ShiftTradeStatus = ShiftTradeStatusDto.OkByMe;
            personRequestDto.Person = tradeFrom;
            shiftTradeRequestDto.ShiftTradeSwapDetails = new ShiftTradeSwapDetailDto[1];
            shiftTradeRequestDto.ShiftTradeSwapDetails[0] = new ShiftTradeSwapDetailDto();
            shiftTradeRequestDto.ShiftTradeSwapDetails[0].DateFrom = new DateOnlyDto { DateTime = dateTime };
            shiftTradeRequestDto.ShiftTradeSwapDetails[0].DateTo = new DateOnlyDto { DateTime = dateTime };
            shiftTradeRequestDto.ShiftTradeSwapDetails[0].PersonFrom = tradeFrom;
            shiftTradeRequestDto.ShiftTradeSwapDetails[0].PersonTo = tradeWith;
            personRequestDto.CreatedDate = dateTime;
            return personRequestDto;
        }
    }
}
