using System.Collections.Generic;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	[TestFixture]
	public class TeamBlockSeniorityFairnessOptimizationServiceTest
	{
		private MockRepository _mock;
		private TeamBlockSeniorityFairnessOptimizationService _target;
		private IConstructTeamBlock _constructTeamBlock;
		private IDetermineTeamBlockPriority _determineTeamBlockPriority;
		private ITeamBlockPeriodValidator _teamBlockPeriodValidator;
		private ITeamBlockSeniorityValidator _teamBlockSeniorityValidator;
		private ITeamBlockSwap _teamBlockSwap;
		private IScheduleMatrixPro _scheduleMatrixPro1;
		private IScheduleMatrixPro _scheduleMatrixPro2;
		private IList<IScheduleMatrixPro> _scheduleMatrixPros;
		private DateOnlyPeriod _dateOnlyPeriod;
		private IPerson _person1;
		private IPerson _person2;
		private IList<IPerson> _persons;
		private ISchedulingOptions _schedulingOptions;
		private IShiftCategory _shiftCategoryA;
		private IShiftCategory _shiftCategoryB;
		private IList<IShiftCategory> _shiftCategories; 
		private IScheduleDictionary _scheduleDictionary;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private ITeamBlockInfo _teamBlockInfo1;
		private ITeamBlockInfo _teamBlockInfo2;
		private IList<ITeamBlockInfo> _teamBlockInfos;
		private ITeamBlockPriorityDefinitionInfo _teamBlockPriorityDefinitionInfo;
		private IList<ITeamBlockInfo> _highToLowShiftCategoryPrioryList;
		private IFilterForTeamBlockInSelection _filterForTeamBlockInSelection;
		private IOptimizationPreferences _optimizationPreferences;

		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_constructTeamBlock = _mock.StrictMock<IConstructTeamBlock>();
			_determineTeamBlockPriority = _mock.StrictMock<IDetermineTeamBlockPriority>();
			_teamBlockPeriodValidator = _mock.StrictMock<ITeamBlockPeriodValidator>();
			_teamBlockSeniorityValidator = _mock.StrictMock<ITeamBlockSeniorityValidator>();
			_teamBlockSwap = _mock.StrictMock<ITeamBlockSwap>();
			_scheduleMatrixPro1 = _mock.StrictMock<IScheduleMatrixPro>();
			_scheduleMatrixPro2 = _mock.StrictMock<IScheduleMatrixPro>();
			_scheduleMatrixPros = new List<IScheduleMatrixPro> { _scheduleMatrixPro1, _scheduleMatrixPro2 };
			_dateOnlyPeriod = new DateOnlyPeriod(2013, 1, 1, 2013, 1, 1);
			_person1 = PersonFactory.CreatePerson("PersonA");
			_person2 = PersonFactory.CreatePerson("PersonB");
			_persons = new List<IPerson> { _person1, _person2 };
			_schedulingOptions = new SchedulingOptions();
			_shiftCategoryA = ShiftCategoryFactory.CreateShiftCategory("AA");
			_shiftCategoryB = ShiftCategoryFactory.CreateShiftCategory("BB");
			_shiftCategories = new List<IShiftCategory> { _shiftCategoryA, _shiftCategoryB };
			_scheduleDictionary = _mock.StrictMock<IScheduleDictionary>();
			_rollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
			_teamBlockInfo1 = _mock.StrictMock<ITeamBlockInfo>();
			_teamBlockInfo2 = _mock.StrictMock<ITeamBlockInfo>();
			_teamBlockInfos = new List<ITeamBlockInfo> { _teamBlockInfo1, _teamBlockInfo2 };
			_highToLowShiftCategoryPrioryList = new List<ITeamBlockInfo>{_teamBlockInfo2, _teamBlockInfo1};
			_teamBlockPriorityDefinitionInfo = _mock.StrictMock<ITeamBlockPriorityDefinitionInfo>();
			_filterForTeamBlockInSelection = _mock.StrictMock<IFilterForTeamBlockInSelection>();
			_optimizationPreferences = _mock.StrictMock<IOptimizationPreferences>();
			_target = new TeamBlockSeniorityFairnessOptimizationService(_constructTeamBlock, _determineTeamBlockPriority, _teamBlockPeriodValidator, _teamBlockSeniorityValidator, _teamBlockSwap, _filterForTeamBlockInSelection);
		}

		[Test]
		public void ShouldExecute()
		{
			using (_mock.Record())
			{
				Expect.Call(_constructTeamBlock.Construct(_scheduleMatrixPros, _dateOnlyPeriod, _persons, 
																 _schedulingOptions.BlockFinderTypeForAdvanceScheduling,
																 _schedulingOptions.GroupOnGroupPageForTeamBlockPer)).Return(_teamBlockInfos);
				Expect.Call(_teamBlockSeniorityValidator.ValidateSeniority(_teamBlockInfo1)).Return(true);
				Expect.Call(_teamBlockSeniorityValidator.ValidateSeniority(_teamBlockInfo2)).Return(true);
				Expect.Call(_filterForTeamBlockInSelection.Filter(_teamBlockInfos, _persons, _dateOnlyPeriod)).Return(_teamBlockInfos);
				Expect.Call(_determineTeamBlockPriority.CalculatePriority(_teamBlockInfos, _shiftCategories)).Return(_teamBlockPriorityDefinitionInfo);
				Expect.Call(_teamBlockPriorityDefinitionInfo.HighToLowSeniorityListBlockInfo).Return(_teamBlockInfos);
				Expect.Call(_teamBlockPriorityDefinitionInfo.HighToLowShiftCategoryPriority()).Return(_highToLowShiftCategoryPrioryList);
				Expect.Call(_teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(_teamBlockInfo1)).Return(1);
				Expect.Call(_teamBlockPeriodValidator.ValidatePeriod(_teamBlockInfo1, _teamBlockInfo2)).Return(true);
				Expect.Call(_teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(_teamBlockInfo2)).Return(2);
				Expect.Call(_teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(_teamBlockInfo2)).Return(1);
				Expect.Call(_teamBlockSwap.Swap(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _scheduleDictionary, _dateOnlyPeriod, _optimizationPreferences)).Return(true);
				Expect.Call(() => _teamBlockPriorityDefinitionInfo.SetShiftCategoryPoint(_teamBlockInfo2, 1));
			}

			using (_mock.Playback())
			{
				_target.Execute(_scheduleMatrixPros, _dateOnlyPeriod, _persons, _schedulingOptions, _shiftCategories, _scheduleDictionary, _rollbackService, _optimizationPreferences);
			}
		}

		[Test]
		public void ShouldSkipAndRetryWhenNotPossibleToSwap()
		{
			using (_mock.Record())
			{
				Expect.Call(_constructTeamBlock.Construct(_scheduleMatrixPros, _dateOnlyPeriod, _persons, 
																 _schedulingOptions.BlockFinderTypeForAdvanceScheduling,
																 _schedulingOptions.GroupOnGroupPageForTeamBlockPer)).Return(_teamBlockInfos).Repeat.Twice();
				Expect.Call(_teamBlockSeniorityValidator.ValidateSeniority(_teamBlockInfo1)).Return(true).Repeat.Twice();
				Expect.Call(_teamBlockSeniorityValidator.ValidateSeniority(_teamBlockInfo2)).Return(true).Repeat.Twice();
				Expect.Call(_filterForTeamBlockInSelection.Filter(_teamBlockInfos, _persons, _dateOnlyPeriod)).Return(_teamBlockInfos).Repeat.Twice();
				Expect.Call(_determineTeamBlockPriority.CalculatePriority(_teamBlockInfos, _shiftCategories)).Return(_teamBlockPriorityDefinitionInfo).Repeat.Twice();
				Expect.Call(_teamBlockPriorityDefinitionInfo.HighToLowSeniorityListBlockInfo).Return(_teamBlockInfos).Repeat.Twice();
				Expect.Call(_teamBlockPriorityDefinitionInfo.HighToLowShiftCategoryPriority()).Return(_highToLowShiftCategoryPrioryList).Repeat.Twice();

				Expect.Call(_teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(_teamBlockInfo1)).Return(1).Repeat.Twice();
				Expect.Call(_teamBlockPeriodValidator.ValidatePeriod(_teamBlockInfo1, _teamBlockInfo2)).Return(true);
				Expect.Call(_teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(_teamBlockInfo2)).Return(2);
				Expect.Call(_teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(_teamBlockInfo2)).Return(1);
				Expect.Call(_teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(_teamBlockInfo2)).Return(1);
				Expect.Call(_teamBlockSwap.Swap(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _scheduleDictionary, _dateOnlyPeriod, _optimizationPreferences)).Return(false);
			}

			using (_mock.Playback())
			{
				_target.Execute(_scheduleMatrixPros, _dateOnlyPeriod, _persons, _schedulingOptions, _shiftCategories, _scheduleDictionary, _rollbackService, _optimizationPreferences);
			}	
		}

		[Test]
		public void ShouldSkipOnNotValidatedPeriod()
		{
			using (_mock.Record())
			{
				Expect.Call(_constructTeamBlock.Construct(_scheduleMatrixPros, _dateOnlyPeriod, _persons, 
																 _schedulingOptions.BlockFinderTypeForAdvanceScheduling,
																 _schedulingOptions.GroupOnGroupPageForTeamBlockPer)).Return(_teamBlockInfos);
				Expect.Call(_teamBlockSeniorityValidator.ValidateSeniority(_teamBlockInfo1)).Return(true);
				Expect.Call(_teamBlockSeniorityValidator.ValidateSeniority(_teamBlockInfo2)).Return(true);
				Expect.Call(_filterForTeamBlockInSelection.Filter(_teamBlockInfos, _persons, _dateOnlyPeriod)).Return(_teamBlockInfos);
				Expect.Call(_determineTeamBlockPriority.CalculatePriority(_teamBlockInfos, _shiftCategories)).Return(_teamBlockPriorityDefinitionInfo);
				Expect.Call(_teamBlockPriorityDefinitionInfo.HighToLowSeniorityListBlockInfo).Return(_teamBlockInfos);
				Expect.Call(_teamBlockPriorityDefinitionInfo.HighToLowShiftCategoryPriority()).Return(_highToLowShiftCategoryPrioryList);
				Expect.Call(_teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(_teamBlockInfo1)).Return(1);
				Expect.Call(_teamBlockPeriodValidator.ValidatePeriod(_teamBlockInfo1, _teamBlockInfo2)).Return(false);
				Expect.Call(_teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(_teamBlockInfo2)).Return(2);
			}

			using (_mock.Playback())
			{
				_target.Execute(_scheduleMatrixPros, _dateOnlyPeriod, _persons, _schedulingOptions, _shiftCategories, _scheduleDictionary, _rollbackService, _optimizationPreferences);
			}	
		}

		[Test]
		public void ShouldCancel()
		{
			using (_mock.Record())
			{
				Expect.Call(_constructTeamBlock.Construct(_scheduleMatrixPros, _dateOnlyPeriod, _persons, 
																 _schedulingOptions.BlockFinderTypeForAdvanceScheduling,
																 _schedulingOptions.GroupOnGroupPageForTeamBlockPer)).Return(_teamBlockInfos);
				Expect.Call(_teamBlockSeniorityValidator.ValidateSeniority(_teamBlockInfo1)).Return(true);
				Expect.Call(_teamBlockSeniorityValidator.ValidateSeniority(_teamBlockInfo2)).Return(true);
				Expect.Call(_filterForTeamBlockInSelection.Filter(_teamBlockInfos, _persons, _dateOnlyPeriod)).Return(_teamBlockInfos);
				Expect.Call(_determineTeamBlockPriority.CalculatePriority(_teamBlockInfos, _shiftCategories)).Return(_teamBlockPriorityDefinitionInfo);
				Expect.Call(_teamBlockPriorityDefinitionInfo.HighToLowSeniorityListBlockInfo).Return(_teamBlockInfos);
				Expect.Call(_teamBlockPriorityDefinitionInfo.HighToLowShiftCategoryPriority()).Return(_highToLowShiftCategoryPrioryList);
				Expect.Call(_teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(_teamBlockInfo1)).Return(1);
				Expect.Call(_teamBlockPeriodValidator.ValidatePeriod(_teamBlockInfo1, _teamBlockInfo2)).Return(true);
				Expect.Call(_teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(_teamBlockInfo2)).Return(2);

				Expect.Call(_teamBlockSwap.Swap(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _scheduleDictionary, _dateOnlyPeriod, _optimizationPreferences)).Return(true);
				Expect.Call(() => _teamBlockPriorityDefinitionInfo.SetShiftCategoryPoint(_teamBlockInfo2, 1));
			}

			using (_mock.Playback())
			{
				_target.ReportProgress += reportProgress;
				_target.Execute(_scheduleMatrixPros, _dateOnlyPeriod, _persons, _schedulingOptions, _shiftCategories, _scheduleDictionary, _rollbackService, _optimizationPreferences);
				_target.ReportProgress -= reportProgress;
			}		
		}

		private void reportProgress(object sender, ResourceOptimizerProgressEventArgs e)
		{
			e.Cancel = true;
		}
	}
}
