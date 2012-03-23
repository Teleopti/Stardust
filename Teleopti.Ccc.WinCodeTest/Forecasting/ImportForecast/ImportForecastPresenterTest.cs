using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Models;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Views;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Presenters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Forecasting.ImportForecast
{
    [TestFixture]
    public class ImportForecastPresenterTest
    {
        private MockRepository _mocks;
        private IImportForecastModel _model;
        private ImportForecastPresenter _target;
        private IImportForecastView _view;
        private ISkill _skill;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _skill = SkillFactory.CreateSkillWithWorkloadAndSources();
            _view = _mocks.StrictMock<IImportForecastView>();
            _model = _mocks.StrictMock<IImportForecastModel>();
            _target = new ImportForecastPresenter(_view, _model);
        }

        [Test]
        public void ShouldInitializeSkillName()
        {
            using (_mocks.Record())
            {
                Expect.Call(_model.GetSelectedSkillName()).Return(_skill.Name);
            }
            using (_mocks.Playback())
            {
                _target.GetSelectedSkillName();
                Assert.IsNotNull(_target.SkillName);
            }
        }

        [Test]
        public void ShouldPopulateWorkload()
        {
            var workload = _skill.WorkloadCollection.FirstOrDefault();
            using (_mocks.Record())
            {
                Expect.Call(_model.LoadWorkload()).Return(workload);
            }
            using (_mocks.Playback())
            {
                _target.PopulateWorkload();
                Assert.That(_target.Workload, Is.EqualTo(workload));
            }
        }
        
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ShouldHandleEmptyWorkload()
        {
            using (_mocks.Record())
            {
                Expect.Call(_model.LoadWorkload()).Return(null);
            }
            using (_mocks.Playback())
            {
                _target.PopulateWorkload();
            }
        }

        [Test]
        public void ShouldReturnCorrectOption()
        {
            using(_mocks.Record())
            {
                Expect.Call(_view.IsWorkloadImport).Return(true);
            }
            using (_mocks.Playback())
            {
                Assert.That(_target.GetImportForecastOption(), Is.EqualTo(ImportForecastsOptionsDto.ImportWorkload));
            }
        }
    }
}
