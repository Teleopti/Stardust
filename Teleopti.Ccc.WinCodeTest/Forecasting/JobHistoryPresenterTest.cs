using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting;

namespace Teleopti.Ccc.WinCodeTest.Forecasting
{
	[TestFixture]
	public class JobHistoryPresenterTest
	{
		private JobHistoryPresenter target;
		private MockRepository mocks;
		private IJobHistoryView view;
		private IJobResultProvider jobResultProvider;
		private PagingDetail pagingDetail;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			view = mocks.DynamicMock<IJobHistoryView>();
			jobResultProvider = mocks.DynamicMock<IJobResultProvider>();
			pagingDetail = new PagingDetail {Take = 20};
			target = new JobHistoryPresenter(view,jobResultProvider,pagingDetail);
		}

		[Test]
		public void ShouldInitialize()
		{
			var jobResultModels = new List<JobResultModel>();
			using (mocks.Record())
			{
				Expect.Call(() => view.BindJobResultData(jobResultModels));
				Expect.Call(jobResultProvider.GetJobResults(pagingDetail)).Return(jobResultModels);
			}
			using (mocks.Playback())
			{
				target.Initialize();
			}
		}

		[Test]
		public void ShouldGoToNextResultPage()
		{
			var jobResultModels = new List<JobResultModel>();
			using (mocks.Record())
			{
				Expect.Call(jobResultProvider.GetJobResults(pagingDetail)).Return(jobResultModels);
			}
			using (mocks.Playback())
			{
				target.Next();
			}
		}

		[Test]
		public void ShouldGoToPreviousResultPage()
		{
			var jobResultModels = new List<JobResultModel>();
			using (mocks.Record())
			{
				Expect.Call(jobResultProvider.GetJobResults(pagingDetail)).Return(jobResultModels);
			}
			using (mocks.Playback())
			{
				target.Previous();
			}
		}

		[Test]
		public void ShouldReloadHistoryAfterLoadingMoreData()
		{
			var jobResultModels = new List<JobResultModel>();
			using (mocks.Record())
			{
				Expect.Call(() => view.BindJobResultData(jobResultModels));

				Expect.Call(jobResultProvider.GetJobResults(pagingDetail)).Return(jobResultModels);
			}
			using (mocks.Playback())
			{
				target.ReloadHistory();
			}
		}

		[Test]
		public void ShouldEnableNext()
		{
			using (mocks.Record())
			{
				view.ToggleNext(true);
			}
			using (mocks.Playback())
			{
				pagingDetail.TotalNumberOfResults = pagingDetail.Take + 1;
			}
		}

		[Test]
		public void ShouldEnablePrevious()
		{
			using (mocks.Record())
			{
				view.TogglePrevious(true);
			}
			using (mocks.Playback())
			{
				pagingDetail.Skip = pagingDetail.Take;
			}
		}
	}
}
