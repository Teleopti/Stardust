using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.WinCode.Forecasting;

namespace Teleopti.Ccc.WinCodeTest.Forecasting
{
    [TestFixture]
    public class DetailedJobHistoryPresenterTest
    {
        private DetailedJobHistoryPresenter _target;
        private MockRepository _mocks;
        private IJobHistoryView _view;
        private IDetailedJobHistoryProvider _jobHistoryProvider;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _view = _mocks.DynamicMock<IJobHistoryView>();
            _jobHistoryProvider = _mocks.DynamicMock<IDetailedJobHistoryProvider>();
            _target = new DetailedJobHistoryPresenter(_view, _jobHistoryProvider);
        }

        [Test]
        public void ShouldInitialize()
        {
            var jobModel = new JobResultModel{JobId = new Guid(), JobCategory = "Forecast Import", Owner = "talham", Status = "Done"};
            var details = new List<DetailedJobHistoryResultModel>();
            using (_mocks.Record())
            {
                Expect.Call(_jobHistoryProvider.GetHistory(jobModel)).Return(details);
                Expect.Call(() => _view.BindJobDetailData(details));
            }
            using (_mocks.Playback())
            {
               
                _target.LoadDetailedHistory(jobModel);
            }
        }
    }
}
