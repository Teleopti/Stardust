using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class GroupOptimizationValidatorRunnerTest
	{
		private MockRepository _mock;
		private IGroupOptimizationValidatorRunner _target;
		private IGroupDayOffOptimizerValidateDayOffToRemove _groupDayOffOptimizerValidateDayOffToRemove;
		private IGroupDayOffOptimizerValidateDayOffToAdd _groupDayOffOptimizerValidateDayOffToAdd;
		private IGroupOptimizerValidateProposedDatesInSameMatrix _groupOptimizerValidateProposedDatesInSameMatrix;
		private IGroupOptimizerValidateProposedDatesInSameGroup _groupOptimizerValidateProposedDatesInSameGroup;
		private IPerson _person;
		private DateOnly _dateOffToRemove;
		private DateOnly _dateOffToAdd;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_groupDayOffOptimizerValidateDayOffToRemove = _mock.StrictMock<IGroupDayOffOptimizerValidateDayOffToRemove>();
			_groupDayOffOptimizerValidateDayOffToAdd = _mock.StrictMock<IGroupDayOffOptimizerValidateDayOffToAdd>();
			_groupOptimizerValidateProposedDatesInSameMatrix =
				_mock.StrictMock<IGroupOptimizerValidateProposedDatesInSameMatrix>();
			_groupOptimizerValidateProposedDatesInSameGroup = _mock.StrictMock<IGroupOptimizerValidateProposedDatesInSameGroup>();
			_target = new GroupOptimizationValidatorRunner(_groupDayOffOptimizerValidateDayOffToRemove,
			                                               _groupDayOffOptimizerValidateDayOffToAdd,
			                                               _groupOptimizerValidateProposedDatesInSameMatrix,
														   _groupOptimizerValidateProposedDatesInSameGroup);
			_person = PersonFactory.CreatePerson();
			_dateOffToRemove = DateOnly.MinValue;
			_dateOffToAdd = DateOnly.MaxValue;
		}

		[Test, Ignore]
		public void ShouldRunAllAndReturnTrueIfAllSuccess()
		{
			using (_mock.Record())
			{
				Expect.Call(_groupDayOffOptimizerValidateDayOffToRemove.Validate(_person, new List<DateOnly> {_dateOffToRemove},
				                                                                 true)).Return(new ValidatorResult{Success = true});
				Expect.Call(_groupDayOffOptimizerValidateDayOffToAdd.Validate(_person, new List<DateOnly> { _dateOffToAdd },
																			  true)).Return(new ValidatorResult { Success = true });
				Expect.Call(_groupOptimizerValidateProposedDatesInSameMatrix.Validate(_person, new List<DateOnly> { _dateOffToAdd },
																			  true)).Return(new ValidatorResult { Success = true });
				Expect.Call(_groupOptimizerValidateProposedDatesInSameGroup.Validate(_person, new List<DateOnly> { _dateOffToAdd },
																			  true)).Return(new ValidatorResult { Success = true });
			}

			ValidatorResult result;
			using (_mock.Playback())
			{
				result = _target.Run(_person, new List<DateOnly> {_dateOffToRemove}, new List<DateOnly> {_dateOffToAdd}, true);
			}

			Assert.IsTrue(result.Success);
		}
	}
}