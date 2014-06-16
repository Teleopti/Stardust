﻿using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling.ScheduleSortingCommands;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.ScheduleSortingCommands
{
	[TestFixture]
	public class SchedulerSortCommandMapperTest
	{
		private MockRepository _mock;
		private ISchedulerStateHolder _schedulerStateHolder;
		private SchedulerSortCommandMapper _target;
		private ILifetimeScope _container;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_schedulerStateHolder = _mock.StrictMock<ISchedulerStateHolder>();

			var builder = new ContainerBuilder();
			builder.RegisterType<RankedPersonBasedOnStartDate>().As<IRankedPersonBasedOnStartDate>().SingleInstance();
			builder.RegisterType<PersonStartDateFromPersonPeriod>().As<IPersonStartDateFromPersonPeriod>().SingleInstance();

			_container = builder.Build();

			_target = new SchedulerSortCommandMapper(_schedulerStateHolder, SchedulerSortCommandSetting.NoSortCommand, _container);
		}

		[Test]
		public void ShouldMapFromSettingToCommand()
		{
			var noSortCommand = _target.GetCommandFromSetting(SchedulerSortCommandSetting.NoSortCommand);
			Assert.IsTrue(noSortCommand is NoSortCommand);

			var sortByStartAscendingCommand = _target.GetCommandFromSetting(SchedulerSortCommandSetting.SortByStartAscending);
			Assert.IsTrue(sortByStartAscendingCommand is SortByStartAscendingCommand);

			var sortByStartDescendingCommand = _target.GetCommandFromSetting(SchedulerSortCommandSetting.SortByStartDescending);
			Assert.IsTrue(sortByStartDescendingCommand is SortByStartDescendingCommand);

			var sortByEndDescendingCommand = _target.GetCommandFromSetting(SchedulerSortCommandSetting.SortByEndDescending);
			Assert.IsTrue(sortByEndDescendingCommand is SortByEndDescendingCommand);

			var sortByEndAscendingCommand = _target.GetCommandFromSetting(SchedulerSortCommandSetting.SortByEndAscending);
			Assert.IsTrue(sortByEndAscendingCommand is SortByEndAscendingCommand);

			var sortByContractTimeAscendingCommand = _target.GetCommandFromSetting(SchedulerSortCommandSetting.SortByContractTimeAscending);
			Assert.IsTrue(sortByContractTimeAscendingCommand is SortByContractTimeAscendingCommand);

			var sortByContractTimeDescendingCommand = _target.GetCommandFromSetting(SchedulerSortCommandSetting.SortByContractTimeDescending);
			Assert.IsTrue(sortByContractTimeDescendingCommand is SortByContractTimeDescendingCommand);

			var sortBySeniorityRankAscendingCommand = _target.GetCommandFromSetting(SchedulerSortCommandSetting.SortBySeniorityRankingAscending);
			Assert.IsTrue(sortBySeniorityRankAscendingCommand is SortBySeniorityRankingAscendingCommand);

		}

		[Test]
		public void ShouldMapFromCommandToSetting()
		{
			var noSortCommandSetting = _target.GetSettingFromCommand(new NoSortCommand(_schedulerStateHolder));
			Assert.IsTrue(noSortCommandSetting == SchedulerSortCommandSetting.NoSortCommand);

			var sortByContractTimeAscendingSetting =
				_target.GetSettingFromCommand(new SortByContractTimeAscendingCommand(_schedulerStateHolder));
			Assert.IsTrue(sortByContractTimeAscendingSetting == SchedulerSortCommandSetting.SortByContractTimeAscending);

			var sortByContractTimeDescendingSetting =
				_target.GetSettingFromCommand(new SortByContractTimeDescendingCommand(_schedulerStateHolder));
			Assert.IsTrue(sortByContractTimeDescendingSetting == SchedulerSortCommandSetting.SortByContractTimeDescending);

			var sortByEndAscendingSetting = _target.GetSettingFromCommand(new SortByEndAscendingCommand(_schedulerStateHolder));
			Assert.IsTrue(sortByEndAscendingSetting == SchedulerSortCommandSetting.SortByEndAscending);

			var sortByEndDescendingSetting = _target.GetSettingFromCommand(new SortByEndDescendingCommand(_schedulerStateHolder));
			Assert.IsTrue(sortByEndDescendingSetting == SchedulerSortCommandSetting.SortByEndDescending);

			var sortByStartAscendingSetting =
				_target.GetSettingFromCommand(new SortByStartAscendingCommand(_schedulerStateHolder));
			Assert.IsTrue(sortByStartAscendingSetting == SchedulerSortCommandSetting.SortByStartAscending);

			var sortByStartDescendingSetting =
				_target.GetSettingFromCommand(new SortByStartDescendingCommand(_schedulerStateHolder));
			Assert.IsTrue(sortByStartDescendingSetting == SchedulerSortCommandSetting.SortByStartDescending);

			var sortBySeniorityRankAscendingCommand =
				_target.GetSettingFromCommand(new SortBySeniorityRankingAscendingCommand(_schedulerStateHolder, new RankedPersonBasedOnStartDate(null)));
			Assert.IsTrue(sortBySeniorityRankAscendingCommand == SchedulerSortCommandSetting.SortBySeniorityRankingAscending);
		}
	}
}
