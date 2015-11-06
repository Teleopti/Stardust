using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    [TestFixture]
    public class PostSwapValidationForTeamBlockTest
    {
        private IPostSwapValidationForTeamBlock _target;
        private IOptimizationPreferences _optimizationPreferences;
        private ITeamBlockInfo _teamBlockInfo;
        private ISeniorityTeamBlockSwapValidator _seniorityTeamBlockSwapValidator;
	    private ITeamBlockOptimizationLimits _teamBlockOptimizationLimits;
	    private IDayOffOptimizationPreferenceProvider _dayOffOptimizationPreferenceProvider;
	    private IDaysOffPreferences _daysOffPreferences;

        [SetUp]
        public void Setup()
        {
            _optimizationPreferences = MockRepository.GenerateMock<IOptimizationPreferences>();
            _teamBlockInfo = MockRepository.GenerateMock<ITeamBlockInfo>();
            _seniorityTeamBlockSwapValidator = MockRepository.GenerateMock<ISeniorityTeamBlockSwapValidator>();
	        _teamBlockOptimizationLimits = MockRepository.GenerateMock<ITeamBlockOptimizationLimits>();
			
			_daysOffPreferences = new DaysOffPreferences();
			_dayOffOptimizationPreferenceProvider = new DayOffOptimizationPreferenceProvider(_daysOffPreferences);

			_target = new PostSwapValidationForTeamBlock(_seniorityTeamBlockSwapValidator, _teamBlockOptimizationLimits);
        }

        [Test]
        public void ShouldReturnFalseForTeamBlockValidator()
        {
            _seniorityTeamBlockSwapValidator.Stub(x => x.Validate(_teamBlockInfo, _optimizationPreferences)).IgnoreArguments().Return(false);
                
			Assert.IsFalse(_target.Validate(_teamBlockInfo,_optimizationPreferences, _dayOffOptimizationPreferenceProvider));
        }

	    [Test]
	    public void ShouldReturnFalseForRestrictionValidator()
	    {
		    _seniorityTeamBlockSwapValidator.Stub(x => x.Validate(_teamBlockInfo, _optimizationPreferences))
			    .IgnoreArguments()
			    .Return(true);
			_teamBlockOptimizationLimits.Stub(x => x.Validate(_teamBlockInfo, _optimizationPreferences, _dayOffOptimizationPreferenceProvider))
			    .IgnoreArguments()
			    .Return(false);

			Assert.IsFalse(_target.Validate(_teamBlockInfo, _optimizationPreferences, _dayOffOptimizationPreferenceProvider));
	    }

	    [Test]
	    public void ShouldReturnTrueForCorrectValidationPerformed()
	    {
		    _seniorityTeamBlockSwapValidator.Stub(x => x.Validate(_teamBlockInfo, _optimizationPreferences))
			    .IgnoreArguments()
			    .Return(true);
			_teamBlockOptimizationLimits.Stub(x => x.Validate(_teamBlockInfo, _optimizationPreferences, _dayOffOptimizationPreferenceProvider))
			    .IgnoreArguments()
			    .Return(true);
		    _teamBlockOptimizationLimits.Stub(x => x.ValidateMinWorkTimePerWeek(_teamBlockInfo)).Return(true);

			Assert.IsTrue(_target.Validate(_teamBlockInfo, _optimizationPreferences, _dayOffOptimizationPreferenceProvider));
	    }

	    [Test]
	    public void ShouldReturnFalseWhenMinWorkTimePerWeekFails()
	    {
		    _seniorityTeamBlockSwapValidator.Stub(x => x.Validate(_teamBlockInfo, _optimizationPreferences)).IgnoreArguments().Return(true);
			_teamBlockOptimizationLimits.Stub(x => x.Validate(_teamBlockInfo, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).IgnoreArguments().Return(true);
		    _teamBlockOptimizationLimits.Stub(x => x.ValidateMinWorkTimePerWeek(_teamBlockInfo)).Return(false);

			Assert.IsFalse(_target.Validate(_teamBlockInfo, _optimizationPreferences, _dayOffOptimizationPreferenceProvider));
	    }
    }
}
