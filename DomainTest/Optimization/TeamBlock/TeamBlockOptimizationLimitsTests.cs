using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock
{
	[TestFixture]
	public class TeamBlockOptimizationLimitsTests
	{
		private TeamBlockOptimizationLimits _target;
		private MockRepository _mock;
		private ITeamBlockRestrictionOverLimitValidator _teamBlockRestrictionOverLimitValidator;
		private IMinWeekWorkTimeRule _minWeekWorkTimeRule;
		private ITeamBlockInfo _teamBlockInfo;
		private IOptimizationPreferences _optimizationPreferences;
		private ITeamInfo _teamInfo;
		private IScheduleMatrixPro _scheduleMatrixPro;
		private IPerson _person;
		private IScheduleRange _scheduleRange;
		private Dictionary<IPerson, IScheduleRange> _dictionary;
		private IScheduleDayPro _scheduleDayPro;
		private ReadOnlyCollection<IScheduleDayPro> _effectiveDays;
		private IScheduleDay _scheduleDay;
		private IList<IScheduleMatrixPro> _matrixList;
		private IBusinessRuleResponse _businessRuleResponse;
		private IDaysOffPreferences _daysOffPreferences;
		private IDayOffOptimizationPreferenceProvider _dayOffOptimizationPreferenceProvider;
		
		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_teamBlockRestrictionOverLimitValidator = _mock.StrictMock<ITeamBlockRestrictionOverLimitValidator>();
			_minWeekWorkTimeRule = _mock.StrictMock<IMinWeekWorkTimeRule>();
			_teamBlockInfo = _mock.StrictMock<ITeamBlockInfo>();
			_teamInfo = _mock.StrictMock<ITeamInfo>();
			_optimizationPreferences = _mock.StrictMock<IOptimizationPreferences>();
			_target = new TeamBlockOptimizationLimits(_teamBlockRestrictionOverLimitValidator, _minWeekWorkTimeRule);
			_scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
			_person = _mock.StrictMock<IPerson>();
			_scheduleRange = _mock.StrictMock<IScheduleRange>();
			_dictionary = new Dictionary<IPerson, IScheduleRange> { { _person, _scheduleRange } };
			_scheduleDayPro = _mock.StrictMock<IScheduleDayPro>();
			_effectiveDays = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_matrixList = new List<IScheduleMatrixPro>{_scheduleMatrixPro};
			_businessRuleResponse = _mock.StrictMock<IBusinessRuleResponse>();
			_daysOffPreferences = new DaysOffPreferences();
			_dayOffOptimizationPreferenceProvider = new FixedDayOffOptimizationPreferenceProvider(_daysOffPreferences);
		}

		[Test]
		public void ShouldValidateRestrictionsTeamBlock()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamBlockRestrictionOverLimitValidator.Validate(_teamBlockInfo, _optimizationPreferences,_dayOffOptimizationPreferenceProvider)).Return(true);
			}

			using (_mock.Playback())
			{
				var result = _target.Validate(_teamBlockInfo, _optimizationPreferences,_dayOffOptimizationPreferenceProvider);
				Assert.IsTrue(result);
			}		
		}

		[Test]
		public void ShouldValidateRestrictionsTeam()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamBlockRestrictionOverLimitValidator.Validate(_teamInfo, _optimizationPreferences,_dayOffOptimizationPreferenceProvider)).Return(true);
			}

			using (_mock.Playback())
			{
				var result = _target.Validate(_teamInfo, _optimizationPreferences, _dayOffOptimizationPreferenceProvider);
				Assert.IsTrue(result);
			}	
		}

		[Test]
		public void ShouldValidateTrueMinWorkTimePerWeekTeamBlock()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamBlockInfo.MatrixesForGroupAndBlock()).Return(_matrixList);
				Expect.Call(_scheduleMatrixPro.Person).Return(_person);
				Expect.Call(_scheduleMatrixPro.ActiveScheduleRange).Return(_scheduleRange);
				Expect.Call(_scheduleMatrixPro.EffectivePeriodDays).Return(_effectiveDays);
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_minWeekWorkTimeRule.Validate(_dictionary, new List<IScheduleDay> { _scheduleDay })).Return(new HashSet<IBusinessRuleResponse>());
			}

			using (_mock.Playback())
			{
				var result = _target.ValidateMinWorkTimePerWeek(_teamBlockInfo);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldValidateFalseMinWorkTimePerWeekTeamBlock()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamBlockInfo.MatrixesForGroupAndBlock()).Return(_matrixList);
				Expect.Call(_scheduleMatrixPro.Person).Return(_person);
				Expect.Call(_scheduleMatrixPro.ActiveScheduleRange).Return(_scheduleRange);
				Expect.Call(_scheduleMatrixPro.EffectivePeriodDays).Return(_effectiveDays);
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_minWeekWorkTimeRule.Validate(_dictionary, new List<IScheduleDay> { _scheduleDay })).Return(new HashSet<IBusinessRuleResponse> { _businessRuleResponse });
			}

			using (_mock.Playback())
			{
				var result = _target.ValidateMinWorkTimePerWeek(_teamBlockInfo);
				Assert.IsFalse(result);
			}
		}


		[Test]
		public void ShouldValidateTrueMinWorkTimePerWeekTeam()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamInfo.MatrixesForGroup()).Return(_matrixList);
				Expect.Call(_scheduleMatrixPro.Person).Return(_person);
				Expect.Call(_scheduleMatrixPro.ActiveScheduleRange).Return(_scheduleRange);
				Expect.Call(_scheduleMatrixPro.EffectivePeriodDays).Return(_effectiveDays);
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_minWeekWorkTimeRule.Validate(_dictionary, new List<IScheduleDay> { _scheduleDay })).Return(new HashSet<IBusinessRuleResponse>());
			}

			using (_mock.Playback())
			{
				var result = _target.ValidateMinWorkTimePerWeek(_teamInfo);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldValidateFalseMinWorkTimePerWeekTeam()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamInfo.MatrixesForGroup()).Return(_matrixList);
				Expect.Call(_scheduleMatrixPro.Person).Return(_person);
				Expect.Call(_scheduleMatrixPro.ActiveScheduleRange).Return(_scheduleRange);
				Expect.Call(_scheduleMatrixPro.EffectivePeriodDays).Return(_effectiveDays);
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_minWeekWorkTimeRule.Validate(_dictionary, new List<IScheduleDay> { _scheduleDay })).Return(new HashSet<IBusinessRuleResponse> { _businessRuleResponse });
			}

			using (_mock.Playback())
			{
				var result = _target.ValidateMinWorkTimePerWeek(_teamInfo);
				Assert.IsFalse(result);
			}
		}
	}
}
