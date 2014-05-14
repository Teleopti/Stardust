using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.MoveTimeOptimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.MoveTimeOptimization
{
	[Ignore,TestFixture]
	public class TeamBlockMoveTimeOptimizerTest
	{
		private MockRepository _mock;
		private ITeamBlockMoveTimeOptimizer _target;
		private ILockUnSelectedInTeamBlock _lockUnSelectedInTeamBlock;
		private ITeamBlockGenerator _teamBlockGenerator;
		private ITeamBlockScheduler _teamBlockScheduler;
		private ITeamBlockRestrictionOverLimitValidator _teamBlockRestrictionOverLimitValidator;
		private IList<IScheduleMatrixPro> _matrixList;
		private IScheduleMatrixPro _matrix1;
		private ISchedulingOptions _schedulingOptions;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private IResourceCalculateDelayer _resourceCalculateDelayer;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IOptimizationPreferences _optimizationPreferences;
		private IPerson _person1;
		private ITeamBlockInfo _teamBlockInfo;
		private ISchedulingOptionsCreator _schedulingOptionsCreator;
		private ITeamBlockMoveTimeDescisionMaker _decisionMaker;
		private IDeleteAndResourceCalculateService _deleteAndResourceCalculateService;
		private IResourceOptimizationHelper _resourceOptimizationHelper;
		private IConstructAndScheduleSingleDayTeamBlock _constructAndScheduleSingleDayTeamBlock;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_schedulingOptionsCreator = _mock.StrictMock<ISchedulingOptionsCreator>();
			_decisionMaker = _mock.StrictMock<ITeamBlockMoveTimeDescisionMaker>();
			_deleteAndResourceCalculateService = _mock.StrictMock<IDeleteAndResourceCalculateService>();
			_resourceOptimizationHelper = _mock.StrictMock<IResourceOptimizationHelper>();
			_constructAndScheduleSingleDayTeamBlock = _mock.StrictMock<IConstructAndScheduleSingleDayTeamBlock>();
			_target = new TeamBlockMoveTimeOptimizer(_schedulingOptionsCreator,_decisionMaker,_deleteAndResourceCalculateService,_resourceOptimizationHelper,_constructAndScheduleSingleDayTeamBlock );
			_matrix1 = _mock.StrictMock<IScheduleMatrixPro>();
			_matrixList = new List<IScheduleMatrixPro> { _matrix1 };
			_schedulingOptions = new SchedulingOptions();
			_rollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
			_resourceCalculateDelayer = _mock.StrictMock<IResourceCalculateDelayer>();
			_schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
			_optimizationPreferences = new OptimizationPreferences();
			_person1 = PersonFactory.CreatePerson("test");
			_teamBlockInfo = _mock.StrictMock<ITeamBlockInfo>();
		}

		[Test]
		public void ShouldReturnFalseIfFoundDaysAreEqual()
		{
			using (_mock.Record())
			{
				
			}
			using (_mock.Playback())
			{
				
			}
		}

		[Test]
		public void ShouldReturnTrueIfHigherContractTime()
		{
			using (_mock.Record())
			{

			}
			using (_mock.Playback())
			{

			}
		}

		[Test]
		public void ShouldOptimizeSuccessfully()
		{
			using (_mock.Record())
			{

			}
			using (_mock.Playback())
			{

			}
		}
	}
}
