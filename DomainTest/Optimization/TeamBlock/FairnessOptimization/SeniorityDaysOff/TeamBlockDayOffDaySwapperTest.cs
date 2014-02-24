using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
	[TestFixture]
	public class TeamBlockDayOffDaySwapperTest
	{
		private MockRepository _mocks;
		private ITeamBlockDayOffDaySwapper _target;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private IScheduleDictionary _scheduleDictionary;
		private IScheduleRange _range1;
		private IScheduleRange _range2;
		private ISwapServiceNew _swapServiceNew;
		private IPerson _personSenior;
		private IPerson _personJunior;
		private Group _groupSenior;
		private Group _groupJunior;
		private IScheduleMatrixPro _matrixSenior;
		private TeamInfo _teamSenior;
		private TeamInfo _teamJunior;
		private TeamBlockInfo _teamBlockInfoSenior;
		private TeamBlockInfo _teamBlockInfoJunior;
		private DateOnly _dateOnly;
		private ITeamBlockDayOffDaySwapDecisionMaker _decisionMaker;
		private List<DateOnly> _dayOffsToGiveAway;
		private IOptimizationPreferences _optimizationPreferences;
		private DateOnly _dateBefore;
		private PossibleSwappableDays _daysToSwap;
	    private IPostSwapValidationForTeamBlock _postSwapValidationForTeamBlock;
		private ITeamBlockShiftCategoryLimitationValidator _teamBlockShiftCategoryLimitationValidator;

	    [SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_swapServiceNew = _mocks.StrictMock<ISwapServiceNew>();
			_decisionMaker = _mocks.StrictMock<ITeamBlockDayOffDaySwapDecisionMaker>();
	        _postSwapValidationForTeamBlock = _mocks.StrictMock<IPostSwapValidationForTeamBlock>();
		    _teamBlockShiftCategoryLimitationValidator = _mocks.StrictMock<ITeamBlockShiftCategoryLimitationValidator>();
		    _target = new TeamBlockDayOffDaySwapper(_swapServiceNew, _decisionMaker, _postSwapValidationForTeamBlock,
		                                            _teamBlockShiftCategoryLimitationValidator);

			_rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			_optimizationPreferences = _mocks.StrictMock<IOptimizationPreferences>();
			_personSenior = PersonFactory.CreatePerson();
			_personJunior = PersonFactory.CreatePerson();
			_groupSenior = new Group(new List<IPerson> { _personSenior }, "Senior");
			_groupJunior = new Group(new List<IPerson> { _personJunior }, "Junior");
			_matrixSenior = _mocks.StrictMock<IScheduleMatrixPro>();
			IList<IList<IScheduleMatrixPro>> groupMatrixesSenior = new List<IList<IScheduleMatrixPro>>();
			groupMatrixesSenior.Add(new List<IScheduleMatrixPro> { _matrixSenior });
			IList<IList<IScheduleMatrixPro>> groupMatrixesJunior = new List<IList<IScheduleMatrixPro>>();
			groupMatrixesJunior.Add(new List<IScheduleMatrixPro> { _matrixSenior });
			_teamSenior = new TeamInfo(_groupSenior, groupMatrixesSenior);
			_teamJunior = new TeamInfo(_groupJunior, groupMatrixesJunior);
			_dateOnly = new DateOnly(2014, 1, 27);
			_dateBefore = _dateOnly.AddDays(-1);
			var blockInfo = new BlockInfo(new DateOnlyPeriod(_dateBefore, _dateOnly));
			_teamBlockInfoSenior = new TeamBlockInfo(_teamSenior, blockInfo);
			_teamBlockInfoJunior = new TeamBlockInfo(_teamJunior, blockInfo);
			_range1 = _mocks.StrictMock<IScheduleRange>();
			_range2 = _mocks.StrictMock<IScheduleRange>();

			_dayOffsToGiveAway = new List<DateOnly>{_dateBefore};
			_daysToSwap = new PossibleSwappableDays
			{
				DateForSeniorDayOff = _dateOnly,
				DateForRemovingSeniorDayOff = _dateBefore
			};
		}

		[Test]
		public void ShouldFailIfNoPossibleIndividualDaySwaps()
		{
			using (_mocks.Record())
			{
				Expect.Call(_decisionMaker.Decide(_dateOnly, _teamBlockInfoSenior, _teamBlockInfoJunior, _scheduleDictionary,
												  _optimizationPreferences, _dayOffsToGiveAway)).Return(null);
			    Expect.Call(()=> _rollbackService.ClearModificationCollection());
			}
			using (_mocks.Playback())
			{
				var result = _target.TrySwap(_dateOnly, _teamBlockInfoSenior, _teamBlockInfoJunior, _rollbackService,
											 _scheduleDictionary, _optimizationPreferences, _dayOffsToGiveAway);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldSwapAccordingToDecisionMaker()
		{
			var seniorToHaveDayOffScheduleDay = _mocks.StrictMock<IScheduleDay>(); 
			var juniorToRemoveDayOffScheduleDay = _mocks.StrictMock<IScheduleDay>();
			var seniorGiveDayOffScheduleDay = _mocks.StrictMock<IScheduleDay>(); 
			var juniorAcceptDayOffScheduleDay = _mocks.StrictMock<IScheduleDay>();
			var swapList1 = new List<IScheduleDay> { seniorToHaveDayOffScheduleDay, juniorToRemoveDayOffScheduleDay };
			var swapList2 = new List<IScheduleDay> { seniorGiveDayOffScheduleDay, juniorAcceptDayOffScheduleDay };
			var swapList = new List<IScheduleDay>();
			swapList.AddRange(swapList1);
			swapList.AddRange(swapList2);
			using (_mocks.Record())
			{
				commonMocks(seniorToHaveDayOffScheduleDay, juniorToRemoveDayOffScheduleDay, seniorGiveDayOffScheduleDay, juniorAcceptDayOffScheduleDay, swapList1, swapList2);
				Expect.Call(() => _rollbackService.ClearModificationCollection()).Repeat.Twice();
				Expect.Call(_rollbackService.ModifyParts(swapList)).Return(new List<IBusinessRuleResponse>());
                Expect.Call(_postSwapValidationForTeamBlock.Validate(_teamBlockInfoSenior, _optimizationPreferences))
                      .IgnoreArguments()
                      .Return(true);
                Expect.Call(_postSwapValidationForTeamBlock.Validate(_teamBlockInfoJunior, _optimizationPreferences))
                          .IgnoreArguments()
                          .Return(true);
				Expect.Call(_teamBlockShiftCategoryLimitationValidator.Validate(_teamBlockInfoSenior, _teamBlockInfoJunior,
																				_optimizationPreferences)).Return(true);
			}
			using (_mocks.Playback())
			{
				var result = _target.TrySwap(_dateOnly, _teamBlockInfoSenior, _teamBlockInfoJunior, _rollbackService,
				                             _scheduleDictionary, _optimizationPreferences, _dayOffsToGiveAway);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldRollbackIfBusinessRulesViolated()
		{
			
			var seniorToHaveDayOffScheduleDay = _mocks.StrictMock<IScheduleDay>(); 
			var juniorToRemoveDayOffScheduleDay = _mocks.StrictMock<IScheduleDay>();
			var seniorGiveDayOffScheduleDay = _mocks.StrictMock<IScheduleDay>(); 
			var juniorAcceptDayOffScheduleDay = _mocks.StrictMock<IScheduleDay>();
			var swapList1 = new List<IScheduleDay> { seniorToHaveDayOffScheduleDay, juniorToRemoveDayOffScheduleDay };
			var swapList2 = new List<IScheduleDay> { seniorGiveDayOffScheduleDay, juniorAcceptDayOffScheduleDay };
			var swapList = new List<IScheduleDay>();
			swapList.AddRange(swapList1);
			swapList.AddRange(swapList2);
			IBusinessRuleResponse response = new BusinessRuleResponse(typeof(NewDayOffRule), "", true, false,
																	  new DateTimePeriod(), _personSenior,
																	  new DateOnlyPeriod());
			
			using (_mocks.Record())
			{
				commonMocks(seniorToHaveDayOffScheduleDay, juniorToRemoveDayOffScheduleDay, seniorGiveDayOffScheduleDay, juniorAcceptDayOffScheduleDay, swapList1, swapList2);
				Expect.Call(() => _rollbackService.ClearModificationCollection()).Repeat.AtLeastOnce();
				Expect.Call(_rollbackService.ModifyParts(swapList)).Return(new List<IBusinessRuleResponse> { response });
				Expect.Call(()=>_rollbackService.Rollback());
			}
			using (_mocks.Playback())
			{
				var result = _target.TrySwap(_dateOnly, _teamBlockInfoSenior, _teamBlockInfoJunior, _rollbackService,
				                             _scheduleDictionary, _optimizationPreferences, _dayOffsToGiveAway);
				Assert.IsFalse(result);
			}
		}

        //[Test]
        //public void ShouldBeAbleToCancel()
        //{
        //    using (_mocks.Record())
        //    {
        //        Expect.Call(_decisionMaker.Decide(_dateOnly, _teamBlockInfoSenior, _teamBlockInfoJunior, _scheduleDictionary,
        //                                      _optimizationPreferences, _dayOffsToGiveAway)).Return(_daysToSwap);
        //        Expect.Call(() => _rollbackService.ClearModificationCollection());
        //    }
        //    using (_mocks.Playback())
        //    {
        //        _target.Cancel();
        //        var result = _target.TrySwap(_dateOnly, _teamBlockInfoSenior, _teamBlockInfoJunior, _rollbackService,
        //                                     _scheduleDictionary, _optimizationPreferences, _dayOffsToGiveAway);
        //        Assert.IsFalse(result);
        //    }
        //}

		private void commonMocks(IScheduleDay seniorToHaveDayOffScheduleDay,
		                         IScheduleDay juniorToRemoveDayOffScheduleDay, IScheduleDay seniorGiveDayOffScheduleDay,
		                         IScheduleDay juniorAcceptDayOffScheduleDay, List<IScheduleDay> swapList1, List<IScheduleDay> swapList2)
		{
			Expect.Call(_scheduleDictionary[_personSenior]).Return(_range1);
			Expect.Call(_scheduleDictionary[_personJunior]).Return(_range2);
			Expect.Call(_decisionMaker.Decide(_dateOnly, _teamBlockInfoSenior, _teamBlockInfoJunior, _scheduleDictionary,
			                                  _optimizationPreferences, _dayOffsToGiveAway)).Return(_daysToSwap);
			Expect.Call(_range1.ScheduledDay(_dateOnly)).Return(seniorToHaveDayOffScheduleDay);
			Expect.Call(_range2.ScheduledDay(_dateOnly)).Return(juniorToRemoveDayOffScheduleDay);
			Expect.Call(_range1.ScheduledDay(_dateBefore)).Return(seniorGiveDayOffScheduleDay);
			Expect.Call(_range2.ScheduledDay(_dateBefore)).Return(juniorAcceptDayOffScheduleDay);
			Expect.Call(
				_swapServiceNew.Swap(new List<IScheduleDay> {seniorToHaveDayOffScheduleDay, juniorToRemoveDayOffScheduleDay},
				                     _scheduleDictionary)).Return(swapList1);
			Expect.Call(_swapServiceNew.Swap(new List<IScheduleDay> {seniorGiveDayOffScheduleDay, juniorAcceptDayOffScheduleDay},
			                                 _scheduleDictionary)).Return(swapList2);
            
		}
	}
}
