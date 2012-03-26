using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Win.Forecasting;
using Teleopti.Ccc.WinCode.Forecasting;

namespace Teleopti.Ccc.WinCodeTest.Forecasting
{
    [TestFixture]
    public class DetailedJobHistoryPresenterTest
    {
        private DetailedJobHistoryPresenter target;
        private MockRepository mocks;
        private IJobHistoryView view;
        private IDetailedJobHistoryProvider jobHistoryProvider;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            //view = mocks.DynamicMock<IDetailedJobHistoryView>();
            view = mocks.DynamicMock<IJobHistoryView>();
            jobHistoryProvider = mocks.DynamicMock<IDetailedJobHistoryProvider>();
            target = new DetailedJobHistoryPresenter(view, jobHistoryProvider);
        }

        [Test]
        public void ShouldInitialize()
        {
            var jobModel = new JobResultModel{JobId = new Guid(), JobCategory = "Forecast Import", Owner = "talham", Status = "Done"};
            var details = new List<DetailedJobHistoryResultModel>();
            using (mocks.Record())
            {
                Expect.Call(jobHistoryProvider.GetHistory(jobModel)).Return(details);
                Expect.Call(() => view.BindJobDetailData(details));
            }
            using (mocks.Playback())
            {
               
                target.LoadDetailedHistory(jobModel);
            }
        }


    }
}
