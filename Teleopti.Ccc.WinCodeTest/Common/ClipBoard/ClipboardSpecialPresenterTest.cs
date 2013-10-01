using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Common.Clipboard
{
    [TestFixture]
    public class ClipboardSpecialPresenterTest
    {
        private MockRepository _mocks;
        private IClipboardSpecialView _view;
        private PasteOptions _model;
        private ClipboardSpecialPresenterTestClass _presenter;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _view = _mocks.StrictMock<IClipboardSpecialView>();
            _model = new PasteOptions();
            _presenter = new ClipboardSpecialPresenterTestClass(_view, _model, false, true);
        }

        [Test]
        public void VerifyInitialize()
        {
            var authorization = _mocks.StrictMock<IPrincipalAuthorization>();
            using (_mocks.Record())
            {
                _view.SetTexts();
                _view.SetColor();

                Expect.Call(authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence)).Return(true).Repeat.Once();
                Expect.Call(authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment)).Return(true).Repeat.Times(5);
                Expect.Call(authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonRestriction)).Return(true).Repeat.Once();
                
                _view.SetPermissionOnAbsences(true);
                _view.SetPermissionOnDayOffs(true);
                _view.SetPermissionOnPersonalAssignments(true);
                _view.SetPermissionOnAssignments(true);
                _view.SetPermissionOnOvertime(true);
                _view.SetPermissionsOnRestrictions(true);
				_view.SetPermissionsOnShiftAsOvertime(true);
                _view.ShowRestrictions(true);
            }

            using (_mocks.Playback())
            {
                using(new CustomAuthorizationContext(authorization))
                {
                    _presenter.Initialize();
                }
            }
        }

        [Test]
        public void VerifyIsCanceled()
        {
            Assert.IsFalse(_presenter.IsCanceled());

            _presenter.OnButtonCancelClick();
            Assert.IsTrue(_presenter.IsCanceled());
        }

        [Test]
        public void VerifyOnButtonOkClick()
        {
            using (_mocks.Record())
            {
                _view.HideForm();  
            }

            using (_mocks.Playback())
            {
               _presenter.OnButtonOkClick();
            }
        }

        [Test]
        public void VerifyOnButtonCancelClick()
        {
            using (_mocks.Record())
            {
                _view.HideForm();
            }

            using (_mocks.Record())
            {
                _presenter.OnButtonCancelClick();
            }

            Assert.IsTrue(_presenter.IsCanceled());
        }

        [Test]
        public void VerifyOnFormClosing()
        {
            _presenter.OnFormClosing();
            Assert.IsTrue(_presenter.IsCanceled());
        }

        [Test]
        public void VerifyOnCheckBoxAssignmentsCheckedChanged()
        {
            _presenter.OnCheckBoxAssignmentsCheckedChanged(true);
            Assert.IsTrue(_model.MainShift);

            _presenter.OnCheckBoxAssignmentsCheckedChanged(false);
            Assert.IsFalse(_model.MainShift);
        }

        [Test]
        public void VerifyOnCheckBoxAbsencesCheckedChanged()
        {
            _presenter.OnCheckBoxAbsencesCheckedChanged(true);
            Assert.AreEqual(PasteAction.Add, _model.Absences);

            _presenter = new ClipboardSpecialPresenterTestClass(_view, _model, true, true);
            _presenter.OnCheckBoxAbsencesCheckedChanged(true);
            Assert.AreEqual(PasteAction.Replace,_model.Absences);

            _presenter.OnCheckBoxAbsencesCheckedChanged(false);
            Assert.AreEqual(PasteAction.Ignore, _model.Absences);
        }

        [Test]
        public void VerifyOnCheckBoxDayOffsCheckedChanged()
        {
            _presenter.OnCheckBoxDayOffsCheckedChanged(true);
            Assert.IsTrue(_model.DayOff);

            _presenter.OnCheckBoxDayOffsCheckedChanged(false);
            Assert.IsFalse(_model.DayOff);
        }

        [Test]
        public void VerifyOnCheckBoxOvertimeCheckedChanged()
        {
            _presenter.OnCheckBoxOvertimeCheckedChanged(true);
            Assert.IsTrue(_model.Overtime);

            _presenter.OnCheckBoxOvertimeCheckedChanged(false);
            Assert.IsFalse(_model.Overtime);
        }

        [Test]
        public void VerifyOnCheckBoxPreferencesCheckedChanged()
        {
            _presenter.OnCheckBoxPreferencesCheckedChanged(true);
            Assert.IsTrue(_model.Preference);

            _presenter.OnCheckBoxPreferencesCheckedChanged(false);
            Assert.IsFalse(_model.Preference);
        }

        [Test]
        public void VerifyOnCheckBoxStudentAvailabilityCheckedChange()
        {
            _presenter.OnCheckBoxStudentAvailabilityCheckedChange(true);
            Assert.IsTrue(_model.StudentAvailability);

            _presenter.OnCheckBoxStudentAvailabilityCheckedChange(false);
            Assert.IsFalse(_model.StudentAvailability);
        }

		[Test]
		public void VerifyOnCheckBoxShiftAsOvertimeChange()
		{
			_presenter.OnCheckBoxShiftAsOvertimeCheckedChanged(true);
			Assert.IsTrue(_model.ShiftAsOvertime);

			_presenter.OnCheckBoxShiftAsOvertimeCheckedChanged(false);
			Assert.IsFalse(_model.ShiftAsOvertime);
		}

		[Test]
		public void VerifyOnComboboxAdvOvertimeSelectedIndexChanged()
		{
			var multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("name", MultiplicatorType.Overtime);
			_presenter.OnComboBoxAdvOvertimeSelectedIndexChanged(multiplicatorDefinitionSet);
			Assert.AreEqual(multiplicatorDefinitionSet, _model.MulitiplicatorDefinitionSet);
		}

        private class ClipboardSpecialPresenterTestClass : ClipboardSpecialPresenter
        {
            public ClipboardSpecialPresenterTestClass(IClipboardSpecialView view, PasteOptions model, bool deleteMode, bool showRestrictions)
                : base(view, model, deleteMode, showRestrictions)
            {
                
            }
        }
    }
}
