using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Interfaces.Domain;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.AgentRestrictions;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.AgentRestrictions
{
	[TestFixture]
	public class AgentRestrictionsModelTest
	{
		private AgentRestrictionsModel _model;
		private AgentRestrictionsDisplayRow _agentRestrictionsDisplayRow1;
		private AgentRestrictionsDisplayRow _agentRestrictionsDisplayRow2;
		private AgentRestrictionsDisplayRow _agentRestrictionsDisplayRow3;
		private MockRepository _mocks;
		private IScheduleMatrixPro _scheduleMatrixPro1;
		private IScheduleMatrixPro _scheduleMatrixPro2;
		private IScheduleMatrixPro _scheduleMatrixPro3;
		private IVirtualSchedulePeriod _schedulePeriod1;
		private IVirtualSchedulePeriod _schedulePeriod2;
		private IVirtualSchedulePeriod _schedulePeriod3;
		private IAgentRestrictionsDisplayRowCreator _agentRestrictionsDisplayRowCreator;
		private IPerson _person;
		private IList<IPerson> _persons;
		private DateOnlyPeriod _dateOnlyPeriod1;
		private DateOnlyPeriod _dateOnlyPeriod2;
		private DateOnlyPeriod _dateOnlyPeriod3;
		private IContract _contract1;
		private IContract _contract2;
		private IContract _contract3;
		

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_scheduleMatrixPro1 = _mocks.StrictMock<IScheduleMatrixPro>();
			_scheduleMatrixPro2 = _mocks.StrictMock<IScheduleMatrixPro>();
			_scheduleMatrixPro3 = _mocks.StrictMock<IScheduleMatrixPro>();
			_agentRestrictionsDisplayRow1 = new AgentRestrictionsDisplayRow(_scheduleMatrixPro1);
			_agentRestrictionsDisplayRow2 = new AgentRestrictionsDisplayRow(_scheduleMatrixPro2);
			_agentRestrictionsDisplayRow3 = new AgentRestrictionsDisplayRow(_scheduleMatrixPro3);
			_agentRestrictionsDisplayRowCreator = _mocks.StrictMock<IAgentRestrictionsDisplayRowCreator>();
			_schedulePeriod1 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_schedulePeriod2 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_schedulePeriod3 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_model = new AgentRestrictionsModel();
			_person = _mocks.StrictMock<IPerson>();
			_persons = new List<IPerson>{_person};
			_dateOnlyPeriod1 = new DateOnlyPeriod(2011, 1, 1, 2011, 1, 2);
			_dateOnlyPeriod2 = new DateOnlyPeriod(2011, 2, 1, 2011, 2, 2);
			_dateOnlyPeriod3 = new DateOnlyPeriod(2011, 3, 1, 2011, 3, 2);
			_contract1 = _mocks.StrictMock<IContract>();
			_contract2 = _mocks.StrictMock<IContract>();
			_contract3 = _mocks.StrictMock<IContract>();
		}

		[Test]
		public void ShouldReturnDisplayRows()
		{
			Assert.IsNotNull(_model.DisplayRows);	
		}

		[Test]
		public void ShouldLoadDisplayRows()
		{
			using (_mocks.Record())
			{
				Expect.Call(_agentRestrictionsDisplayRowCreator.Create(_persons)).Return(new List<AgentRestrictionsDisplayRow>{_agentRestrictionsDisplayRow1});
			}

			using (_mocks.Playback())
			{
				_model.LoadDisplayRows(_agentRestrictionsDisplayRowCreator, _persons);
				Assert.AreEqual(1, _model.DisplayRows.Count);
			}
		}

		[Test]
		public void ShouldGetDisplayRowFromIndex()
		{
			_model.DisplayRows.Add(_agentRestrictionsDisplayRow1);
			Assert.AreEqual(_agentRestrictionsDisplayRow1, _model.DisplayRowFromRowIndex(2));
		}

		[Test]
		public void ShouldSortAgentName()
		{
			_agentRestrictionsDisplayRow1.AgentName = "b";
			_agentRestrictionsDisplayRow2.AgentName = "c";
			_agentRestrictionsDisplayRow3.AgentName = "a";
			_model.DisplayRows.Add(_agentRestrictionsDisplayRow1);
			_model.DisplayRows.Add(_agentRestrictionsDisplayRow2);
			_model.DisplayRows.Add(_agentRestrictionsDisplayRow3);
			
			_model.SortAgentName(true);

			Assert.AreEqual(_agentRestrictionsDisplayRow3, _model.DisplayRows[0]);
			Assert.AreEqual(_agentRestrictionsDisplayRow1, _model.DisplayRows[1]);
			Assert.AreEqual(_agentRestrictionsDisplayRow2, _model.DisplayRows[2]);

			_model.SortAgentName(false);

			Assert.AreEqual(_agentRestrictionsDisplayRow3, _model.DisplayRows[2]);
			Assert.AreEqual(_agentRestrictionsDisplayRow1, _model.DisplayRows[1]);
			Assert.AreEqual(_agentRestrictionsDisplayRow2, _model.DisplayRows[0]);		
		}

		[Test]
		public void ShouldSortWarnings()
		{
			using(_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro2.SchedulePeriod).Return(_schedulePeriod1).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod1.DaysOff()).Return(11).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod1.Contract).Return(_contract1);
				Expect.Call(_contract1.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
			}

			using (_mocks.Playback())
			{
				_model.DisplayRows.Add(_agentRestrictionsDisplayRow1);
				_model.DisplayRows.Add(_agentRestrictionsDisplayRow2);
				_model.DisplayRows.Add(_agentRestrictionsDisplayRow3);

				_agentRestrictionsDisplayRow2.ContractTargetTime = TimeSpan.FromMinutes(10);
				_agentRestrictionsDisplayRow2.ContractCurrentTime = TimeSpan.FromMinutes(11);
				_agentRestrictionsDisplayRow2.CurrentDaysOff = 12;
				((IAgentDisplayData) _agentRestrictionsDisplayRow2).MinimumPossibleTime = TimeSpan.FromMinutes(17);
				((IAgentDisplayData) _agentRestrictionsDisplayRow2).MaximumPossibleTime = TimeSpan.FromMinutes(14);
				_agentRestrictionsDisplayRow2.MinMaxTime = new TimePeriod(TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(16));
				_agentRestrictionsDisplayRow2.SetWarnings();

				_model.SortWarnings(true);

				Assert.AreEqual(_agentRestrictionsDisplayRow3, _model.DisplayRows[1]);
				Assert.AreEqual(_agentRestrictionsDisplayRow1, _model.DisplayRows[0]);
				Assert.AreEqual(_agentRestrictionsDisplayRow2, _model.DisplayRows[2]);

				_model.SortWarnings(false);

				Assert.AreEqual(_agentRestrictionsDisplayRow3, _model.DisplayRows[2]);
				Assert.AreEqual(_agentRestrictionsDisplayRow1, _model.DisplayRows[1]);
				Assert.AreEqual(_agentRestrictionsDisplayRow2, _model.DisplayRows[0]);
			}	
		}

		[Test]
		public void ShouldSortType()
		{
			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro1.SchedulePeriod).Return(_schedulePeriod1).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod1.PeriodType).Return(SchedulePeriodType.Day).Repeat.AtLeastOnce();

				Expect.Call(_scheduleMatrixPro2.SchedulePeriod).Return(_schedulePeriod2).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod2.PeriodType).Return(SchedulePeriodType.Week).Repeat.AtLeastOnce();

				Expect.Call(_scheduleMatrixPro3.SchedulePeriod).Return(_schedulePeriod3).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod3.PeriodType).Return(SchedulePeriodType.Month).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				_model.DisplayRows.Add(_agentRestrictionsDisplayRow3);
				_model.DisplayRows.Add(_agentRestrictionsDisplayRow2);
				_model.DisplayRows.Add(_agentRestrictionsDisplayRow1);

				_model.SortPeriodType(true);

				Assert.AreEqual(_agentRestrictionsDisplayRow1, _model.DisplayRows[0]);
				Assert.AreEqual(_agentRestrictionsDisplayRow2, _model.DisplayRows[2]);
				Assert.AreEqual(_agentRestrictionsDisplayRow3, _model.DisplayRows[1]);

				_model.SortPeriodType(false);

				Assert.AreEqual(_agentRestrictionsDisplayRow1, _model.DisplayRows[2]);
				Assert.AreEqual(_agentRestrictionsDisplayRow2, _model.DisplayRows[0]);
				Assert.AreEqual(_agentRestrictionsDisplayRow3, _model.DisplayRows[1]);
			}	
		}

		[Test]
		public void ShouldSortFrom()
		{
			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro1.SchedulePeriod).Return(_schedulePeriod1).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod1.DateOnlyPeriod).Return(_dateOnlyPeriod1).Repeat.AtLeastOnce();

				Expect.Call(_scheduleMatrixPro2.SchedulePeriod).Return(_schedulePeriod2).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod2.DateOnlyPeriod).Return(_dateOnlyPeriod2).Repeat.AtLeastOnce();

				Expect.Call(_scheduleMatrixPro3.SchedulePeriod).Return(_schedulePeriod3).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod3.DateOnlyPeriod).Return(_dateOnlyPeriod3).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				_model.DisplayRows.Add(_agentRestrictionsDisplayRow3);
				_model.DisplayRows.Add(_agentRestrictionsDisplayRow2);
				_model.DisplayRows.Add(_agentRestrictionsDisplayRow1);

				_model.SortStartDate(true);

				Assert.AreEqual(_agentRestrictionsDisplayRow1, _model.DisplayRows[0]);
				Assert.AreEqual(_agentRestrictionsDisplayRow2, _model.DisplayRows[1]);
				Assert.AreEqual(_agentRestrictionsDisplayRow3, _model.DisplayRows[2]);

				_model.SortStartDate(false);

				Assert.AreEqual(_agentRestrictionsDisplayRow1, _model.DisplayRows[2]);
				Assert.AreEqual(_agentRestrictionsDisplayRow2, _model.DisplayRows[1]);
				Assert.AreEqual(_agentRestrictionsDisplayRow3, _model.DisplayRows[0]);
			}	
		}

		[Test]
		public void ShouldSortTo()
		{
			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro1.SchedulePeriod).Return(_schedulePeriod1).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod1.DateOnlyPeriod).Return(_dateOnlyPeriod1).Repeat.AtLeastOnce();

				Expect.Call(_scheduleMatrixPro2.SchedulePeriod).Return(_schedulePeriod2).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod2.DateOnlyPeriod).Return(_dateOnlyPeriod2).Repeat.AtLeastOnce();

				Expect.Call(_scheduleMatrixPro3.SchedulePeriod).Return(_schedulePeriod3).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod3.DateOnlyPeriod).Return(_dateOnlyPeriod3).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				_model.DisplayRows.Add(_agentRestrictionsDisplayRow3);
				_model.DisplayRows.Add(_agentRestrictionsDisplayRow2);
				_model.DisplayRows.Add(_agentRestrictionsDisplayRow1);

				_model.SortEndDate(true);

				Assert.AreEqual(_agentRestrictionsDisplayRow1, _model.DisplayRows[0]);
				Assert.AreEqual(_agentRestrictionsDisplayRow2, _model.DisplayRows[1]);
				Assert.AreEqual(_agentRestrictionsDisplayRow3, _model.DisplayRows[2]);

				_model.SortEndDate(false);

				Assert.AreEqual(_agentRestrictionsDisplayRow1, _model.DisplayRows[2]);
				Assert.AreEqual(_agentRestrictionsDisplayRow2, _model.DisplayRows[1]);
				Assert.AreEqual(_agentRestrictionsDisplayRow3, _model.DisplayRows[0]);
			}	
		}

		[Test]
		public void ShouldSortContractTargetTime()
		{
			_agentRestrictionsDisplayRow1.ContractTargetTime = TimeSpan.FromMinutes(10);
			_agentRestrictionsDisplayRow2.ContractTargetTime = TimeSpan.FromMinutes(15);
			_agentRestrictionsDisplayRow3.ContractTargetTime = TimeSpan.FromMinutes(5);
			_model.DisplayRows.Add(_agentRestrictionsDisplayRow1);
			_model.DisplayRows.Add(_agentRestrictionsDisplayRow2);
			_model.DisplayRows.Add(_agentRestrictionsDisplayRow3);

			_model.SortContractTargetTime(true);

			Assert.AreEqual(_agentRestrictionsDisplayRow3, _model.DisplayRows[0]);
			Assert.AreEqual(_agentRestrictionsDisplayRow1, _model.DisplayRows[1]);
			Assert.AreEqual(_agentRestrictionsDisplayRow2, _model.DisplayRows[2]);

			_model.SortContractTargetTime(false);

			Assert.AreEqual(_agentRestrictionsDisplayRow3, _model.DisplayRows[2]);
			Assert.AreEqual(_agentRestrictionsDisplayRow1, _model.DisplayRows[1]);
			Assert.AreEqual(_agentRestrictionsDisplayRow2, _model.DisplayRows[0]);		
		}

		[Test]
		public void ShouldSortOnTargetDayOffs()
		{
			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro1.SchedulePeriod).Return(_schedulePeriod1).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod1.DaysOff()).Return(1).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod1.Contract).Return(_contract1).Repeat.AtLeastOnce();
				Expect.Call(_contract1.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime).Repeat.AtLeastOnce();

				Expect.Call(_scheduleMatrixPro2.SchedulePeriod).Return(_schedulePeriod2).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod2.DaysOff()).Return(2).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod2.Contract).Return(_contract2).Repeat.AtLeastOnce();
				Expect.Call(_contract2.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime).Repeat.AtLeastOnce();

				Expect.Call(_scheduleMatrixPro3.SchedulePeriod).Return(_schedulePeriod3).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod3.DaysOff()).Return(3).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod3.Contract).Return(_contract3).Repeat.AtLeastOnce();
				Expect.Call(_contract3.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				_model.DisplayRows.Add(_agentRestrictionsDisplayRow3);
				_model.DisplayRows.Add(_agentRestrictionsDisplayRow2);
				_model.DisplayRows.Add(_agentRestrictionsDisplayRow1);

				_model.SortTargetDayOffs(true);

				Assert.AreEqual(_agentRestrictionsDisplayRow1, _model.DisplayRows[0]);
				Assert.AreEqual(_agentRestrictionsDisplayRow2, _model.DisplayRows[1]);
				Assert.AreEqual(_agentRestrictionsDisplayRow3, _model.DisplayRows[2]);

				_model.SortTargetDayOffs(false);

				Assert.AreEqual(_agentRestrictionsDisplayRow1, _model.DisplayRows[2]);
				Assert.AreEqual(_agentRestrictionsDisplayRow2, _model.DisplayRows[1]);
				Assert.AreEqual(_agentRestrictionsDisplayRow3, _model.DisplayRows[0]);
			}	
		}

		[Test]
		public void ShouldSortOnContractCurrentTime()
		{
			_agentRestrictionsDisplayRow1.ContractCurrentTime = TimeSpan.FromMinutes(10);
			_agentRestrictionsDisplayRow2.ContractCurrentTime = TimeSpan.FromMinutes(15);
			_agentRestrictionsDisplayRow3.ContractCurrentTime = TimeSpan.FromMinutes(5);
			_model.DisplayRows.Add(_agentRestrictionsDisplayRow1);
			_model.DisplayRows.Add(_agentRestrictionsDisplayRow2);
			_model.DisplayRows.Add(_agentRestrictionsDisplayRow3);

			_model.SortContractCurrentTime(true);

			Assert.AreEqual(_agentRestrictionsDisplayRow3, _model.DisplayRows[0]);
			Assert.AreEqual(_agentRestrictionsDisplayRow1, _model.DisplayRows[1]);
			Assert.AreEqual(_agentRestrictionsDisplayRow2, _model.DisplayRows[2]);

			_model.SortContractCurrentTime(false);

			Assert.AreEqual(_agentRestrictionsDisplayRow3, _model.DisplayRows[2]);
			Assert.AreEqual(_agentRestrictionsDisplayRow1, _model.DisplayRows[1]);
			Assert.AreEqual(_agentRestrictionsDisplayRow2, _model.DisplayRows[0]);	
		}

		[Test]
		public void ShouldSortOnScheduledDaysOff()
		{
			_agentRestrictionsDisplayRow1.CurrentDaysOff = 10;
			_agentRestrictionsDisplayRow2.CurrentDaysOff = 15;
			_agentRestrictionsDisplayRow3.CurrentDaysOff = 5;
			_model.DisplayRows.Add(_agentRestrictionsDisplayRow1);
			_model.DisplayRows.Add(_agentRestrictionsDisplayRow2);
			_model.DisplayRows.Add(_agentRestrictionsDisplayRow3);

			_model.SortCurrentDayOffs(true);

			Assert.AreEqual(_agentRestrictionsDisplayRow3, _model.DisplayRows[0]);
			Assert.AreEqual(_agentRestrictionsDisplayRow1, _model.DisplayRows[1]);
			Assert.AreEqual(_agentRestrictionsDisplayRow2, _model.DisplayRows[2]);

			_model.SortCurrentDayOffs(false);

			Assert.AreEqual(_agentRestrictionsDisplayRow3, _model.DisplayRows[2]);
			Assert.AreEqual(_agentRestrictionsDisplayRow1, _model.DisplayRows[1]);
			Assert.AreEqual(_agentRestrictionsDisplayRow2, _model.DisplayRows[0]);			
		}

		[Test]
		public void ShouldSortOnMin()
		{
			((IAgentDisplayData) _agentRestrictionsDisplayRow1).MinimumPossibleTime = TimeSpan.FromMinutes(10);
			((IAgentDisplayData)_agentRestrictionsDisplayRow2).MinimumPossibleTime = TimeSpan.FromMinutes(15);
			((IAgentDisplayData)_agentRestrictionsDisplayRow3).MinimumPossibleTime = TimeSpan.FromMinutes(5);
			
			_model.DisplayRows.Add(_agentRestrictionsDisplayRow1);
			_model.DisplayRows.Add(_agentRestrictionsDisplayRow2);
			_model.DisplayRows.Add(_agentRestrictionsDisplayRow3);

			_model.SortMinimumPossibleTime(true);

			Assert.AreEqual(_agentRestrictionsDisplayRow3, _model.DisplayRows[0]);
			Assert.AreEqual(_agentRestrictionsDisplayRow1, _model.DisplayRows[1]);
			Assert.AreEqual(_agentRestrictionsDisplayRow2, _model.DisplayRows[2]);

			_model.SortMinimumPossibleTime(false);

			Assert.AreEqual(_agentRestrictionsDisplayRow3, _model.DisplayRows[2]);
			Assert.AreEqual(_agentRestrictionsDisplayRow1, _model.DisplayRows[1]);
			Assert.AreEqual(_agentRestrictionsDisplayRow2, _model.DisplayRows[0]);		
		}

		[Test]
		public void ShouldSortOnMax()
		{
			((IAgentDisplayData)_agentRestrictionsDisplayRow1).MaximumPossibleTime = TimeSpan.FromMinutes(10);
			((IAgentDisplayData)_agentRestrictionsDisplayRow2).MaximumPossibleTime = TimeSpan.FromMinutes(15);
			((IAgentDisplayData)_agentRestrictionsDisplayRow3).MaximumPossibleTime = TimeSpan.FromMinutes(5);

			_model.DisplayRows.Add(_agentRestrictionsDisplayRow1);
			_model.DisplayRows.Add(_agentRestrictionsDisplayRow2);
			_model.DisplayRows.Add(_agentRestrictionsDisplayRow3);

			_model.SortMaximumPossibleTime(true);

			Assert.AreEqual(_agentRestrictionsDisplayRow3, _model.DisplayRows[0]);
			Assert.AreEqual(_agentRestrictionsDisplayRow1, _model.DisplayRows[1]);
			Assert.AreEqual(_agentRestrictionsDisplayRow2, _model.DisplayRows[2]);

			_model.SortMaximumPossibleTime(false);

			Assert.AreEqual(_agentRestrictionsDisplayRow3, _model.DisplayRows[2]);
			Assert.AreEqual(_agentRestrictionsDisplayRow1, _model.DisplayRows[1]);
			Assert.AreEqual(_agentRestrictionsDisplayRow2, _model.DisplayRows[0]);	
		}

		[Test]
		public void ShouldSortOnDayOffsSchedulesAndRestrictions()
		{
			((IAgentDisplayData) _agentRestrictionsDisplayRow1).ScheduledAndRestrictionDaysOff = 10;
			((IAgentDisplayData) _agentRestrictionsDisplayRow2).ScheduledAndRestrictionDaysOff = 15;
			((IAgentDisplayData) _agentRestrictionsDisplayRow3).ScheduledAndRestrictionDaysOff = 5;

			_model.DisplayRows.Add(_agentRestrictionsDisplayRow1);
			_model.DisplayRows.Add(_agentRestrictionsDisplayRow2);
			_model.DisplayRows.Add(_agentRestrictionsDisplayRow3);

			_model.SortScheduledAndRestrictionDayOffs(true);

			Assert.AreEqual(_agentRestrictionsDisplayRow3, _model.DisplayRows[0]);
			Assert.AreEqual(_agentRestrictionsDisplayRow1, _model.DisplayRows[1]);
			Assert.AreEqual(_agentRestrictionsDisplayRow2, _model.DisplayRows[2]);

			_model.SortScheduledAndRestrictionDayOffs(false);

			Assert.AreEqual(_agentRestrictionsDisplayRow3, _model.DisplayRows[2]);
			Assert.AreEqual(_agentRestrictionsDisplayRow1, _model.DisplayRows[1]);
			Assert.AreEqual(_agentRestrictionsDisplayRow2, _model.DisplayRows[0]);	
		}

		[Test]
		public void ShouldSortOnOk()
		{
			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro2.SchedulePeriod).Return(_schedulePeriod1).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod1.DaysOff()).Return(11).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod1.Contract).Return(_contract1);
				Expect.Call(_contract1.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
			}

			using (_mocks.Playback())
			{
				_model.DisplayRows.Add(_agentRestrictionsDisplayRow1);
				_model.DisplayRows.Add(_agentRestrictionsDisplayRow2);
				
				_agentRestrictionsDisplayRow2.ContractTargetTime = TimeSpan.FromMinutes(10);
				_agentRestrictionsDisplayRow2.ContractCurrentTime = TimeSpan.FromMinutes(11);
				_agentRestrictionsDisplayRow2.CurrentDaysOff = 12;
				((IAgentDisplayData)_agentRestrictionsDisplayRow2).MinimumPossibleTime = TimeSpan.FromMinutes(17);
				((IAgentDisplayData)_agentRestrictionsDisplayRow2).MaximumPossibleTime = TimeSpan.FromMinutes(14);
				_agentRestrictionsDisplayRow2.MinMaxTime = new TimePeriod(TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(16));
				_agentRestrictionsDisplayRow2.SetWarnings();

				_model.SortOk(true);

				Assert.AreEqual(_agentRestrictionsDisplayRow2, _model.DisplayRows[0]);
				Assert.AreEqual(_agentRestrictionsDisplayRow1, _model.DisplayRows[1]);
				

				_model.SortOk(false);

				Assert.AreEqual(_agentRestrictionsDisplayRow1, _model.DisplayRows[0]);
				Assert.AreEqual(_agentRestrictionsDisplayRow2, _model.DisplayRows[1]);
			}				
		}
	}
}
