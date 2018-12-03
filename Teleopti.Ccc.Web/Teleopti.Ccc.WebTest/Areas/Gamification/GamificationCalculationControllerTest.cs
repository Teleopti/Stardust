using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Gamification.Controller;
using Teleopti.Ccc.Web.Areas.Gamification.Models;
using Teleopti.Ccc.Web.Core.IoC;


namespace Teleopti.Ccc.WebTest.Areas.Gamification
{
	[DomainTest]
	[TestFixture]
	public class GamificationCalculationControllerTest : IExtendSystem
	{
		public GamificationCalculationController Target;
		public FakeAgentBadgeWithRankTransactionRepository AgentBadgeWithRankTransactionRepository;
		public FakeAgentBadgeTransactionRepository AgentBadgeTransactionRepository;
		public FakeJobResultRepository JobResultRepository;
		public FakePurgeSettingRepository PurgeSettingRepository;
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddModule(new WebModule(configuration, null));
		}

		[Test]
		public void ShouldResetAgentBadges()
		{
			AgentBadgeTransactionRepository.Add(new AgentBadgeTransaction());
			AgentBadgeWithRankTransactionRepository.Add(new AgentBadgeWithRankTransaction());

			Target.ResetBadge();

			AgentBadgeTransactionRepository.LoadAll().Any().Should().Be.False();
			AgentBadgeWithRankTransactionRepository.LoadAll().Any().Should().Be.False();
		}

		[Test]
		public void ShouldCreatNewJob()
		{
			var form = new RecalculateForm {Start = DateTime.Today, End = DateTime.Today};
			var peroid = DateOnly.Today.ToDateOnlyPeriod();
			Target.NewRecalculateBadgeJob(form);

			var jobResult = JobResultRepository.LoadAll().ToList();
			jobResult.Count.Should().Be.EqualTo(1);
			jobResult[0].JobCategory.Should().Be.EqualTo(JobCategory.WebRecalculateBadge);
			jobResult[0].Period.Should().Be.EqualTo(peroid);
		}

		[Test]
		public void ShouldGetJobListWithFinishedResult()
		{
			var period = DateOnly.Today.ToDateOnlyPeriod();
			var person = PersonFactory.CreatePersonWithId(Guid.NewGuid());
			var jobResult = new JobResult(JobCategory.WebRecalculateBadge, period, person, DateTime.UtcNow);
			jobResult.SetId(Guid.NewGuid());
			jobResult.FinishedOk = true;
			JobResultRepository.Add(jobResult);

			var result = Target.GetJobList();

			result.Count.Should().Be.EqualTo(1);
			result[0].Id.Should().Be.EqualTo(jobResult.Id);
			result[0].CreateDateTime.Should().Be.EqualTo(jobResult.Timestamp);
			result[0].Owner.Should().Be.EqualTo(jobResult.Owner.Name.ToString());
			result[0].StartDate.Should().Be.EqualTo(jobResult.Period.StartDate.Utc());
			result[0].EndDate.Should().Be.EqualTo(jobResult.Period.EndDate.Utc());
			result[0].Status.Should().Be.EqualTo(GamificationJobStatus.Finished.ToString().ToLower());
			result[0].HasError.Should().Be.EqualTo(false);
			result[0].ErrorMessage.Should().Be.EqualTo("");
		}

		[Test]
		public void ShouldSortJobListByStartTime()
		{
			var startTime = DateTime.UtcNow;
			var period = DateOnly.Today.ToDateOnlyPeriod();
			var person = PersonFactory.CreatePersonWithId(Guid.NewGuid());
			var jobResult1 = new JobResult(JobCategory.WebRecalculateBadge, period, person, startTime);
			jobResult1.SetId(Guid.NewGuid());
			jobResult1.FinishedOk = true;
			var jobResult2 = new JobResult(JobCategory.WebRecalculateBadge, period, person, startTime.AddHours(1));
			jobResult2.SetId(Guid.NewGuid());
			jobResult2.FinishedOk = true;
			JobResultRepository.Add(jobResult1);
			JobResultRepository.Add(jobResult2);

			var result = Target.GetJobList();

			result.First().Id.Should().Be.EqualTo(jobResult2.Id);
			result.Last().Id.Should().Be.EqualTo(jobResult1.Id);
		}

		[Test]
		public void ShouldGetJobListWithInternalErrorResult()
		{
			var period = DateOnly.Today.ToDateOnlyPeriod();
			var person = PersonFactory.CreatePersonWithId(Guid.NewGuid());
			var jobResult = new JobResult(JobCategory.WebRecalculateBadge, period, person, DateTime.UtcNow);
			jobResult.SetId(Guid.NewGuid());
			jobResult.FinishedOk = true;
			jobResult.AddDetail(new JobResultDetail(DetailLevel.Error, "errorMessage", DateTime.Now, new Exception()));
			JobResultRepository.Add(jobResult);

			var result = Target.GetJobList();

			result.Count.Should().Be.EqualTo(1);
			result[0].Status.Should().Be.EqualTo(GamificationJobStatus.Failed.ToString().ToLower());
			result[0].HasError.Should().Be.EqualTo(true);
			result[0].ErrorMessage.Should().Be.EqualTo(Resources.InternalErrorMsg);
		}

		[Test]
		public void ShouldGetJobListWithErrorDetailResult()
		{
			var period = DateOnly.Today.ToDateOnlyPeriod();
			var person = PersonFactory.CreatePersonWithId(Guid.NewGuid());
			var jobResult = new JobResult(JobCategory.WebRecalculateBadge, period, person, DateTime.UtcNow);
			jobResult.SetId(Guid.NewGuid());
			jobResult.AddDetail(new JobResultDetail(DetailLevel.Error, "detailError", DateTime.Now, null));
			JobResultRepository.Add(jobResult);

			var result = Target.GetJobList();

			result.Count.Should().Be.EqualTo(1);
			result[0].Status.Should().Be.EqualTo(GamificationJobStatus.Failed.ToString().ToLower());
			result[0].HasError.Should().Be.EqualTo(true);
			result[0].ErrorMessage.Should().Be.EqualTo("detailError");
		}

		[Test]
		public void ShouldGetExternalPerformanceDataPurgeDays()
		{
			var days = Target.GetPrugeDays();
			days.Should().Be.EqualTo(60);
		}
	}
}
