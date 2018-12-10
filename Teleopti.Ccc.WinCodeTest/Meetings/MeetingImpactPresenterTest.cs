using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;


namespace Teleopti.Ccc.WinCodeTest.Meetings
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture]
	public class MeetingImpactPresenterTest
	{
		private MeetingImpactPresenter _target;
		private MockRepository _mocks;
		private ISchedulerStateHolder _schedulerStateHolder;
		private IMeetingImpactView _meetingImpactView;
		private TimeZoneInfo _timeZone;
		private IBestSlotForMeetingFinder _bestSlotFinder;
		private IMeetingViewModel _meetingViewModel;
		private ISkill _skill;
		private IMeetingImpactCalculator _impactCalculator;
		private IMeetingImpactSkillGridHandler _gridHandler;
		private IMeetingImpactTransparentWindowHandler _transparentMeetingHandler;
		private IMeetingStateHolderLoaderHelper _meetingStateHolderLoaderHelper;
		private IScenario _scenario;
		private IUnitOfWorkFactory _unitOfWorkFactory;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_schedulerStateHolder = _mocks.StrictMock<ISchedulerStateHolder>();
			_meetingImpactView = _mocks.StrictMock<IMeetingImpactView>();
			_meetingViewModel = _mocks.StrictMock<IMeetingViewModel>();
			_timeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			_bestSlotFinder = _mocks.StrictMock<IBestSlotForMeetingFinder>();
			_gridHandler = _mocks.StrictMock<IMeetingImpactSkillGridHandler>();
			_transparentMeetingHandler = _mocks.DynamicMock<IMeetingImpactTransparentWindowHandler>();
			_impactCalculator = _mocks.StrictMock<IMeetingImpactCalculator>();
			_meetingStateHolderLoaderHelper = _mocks.StrictMock<IMeetingStateHolderLoaderHelper>();
			_skill = _mocks.StrictMock<ISkill>();
			_scenario = _mocks.StrictMock<IScenario>();
			_unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
			_target = new MeetingImpactPresenter(_schedulerStateHolder, _meetingImpactView, _meetingViewModel, _meetingStateHolderLoaderHelper,
				_bestSlotFinder, _impactCalculator, _gridHandler, _transparentMeetingHandler, _unitOfWorkFactory);

		}

		[TearDown]
		public void Teardown()
		{
			_mocks.BackToRecordAll();
			_target.Dispose();
		}

		private void SetInitExpectations()
		{
			var startDate = new DateOnly(2010, 11, 1);
			Expect.Call(_meetingViewModel.StartDate).Return(startDate);
			Expect.Call(() => _meetingImpactView.SetSearchStartDate(startDate));
			Expect.Call(() => _meetingImpactView.SetSearchEndDate(startDate.AddDays(14)));
		}
		private void SetLoadExpectations()
		{
			Expect.Call(_meetingViewModel.TimeZone).Return(_timeZone).Repeat.AtLeastOnce();
			Expect.Call(() => _meetingImpactView.SetPreviousState(false));
			Expect.Call(() => _meetingImpactView.SetNextState(false));
			Expect.Call(() => _meetingImpactView.ShowWaiting()).Repeat.AtLeastOnce();
			Expect.Call(_schedulerStateHolder.RequestedScenario).Return(_scenario);
			Expect.Call(_meetingViewModel.RequiredParticipants).Return(
				new ReadOnlyCollection<ContactPersonViewModel>(new List<ContactPersonViewModel>()));
			Expect.Call(() => _meetingStateHolderLoaderHelper.ReloadResultIfNeeded(_scenario, new DateTimePeriod(), null)).
				IgnoreArguments();
			Expect.Call(() => _meetingImpactView.HideWaiting()).Repeat.AtLeastOnce();
		}

		private void SetFindButtonStatusExpectations()
		{
			Expect.Call(_meetingViewModel.IsRecurring).Return(false);
			Expect.Call(_meetingImpactView.FindButtonEnabled = true);
		}

		private void SetUpdateViewFromModelExpectations()
		{
			Expect.Call(_meetingViewModel.StartDate).Return(new DateOnly(2010, 11, 1));
			Expect.Call(() => _meetingImpactView.SetStartDate(new DateOnly(2010, 11, 1)));
			Expect.Call(_meetingViewModel.StartTime).Return(TimeSpan.FromHours(11));
			Expect.Call(() => _meetingImpactView.SetStartTime(TimeSpan.FromHours(11)));
			Expect.Call(_meetingViewModel.EndDate).Return(new DateOnly(2010, 11, 1));
			Expect.Call(() => _meetingImpactView.SetEndDate(new DateOnly(2010, 11, 1)));
			Expect.Call(_meetingViewModel.EndTime).Return(TimeSpan.FromHours(12));
			Expect.Call(() => _meetingImpactView.SetEndTime(TimeSpan.FromHours(12)));
		}

		private void SetUpdateModelExpectations()
		{
			Expect.Call(_meetingImpactView.StartDate).Return(new DateOnly(2010, 11, 1)).Repeat.Twice();
			Expect.Call(() => _meetingViewModel.StartDate = new DateOnly(2010, 11, 1));
			Expect.Call(() => _meetingViewModel.RecurringEndDate = new DateOnly(2010, 11, 1));

			Expect.Call(_meetingImpactView.StartTime).Return(TimeSpan.FromHours(11));
			Expect.Call(() => _meetingViewModel.StartTime = TimeSpan.FromHours(11));

			Expect.Call(_meetingImpactView.EndTime).Return(TimeSpan.FromHours(12));
			Expect.Call(() => _meetingViewModel.EndTime = TimeSpan.FromHours(12));
		}

		private void SetUpdateBeforeDrawMeetingExpectations()
		{
			var startDate = new DateOnly(2010, 11, 1);
			Expect.Call(_meetingViewModel.EndDate).Return(startDate);
			Expect.Call(_meetingViewModel.StartDate).Return(startDate);
			Expect.Call(_meetingViewModel.StartTime).Return(TimeSpan.FromHours(11));
			Expect.Call(_meetingViewModel.EndTime).Return(TimeSpan.FromHours(12));
		}

		//[Test]
		//public void ShouldLoadResultIfOutsidePeriod()
		//{
		//    _meetingImpactView = _mocks.DynamicMock<IMeetingImpactView>();
		//    _meetingViewModel = _mocks.DynamicMock<IMeetingViewModel>();
		//    //_stateHolderLoader = _mocks.DynamicMock<ISchedulerStateLoader>();

		//    _meetingImpactView = _mocks.DynamicMock<IMeetingImpactView>();
		//    //_meetingViewModel = _mocks.DynamicMock<IMeetingViewModel>();
		//    _gridHandler = _mocks.DynamicMock<IMeetingImpactSkillGridHandler>();
		//    _impactCalculator = _mocks.DynamicMock<IMeetingImpactCalculator>();
		//    _target = new MeetingImpactPresenter(_schedulerStateHolder, _meetingImpactView, _meetingViewModel, _meetingStateHolderLoaderHelper,
		//        _bestSlotFinder, _impactCalculator, _gridHandler, _transparentMeetingHandler);
		//    _mocks.BackToRecordAll();
		//    SetInitExpectations();
		//    Expect.Call(_meetingViewModel.StartDate).Return(new DateOnly(2010, 11, 1));
		//    SetLoadExpectations();
		//    SetUpdateViewFromModelExpectations();
		//    Expect.Call(_gridHandler.SetupSkillTabs);
		//    Expect.Call(_gridHandler.DrawSkillGrid);
		//    Expect.Call(_meetingImpactView.StartDate).Return(new DateOnly(2010, 11, 1));
		//    Expect.Call(() => _impactCalculator.RecalculateResources(new DateOnly(2010, 11, 1)));
		//    Expect.Call(() => _meetingImpactView.SetSearchInfo(""));
		//    SetUpdateBeforeDrawMeetingExpectations();

		//    Expect.Call(() =>_transparentMeetingHandler.DrawMeeting(TimeSpan.FromHours(11), TimeSpan.FromHours(12)));
		//    Expect.Call(() => _transparentMeetingHandler.ScrollMeetingIntoView());

		//    _meetingStateHolderLoaderHelper.FinishedReloading += _target.MeetingStateHolderLoaderHelperFinishedReloading;
		//    IEventRaiser raiser = LastCall.IgnoreArguments().GetEventRaiser();

		//    _mocks.ReplayAll();
		//    _target.UpdateView();
		//    raiser.Raise(_meetingStateHolderLoaderHelper, EventArgs.Empty);
		//    _mocks.VerifyAll();
		//}


		[Test]
		public void ShouldDisablePreviousAndNextOnNoResult()
		{
			var uow = _mocks.StrictMock<IUnitOfWork>();
			_meetingImpactView = _mocks.DynamicMock<IMeetingImpactView>();
			_meetingViewModel = _mocks.DynamicMock<IMeetingViewModel>();
			_meetingImpactView = _mocks.DynamicMock<IMeetingImpactView>();
			_meetingViewModel = _mocks.DynamicMock<IMeetingViewModel>();
			_gridHandler = _mocks.DynamicMock<IMeetingImpactSkillGridHandler>();
			_impactCalculator = _mocks.DynamicMock<IMeetingImpactCalculator>();
			_target = new MeetingImpactPresenter(_schedulerStateHolder, _meetingImpactView, _meetingViewModel, _meetingStateHolderLoaderHelper,
				_bestSlotFinder, _impactCalculator, _gridHandler, _transparentMeetingHandler, _unitOfWorkFactory);

			_mocks.BackToRecordAll();

			SetLoadExpectations();
			Expect.Call(_meetingImpactView.BestSlotSearchPeriodStart).Return(new DateTime(2010, 11, 1));
			Expect.Call(_meetingImpactView.BestSlotSearchPeriodEnd).Return(new DateTime(2010, 11, 15));
			Expect.Call(_meetingViewModel.MeetingDuration).Return(TimeSpan.FromHours(1));
			Expect.Call(_meetingViewModel.IsRecurring).Return(false);
			Expect.Call(() => _meetingImpactView.SetSearchInfo("")).IgnoreArguments();
			Expect.Call(() => _meetingViewModel.StartDate = new DateOnly(2010, 11, 1));
			Expect.Call(() => _meetingViewModel.RecurringEndDate = new DateOnly(2010, 11, 1));
			Expect.Call(() => _meetingViewModel.StartTime = TimeSpan.FromHours(12));
			Expect.Call(() => _meetingViewModel.MeetingDuration = TimeSpan.FromHours(1));
			Expect.Call(_meetingViewModel.RequiredParticipants).Return(
				new ReadOnlyCollection<ContactPersonViewModel>(new List<ContactPersonViewModel> { new ContactPersonViewModel(new Person()) }));
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
			Expect.Call(() => uow.Reassociate(new List<IPerson>())).IgnoreArguments();
			Expect.Call(_bestSlotFinder.FindBestSlot(new List<IPerson>(), new DateTimePeriod(), TimeSpan.FromHours(1), 15))
				.Return(new List<BestMeetingSlotResult>())
				.IgnoreArguments();
			Expect.Call(uow.Dispose);
			_meetingStateHolderLoaderHelper.FinishedReloading += _target.MeetingStateHolderLoaderHelperFinishedReloading;
			IEventRaiser raiser = LastCall.IgnoreArguments().GetEventRaiser();

			_mocks.ReplayAll();
			_target.FindBestMeetingSlot();
			raiser.Raise(_meetingStateHolderLoaderHelper, new ReloadEventArgs());
			_mocks.Verify(_meetingImpactView);
		}

		[Test]
		public void ShouldDisablePreviousAndNextOnResultWithOne()
		{
			var uow = _mocks.StrictMock<IUnitOfWork>();
			var foundSlotStartTime = new DateTime(2010, 11, 5, 11, 30, 0, DateTimeKind.Utc);
			var foundSlotPeriod = new DateTimePeriod(foundSlotStartTime, foundSlotStartTime.AddHours(1));
			var bestSlotResult1 = new BestMeetingSlotResult { SlotPeriod = foundSlotPeriod, SlotValue = 15, SlotLength = TimeSpan.FromHours(1) };

			_meetingImpactView = _mocks.DynamicMock<IMeetingImpactView>();
			_meetingViewModel = _mocks.DynamicMock<IMeetingViewModel>();
			_gridHandler = _mocks.DynamicMock<IMeetingImpactSkillGridHandler>();
			_impactCalculator = _mocks.DynamicMock<IMeetingImpactCalculator>();
			_target = new MeetingImpactPresenter(_schedulerStateHolder, _meetingImpactView, _meetingViewModel, _meetingStateHolderLoaderHelper,
				_bestSlotFinder, _impactCalculator, _gridHandler, _transparentMeetingHandler, _unitOfWorkFactory);

			_mocks.BackToRecordAll();
			SetLoadExpectations();
			SetUpdateViewFromModelExpectations();
			Expect.Call(_gridHandler.SetupSkillTabs);
			Expect.Call(_gridHandler.DrawSkillGrid);
			Expect.Call(_meetingImpactView.StartDate).Return(new DateOnly(2010, 11, 1));
			Expect.Call(() => _impactCalculator.RecalculateResources(new DateOnly(2010, 11, 1)));

			SetUpdateBeforeDrawMeetingExpectations();
			Expect.Call(_meetingImpactView.BestSlotSearchPeriodStart).Return(new DateTime(2010, 11, 1));
			Expect.Call(_meetingImpactView.BestSlotSearchPeriodEnd).Return(new DateTime(2010, 11, 15));
			Expect.Call(_meetingViewModel.MeetingDuration).Return(TimeSpan.FromHours(1));
			Expect.Call(_meetingViewModel.IsRecurring).Return(false);
			Expect.Call(() => _meetingImpactView.SetSearchInfo("")).IgnoreArguments();
			Expect.Call(() => _meetingViewModel.StartDate = new DateOnly(2010, 11, 1));
			Expect.Call(() => _meetingViewModel.RecurringEndDate = new DateOnly(2010, 11, 1));
			Expect.Call(() => _meetingViewModel.StartTime = TimeSpan.FromHours(12));
			Expect.Call(() => _meetingViewModel.MeetingDuration = TimeSpan.FromHours(1));
			Expect.Call(_meetingViewModel.RequiredParticipants).Return(
				new ReadOnlyCollection<ContactPersonViewModel>(new List<ContactPersonViewModel>()));
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
			Expect.Call(() => uow.Reassociate(new List<IPerson>())).IgnoreArguments();
			Expect.Call(_bestSlotFinder.FindBestSlot(new List<IPerson>(), new DateTimePeriod(), TimeSpan.FromHours(1), 15))
				.Return(new List<BestMeetingSlotResult> { bestSlotResult1 })
				.IgnoreArguments();
			Expect.Call(uow.Dispose);
			_meetingStateHolderLoaderHelper.FinishedReloading += _target.MeetingStateHolderLoaderHelperFinishedReloading;
			IEventRaiser raiser = LastCall.IgnoreArguments().GetEventRaiser();

			_mocks.ReplayAll();
			_target.FindBestMeetingSlot();
			raiser.Raise(_meetingStateHolderLoaderHelper, new ReloadEventArgs { HasReloaded = true });
			_mocks.Verify(_meetingImpactView);
		}

		[Test]
		public void ShouldDisablePreviousAndEnableNextOnResultWithMoreThanOne()
		{
			var uow = _mocks.StrictMock<IUnitOfWork>();
			var foundSlotStartTime = new DateTime(2010, 11, 5, 11, 30, 0, DateTimeKind.Utc);
			var foundSlotPeriod = new DateTimePeriod(foundSlotStartTime, foundSlotStartTime.AddHours(1));
			var bestSlotResult1 = new BestMeetingSlotResult { SlotPeriod = foundSlotPeriod, SlotValue = 15, SlotLength = TimeSpan.FromHours(1) };
			var bestSlotResult2 = new BestMeetingSlotResult { SlotPeriod = foundSlotPeriod, SlotValue = 15, SlotLength = TimeSpan.FromHours(1) };

			_meetingImpactView = _mocks.DynamicMock<IMeetingImpactView>();
			_meetingViewModel = _mocks.DynamicMock<IMeetingViewModel>();
			_gridHandler = _mocks.DynamicMock<IMeetingImpactSkillGridHandler>();
			_impactCalculator = _mocks.DynamicMock<IMeetingImpactCalculator>();
			_target = new MeetingImpactPresenter(_schedulerStateHolder, _meetingImpactView, _meetingViewModel, _meetingStateHolderLoaderHelper,
				_bestSlotFinder, _impactCalculator, _gridHandler, _transparentMeetingHandler, _unitOfWorkFactory);

			_mocks.BackToRecordAll();
			SetLoadExpectations();
			SetUpdateViewFromModelExpectations();
			Expect.Call(_gridHandler.SetupSkillTabs);
			Expect.Call(_gridHandler.DrawSkillGrid);
			Expect.Call(_meetingImpactView.StartDate).Return(new DateOnly(2010, 11, 1));
			Expect.Call(() => _impactCalculator.RecalculateResources(new DateOnly(2010, 11, 1)));
			SetUpdateBeforeDrawMeetingExpectations();
			Expect.Call(_meetingImpactView.BestSlotSearchPeriodStart).Return(new DateTime(2010, 11, 1));
			Expect.Call(_meetingImpactView.BestSlotSearchPeriodEnd).Return(new DateTime(2010, 11, 15));
			Expect.Call(_meetingViewModel.MeetingDuration).Return(TimeSpan.FromHours(1));
			Expect.Call(_meetingViewModel.IsRecurring).Return(false);
			Expect.Call(() => _meetingImpactView.SetSearchInfo("")).IgnoreArguments();
			Expect.Call(() => _meetingViewModel.StartDate = new DateOnly(2010, 11, 1));
			Expect.Call(() => _meetingViewModel.RecurringEndDate = new DateOnly(2010, 11, 1));
			Expect.Call(() => _meetingViewModel.StartTime = TimeSpan.FromHours(12));
			Expect.Call(() => _meetingViewModel.MeetingDuration = TimeSpan.FromHours(1));
			Expect.Call(_meetingViewModel.RequiredParticipants).Return(
				new ReadOnlyCollection<ContactPersonViewModel>(new List<ContactPersonViewModel>()));
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
			Expect.Call(() => uow.Reassociate(new List<IPerson>())).IgnoreArguments();
			Expect.Call(_bestSlotFinder.FindBestSlot(new List<IPerson>(), new DateTimePeriod(), TimeSpan.FromHours(1), 15))
				.Return(new List<BestMeetingSlotResult> { bestSlotResult1, bestSlotResult2 })
				.IgnoreArguments();
			Expect.Call(uow.Dispose);
			_meetingStateHolderLoaderHelper.FinishedReloading += _target.MeetingStateHolderLoaderHelperFinishedReloading;
			IEventRaiser raiser = LastCall.IgnoreArguments().GetEventRaiser();

			_mocks.ReplayAll();
			_target.FindBestMeetingSlot();
			raiser.Raise(_meetingStateHolderLoaderHelper, new ReloadEventArgs());
			_mocks.Verify(_meetingImpactView);
		}

		[Test]
		public void ShouldEnablePreviousAndEnableNextWhenNotInStartOrEndOfList()
		{
			var uow = _mocks.StrictMock<IUnitOfWork>();
			var foundSlotStartTime = new DateTime(2010, 11, 1, 11, 0, 0, DateTimeKind.Utc);
			var foundSlotPeriod = new DateTimePeriod(foundSlotStartTime, foundSlotStartTime.AddHours(1));
			var bestSlotResult1 = new BestMeetingSlotResult { SlotPeriod = foundSlotPeriod, SlotValue = 15, SlotLength = TimeSpan.FromHours(1) };
			var bestSlotResult2 = new BestMeetingSlotResult { SlotPeriod = foundSlotPeriod, SlotValue = 15, SlotLength = TimeSpan.FromHours(1) };
			var bestSlotResult3 = new BestMeetingSlotResult { SlotPeriod = foundSlotPeriod, SlotValue = 15, SlotLength = TimeSpan.FromHours(1) };

			_meetingImpactView = _mocks.DynamicMock<IMeetingImpactView>();
			_meetingViewModel = _mocks.DynamicMock<IMeetingViewModel>();
			_gridHandler = _mocks.DynamicMock<IMeetingImpactSkillGridHandler>();
			_impactCalculator = _mocks.DynamicMock<IMeetingImpactCalculator>();
			_target = new MeetingImpactPresenter(_schedulerStateHolder, _meetingImpactView, _meetingViewModel, _meetingStateHolderLoaderHelper,
				_bestSlotFinder, _impactCalculator, _gridHandler, _transparentMeetingHandler, _unitOfWorkFactory);

			_mocks.BackToRecordAll();
			SetLoadExpectations();
			SetUpdateViewFromModelExpectations();
			Expect.Call(_gridHandler.SetupSkillTabs);
			Expect.Call(_gridHandler.DrawSkillGrid);
			Expect.Call(_meetingImpactView.StartDate).Return(new DateOnly(2010, 11, 1));
			Expect.Call(() => _impactCalculator.RecalculateResources(new DateOnly(2010, 11, 1)));

			SetUpdateBeforeDrawMeetingExpectations();
			Expect.Call(_meetingImpactView.BestSlotSearchPeriodStart).Return(new DateTime(2010, 11, 1));
			Expect.Call(_meetingImpactView.BestSlotSearchPeriodEnd).Return(new DateTime(2010, 11, 15));

			Expect.Call(_meetingViewModel.MeetingDuration).Return(TimeSpan.FromHours(1));

			Expect.Call(_meetingViewModel.IsRecurring).Return(false);
			Expect.Call(() => _meetingImpactView.SetSearchInfo("")).IgnoreArguments();
			Expect.Call(() => _meetingViewModel.StartDate = new DateOnly(2010, 11, 1));
			Expect.Call(() => _meetingViewModel.RecurringEndDate = new DateOnly(2010, 11, 1));
			Expect.Call(() => _meetingViewModel.StartTime = TimeSpan.FromHours(12));
			Expect.Call(() => _meetingViewModel.MeetingDuration = TimeSpan.FromHours(1));

			Expect.Call(_meetingViewModel.RequiredParticipants).Return(
				new ReadOnlyCollection<ContactPersonViewModel>(new List<ContactPersonViewModel>()));
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
			Expect.Call(() => uow.Reassociate(new List<IPerson>())).IgnoreArguments();
			Expect.Call(_bestSlotFinder.FindBestSlot(new List<IPerson>(), new DateTimePeriod(), TimeSpan.FromHours(1), 15))
				.Return(new List<BestMeetingSlotResult> { bestSlotResult1, bestSlotResult2, bestSlotResult3 })
				.IgnoreArguments();
			Expect.Call(uow.Dispose);
			_meetingStateHolderLoaderHelper.FinishedReloading += _target.MeetingStateHolderLoaderHelperFinishedReloading;
			IEventRaiser raiser = LastCall.IgnoreArguments().GetEventRaiser();

			_mocks.ReplayAll();
			_target.FindBestMeetingSlot();
			raiser.Raise(_meetingStateHolderLoaderHelper, new ReloadEventArgs());
			//first
			_mocks.Verify(_meetingImpactView);

			//next
			_mocks.BackToRecord(_meetingImpactView);
			_meetingImpactView.SetPreviousState(true);
			_meetingImpactView.SetNextState(true);
			_mocks.ReplayAll();
			_target.GoToNextSlot();
			_mocks.Verify(_meetingImpactView);

			////next
			_mocks.BackToRecord(_meetingImpactView);
			_meetingImpactView.SetPreviousState(true);
			_meetingImpactView.SetNextState(false);
			_mocks.ReplayAll();
			_target.GoToNextSlot();
			_mocks.Verify(_meetingImpactView);

			////previous
			_mocks.BackToRecord(_meetingImpactView);
			_meetingImpactView.SetPreviousState(true);
			_meetingImpactView.SetNextState(true);
			_mocks.ReplayAll();
			_target.GoToPreviousSlot();

			_mocks.Verify(_meetingImpactView);
		}

		[Test]
		public void ShouldUpdateImpactWhenDateIsChanged()
		{
			_mocks.BackToRecordAll();

			SetUpdateModelExpectations();
			SetFindButtonStatusExpectations();
			SetInitExpectations();
			Expect.Call(_meetingViewModel.StartDate).Return(new DateOnly(2010, 11, 1));
			SetLoadExpectations();
			SetUpdateViewFromModelExpectations();
			Expect.Call(_gridHandler.SetupSkillTabs);
			Expect.Call(_gridHandler.DrawSkillGrid);
			Expect.Call(_meetingImpactView.StartDate).Return(new DateOnly(2010, 11, 1));
			Expect.Call(() => _impactCalculator.RecalculateResources(new DateOnly(2010, 11, 1)));
			//Expect.Call(() => _meetingImpactView.SetSearchInfo(""));
			SetUpdateBeforeDrawMeetingExpectations();

			Expect.Call(() => _transparentMeetingHandler.DrawMeeting(TimeSpan.FromHours(11), TimeSpan.FromHours(12)));
			Expect.Call(() => _transparentMeetingHandler.ScrollMeetingIntoView());

			_meetingStateHolderLoaderHelper.FinishedReloading += null;
			IEventRaiser raiser = LastCall.IgnoreArguments().GetEventRaiser();

			_mocks.ReplayAll();
			_target = new MeetingImpactPresenter(_schedulerStateHolder, _meetingImpactView, _meetingViewModel, _meetingStateHolderLoaderHelper,
				_bestSlotFinder, _impactCalculator, _gridHandler, _transparentMeetingHandler, _unitOfWorkFactory);
			_target.MeetingDateChanged();
			raiser.Raise(_meetingStateHolderLoaderHelper, new ReloadEventArgs());
			_mocks.VerifyAll();
		}


		[Test]
		public void ShouldUpdateWhenSkillTabChange()
		{
			_mocks.BackToRecordAll();

			Expect.Call(_meetingImpactView.SelectedSkill()).Return(_skill);
			Expect.Call(_gridHandler.DrawSkillGrid);
			Expect.Call(_meetingImpactView.StartDate).Return(new DateOnly(2010, 11, 1));
			Expect.Call(() => _impactCalculator.RecalculateResources(new DateOnly(2010, 11, 1)));
			SetUpdateBeforeDrawMeetingExpectations();
			Expect.Call(() => _transparentMeetingHandler.DrawMeeting(TimeSpan.FromHours(11), TimeSpan.FromHours(12)));
			Expect.Call(() => _transparentMeetingHandler.ScrollMeetingIntoView());
			_mocks.ReplayAll();
			_target.SkillTabChange();
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldRefreshMeetingOnLeftColChanged()
		{
			_mocks.BackToRecordAll();
			Expect.Call(_meetingImpactView.SelectedSkill()).Return(_skill);
			SetUpdateBeforeDrawMeetingExpectations();
			Expect.Call(() => _transparentMeetingHandler.DrawMeeting(TimeSpan.FromHours(11), TimeSpan.FromHours(12)));

			_mocks.ReplayAll();
			_target.OnLeftColChanged();
			_mocks.VerifyAll();
		}


		[Test]
		public void ShouldThrowExceptionWhenMeetingViewModelIsNull()
		{
			Assert.Throws<ArgumentNullException>(() =>
				_target = new MeetingImpactPresenter(_schedulerStateHolder, _meetingImpactView, null, _meetingStateHolderLoaderHelper, _bestSlotFinder,
					_impactCalculator, _gridHandler, _transparentMeetingHandler, _unitOfWorkFactory));
		}

		[Test]
		public void ShouldThrowExceptionWhenTransparentHandlerNull()
		{
			Assert.Throws<ArgumentNullException>(() =>
				_target = new MeetingImpactPresenter(_schedulerStateHolder, _meetingImpactView, _meetingViewModel, _meetingStateHolderLoaderHelper, _bestSlotFinder,
					_impactCalculator, _gridHandler, null, _unitOfWorkFactory));
		}

		[Test]
		public void ShouldUpdateModelAndMeetingWhenCorrectTime()
		{
			_mocks.BackToRecordAll();

			SetUpdateModelExpectations();

			SetUpdateBeforeDrawMeetingExpectations();
			Expect.Call(() => _transparentMeetingHandler.DrawMeeting(TimeSpan.FromHours(11), TimeSpan.FromHours(12)));
			Expect.Call(() => _transparentMeetingHandler.ScrollMeetingIntoView());
			_mocks.ReplayAll();

			_target.OnMeetingTimeChange("11:00");
			_mocks.VerifyAll();


		}

		[Test]
		public void ShouldUpdateViewWhenInputTimeIsNotOk()
		{
			_mocks.BackToRecordAll();
			Expect.Call(_meetingViewModel.StartTime).Return(TimeSpan.FromHours(11));
			Expect.Call(_meetingViewModel.EndTime).Return(TimeSpan.FromHours(12));
			Expect.Call(() => _meetingImpactView.SetStartTime(TimeSpan.FromHours(11)));
			Expect.Call(() => _meetingImpactView.SetEndTime(TimeSpan.FromHours(12)));
			_mocks.ReplayAll();
			_target.OnMeetingTimeChange("ska inte fungera");
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldUpdateStartTimeslotOnCorrectTime()
		{
			_mocks.BackToRecordAll();
			Expect.Call(_meetingViewModel.SlotStartTime = TimeSpan.FromHours(11));
			_mocks.ReplayAll();
			_target.OnSlotTimeChange("11:00");
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldUpdateViewWhenInputIsNotOk()
		{
			_mocks.BackToRecordAll();
			Expect.Call(_meetingViewModel.SlotEndTime).Return(TimeSpan.FromHours(12));
			Expect.Call(() => _meetingImpactView.SetSlotEndTime(TimeSpan.FromHours(12)));

			Expect.Call(_meetingViewModel.SlotStartTime).Return(TimeSpan.FromHours(11));
			Expect.Call(() => _meetingImpactView.SetSlotStartTime(TimeSpan.FromHours(11)));
			_mocks.ReplayAll();

			_target.OnSlotTimeChange("ska inte fungera");
			_mocks.VerifyAll();
		}


		[Test]
		public void ShouldChangeEndDateWhenStartDateIsGreaterThanEndDate()
		{
			_mocks.BackToRecordAll();
			Expect.Call(() => _meetingImpactView.SetSlotEndDate(new DateTime(2010, 11, 2, 8, 0, 0, DateTimeKind.Utc)));
			_mocks.ReplayAll();

			_target.OnSlotDateChange(new DateTime(2010, 11, 2, 8, 0, 0, DateTimeKind.Utc), new DateTime(2010, 11, 1, 8, 0, 0, DateTimeKind.Utc));
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldJumpOutIfSelectedSkillIsNull()
		{
			_mocks.BackToRecordAll();
			Expect.Call(_meetingImpactView.SelectedSkill()).Return(null).Repeat.Twice();
			_mocks.ReplayAll();
			_target.OnLeftColChanged();
			_target.SkillTabChange();
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldJumpOutIfCurrentResultNotExists()
		{
			_target.GoToNextSlot();
			_target.GoToPreviousSlot();
		}

		[Test]
		public void ShouldHandleTransparentWindowMove()
		{
			_mocks.BackToRecordAll();
			_transparentMeetingHandler.TransparentWindowMoved += null;
			IEventRaiser raiser = LastCall.IgnoreArguments().GetEventRaiser();
			SetUpdateViewFromModelExpectations();
			Expect.Call(_meetingImpactView.StartDate).Return(new DateOnly(2010, 11, 1));
			Expect.Call(() => _impactCalculator.RecalculateResources(new DateOnly(2010, 11, 1)));
			Expect.Call(() => _meetingImpactView.RefreshMeetingControl());

			_meetingStateHolderLoaderHelper.FinishedReloading += _target.MeetingStateHolderLoaderHelperFinishedReloading;
			LastCall.IgnoreArguments().GetEventRaiser();

			_mocks.ReplayAll();
			_target = new MeetingImpactPresenter(_schedulerStateHolder, _meetingImpactView, _meetingViewModel, _meetingStateHolderLoaderHelper,
				_bestSlotFinder, _impactCalculator, _gridHandler, _transparentMeetingHandler, _unitOfWorkFactory);
			raiser.Raise(_transparentMeetingHandler, EventArgs.Empty);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldAddOneDayToStartDateAsEndDateIfEndDateIsTooSmall()
		{
			var uow = _mocks.StrictMock<IUnitOfWork>();
			_meetingImpactView = _mocks.DynamicMock<IMeetingImpactView>();
			_meetingViewModel = _mocks.DynamicMock<IMeetingViewModel>();
			_meetingImpactView = _mocks.DynamicMock<IMeetingImpactView>();
			_meetingViewModel = _mocks.DynamicMock<IMeetingViewModel>();
			_gridHandler = _mocks.DynamicMock<IMeetingImpactSkillGridHandler>();
			_impactCalculator = _mocks.DynamicMock<IMeetingImpactCalculator>();
			_target = new MeetingImpactPresenter(_schedulerStateHolder, _meetingImpactView, _meetingViewModel, _meetingStateHolderLoaderHelper,
				_bestSlotFinder, _impactCalculator, _gridHandler, _transparentMeetingHandler, _unitOfWorkFactory);

			_mocks.BackToRecordAll();

			SetLoadExpectations();
			Expect.Call(_meetingImpactView.BestSlotSearchPeriodStart).Return(new DateTime(2011, 3, 15));
			Expect.Call(_meetingImpactView.BestSlotSearchPeriodEnd).Return(new DateTime(2011, 3, 15));
			Expect.Call(_meetingViewModel.MeetingDuration).Return(TimeSpan.FromHours(1));
			Expect.Call(_meetingViewModel.IsRecurring).Return(false);
			Expect.Call(() => _meetingImpactView.SetSearchInfo("")).IgnoreArguments();
			Expect.Call(() => _meetingViewModel.StartDate = new DateOnly(2010, 11, 1));
			Expect.Call(() => _meetingViewModel.RecurringEndDate = new DateOnly(2010, 11, 1));
			Expect.Call(() => _meetingViewModel.StartTime = TimeSpan.FromHours(12));
			Expect.Call(() => _meetingViewModel.MeetingDuration = TimeSpan.FromHours(1));
			Expect.Call(_meetingViewModel.RequiredParticipants).Return(
				new ReadOnlyCollection<ContactPersonViewModel>(new List<ContactPersonViewModel>()));
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
			Expect.Call(() => uow.Reassociate(new List<IPerson>())).IgnoreArguments();
			Expect.Call(_bestSlotFinder.FindBestSlot(new List<IPerson>(), new DateTimePeriod(), TimeSpan.FromHours(1), 15))
				.Return(new List<BestMeetingSlotResult>())
				.IgnoreArguments();
			Expect.Call(uow.Dispose);
			_meetingStateHolderLoaderHelper.FinishedReloading += _target.MeetingStateHolderLoaderHelperFinishedReloading;
			IEventRaiser raiser = LastCall.IgnoreArguments().GetEventRaiser();

			_mocks.ReplayAll();
			_target.FindBestMeetingSlot();
			raiser.Raise(_meetingStateHolderLoaderHelper, new ReloadEventArgs());
			_mocks.Verify(_meetingImpactView);
		}

		[Test]
		public void ShouldCancelStateLoaderHelperOnCancel()
		{
			_mocks.BackToRecordAll();
			Expect.Call(_meetingStateHolderLoaderHelper.CancelEveryReload);
			_mocks.ReplayAll();
			_target.CancelAllLoads();
			_mocks.VerifyAll();
		}
	}
}
