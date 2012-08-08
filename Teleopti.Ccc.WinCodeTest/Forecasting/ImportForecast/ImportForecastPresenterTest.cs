using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Models;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Views;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Presenters;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.WinCodeTest.Forecasting.ImportForecast
{
    [TestFixture]
    public class ImportForecastPresenterTest
    {
        private MockRepository _mocks;
        private ImportForecastModel _model;
        private ImportForecastPresenter _target;
        private IImportForecastView _view;
        private ISkill _skill;
        private IValidateImportForecastFileCommand _validateImportForecastFileCommand;
        private ISendCommandToSdk _sendCommandToSdk;
        private ISaveImportForecastFileCommand _saveImportForecastFileCommand;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _skill = SkillFactory.CreateSkillWithWorkloadAndSources();
            _view = _mocks.StrictMock<IImportForecastView>();
            _model = new ImportForecastModel();
            _validateImportForecastFileCommand = _mocks.StrictMock<IValidateImportForecastFileCommand>();
            _saveImportForecastFileCommand = _mocks.StrictMock<ISaveImportForecastFileCommand>();
            _sendCommandToSdk = _mocks.StrictMock<ISendCommandToSdk>();
            _target = new ImportForecastPresenter(_view, _model, _saveImportForecastFileCommand, _validateImportForecastFileCommand, _sendCommandToSdk);
        }

        [Test]
        public void ShouldInitialize()
        {
            var workload = _skill.WorkloadCollection.FirstOrDefault();
            _model.SelectedSkill = _skill;
            using (_mocks.Record())
            {
                Expect.Call(() => _view.SetSkillName(_skill.Name));
                Expect.Call(() => _view.SetWorkloadName(workload.Name));
                Expect.Call(() => _view.SetVisibility(_skill.SkillType));
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
            }
        }

        [Test]
        public void ShouldStartImport()
        {
            string fileName = "C:\\Test.csv";
            Guid jobId = Guid.NewGuid();
            _model.FileId = Guid.NewGuid();
            _model.HasValidationError = false;
            _model.SelectedSkill = _skill;

            using(_mocks.Record())
            {
                Expect.Call(()=>_validateImportForecastFileCommand.Execute(fileName));
                Expect.Call(()=>_saveImportForecastFileCommand.Execute(fileName));
                Expect.Call(_sendCommandToSdk.ExecuteCommand(null)).IgnoreArguments().Return(new CommandResultDto{AffectedId = jobId});
                Expect.Call(()=>_view.ShowStatusDialog(jobId));
            }
            using(_mocks.Playback())
            {
                _target.StartImport(fileName);
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ShouldThrowInvalidOperationException()
        {
            _model.SelectedSkill = _skill;

            using (_mocks.Record())
            {
                Expect.Call(() => _view.SetSkillName(_skill.Name));
                Expect.Call(_model.SelectedWorkload()).Return(null);
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
            }
        }

        [Test]
        public void ShouldDetectValidationError()
        {
            string fileName = "test.csv";
            _model.FileId = Guid.NewGuid();
            _model.HasValidationError = true;
            _model.SelectedSkill = _skill;

            using(_mocks.Record())
            {
                Expect.Call(() => _validateImportForecastFileCommand.Execute(fileName));
                Expect.Call(() => _view.ShowValidationException(null));
            }
            using(_mocks.Playback())
            {
                _target.StartImport(fileName);
            }
        }

        [Test]
        public void ShouldFailedWhenFileIdIsEmpty()
        {
            string fileName = "test.csv";
            _model.FileId = Guid.Empty;
            _model.HasValidationError = false;
            _model.SelectedSkill = _skill;

            using (_mocks.Record())
            {
                Expect.Call(() => _validateImportForecastFileCommand.Execute(fileName));
                Expect.Call(() => _saveImportForecastFileCommand.Execute(fileName));
                Expect.Call(() => _view.ShowError(null)).IgnoreArguments();
                
            }
            using (_mocks.Playback())
            {
                _target.StartImport(fileName);
            }
        }

        [Test]
        public void ShouldSetImportType()
        {
            _target.SetImportType(ImportForecastsMode.ImportWorkloadAndStaffing);
            Assert.IsNotNull(_model.ImportMode);
        }
    }
}
