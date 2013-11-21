using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.ShiftCategoryFairness
{
	[TestFixture]
	public class ShiftCategoryFairnessReSchedulerTest
	{
		private MockRepository _mocks;
		private ShiftCategoryFairnessReScheduler _target;
		private OptimizationPreferences _optPrefs;
		private IGroupPersonBuilderForOptimization _groupPersonBuilder;
		private IGroupPersonConsistentChecker _groupPersonConsistChecker;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_optPrefs = new OptimizationPreferences();
			_groupPersonBuilder = _mocks.DynamicMock<IGroupPersonBuilderForOptimization>();
			_groupPersonConsistChecker = _mocks.DynamicMock<IGroupPersonConsistentChecker>();
			_target = new ShiftCategoryFairnessReScheduler(_optPrefs,_groupPersonBuilder,_groupPersonConsistChecker);
		}

		[Test]
		public void ShouldReturnFalseIfInconsistent()
		{
			var person = new Person();
			var groupPerson = _mocks.DynamicMock<IGroupPerson>();
			var dateOnly = new DateOnly(2012, 10, 2);

			Expect.Call(_groupPersonBuilder.BuildGroupPerson(person, dateOnly)).Return(groupPerson);
			Expect.Call(_groupPersonConsistChecker.AllPersonsHasSameOrNoneScheduled(groupPerson, dateOnly, null)).IgnoreArguments().Return(false);
			
			_mocks.ReplayAll();
			Assert.That(_target.Execute(new List<IPerson> { person }, dateOnly, new List<IScheduleMatrixPro>()), Is.False);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnFalseIfCannotSchedule()
		{
			var person = new Person();
			var groupPerson = _mocks.DynamicMock<IGroupPerson>();
			var dateOnly = new DateOnly(2012, 10, 2);

			Expect.Call(_groupPersonBuilder.BuildGroupPerson(person, dateOnly)).Return(groupPerson);
			Expect.Call(_groupPersonConsistChecker.AllPersonsHasSameOrNoneScheduled(groupPerson, dateOnly, null)).IgnoreArguments().Return(true);

			_mocks.ReplayAll();
			Assert.That(_target.Execute(new List<IPerson> { person }, dateOnly, new List<IScheduleMatrixPro>()), Is.False);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldBuildGroupPersonAndSchedule()
		{
			var person = new Person();
			var groupPerson = _mocks.DynamicMock<IGroupPerson>();
			var dateOnly = new DateOnly(2012,10,2);

			Expect.Call(_groupPersonBuilder.BuildGroupPerson(person, dateOnly)).Return(groupPerson);
			Expect.Call(_groupPersonConsistChecker.AllPersonsHasSameOrNoneScheduled(groupPerson, dateOnly, null)).IgnoreArguments().Return(true);

			_mocks.ReplayAll();
			Assert.That(_target.Execute(new List<IPerson> {person}, dateOnly, new List<IScheduleMatrixPro>()),Is.True);
			_mocks.VerifyAll();
		}

	}


	
}