using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[TestFixture]
	public class TeamSteadyStateMainShiftSchedulerTest
	{
		private MockRepository _mocks;
		private TeamSteadyStateMainShiftScheduler _target;
		private DateOnly _dateOnly;
		private IPerson _person1;
		private IPerson _person2;
		private IGroupPerson _groupPerson;
		private IGroupSchedulingService _groupSchedulingService;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private ISchedulingOptions _schedulingOptions;
		private IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
		private IList<IScheduleMatrixPro> _matrixes;
		private IScheduleDictionary _scheduleDictionary;
		private IScheduleRange _scheduleRange1;
		private IScheduleRange _scheduleRange2;
		private IScheduleDay _scheduleDay1;
		private IScheduleDay _scheduleDay2;
		private IPersonAssignment _personAssignment1;
		private IEditableShift _mainShift1;
		private IScheduleMatrixPro _scheduleMatrixPro;
		private IScheduleMatrixPro _scheduleMatrixPro2;
		private IScheduleDayPro _scheduleDayPro;
		private Guid _guid;
		private ITeamSteadyStateCoherentChecker _coherentChecker;
		private ITeamSteadyStateScheduleMatrixProFinder _teamSteadyStateScheduleMatrixProFinder;
		private IResourceOptimizationHelper _resourceOptimizationHelper;
		private DateTimePeriod _dateTimePeriod;
		private IEditableShiftMapper _editableShiftMapper;
	
		[SetUp]
		public void Setup()
		{
			_guid = Guid.NewGuid();
			_mocks = new MockRepository();
			_editableShiftMapper = _mocks.StrictMock<IEditableShiftMapper>();
			_dateOnly = new DateOnly(2012, 1, 1);
			_person1 = PersonFactory.CreatePersonWithPersonPeriod(_dateOnly, new List<ISkill>());
			_person2 = PersonFactory.CreatePersonWithPersonPeriod(_dateOnly, new List<ISkill>());
			_groupPerson = new GroupPerson(new List<IPerson> { _person1, _person2 }, _dateOnly, "groupPerson1", _guid);
			_groupSchedulingService = _mocks.StrictMock<IGroupSchedulingService>();
			_rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_schedulingOptions = new SchedulingOptions();
			_groupPersonBuilderForOptimization = _mocks.StrictMock<IGroupPersonBuilderForOptimization>();
			_scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			_scheduleMatrixPro2 = _mocks.StrictMock<IScheduleMatrixPro>();
			_scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
			_matrixes = new List<IScheduleMatrixPro>{_scheduleMatrixPro, _scheduleMatrixPro2};
			_scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			_scheduleRange1 = _mocks.StrictMock<IScheduleRange>();
			_scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			_personAssignment1 = _mocks.StrictMock<IPersonAssignment>();
			_mainShift1 = _mocks.StrictMock<IEditableShift>();
			_scheduleRange2 = _mocks.StrictMock<IScheduleRange>();
			_scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			_coherentChecker = _mocks.StrictMock<ITeamSteadyStateCoherentChecker>();
			_teamSteadyStateScheduleMatrixProFinder = _mocks.StrictMock<ITeamSteadyStateScheduleMatrixProFinder>();
			_resourceOptimizationHelper = _mocks.StrictMock<IResourceOptimizationHelper>();
			_dateTimePeriod = new DateTimePeriod(2012, 1, 1, 2012, 1, 2);
			_target = new TeamSteadyStateMainShiftScheduler(_coherentChecker, _teamSteadyStateScheduleMatrixProFinder, _resourceOptimizationHelper, _editableShiftMapper);
		}

		[Test]
		public void ShouldReturnFalseWhenNoCoherence()
		{
			using(_mocks.Record())
			{
				Expect.Call(_scheduleDictionary[_person1]).Return(_scheduleRange1);
				Expect.Call(_scheduleRange1.ScheduledDay(_dateOnly)).Return(_scheduleDay1);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.None);
				Expect.Call(_teamSteadyStateScheduleMatrixProFinder.MatrixPro(_matrixes, _scheduleDay1)).Return(_scheduleMatrixPro);
				Expect.Call(_coherentChecker.CheckCoherent(_matrixes, _dateOnly, _scheduleDictionary, _scheduleDay1, _groupPerson.GroupMembers)).Return(null);
			}

			using(_mocks.Playback())
			{
				var result = _target.ScheduleTeam(_dateOnly, _groupPerson, _groupSchedulingService, _rollbackService, _schedulingOptions, _groupPersonBuilderForOptimization, _matrixes, _scheduleDictionary);
				Assert.IsFalse(result);
			}	
		}

		[Test]
		public void ShouldSkipMemberIfNoMatrix()
		{
			using (_mocks.Record())
			{
				Expect.Call(_scheduleDictionary[_person1]).Return(_scheduleRange1);
				Expect.Call(_scheduleDictionary[_person2]).Return(_scheduleRange2);
				Expect.Call(_scheduleRange1.ScheduledDay(_dateOnly)).Return(_scheduleDay1);
				Expect.Call(_scheduleRange2.ScheduledDay(_dateOnly)).Return(_scheduleDay2);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.None);
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.None);
				Expect.Call(_teamSteadyStateScheduleMatrixProFinder.MatrixPro(_matrixes, _scheduleDay1)).Return(null);
				Expect.Call(_teamSteadyStateScheduleMatrixProFinder.MatrixPro(_matrixes, _scheduleDay2)).Return(_scheduleMatrixPro);
				Expect.Call(_coherentChecker.CheckCoherent(_matrixes, _dateOnly, _scheduleDictionary, _scheduleDay2, _groupPerson.GroupMembers)).Return(null);
			}

			using (_mocks.Playback())
			{
				var result = _target.ScheduleTeam(_dateOnly, _groupPerson, _groupSchedulingService, _rollbackService, _schedulingOptions, _groupPersonBuilderForOptimization, _matrixes, _scheduleDictionary);
				Assert.IsFalse(result);
			}	
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldSetAllMembersWithSameMainShift()
		{
			using(_mocks.Record())
			{
				Expect.Call(_scheduleDictionary[_person1]).Return(_scheduleRange1).Repeat.Twice();
				Expect.Call(_scheduleRange1.ScheduledDay(_dateOnly)).Return(_scheduleDay1).Repeat.Twice();
				Expect.Call(_scheduleDay1.PersonAssignment()).Return(_personAssignment1);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
				Expect.Call(_editableShiftMapper.CreateEditorShift(_personAssignment1)).Return(_mainShift1).Repeat.Twice();
				Expect.Call(_scheduleDictionary[_person2]).Return(_scheduleRange2);
				Expect.Call(_scheduleRange2.ScheduledDay(_dateOnly)).Return(_scheduleDay2);
				Expect.Call(()=>_scheduleDay2.AddMainShift(_mainShift1));
				Expect.Call(()=>_rollbackService.Modify(_scheduleDay2));
				Expect.Call(_scheduleDay2.SignificantPartForDisplay()).Return(SchedulePartView.None);
				Expect.Call(_scheduleDay1.SignificantPartForDisplay()).Return(SchedulePartView.MainShift);

				Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
				Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(_scheduleDayPro);

				Expect.Call(_scheduleMatrixPro2.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>{_scheduleDayPro}));
				Expect.Call(_scheduleMatrixPro2.GetScheduleDayByKey(_dateOnly)).Return(_scheduleDayPro);

				Expect.Call(_coherentChecker.CheckCoherent(_matrixes, _dateOnly, _scheduleDictionary, _scheduleDay1, _groupPerson.GroupMembers)).Return(_scheduleDay1);
				Expect.Call(_teamSteadyStateScheduleMatrixProFinder.MatrixPro(_matrixes, _scheduleDay1)).Return(_scheduleMatrixPro);
				Expect.Call(_teamSteadyStateScheduleMatrixProFinder.MatrixPro(_matrixes, _scheduleDay2)).Return(_scheduleMatrixPro2);
				Expect.Call(_scheduleDay2.Period).Return(_dateTimePeriod);
				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_dateOnly, true, true, new List<IScheduleDay>(), new List<IScheduleDay> {_scheduleDay2}));
				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_dateOnly.AddDays(1), true, true, new List<IScheduleDay>(), new List<IScheduleDay> { _scheduleDay2 }));
			}

			using(_mocks.Playback())
			{
				var result = _target.ScheduleTeam(_dateOnly, _groupPerson, _groupSchedulingService, _rollbackService, _schedulingOptions, _groupPersonBuilderForOptimization, _matrixes, _scheduleDictionary);
				Assert.IsTrue(result);	
			}
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldScheduleFirstMemberIfNoMainShift()
		{
			_groupPerson = new GroupPerson(new List<IPerson> { _person1}, _dateOnly, "groupPerson1", _guid);

			using(_mocks.Record())
			{
				Expect.Call(_scheduleDictionary[_person1]).Return(_scheduleRange1).Repeat.Twice();
				Expect.Call(_scheduleRange1.ScheduledDay(_dateOnly)).Return(_scheduleDay1).Repeat.Twice();
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.None).Repeat.Twice();
				Expect.Call(_teamSteadyStateScheduleMatrixProFinder.MatrixPro(_matrixes, _scheduleDay1)).Return(_scheduleMatrixPro);
				Expect.Call(_coherentChecker.CheckCoherent(_matrixes, _dateOnly, _scheduleDictionary, _scheduleDay1, _groupPerson.GroupMembers)).Return(_scheduleDay1);
				Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
				Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(_scheduleDayPro);
				Expect.Call(_groupSchedulingService.ScheduleOneDayOnePersonSteadyState(_dateOnly, _person1, _schedulingOptions, _groupPerson, _matrixes, _rollbackService)).Return(true).IgnoreArguments();
				Expect.Call(_scheduleDay1.PersonAssignment()).Return(_personAssignment1);
			}

			using(_mocks.Playback())
			{
				var result = _target.ScheduleTeam(_dateOnly, _groupPerson, _groupSchedulingService, _rollbackService, _schedulingOptions, _groupPersonBuilderForOptimization, _matrixes, _scheduleDictionary);
				Assert.IsTrue(result);	
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldReturnFalseIfNotPossibleToSchedule()
		{
			_groupPerson = new GroupPerson(new List<IPerson> { _person1 }, _dateOnly, "groupPerson1", _guid);

			using (_mocks.Record())
			{
				Expect.Call(_scheduleDictionary[_person1]).Return(_scheduleRange1);
				Expect.Call(_scheduleRange1.ScheduledDay(_dateOnly)).Return(_scheduleDay1);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.None).Repeat.Twice();
				Expect.Call(_teamSteadyStateScheduleMatrixProFinder.MatrixPro(_matrixes, _scheduleDay1)).Return(_scheduleMatrixPro);
				Expect.Call(_coherentChecker.CheckCoherent(_matrixes, _dateOnly, _scheduleDictionary, _scheduleDay1, _groupPerson.GroupMembers)).Return(_scheduleDay1);
				Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
				Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(_scheduleDayPro);
				Expect.Call(_groupSchedulingService.ScheduleOneDayOnePersonSteadyState(_dateOnly, _person1, _schedulingOptions, _groupPerson, _matrixes, _rollbackService)).Return(false).IgnoreArguments();
			}

			using (_mocks.Playback())
			{
				var result = _target.ScheduleTeam(_dateOnly, _groupPerson, _groupSchedulingService, _rollbackService, _schedulingOptions, _groupPersonBuilderForOptimization, _matrixes, _scheduleDictionary);
				Assert.IsFalse(result);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldNotAddMainShiftIfGroupMemberDayIsLocked()
		{
			using (_mocks.Record())
			{
				Expect.Call(_scheduleDictionary[_person1]).Return(_scheduleRange1).Repeat.Twice();
				Expect.Call(_scheduleRange1.ScheduledDay(_dateOnly)).Return(_scheduleDay1).Repeat.Twice();	
				Expect.Call(_scheduleDay1.PersonAssignment()).Return(_personAssignment1);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
				Expect.Call(_editableShiftMapper.CreateEditorShift(_personAssignment1)).Return(_mainShift1).Repeat.Twice();
				Expect.Call(_scheduleDictionary[_person2]).Return(_scheduleRange2);
				Expect.Call(_scheduleRange2.ScheduledDay(_dateOnly)).Return(_scheduleDay2);
				Expect.Call(_scheduleDay2.SignificantPartForDisplay()).Return(SchedulePartView.None);
				Expect.Call(_scheduleDay1.SignificantPartForDisplay()).Return(SchedulePartView.MainShift);

				Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>{_scheduleDayPro}));
				Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(_scheduleDayPro);

				Expect.Call(_scheduleMatrixPro2.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>()));
				Expect.Call(_scheduleMatrixPro2.GetScheduleDayByKey(_dateOnly)).Return(_scheduleDayPro);

				Expect.Call(_coherentChecker.CheckCoherent(_matrixes, _dateOnly, _scheduleDictionary, _scheduleDay1, _groupPerson.GroupMembers)).Return(_scheduleDay1);
				Expect.Call(_teamSteadyStateScheduleMatrixProFinder.MatrixPro(_matrixes, _scheduleDay1)).Return(_scheduleMatrixPro);
				Expect.Call(_teamSteadyStateScheduleMatrixProFinder.MatrixPro(_matrixes, _scheduleDay2)).Return(_scheduleMatrixPro2);
			}

			using (_mocks.Playback())
			{
				var result = _target.ScheduleTeam(_dateOnly, _groupPerson, _groupSchedulingService, _rollbackService, _schedulingOptions, _groupPersonBuilderForOptimization, _matrixes, _scheduleDictionary);
				Assert.IsTrue(result);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldNotAddMainShiftIfNoMatrix()
		{
			using (_mocks.Record())
			{
				Expect.Call(_scheduleDictionary[_person1]).Return(_scheduleRange1).Repeat.Twice();
				Expect.Call(_scheduleRange1.ScheduledDay(_dateOnly)).Return(_scheduleDay1).Repeat.Twice();
				Expect.Call(_scheduleDay1.PersonAssignment()).Return(_personAssignment1);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
				Expect.Call(_editableShiftMapper.CreateEditorShift(_personAssignment1)).Return(_mainShift1).Repeat.Twice();
				Expect.Call(_scheduleDictionary[_person2]).Return(_scheduleRange2);
				Expect.Call(_scheduleRange2.ScheduledDay(_dateOnly)).Return(_scheduleDay2);
				Expect.Call(_scheduleDay2.SignificantPartForDisplay()).Return(SchedulePartView.None);
				Expect.Call(_scheduleDay1.SignificantPartForDisplay()).Return(SchedulePartView.MainShift);

				Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
				Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(_scheduleDayPro);

				Expect.Call(_coherentChecker.CheckCoherent(_matrixes, _dateOnly, _scheduleDictionary, _scheduleDay1, _groupPerson.GroupMembers)).Return(_scheduleDay1);
				Expect.Call(_teamSteadyStateScheduleMatrixProFinder.MatrixPro(_matrixes, _scheduleDay1)).Return(_scheduleMatrixPro);
				Expect.Call(_teamSteadyStateScheduleMatrixProFinder.MatrixPro(_matrixes, _scheduleDay2)).Return(null);
			}

			using (_mocks.Playback())
			{
				var result = _target.ScheduleTeam(_dateOnly, _groupPerson, _groupSchedulingService, _rollbackService, _schedulingOptions, _groupPersonBuilderForOptimization, _matrixes, _scheduleDictionary);
				Assert.IsTrue(result);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldNotAddMainShiftIfPersonAssignmentIsNull()
		{
			using (_mocks.Record())
			{
				Expect.Call(_scheduleDictionary[_person1]).Return(_scheduleRange1);
				Expect.Call(_scheduleRange1.ScheduledDay(_dateOnly)).Return(_scheduleDay1);
				Expect.Call(_scheduleDay1.PersonAssignment()).Return(null);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
				
				Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
				Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(_scheduleDayPro);

				Expect.Call(_coherentChecker.CheckCoherent(_matrixes, _dateOnly, _scheduleDictionary, _scheduleDay1, _groupPerson.GroupMembers)).Return(_scheduleDay1);
				Expect.Call(_teamSteadyStateScheduleMatrixProFinder.MatrixPro(_matrixes, _scheduleDay1)).Return(_scheduleMatrixPro);
			}

			using (_mocks.Playback())
			{
				var result = _target.ScheduleTeam(_dateOnly, _groupPerson, _groupSchedulingService, _rollbackService, _schedulingOptions, _groupPersonBuilderForOptimization, _matrixes, _scheduleDictionary);
				Assert.IsFalse(result);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldNotAddMainShiftIfGroupMemberDayHaveAbsence()
		{
			using (_mocks.Record())
			{
				Expect.Call(_scheduleDictionary[_person1]).Return(_scheduleRange1).Repeat.Twice();
				Expect.Call(_scheduleRange1.ScheduledDay(_dateOnly)).Return(_scheduleDay1).Repeat.Twice();
				Expect.Call(_scheduleDay1.PersonAssignment()).Return(_personAssignment1);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
				Expect.Call(_editableShiftMapper.CreateEditorShift(_personAssignment1)).Return(_mainShift1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDictionary[_person2]).Return(_scheduleRange2);
				Expect.Call(_scheduleRange2.ScheduledDay(_dateOnly)).Return(_scheduleDay2);
				Expect.Call(_scheduleDay2.SignificantPartForDisplay()).Return(SchedulePartView.FullDayAbsence);
				Expect.Call(_scheduleDay1.SignificantPartForDisplay()).Return(SchedulePartView.MainShift);

				Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
				Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(_scheduleDayPro);

				Expect.Call(_coherentChecker.CheckCoherent(_matrixes, _dateOnly, _scheduleDictionary, _scheduleDay1, _groupPerson.GroupMembers)).Return(_scheduleDay1);
				Expect.Call(_teamSteadyStateScheduleMatrixProFinder.MatrixPro(_matrixes, _scheduleDay1)).Return(_scheduleMatrixPro);
			}

			using (_mocks.Playback())
			{
				var result = _target.ScheduleTeam(_dateOnly, _groupPerson, _groupSchedulingService, _rollbackService, _schedulingOptions, _groupPersonBuilderForOptimization, _matrixes, _scheduleDictionary);
				Assert.IsTrue(result);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldNotAddMainShiftIfGroupMemberDayHaveAbsenceOnTopOfDayOff()
		{
			using (_mocks.Record())
			{
				Expect.Call(_scheduleDictionary[_person1]).Return(_scheduleRange1).Repeat.Twice();
				Expect.Call(_scheduleRange1.ScheduledDay(_dateOnly)).Return(_scheduleDay1).Repeat.Twice();
				Expect.Call(_scheduleDay1.PersonAssignment()).Return(_personAssignment1);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
				Expect.Call(_editableShiftMapper.CreateEditorShift(_personAssignment1)).Return(_mainShift1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDictionary[_person2]).Return(_scheduleRange2);
				Expect.Call(_scheduleRange2.ScheduledDay(_dateOnly)).Return(_scheduleDay2);
				Expect.Call(_scheduleDay1.SignificantPartForDisplay()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay2.SignificantPartForDisplay()).Return(SchedulePartView.ContractDayOff);

				Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
				Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(_scheduleDayPro);

				Expect.Call(_coherentChecker.CheckCoherent(_matrixes, _dateOnly, _scheduleDictionary, _scheduleDay1, _groupPerson.GroupMembers)).Return(_scheduleDay1);
				Expect.Call(_teamSteadyStateScheduleMatrixProFinder.MatrixPro(_matrixes, _scheduleDay1)).Return(_scheduleMatrixPro);
			}

			using (_mocks.Playback())
			{
				var result = _target.ScheduleTeam(_dateOnly, _groupPerson, _groupSchedulingService, _rollbackService, _schedulingOptions, _groupPersonBuilderForOptimization, _matrixes, _scheduleDictionary);
				Assert.IsTrue(result);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldSkipAgentAsSourceIfAssignedDayOff()
		{
			using (_mocks.Record())
			{
				Expect.Call(_scheduleDictionary[_person1]).Return(_scheduleRange1);
				Expect.Call(_scheduleRange1.ScheduledDay(_dateOnly)).Return(_scheduleDay1);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.ContractDayOff);

				Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
				Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(_scheduleDayPro);

				Expect.Call(_scheduleMatrixPro2.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
				Expect.Call(_scheduleMatrixPro2.GetScheduleDayByKey(_dateOnly)).Return(_scheduleDayPro);

				Expect.Call(_scheduleDictionary[_person2]).Return(_scheduleRange2).Repeat.Twice();
				Expect.Call(_scheduleRange2.ScheduledDay(_dateOnly)).Return(_scheduleDay2).Repeat.Twice();
				Expect.Call(_scheduleDay2.PersonAssignment()).Return(_personAssignment1);
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay2.SignificantPartForDisplay()).Return(SchedulePartView.MainShift);

				Expect.Call(_editableShiftMapper.CreateEditorShift(_personAssignment1)).Return(_mainShift1);

				Expect.Call(_coherentChecker.CheckCoherent(_matrixes, _dateOnly, _scheduleDictionary, _scheduleDay1, _groupPerson.GroupMembers)).Return(_scheduleDay1);
				Expect.Call(_coherentChecker.CheckCoherent(_matrixes, _dateOnly, _scheduleDictionary, _scheduleDay2, _groupPerson.GroupMembers)).Return(_scheduleDay2);
				Expect.Call(_teamSteadyStateScheduleMatrixProFinder.MatrixPro(_matrixes, _scheduleDay1)).Return(_scheduleMatrixPro);
				Expect.Call(_teamSteadyStateScheduleMatrixProFinder.MatrixPro(_matrixes, _scheduleDay2)).Return(_scheduleMatrixPro2);
			}

			using (_mocks.Playback())
			{
				var result = _target.ScheduleTeam(_dateOnly, _groupPerson, _groupSchedulingService, _rollbackService, _schedulingOptions, _groupPersonBuilderForOptimization, _matrixes, _scheduleDictionary);
				Assert.IsTrue(result);
			}	
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionWhenGroupPersonBuilderIsNull()
		{
			_target.ScheduleTeam(_dateOnly, _groupPerson, _groupSchedulingService, _rollbackService, _schedulingOptions, null, _matrixes, _scheduleDictionary);		
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionWhenScheduleDictionaryIsNull()
		{
			_target.ScheduleTeam(_dateOnly, _groupPerson, _groupSchedulingService, _rollbackService, _schedulingOptions, _groupPersonBuilderForOptimization, _matrixes, null);
		}
	}
}
