using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class GroupOptimizerFindMatrixesForGroupTest
	{
		private MockRepository _mock;
		private IGroupOptimizerFindMatrixesForGroup _target;
		private IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
		private IScheduleMatrixPro _matrix1;
		private IScheduleMatrixPro _matrix2;
		private IScheduleMatrixPro _matrix3;
		private IList<IScheduleMatrixPro> __matrixes;
		private IPerson _personToFind;
		private DateOnly _dateToFind;
		private IGroupPerson _groupPerson;
		private IPerson _otherPerson;
		private IVirtualSchedulePeriod _schedulePeriod1;
		private IVirtualSchedulePeriod _schedulePeriod2;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_groupPersonBuilderForOptimization = _mock.StrictMock<IGroupPersonBuilderForOptimization>();
			_matrix1 = _mock.StrictMock<IScheduleMatrixPro>();
			_matrix2 = _mock.StrictMock<IScheduleMatrixPro>();
			_matrix3 = _mock.StrictMock<IScheduleMatrixPro>();
			__matrixes = new List<IScheduleMatrixPro>{ _matrix1, _matrix2, _matrix3};
			_target = new GroupOptimizerFindMatrixesForGroup(_groupPersonBuilderForOptimization, __matrixes);
			_personToFind = PersonFactory.CreatePerson();
			_dateToFind = new DateOnly(2012,1,1);
			_groupPerson = _mock.StrictMock<IGroupPerson>();
			_otherPerson = PersonFactory.CreatePerson();
			_schedulePeriod1 = _mock.StrictMock<IVirtualSchedulePeriod>();
			_schedulePeriod2 = _mock.StrictMock<IVirtualSchedulePeriod>();
		}

		[Test]
		public void ShouldFindCorrectMatrixes()
		{
			using (_mock.Record())
			{
				Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(_personToFind, _dateToFind)).Return(_groupPerson);
				Expect.Call(_groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> {_personToFind}));
				Expect.Call(_matrix1.Person).Return(_personToFind);
				Expect.Call(_matrix2.Person).Return(_personToFind);
				Expect.Call(_matrix3.Person).Return(_otherPerson);
				Expect.Call(_matrix1.SchedulePeriod).Return(_schedulePeriod1);
				Expect.Call(_schedulePeriod1.DateOnlyPeriod).Return(new DateOnlyPeriod(new DateOnly(), new DateOnly()));
				Expect.Call(_matrix2.SchedulePeriod).Return(_schedulePeriod2);
				Expect.Call(_schedulePeriod2.DateOnlyPeriod).Return(new DateOnlyPeriod(_dateToFind, _dateToFind));
			}

			IList<IScheduleMatrixPro> result;
			using (_mock.Playback())
			{
				result = _target.Find(_personToFind, _dateToFind);
			}

			Assert.AreSame(_matrix2, result[0]);
		}

		[Test]
		public void ShouldReturnEmptyListIfNoGroupPerson()
		{
			using (_mock.Record())
			{
				Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(_personToFind, _dateToFind)).Return(null);
			}

			IList<IScheduleMatrixPro> result;
			using (_mock.Playback())
			{
				result = _target.Find(_personToFind, _dateToFind);
			}

			Assert.IsTrue(result.Count == 0);
		}
	}
}