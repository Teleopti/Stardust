using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job.Steps
{
	[TestFixture]
	public class StageGroupPagePersonJobStepTest
	{
		private StageGroupPagePersonJobStep _target;
		private IGroupPagePersonTransformer _transformer;
		private MockRepository _mocks;
		private IRaptorRepository _repository;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_transformer = _mocks.StrictMock<IGroupPagePersonTransformer>();
			_repository = _mocks.StrictMock<IRaptorRepository>();

			IJobParameters jobParameters = JobParametersFactory.SimpleParameters(false);
			jobParameters.Helper = new JobHelperForTest(_repository, null);
			_target = new StageGroupPagePersonJobStep(jobParameters);
			_target.Transformer = _transformer;
		}

		[Test]
		public void ShouldHaveThePropertiesSetCorrect()
		{
			Assert.AreEqual("stg_group_page_person", _target.Name);
			Assert.AreEqual(JobCategoryType.DoNotNeedDatePeriod, _target.JobCategory);
			Assert.IsFalse(_target.IsBusinessUnitIndependent);
		}

		[Test]
		public void ShouldTransformGroupPagesToDataTable()
		{
			using (_mocks.Record())
			{
				Expect.Call(_transformer.BuiltInGroupPages).Return(new List<IGroupPage>());
				Expect.Call(_transformer.UserDefinedGroupings).Return(new List<IGroupPage>());
				Expect.Call(() => _transformer.Transform(new List<IGroupPage>(), new List<IGroupPage>(), null)).IgnoreArguments();
				Expect.Call(_repository.PersistGroupPagePerson(null)).Return(0).IgnoreArguments();
			}
			using (_mocks.Playback())
			{
				IJobStepResult result = _target.Run(new List<IJobStep>(), null, null, false);
				Assert.AreEqual(0, result.RowsAffected);
			}
		}
	}
}
