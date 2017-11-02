using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Configuration
{
	[TestFixture]
	public class WorkflowControlSetPresenterOvertimeTest
	{
		private WorkflowControlSetPresenter _target;
		private MockRepository _mocks;
		private IWorkflowControlSetView _view;
		private IRepositoryFactory _repositoryFactory;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private IList<IAbsence> _absenceList;
		private IList<IActivity> _activityList;
		private IList<IShiftCategory> _categories;
		private IList<IDayOffTemplate> _dayOffTemplates;
		private WorkflowControlSetModel _workflowControlSetModel;
		private IWorkflowControlSet _workflowControlSet;
		private CultureInfo _culture;
		private List<ISkill> _skillList;
		private FakeToggleManager _fakeToggleManager;

		[DatapointSource]
		public Toggles[] ToggleList =
		{
			Toggles.Wfm_Staffing_StaffingReadModel49DaysStep2_45109,
			Toggles.Wfm_Staffing_StaffingReadModel28DaysStep1_45109,
			Toggles.OvertimeRequestPeriodSetting_46417
		};

		[SetUp]
		public void Setup()
		{
			_culture = TeleoptiPrincipal.CurrentPrincipal.Regional.Culture;
			_mocks = new MockRepository();
			_mocks.StrictMock<IWorkflowControlSetRepository>();
			_repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
			_unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
			_mocks.StrictMock<IUnitOfWork>();
			_view = _mocks.StrictMock<IWorkflowControlSetView>();
			_absenceList = new List<IAbsence>();
			_activityList = new List<IActivity>();
			_categories = new List<IShiftCategory>();
			_dayOffTemplates = new List<IDayOffTemplate>();
			_skillList = new List<ISkill>();
			_workflowControlSet = new WorkflowControlSet("My Workflow Control Set");
			_workflowControlSet.SetId(Guid.NewGuid());
			_workflowControlSetModel = new WorkflowControlSetModel(_workflowControlSet);
			_fakeToggleManager = new FakeToggleManager();
			_target = new WorkflowControlSetPresenter(_view, _unitOfWorkFactory, _repositoryFactory, _fakeToggleManager);
		}

		[Theory]
		public void VerifyAddNewDateOpenPeriod(Toggles toggles)
		{
			_fakeToggleManager.Enable(toggles);
			IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };

			using (_mocks.Record())
			{
				ExpectInitialize(repositoryCollection);
				ExpectSetSelectedWorkflowControlSetModel();
				Expect.Call(() => _view.SetOvertimeOpenPeriodsGridRowCount(_workflowControlSetModel.OvertimeRequestPeriodModels.Count))
					.IgnoreArguments();
				_view.RefreshOvertimeOpenPeriodsGrid();
			}

			using (_mocks.Playback())
			{
				_target.Initialize();
				_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());

				Assert.AreEqual(0, _target.SelectedModel.OvertimeRequestPeriodModels.Count);
				_target.AddOvertimeRequestOpenDatePeriod();

				var staffingInfoAvailableDays = StaffingInfoAvailableDaysProvider.GetDays(_fakeToggleManager);
				Assert.AreEqual(1, _target.SelectedModel.OvertimeRequestPeriodModels.Count);
				Assert.AreEqual(DateOnly.Today, _target.SelectedModel.OvertimeRequestPeriodModels[0].PeriodStartDate);
				Assert.AreEqual(DateOnly.Today.AddDays(staffingInfoAvailableDays), _target.SelectedModel.OvertimeRequestPeriodModels[0].PeriodEndDate);
			}
		}

		[Theory]
		public void VerifyAddNewRollingOpenPeriod(Toggles toggles)
		{
			_fakeToggleManager.Enable(toggles);
			IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };

			using (_mocks.Record())
			{
				ExpectInitialize(repositoryCollection);
				ExpectSetSelectedWorkflowControlSetModel();
				Expect.Call(() => _view.SetOvertimeOpenPeriodsGridRowCount(_workflowControlSetModel.OvertimeRequestPeriodModels.Count))
					.IgnoreArguments();
				_view.RefreshOvertimeOpenPeriodsGrid();
			}

			using (_mocks.Playback())
			{
				_target.Initialize();
				_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());

				Assert.AreEqual(0, _target.SelectedModel.OvertimeRequestPeriodModels.Count);
				_target.AddOvertimeRequestOpenRollingPeriod();

				var staffingInfoAvailableDays = StaffingInfoAvailableDaysProvider.GetDays(_fakeToggleManager);
				Assert.AreEqual(1, _target.SelectedModel.OvertimeRequestPeriodModels.Count);
				Assert.AreEqual(0, _target.SelectedModel.OvertimeRequestPeriodModels[0].RollingStart);
				Assert.AreEqual(staffingInfoAvailableDays, _target.SelectedModel.OvertimeRequestPeriodModels[0].RollingEnd);
			}
		}

		[Test]
		public void VerifyDeleteOvertimeRequestPeriodCanDelete()
		{
			_workflowControlSet.AddOpenOvertimeRequestPeriod(WorkflowControlSetModel.DefaultOvertimeRequestPeriodAdapters[0].Item);
			IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };

			using (_mocks.Record())
			{
				ExpectInitialize(repositoryCollection);
				ExpectSetSelectedWorkflowControlSetModel();
				Expect.Call(_view.ConfirmDeleteOfRequestPeriod()).Return(true);
				_view.SetOvertimeOpenPeriodsGridRowCount(0);
				_view.RefreshOvertimeOpenPeriodsGrid();
			}
			using (_mocks.Playback())
			{
				_target.Initialize();
				_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());

				Assert.AreEqual(1, _target.SelectedModel.OvertimeRequestPeriodModels.Count);
				_target.DeleteOvertimeRequestPeriod(
					new ReadOnlyCollection<OvertimeRequestPeriodModel>(_target.SelectedModel.OvertimeRequestPeriodModels));
				Assert.AreEqual(0, _target.SelectedModel.OvertimeRequestPeriodModels.Count);
			}
		}

		[Test]
		public void VerifyCanCancelDeleteOvertimeRequestPeriod()
		{
			_workflowControlSet.AddOpenOvertimeRequestPeriod(WorkflowControlSetModel.DefaultOvertimeRequestPeriodAdapters[0].Item);
			IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };

			using (_mocks.Record())
			{
				ExpectInitialize(repositoryCollection);
				ExpectSetSelectedWorkflowControlSetModel();
				Expect.Call(_view.ConfirmDeleteOfRequestPeriod()).Return(false);
			}
			using (_mocks.Playback())
			{
				_target.Initialize();
				_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());

				Assert.AreEqual(1, _target.SelectedModel.OvertimeRequestPeriodModels.Count);
				_target.DeleteOvertimeRequestPeriod(
					new ReadOnlyCollection<OvertimeRequestPeriodModel>(_target.SelectedModel.OvertimeRequestPeriodModels));
				Assert.AreEqual(1, _target.SelectedModel.OvertimeRequestPeriodModels.Count);
			}
		}

		[Test]
		public void VerifyCanGetDefaultPeriod()
		{
			DateTime startDate = DateTime.Today;
			startDate = DateHelper.GetFirstDateInMonth(startDate, CultureInfo.CurrentCulture);
			DateTime endDate = CultureInfo.CurrentCulture.Calendar.AddMonths(startDate, 3).AddDays(-1);

			using (_mocks.Record())
			{
				Expect.Call(() => _view.RefreshOpenPeriodsGrid()).Repeat.Twice();
				Expect.Call(() => _view.RefreshOvertimeOpenPeriodsGrid()).Repeat.Twice();
			}
			using (_mocks.Playback())
			{
				Assert.AreEqual(new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate)), _target.ProjectionPeriod);

				_target.NextProjectionPeriod();

				startDate = CultureInfo.CurrentCulture.Calendar.AddMonths(startDate, 1);
				endDate = CultureInfo.CurrentCulture.Calendar.AddMonths(endDate, 1);
				Assert.AreEqual(new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate)),
					_target.ProjectionPeriod);

				_target.PreviousProjectionPeriod();

				startDate = CultureInfo.CurrentCulture.Calendar.AddMonths(startDate, -1);
				endDate = CultureInfo.CurrentCulture.Calendar.AddMonths(endDate, -1);
				Assert.AreEqual(new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate)),
					_target.ProjectionPeriod);
			}
		}

		[Test]
		public void VerifyDeleteOvertimeRequestPeriodNothingToDelete()
		{
			IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };

			using (_mocks.Record())
			{
				ExpectInitialize(repositoryCollection);
				ExpectSetSelectedWorkflowControlSetModel();
			}
			using (_mocks.Playback())
			{
				_target.Initialize();
				_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());

				Assert.AreEqual(0, _target.SelectedModel.OvertimeRequestPeriodModels.Count);
				_target.DeleteOvertimeRequestPeriod(_target.SelectedModel.OvertimeRequestPeriodModels);
				Assert.AreEqual(0, _target.SelectedModel.OvertimeRequestPeriodModels.Count);
			}
		}

		[Test]
		public void VerifyMove()
		{
			var openDatePeriod =
				(OvertimeRequestOpenDatePeriod)WorkflowControlSetModel.DefaultOvertimeRequestPeriodAdapters[0].Item;
			((IEntity)openDatePeriod).SetId(Guid.NewGuid());
			var openRollingPeriod =
				(OvertimeRequestOpenRollingPeriod)WorkflowControlSetModel.DefaultOvertimeRequestPeriodAdapters[1].Item;
			((IEntity)openRollingPeriod).SetId(Guid.NewGuid());
			_workflowControlSet.AddOpenOvertimeRequestPeriod(openDatePeriod);
			_workflowControlSet.AddOpenOvertimeRequestPeriod(openRollingPeriod);
			IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };

			using (_mocks.Record())
			{
				ExpectInitialize(repositoryCollection);
				ExpectSetSelectedWorkflowControlSetModel();
				Expect.Call(() => _view.SetOvertimeOpenPeriodsGridRowCount(2)).IgnoreArguments().Repeat.Twice();
				Expect.Call(() => _view.RefreshOvertimeOpenPeriodsGrid()).Repeat.Twice();
			}
			using (_mocks.Playback())
			{
				_target.Initialize();
				_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());

				Assert.AreEqual(openDatePeriod, _target.SelectedModel.OvertimeRequestPeriodModels[0].DomainEntity);
				Assert.AreEqual(openRollingPeriod, _target.SelectedModel.OvertimeRequestPeriodModels[1].DomainEntity);
				_target.MoveUp(_target.SelectedModel.OvertimeRequestPeriodModels[1]);
				Assert.AreEqual(openDatePeriod, _target.SelectedModel.OvertimeRequestPeriodModels[1].DomainEntity);
				Assert.AreEqual(openRollingPeriod, _target.SelectedModel.OvertimeRequestPeriodModels[0].DomainEntity);
				_target.MoveDown(_target.SelectedModel.OvertimeRequestPeriodModels[0]);
				Assert.AreEqual(openDatePeriod, _target.SelectedModel.OvertimeRequestPeriodModels[0].DomainEntity);
				Assert.AreEqual(openRollingPeriod, _target.SelectedModel.OvertimeRequestPeriodModels[1].DomainEntity);
			}
		}

		public void ExpectInitialize(IList<IWorkflowControlSet> repositoryCollection)
		{
			ExpectInitialize(_mocks, _view, _unitOfWorkFactory, _repositoryFactory, repositoryCollection, _categories,
				_dayOffTemplates, _absenceList, _activityList, _skillList, _culture);
		}

		public static void ExpectInitialize(MockRepository mocks, IWorkflowControlSetView view,
			IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory,
			IList<IWorkflowControlSet> workflowControlSets, IList<IShiftCategory> shiftCategories,
			IList<IDayOffTemplate> dayOffTemplates, IList<IAbsence> absences, IList<IActivity> activities, IList<ISkill> skills,
			CultureInfo culture)
		{
			var unitOfWork = mocks.StrictMock<IUnitOfWork>();
			var repository = mocks.StrictMock<IWorkflowControlSetRepository>();
			var absenceRepository = mocks.StrictMock<IAbsenceRepository>();
			var activityRepository = mocks.StrictMock<IActivityRepository>();
			var categoryRepository = mocks.StrictMock<IShiftCategoryRepository>();
			var dayOffRepository = mocks.StrictMock<IDayOffTemplateRepository>();
			var skillRepository = mocks.StrictMock<ISkillRepository>();

			Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			Expect.Call(() => unitOfWork.DisableFilter(QueryFilter.Deleted));
			Expect.Call(repositoryFactory.CreateWorkflowControlSetRepository(unitOfWork)).Return(repository);
			Expect.Call(repository.LoadAllSortByName()).Return(workflowControlSets).Repeat.Once();
			Expect.Call(repositoryFactory.CreateAbsenceRepository(unitOfWork)).Return(absenceRepository);
			Expect.Call(repositoryFactory.CreateActivityRepository(unitOfWork)).Return(activityRepository);
			Expect.Call(repositoryFactory.CreateSkillRepository(unitOfWork)).Return(skillRepository);
			Expect.Call(repositoryFactory.CreateShiftCategoryRepository(unitOfWork)).Return(categoryRepository);
			Expect.Call(repositoryFactory.CreateDayOffRepository(unitOfWork)).Return(dayOffRepository);
			Expect.Call(categoryRepository.LoadAll()).Return(shiftCategories);
			Expect.Call(dayOffRepository.LoadAll()).Return(dayOffTemplates);
			Expect.Call(absenceRepository.LoadRequestableAbsence()).Return(absences);
			Expect.Call(absenceRepository.LoadAll()).Return(absences);
			Expect.Call(activityRepository.LoadAllSortByName()).Return(activities);
			Expect.Call(skillRepository.FindAllWithoutMultisiteSkills()).Return(skills);
			Expect.Call(unitOfWork.Dispose);
			Expect.Call(() => view.SetCalendarCultureInfo(culture));

			if (absences.Count > 0)
			{
				Expect.Call(() => view.EnableHandlingOfAbsenceRequestPeriods(true));
			}
			else
			{
				Expect.Call(() => view.EnableHandlingOfAbsenceRequestPeriods(false));
			}

			Expect.Call(() => view.FillAllowedPreferenceActivityCombo(null, "Name")).IgnoreArguments();
			LastCall.IgnoreArguments();
			if (!workflowControlSets.IsEmpty())
			{
				Expect.Call(() => view.FillWorkloadControlSetCombo(null, "Name")).IgnoreArguments();
				LastCall.IgnoreArguments();
			}
			Expect.Call(view.InitializeView);
		}

		public void ExpectSetSelectedWorkflowControlSetModel()
		{
			ExpectSetSelectedWorkflowControlSetModel(_view);
		}

		public static void ExpectSetSelectedWorkflowControlSetModel(IWorkflowControlSetView view)
		{
			Expect.Call(() => view.SetName("name")).IgnoreArguments();
			Expect.Call(() => view.SetUpdatedInfo("")).IgnoreArguments();
			Expect.Call(() => view.SetOpenPeriodsGridRowCount(1)).IgnoreArguments();
			Expect.Call(() => view.SetOvertimeOpenPeriodsGridRowCount(1)).IgnoreArguments();
			Expect.Call(() => view.SetWriteProtectedDays(0)).IgnoreArguments();
			Expect.Call(view.LoadDateOnlyVisualizer).Repeat.Any();
			Expect.Call(() => view.SetMatchingSkills(null)).IgnoreArguments();
			Expect.Call(() => view.SetAllowedDayOffs(null)).IgnoreArguments();
			Expect.Call(() => view.SetAllowedShiftCategories(null)).IgnoreArguments();
			Expect.Call(() => view.SetAllowedAbsences(null)).IgnoreArguments();
			Expect.Call(() => view.SetAllowedAbsencesForReport(null)).IgnoreArguments();
			Expect.Call(() => view.SetShiftTradeTargetTimeFlexibility(new TimeSpan()));
			Expect.Call(() => view.SetAutoGrant(false)).IgnoreArguments();
			Expect.Call(() => view.SetAnonymousTrading(false)).IgnoreArguments();
			Expect.Call(() => view.SetLockTrading(false)).IgnoreArguments();
			Expect.Call(() => view.SetFairnessType(FairnessType.EqualNumberOfShiftCategory)).IgnoreArguments();
			Expect.Call(() => view.SetAbsenceRequestWaitlisting(false, WaitlistProcessOrder.FirstComeFirstServed)).IgnoreArguments();
			Expect.Call(() => view.SetAbsenceRequestCancellation(null)).IgnoreArguments();
			Expect.Call(() => view.SetAbsenceRequestExpiration(null)).IgnoreArguments();
			Expect.Call(view.EnableAllAuthorized);
			Expect.Call(() => view.SetAbsenceProbability(false)).IgnoreArguments();
			Expect.Call(() => view.SetOvertimeProbability(false)).IgnoreArguments();
			Expect.Call(() => view.SetAutoGrantOvertimeRequest(false)).IgnoreArguments();
		}
	}
}