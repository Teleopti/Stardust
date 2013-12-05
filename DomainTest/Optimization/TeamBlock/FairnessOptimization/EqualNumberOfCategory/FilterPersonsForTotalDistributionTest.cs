using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	[TestFixture]
	public class FilterPersonsForTotalDistributionTest
	{
		private MockRepository _mocks;
		private IFilterPersonsForTotalDistribution _target;
		private IScheduleMatrixPro _matrix1;
		private IScheduleMatrixPro _matrix2;
		private IScheduleMatrixPro _matrix3;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new FilterPersonsForTotalDistribution();
			_matrix1 = _mocks.StrictMock<IScheduleMatrixPro>();
			_matrix2 = _mocks.StrictMock<IScheduleMatrixPro>();
			_matrix3 = _mocks.StrictMock<IScheduleMatrixPro>();
		}

		[Test]
		public void ShouldRemovePersonsWithNullOrIncorrectWorkFlowControlSet()
		{
			var person1 = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();
			person2.WorkflowControlSet = new WorkflowControlSet();
			var person3 = PersonFactory.CreatePerson();
			var wfcs = new WorkflowControlSet();
			wfcs.UseShiftCategoryFairness = true;
			person3.WorkflowControlSet = wfcs;
			var allMatrixes = new List<IScheduleMatrixPro> {_matrix1, _matrix2, _matrix3};

			using (_mocks.Record())
			{
				Expect.Call(_matrix1.Person).Return(person1);
				Expect.Call(_matrix2.Person).Return(person2);
				Expect.Call(_matrix3.Person).Return(person3);
			}

			using (_mocks.Playback())
			{
				var result = _target.Filter(allMatrixes).ToList();
				Assert.That(result[0].Equals(person3));
			}
		}

	}
}