using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common.Logging;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common.Logging
{
    [TestFixture]
    public class SchedulingOptionsValueExtractorTest
    {
        private SchedulingOptionsValueExtractor _target;
        
        [Test]
        public void VerifyTeamOptions()
        {
            IGroupPageLight groupPage = new GroupPageLight();
            groupPage.Name = "Test1";
            ISchedulingOptions schedulingOptions = new SchedulingOptions
                {
                    TeamSameShiftCategory = true,
                    TeamSameStartTime = true,
                    TeamSameActivity = true,
                    TeamSameEndTime = true,
                    GroupOnGroupPageForTeamBlockPer = groupPage
                };
            _target = new SchedulingOptionsValueExtractor(schedulingOptions );

            var result = _target.GetTeamOptions();
            Assert.AreEqual(result, "Test1,Same shift category,Same start time,Same activity,Same end time");
        }

        [Test]
        public void VerifyBlockOptions()
        {
            ISchedulingOptions schedulingOptions = new SchedulingOptions
            {
                BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff ,
                BlockSameShiftCategory = true,
					 BlockSameShift = true,
                BlockSameStartTime = true,
				UseTeamBlockPerOption = true
            };
            _target = new SchedulingOptionsValueExtractor(schedulingOptions);

            var result = _target.GetBlockOptions() ;
            Assert.AreEqual(result, "BetweenDayOff,Same shift category,Same shift,Same start time");
        }

        [Test]
        public void VerifyGeneralSchedulingOptions()
        {
            ISchedulingOptions schedulingOptions = new SchedulingOptions
            {
                UsePreferences = true,
                UseRotations = true
            };
            _target = new SchedulingOptionsValueExtractor(schedulingOptions);

            var result = _target.GetGeneralSchedulingOptions();
            Assert.AreEqual(result, "Scheduling,Prefrences,Rotations");
        }

        [Test]
        public void VerifyGeneralOptimizationOptions()
        {
            IOptimizationPreferences optimizationOption = new OptimizationPreferences();
            optimizationOption.General.OptimizationStepDaysOff = true;
            optimizationOption.General.OptimizationStepShiftsWithinDay = true;
            optimizationOption.General.OptimizationStepDaysOff = true;
            optimizationOption.General.OptimizationStepTimeBetweenDays = true;
            optimizationOption.General.UsePreferences = true;
            optimizationOption.General.UseMustHaves = true;
            optimizationOption.General.UseRotations = true;
            optimizationOption.General.UseAvailabilities = true;
            optimizationOption.Advanced.TargetValueCalculation = TargetValueOptions.StandardDeviation ;

            _target = new SchedulingOptionsValueExtractor(new SchedulingOptionsCreator().CreateSchedulingOptions(optimizationOption));

            var result = _target.GetGeneralOptimizationOptions(optimizationOption);
            Assert.AreEqual(result, "StandardDeviation,Dayoff Optimization,Shift within day,Intraday,Prefrences,Must Have,Rotations,Availability");
        }
    }
}
