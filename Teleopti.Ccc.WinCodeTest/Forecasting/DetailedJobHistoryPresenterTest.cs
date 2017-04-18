using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting;

namespace Teleopti.Ccc.WinCodeTest.Forecasting
{
    [TestFixture]
    public class DetailedJobHistoryPresenterTest
    {
        private JobHistoryPresenter target;
        private MockRepository mocks;
        private IJobHistoryView view;
        private IJobResultProvider jobResultProvider;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            view = mocks.DynamicMock<IJobHistoryView>();
            jobResultProvider = mocks.DynamicMock<IJobResultProvider>();
            target = new JobHistoryPresenter(view, jobResultProvider, new PagingDetail());
        }

        [Test]
        public void ShouldInitialize()
        {
            var jobModel = new JobResultModel{JobId = Guid.NewGuid(), JobCategory = "Forecast Import", Owner = "talham", Status = "Done"};
            var details = new List<JobResultDetailModel>();
            using (mocks.Record())
            {
	            Expect.Call(view.DetailLevel).Return(1);
                Expect.Call(jobResultProvider.GetJobResultDetails(jobModel, 1)).Return(details);
                Expect.Call(() => view.BindJobResultDetailData(details));
            }
            using (mocks.Playback())
            {
               
                target.LoadDetailedHistory(jobModel);
            }
        }
    }
}
