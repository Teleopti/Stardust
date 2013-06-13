using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.AgentPortalCode.AgentPreference;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Common.Clipboard;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.AgentPortalCodeTest.AgentPreference
{
    [TestFixture]
    public class PreferencePresenterTest
    {
        private MockRepository _mocks;
        private IPreferenceView _view;
        private IPreferenceModel _model;
        private ClipHandler<IPreferenceCellData> _clipHandler;
        private IToggleButtonState _toggleButtonState;
        private PreferencePresenter _target;
        private IAgentScheduleStateHolder _scheduleStateHolder;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _view = _mocks.StrictMock<IPreferenceView>();
            _model = _mocks.StrictMock<IPreferenceModel>();
            _scheduleStateHolder = _mocks.StrictMock<IAgentScheduleStateHolder>();
            _clipHandler = new ClipHandler<IPreferenceCellData>();
            _toggleButtonState = _mocks.StrictMock<IToggleButtonState>();
            _target = new PreferencePresenter(_model,_view,_clipHandler,_toggleButtonState, _scheduleStateHolder);
        }

        [Test]
        public void HasOpenDaysShouldReturnFalseIfNoEnabledDays()
        {
            Expect.Call(_model.CellDataCollection).Return(new Dictionary<int, IPreferenceCellData>());
            _mocks.ReplayAll();
            var result = _target.HasOpenDays();
            Assert.That(result,Is.False);
            _mocks.VerifyAll();
        }

        [Test]
        public void HasOpenDaysShouldReturnTrueIfHasEnabledDay()
        {
            var cellData = _mocks.StrictMock<IPreferenceCellData>();
            var dic = new Dictionary<int, IPreferenceCellData> {{1, cellData}};
            Expect.Call(_model.CellDataCollection).Return(dic);
            Expect.Call(cellData.Enabled).Return(true);
            _mocks.ReplayAll();
            var result = _target.HasOpenDays();
            Assert.That(result, Is.True);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldCopyPreferenceData()
        {
            var cellData = new PreferenceCellData();
            var timeLimitationValidator = _mocks.StrictMock<ITimeLimitationValidator>();
            var start = new TimeLimitation(timeLimitationValidator);
            var end = new TimeLimitation(timeLimitationValidator);
            var work = new TimeLimitation(timeLimitationValidator);

            start.MinTime = TimeSpan.FromHours(8);
            end.MinTime = TimeSpan.FromHours(9);
            work.MinTime = TimeSpan.FromHours(1);
            
            var preference = new Preference
                                 {
                                     DayOff = new DayOff("name", "shortName", Guid.NewGuid(), Color.Empty),
                                     ShiftCategory = new ShiftCategory("name", "shortName", Guid.NewGuid(), Color.Empty),
                                     Absence = new Absence("name", "shortName", Guid.NewGuid(), Color.Empty),
                                     TemplateName = "templateName",
                                     StartTimeLimitation = start,
                                     EndTimeLimitation = end,
                                     WorkTimeLimitation = work,
                                     Activity = new Activity(Guid.NewGuid(), "name")
                                 };

            cellData.Preference = preference;
            var dic = new Dictionary<int, IPreferenceCellData> { { 0, cellData } };

            using(_mocks.Record())
            {
                Expect.Call(_model.CellDataCollection).Return(dic).Repeat.AtLeastOnce();
                Expect.Call(() => _toggleButtonState.ToggleButtonEnabled("clipboardControl", true));
                Expect.Call(() => _view.ToggleStateContextMenuItemPaste(true));
            }

            using(_mocks.Playback())
            {
                _target.OnSetCellDataClip(1,1,1,1);
                var newPreference = _target.CellDataClipHandler.ClipList[0].ClipValue.Preference;
                Assert.AreNotSame(newPreference, preference);
                Assert.AreEqual(newPreference.DayOff, preference.DayOff);
                Assert.AreEqual(newPreference.ShiftCategory, preference.ShiftCategory);
                Assert.AreEqual(newPreference.Absence, preference.Absence);
                Assert.AreEqual(newPreference.TemplateName, preference.TemplateName);
                Assert.AreEqual(newPreference.StartTimeLimitation.MinTime, preference.StartTimeLimitation.MinTime);
                Assert.AreEqual(newPreference.EndTimeLimitation.MinTime, preference.EndTimeLimitation.MinTime);
                Assert.AreEqual(newPreference.WorkTimeLimitation.MinTime, preference.WorkTimeLimitation.MinTime);
                Assert.AreEqual(newPreference.Activity, preference.Activity);
            }
        }

        [Test]
        public void ShouldPastePreferenceData()
        {
            var cellData = new PreferenceCellData();
            var timeLimitationValidator = _mocks.StrictMock<ITimeLimitationValidator>();
            var start = new TimeLimitation(timeLimitationValidator);
            var end = new TimeLimitation(timeLimitationValidator);
            var work = new TimeLimitation(timeLimitationValidator);

            start.MinTime = TimeSpan.FromHours(8);
            end.MinTime = TimeSpan.FromHours(9);
            work.MinTime = TimeSpan.FromHours(1);

            var preference = new Preference
            {
                TemplateName = "templateName",
                StartTimeLimitation = start,
                EndTimeLimitation = end,
                WorkTimeLimitation = work,
                Activity = new Activity(Guid.NewGuid(), "name")
            };

            cellData.Preference = preference;
            _target.CellDataClipHandler.AddClip(cellData,ScheduleAppointmentTypes.PreferenceRestriction, DateTime.Today);
            var dic = new Dictionary<int, IPreferenceCellData> { { 0, cellData } };

            using (_mocks.Record())
            {
                Expect.Call(_model.CellDataCollection).Return(dic).Repeat.AtLeastOnce();
                Expect.Call(()=> _scheduleStateHolder.UpdateOrAddPreference(new List<DateTime>{ DateTime.Today}, null)).IgnoreArguments();
                Expect.Call(() => _toggleButtonState.ToggleButtonEnabled("toolStripButtonValidate", true));
                Expect.Call(() => _view.CellDataLoaded());
                Expect.Call(() => _view.SetValidationPicture(null)).IgnoreArguments();
            }
            using (_mocks.Playback())
            {
                _target.PasteClipsInCellData(new List<int>{0});
                var newPreference = _target.CellDataClipHandler.ClipList[0].ClipValue.Preference;
                Assert.AreNotSame(newPreference, preference);
                Assert.AreEqual(newPreference.DayOff, preference.DayOff);
                Assert.AreEqual(newPreference.ShiftCategory, preference.ShiftCategory);
                Assert.AreEqual(newPreference.Absence, preference.Absence);
                Assert.AreEqual(newPreference.TemplateName, preference.TemplateName);
                Assert.AreEqual(newPreference.StartTimeLimitation.MinTime, preference.StartTimeLimitation.MinTime);
                Assert.AreEqual(newPreference.EndTimeLimitation.MinTime, preference.EndTimeLimitation.MinTime);
                Assert.AreEqual(newPreference.WorkTimeLimitation.MinTime, preference.WorkTimeLimitation.MinTime);
                Assert.AreEqual(newPreference.Activity, preference.Activity);
            }
        }

        [Test]
        public void ShouldAddAbsenceToExtendedPreferenceTemplateDto()
        {
            var dto = new ExtendedPreferenceTemplateDto();

            var preference = new Preference
                {
                    Absence = new Absence("name", "shortName", Guid.NewGuid(), Color.DeepSkyBlue)     
                };

            var cellData = new PreferenceCellData {Preference = preference};

            _target.AddAbsenceToExtendedPreferenceTemplateDto(dto, cellData);

            Assert.AreEqual(preference.Absence.Id, dto.Absence.Id);
            Assert.AreEqual(preference.Absence.Name, dto.Absence.Name);
            Assert.AreEqual(preference.Absence.ShortName, dto.Absence.ShortName);
            Assert.AreEqual(preference.Absence.Color.R, dto.Absence.DisplayColor.Red);
            Assert.AreEqual(preference.Absence.Color.G, dto.Absence.DisplayColor.Green);
            Assert.AreEqual(preference.Absence.Color.B, dto.Absence.DisplayColor.Blue);
        }
    }

}