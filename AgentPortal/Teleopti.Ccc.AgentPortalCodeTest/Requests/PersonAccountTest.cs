using System;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.AgentPortalCode.Requests;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortalCodeTest.Requests
{
    [TestFixture]
    public class PersonAccountTest
    {
        private IPersonAccountView _view;
        private MockRepository _mocks;
        private PersonAccountPresenter _target;
        private ITeleoptiOrganizationService _service;
        private PersonDto _person;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _view = _mocks.StrictMock<IPersonAccountView>();
            _service = _mocks.StrictMock<ITeleoptiOrganizationService>();
            _person = new PersonDto {Id = Guid.NewGuid(), Name = "Kalle Anka"};
            _target = new PersonAccountPresenter(_view, _service, _person);
        }

        [Test]
        public void VerifyCanInitialize()
        {
            using(_mocks.Record())
            {
                _view.AccruedHeader = UserTexts.Resources.Accrued;
                _view.DescriptionHeader = UserTexts.Resources.Absence;
                _view.PeriodFromHeader = UserTexts.Resources.FromDate;
                _view.PeriodToHeader = UserTexts.Resources.To;
                _view.RemainingHeader = UserTexts.Resources.Remaining;
                _view.TypeOfValueHeader = UserTexts.Resources.Type;
                _view.UsedHeader = UserTexts.Resources.Used;
                _view.SetDateText(UserTexts.Resources.SelectedDateColon);
                _view.SetDate(DateTime.Today);
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
            }
        }

        [Test] //Fix later
        public void VerifyCanLoadDataWhenDateChanged()
        {
            using (_mocks.Record())
            {
                Expect.Call(_service.GetPersonAccounts(_person,
                                                       new DateOnlyDto
                                                           {DateTime = DateTime.Today})).
                    Return(new[]
                               {
                                   new PersonAccountDto
                                       {
                                           Accrued = TimeSpan.FromDays(25).Ticks,
                                           BalanceIn = TimeSpan.FromDays(3).Ticks,
                                           Extra = TimeSpan.FromDays(-1).Ticks,
                                           IsInMinutes = false,
                                           LatestCalculatedBalance = TimeSpan.FromDays(14).Ticks,
                                           Remaining = TimeSpan.FromDays(13).Ticks,
                                           Period =
                                               new DateOnlyPeriodDto
                                                   {
                                                       StartDate =
                                                           new DateOnlyDto
                                                               {
                                                                   DateTime = new DateTime(2009, 1, 1),
                                                               },
                                                       EndDate =
                                                           new DateOnlyDto
                                                               {
                                                                   DateTime = new DateTime(2009, 12, 31),
                                                               }
                                                   },
                                           TrackingDescription = "Holiday"
                                       },
                                   new PersonAccountDto
                                       {
                                           Accrued = TimeSpan.FromMinutes(300).Ticks,
                                           BalanceIn = TimeSpan.FromMinutes(-60).Ticks,
                                           Extra = TimeSpan.FromMinutes(40).Ticks,
                                           IsInMinutes = true,
                                           LatestCalculatedBalance = TimeSpan.FromMinutes(45).Ticks,
                                           Remaining = TimeSpan.FromMinutes(235).Ticks,
                                           Period =
                                               new DateOnlyPeriodDto
                                                   {
                                                       StartDate =
                                                           new DateOnlyDto
                                                               {
                                                                   DateTime = new DateTime(2009, 1, 1),
                                                               },
                                                       EndDate =
                                                           new DateOnlyDto
                                                               {
                                                                   DateTime = new DateTime(2009, 1, 1).AddDays(((int)TimeSpan.FromDays(3600).TotalDays)),
                                                               }
                                                   },
                                           TrackingDescription = "Time in lieu"
                                       }
                               }).IgnoreArguments();
                _view.DataLoaded();
            }
            using (_mocks.Playback())
            {
                _target.ChangeDate(DateTime.Today);
                Assert.AreEqual(2,_target.ItemCount);
                var item = _target.Items[0];

                Assert.AreEqual("Holiday",item.Description);
                Assert.AreEqual(UserTexts.Resources.Days, item.TypeOfValue);
                Assert.AreEqual(27.ToString(CultureInfo.CurrentCulture),item.Accrued);
                Assert.AreEqual(14.ToString(CultureInfo.CurrentCulture),item.Used);
                Assert.AreEqual(13.ToString(CultureInfo.CurrentCulture),item.Remaining);
                Assert.AreEqual(new DateTime(2009, 1, 1),item.PeriodFrom.DateTime);
                Assert.AreEqual(new DateTime(2009, 12, 31), item.PeriodTo.DateTime);
                Assert.AreEqual(new DateTime(2009, 12, 31), item.EndDate.DateTime);

                item = _target.Items[1];

                Assert.AreEqual("Time in lieu", item.Description);
                Assert.AreEqual(UserTexts.Resources.Hours, item.TypeOfValue);
                Assert.AreEqual(TimeHelper.GetLongHourMinuteTimeString(new TimeSpan(TimeSpan.FromMinutes(280).Ticks),CultureInfo.CurrentCulture), item.Accrued);
                Assert.AreEqual(TimeHelper.GetLongHourMinuteTimeString(new TimeSpan(TimeSpan.FromMinutes(45).Ticks), CultureInfo.CurrentCulture), item.Used);
                Assert.AreEqual(TimeHelper.GetLongHourMinuteTimeString(new TimeSpan(TimeSpan.FromMinutes(235).Ticks), CultureInfo.CurrentCulture), item.Remaining);
                Assert.AreEqual(new DateTime(2009, 1, 1), item.PeriodFrom.DateTime);
                Assert.AreEqual(new DateTime(2009, 1, 1).AddDays(((int)TimeSpan.FromDays(3600).TotalDays)), item.PeriodTo.DateTime);
                Assert.IsNull(item.EndDate);
            }
        }
    }
}
