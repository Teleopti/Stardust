using System.ComponentModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
	[TestFixture]
	public class SeniorityTeamBlockSwapValidatorTest
	{
		private MockRepository _mocks;
		private ISeniorityTeamBlockSwapValidator _target;
		private IDayOffRulesValidator _dayOffRulesValidator;
		private ITeamBlockInfo _teamBlockInfo;
		private IOptimizationPreferences _optimizationPreferences;
		private IScheduleMatrixPro _matrix;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_dayOffRulesValidator = _mocks.StrictMock<IDayOffRulesValidator>();
			_target = new SeniorityTeamBlockSwapValidator(_dayOffRulesValidator);
			_teamBlockInfo = _mocks.StrictMock<ITeamBlockInfo>();
			_optimizationPreferences = new OptimizationPreferences();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
		}

		[Test]
		public void ShouldReturnFalseIfAnyDaysOffValidatorFail()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockInfo.MatrixesForGroupAndBlock()).Return(new[] {_matrix});
				Expect.Call(_dayOffRulesValidator.Validate(_matrix, _optimizationPreferences)).Return(false);
			}

			using (_mocks.Playback())
			{
				var result = _target.Validate(_teamBlockInfo, _optimizationPreferences);
				Assert.IsFalse(result);
			}
		}

	}
}