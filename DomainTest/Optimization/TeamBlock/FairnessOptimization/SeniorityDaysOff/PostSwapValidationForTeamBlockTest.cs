using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    [TestFixture]
    public class PostSwapValidationForTeamBlockTest
    {

        private MockRepository _mock;
        private IPostSwapValidationForTeamBlock _target;
        private IOptimizationPreferences _optimizationPreferences;
        private ITeamBlockInfo _teamBlockInfo;
        private ISeniorityTeamBlockSwapValidator _seniorityTeamBlockSwapValidator;
        private ITeamBlockRestrictionOverLimitValidator _teamBlockRestrictionOverLimitValidator;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _optimizationPreferences = _mock.StrictMock<IOptimizationPreferences>();
            _teamBlockInfo = _mock.StrictMock<ITeamBlockInfo>();
            _seniorityTeamBlockSwapValidator = _mock.StrictMock<ISeniorityTeamBlockSwapValidator>();
            _teamBlockRestrictionOverLimitValidator = _mock.StrictMock<ITeamBlockRestrictionOverLimitValidator>();

            _target = new PostSwapValidationForTeamBlock(_seniorityTeamBlockSwapValidator, _teamBlockRestrictionOverLimitValidator);
        }

        [Test]
        public void ShouldReturnFalseForTeamBlockValidator()
        {
            using (_mock.Record())
            {
                Expect.Call(_seniorityTeamBlockSwapValidator.Validate(_teamBlockInfo, _optimizationPreferences))
                      .IgnoreArguments()
                      .Return(false);
            }
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.Validate(_teamBlockInfo,_optimizationPreferences));
            }
        }

        [Test]
        public void ShouldReturnFalseForRestrictionValidator()
        {
            using (_mock.Record())
            {
                Expect.Call(_seniorityTeamBlockSwapValidator.Validate(_teamBlockInfo, _optimizationPreferences))
                      .IgnoreArguments()
                      .Return(true);
                Expect.Call(_teamBlockRestrictionOverLimitValidator .Validate(_teamBlockInfo, _optimizationPreferences))
                      .IgnoreArguments()
                      .Return(false);
            }
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.Validate(_teamBlockInfo, _optimizationPreferences));
            }
        }
        [Test]
        public void ShouldReturnTrueForCorrectValidationPerformed()
        {
            using (_mock.Record())
            {
                Expect.Call(_seniorityTeamBlockSwapValidator.Validate(_teamBlockInfo, _optimizationPreferences))
                      .IgnoreArguments()
                      .Return(true);
                Expect.Call(_teamBlockRestrictionOverLimitValidator.Validate(_teamBlockInfo, _optimizationPreferences))
                      .IgnoreArguments()
                      .Return(true);
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.Validate(_teamBlockInfo, _optimizationPreferences));
            }
        }

    }
}
