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
	public class GroupOptimizerValidateProposedDatesInSameGroupTest
	{
		private IGroupOptimizerValidateProposedDatesInSameGroup _target;
		private MockRepository _mock;
		private IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
		private IGroupOptimizerFindMatrixesForGroup _groupOptimizerFindMatrixesForGroup;
		private IPerson _person;
		private DateOnly _firstDate;
		private DateOnly _secondDate;
		private IGroupPerson _groupPerson1;
		private IGroupPerson _groupPerson2;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_groupPersonBuilderForOptimization = _mock.StrictMock<IGroupPersonBuilderForOptimization>();
			_groupOptimizerFindMatrixesForGroup = _mock.StrictMock<IGroupOptimizerFindMatrixesForGroup>();
			_target = new GroupOptimizerValidateProposedDatesInSameGroup(_groupPersonBuilderForOptimization,
			                                                             _groupOptimizerFindMatrixesForGroup);
			_person = PersonFactory.CreatePerson();
			_firstDate = new DateOnly(2012, 1, 1);
			_secondDate = new DateOnly(2012, 1, 2);
			_groupPerson1 = _mock.StrictMock<IGroupPerson>();
			_groupPerson2 = _mock.StrictMock<IGroupPerson>();
			//_personGroup1 = _mock.StrictMock<IPersonGroup>();
			//_personGroup2 = _mock.StrictMock<IPersonGroup>();
		}

		[Test]
		public void ShouldReturnTrueButDateToLockIfNotSameCountOfGroupMembers()
		{
			IList<IPerson> members1 = new List<IPerson> {_person};
			IList<IPerson> members2 = new List<IPerson>();
			using (_mock.Record())
			{
				Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(_person, _firstDate)).Return(_groupPerson1);
				Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(_person, _secondDate)).Return(_groupPerson2);
				Expect.Call(_groupPerson1.GroupMembers).Return(new ReadOnlyCollection<IPerson>(members1));
				Expect.Call(_groupPerson2.GroupMembers).Return(new ReadOnlyCollection<IPerson>(members2));
				Expect.Call(_groupOptimizerFindMatrixesForGroup.Find(_person, _firstDate)).Return(new List<IScheduleMatrixPro>());
			}

			ValidatorResult result;

			using (_mock.Playback())
			{
				result = _target.Validate(_person, new List<DateOnly> {_firstDate, _secondDate}, true);
			}

			Assert.IsTrue((result.Success));
			Assert.AreEqual(_firstDate, result.DaysToLock.Value.StartDate);
		}

		[Test]
		public void ShouldReturnTrueAndDayToLockIfGroupMembersDoesNotMatch()
		{
			DateOnly thirdDate = new DateOnly(2012, 1, 3);
			IList<IPerson> members2 = new List<IPerson> { PersonFactory.CreatePerson(), _person };
			IList<IPerson> members3 = new List<IPerson> { PersonFactory.CreatePerson(), _person };
			using (_mock.Record())
			{
				Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(_person, _firstDate)).Return(_groupPerson1);
				Expect.Call(_groupPerson1.GroupMembers).Return(new ReadOnlyCollection<IPerson>(members2));
				Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(_person, _secondDate)).Return(_groupPerson1);
				Expect.Call(_groupPerson1.GroupMembers).Return(new ReadOnlyCollection<IPerson>(members2));
				Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(_person, thirdDate)).Return(_groupPerson2);
				Expect.Call(_groupPerson2.GroupMembers).Return(new ReadOnlyCollection<IPerson>(members3));
				Expect.Call(_groupOptimizerFindMatrixesForGroup.Find(_person, _firstDate)).Return(new List<IScheduleMatrixPro>());
			}

			ValidatorResult result;

			using (_mock.Playback())
			{
				result = _target.Validate(_person, new List<DateOnly> {_firstDate, _secondDate, thirdDate}, true);
			}

			Assert.IsTrue((result.Success));
			Assert.AreEqual(_firstDate, result.DaysToLock.Value.StartDate);
		}

		[Test]
		public void ShouldReturnTrueAndNoDatesToLockIfNotUseSameDaysOff()
		{
			using (_mock.Record())
			{
				
			}

			ValidatorResult result;

			using (_mock.Playback())
			{
				result = _target.Validate(_person, new List<DateOnly> {_firstDate, _secondDate}, false);
			}

			Assert.IsTrue((result.Success));
			Assert.IsFalse(result.DaysToLock.HasValue);
		}

		[Test]
		public void ShouldReturnTrueWithEmptyListIfOnlyOneDate()
		{
			using (_mock.Record())
			{
			}

			ValidatorResult result;

			using (_mock.Playback())
			{
				result = _target.Validate(_person, new List<DateOnly> { _firstDate }, true);
			}

			Assert.IsTrue((result.Success));
			Assert.IsFalse(result.DaysToLock.HasValue);
		}

	}
}