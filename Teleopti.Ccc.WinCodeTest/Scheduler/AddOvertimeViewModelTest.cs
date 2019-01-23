using System;
using System.Collections.Generic;
using System.Windows.Data;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class AddOvertimeViewModelTest
    {
        private IList<IActivity> _acticvities;
        private IList<IMultiplicatorDefinitionSet> _definitionSets;
        private IActivity _defaultActivity;
        private DateTimePeriod _period;
        private AddOvertimeViewModel _target;
        private TimeZoneInfo _timeZoneInfo;
        private DateTimePeriod _defaultTimePeriod;

        [SetUp]
        public void Setup()
        {
            _timeZoneInfo = (TimeZoneInfo.Utc);
            _period = new DateTimePeriod(2001, 1, 1, 2001, 1, 2);
	        var startDateTimeLocal = _period.StartDateTimeLocal(TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);
	        _defaultTimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(startDateTimeLocal.Add(TimeSpan.FromHours(9)), startDateTimeLocal.Add(TimeSpan.FromHours(13)), TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);
            _defaultActivity = ActivityFactory.CreateActivity("default");
            _acticvities = new List<IActivity> { ActivityFactory.CreateActivity("name"), _defaultActivity, ActivityFactory.CreateActivity("anotherName") };
            _definitionSets = new List<IMultiplicatorDefinitionSet>(){MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("m1",MultiplicatorType.OBTime),
            MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("m2",MultiplicatorType.Overtime)};
            _target = new AddOvertimeViewModel(_acticvities, _definitionSets, _defaultActivity, new SetupDateTimePeriodToDefaultLocalHours(_defaultTimePeriod, UserTimeZone.Make(), _timeZoneInfo), TimeSpan.FromMinutes(10));
        }

        [Test]
        public void VerifyDefaultValues()
        {
            Assert.AreEqual(_defaultActivity, _target.SelectedItem, "The default activity should be selected");
            Assert.AreEqual(UserTexts.Resources.AddOvertime, _target.Title, "Title");
            Assert.IsFalse(_target.PeriodViewModel.AutoUpdate);
            Assert.IsFalse(_target.Result);
            Assert.IsTrue(_target.CanOk);
        }

        [Test]
        public void VerifySelectItem()
        {
            IActivity selected = _acticvities[0];
            Assert.IsTrue(CollectionViewSource.GetDefaultView(_target.Payloads).MoveCurrentTo(selected));
            Assert.AreEqual(selected, _target.SelectedItem);
        }

        [Test]
        public void VerifySelectedMultiplicatorDefinitionSet()
        {
            IMultiplicatorDefinitionSet selected = _definitionSets[1];
            //Make sure its not selected:
            Assert.AreNotEqual(selected,_target.SelectedMultiplicatorDefinitionSet);
            //Select it
            _target.MultiplicatorDefinitionSet.MoveCurrentTo(selected);
            Assert.AreEqual(selected, _target.SelectedMultiplicatorDefinitionSet);

        }

        [Test]
        public void VerifyCannotOkIfNoMultiplicatorDefinitionsAreAvailable()
        {
            _target = new AddOvertimeViewModel(_acticvities, new List<IMultiplicatorDefinitionSet>(), _defaultActivity, new SetupDateTimePeriodToDefaultPeriod(_period), TimeSpan.FromMinutes(20));
            Assert.IsFalse(_target.CanOk, "Cannot press ok if there arent any multiplicator definitionsets");
        }

        [Test]
        public void VerifyThatPeriodIsGeneratedFromSetup()
        {
            var mocks = new MockRepository();
            var setupDateTimePeriod = mocks.StrictMock<ISetupDateTimePeriod>();
            using(mocks.Record())
            {
                Expect.Call(setupDateTimePeriod.Period).Return(_period).Repeat.AtLeastOnce();
            }
            using(mocks.Playback())
            {
                _target = new AddOvertimeViewModel(_acticvities, new List<IMultiplicatorDefinitionSet>(), _defaultActivity, setupDateTimePeriod, TimeSpan.FromMinutes(20));
                Assert.AreEqual(setupDateTimePeriod.Period.StartDateTime,_target.PeriodViewModel.Start);
                Assert.AreEqual(setupDateTimePeriod.Period.EndDateTime,_target.PeriodViewModel.End);
            }

        }

        [Test]
        public void VerifyThatCorrectDefaultTimeIsPopulated()
        {
            var period = new DateTimePeriod(2014, 02, 10, 10, 2014, 02, 10, 11);
            _target = new AddOvertimeViewModel(_acticvities, new List<IMultiplicatorDefinitionSet>(), _defaultActivity, new SetupDateTimePeriodToDefaultPeriod(period), TimeSpan.FromMinutes(20));
            Assert.AreEqual(_target.PeriodViewModel.Start,period.StartDateTime  );
            Assert.AreEqual(_target.PeriodViewModel.End ,period.EndDateTime  );
        }



    }
}
