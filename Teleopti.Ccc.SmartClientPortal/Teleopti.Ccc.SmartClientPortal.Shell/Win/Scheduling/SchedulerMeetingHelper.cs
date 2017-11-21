﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Meetings;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
    internal class SchedulerMeetingHelper
    {
        private readonly IInitiatorIdentifier _initiatorIdentifier;
        private readonly ISchedulerStateHolder _schedulerStateHolder;
	    private readonly IResourceCalculation _resourceOptimizationHelper;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly IStaffingCalculatorServiceFacade _staffingCalculatorServiceFacade;
		private readonly ISkillPriorityProvider _skillPriorityProvider;
	    private readonly IScheduleStorageFactory _scheduleStorageFactory;
		private readonly IRepositoryFactory _repositoryFactory = new RepositoryFactory();
	    private readonly MeetingParticipantPermittedChecker _meetingParticipantPermittedChecker = new MeetingParticipantPermittedChecker();
	    private IList<ModifyMeetingEventArgs> _modifiedMeetingArgs;

		internal SchedulerMeetingHelper(IInitiatorIdentifier initiatorIdentifier,
			ISchedulerStateHolder schedulerStateHolder,
			IResourceCalculation resourceOptimizationHelper,
			ISkillPriorityProvider skillPriorityProvider,
			IScheduleStorageFactory scheduleStorageFactory,
			CascadingResourceCalculationContextFactory resourceCalculationContextFactory,
			IStaffingCalculatorServiceFacade staffingCalculatorServiceFacade)
		{

			_initiatorIdentifier = initiatorIdentifier;
			_schedulerStateHolder = schedulerStateHolder;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_skillPriorityProvider = skillPriorityProvider;
			_scheduleStorageFactory = scheduleStorageFactory;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_staffingCalculatorServiceFacade = staffingCalculatorServiceFacade;
		}

		internal event EventHandler<ModifyMeetingEventArgs> ModificationOccured;

        /// <summary>
        /// delete person from meeting
        /// </summary>
        /// <param name="personMeetings">The person meetings.</param>
        internal void MeetingRemoveAttendees(IEnumerable<IPersonMeeting> personMeetings)
        {
            IList<IMeeting> modifiedMeetings = new List<IMeeting>();
            using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                var enumerable = personMeetings.ToArray();
                unitOfWork.Reassociate(_schedulerStateHolder.SchedulingResultState.LoadedAgents);

                IMeetingRepository meetingRepository = _repositoryFactory.CreateMeetingRepository(unitOfWork);
                foreach (IPersonMeeting personMeeting in enumerable)
                {
                    var meeting = meetingRepository.Get(personMeeting.BelongsToMeeting.Id.GetValueOrDefault());
                    if (meeting == null) // recurrent already deleted
                        continue;

					if (!LazyLoadingManager.IsInitialized(meeting.Scenario))
						LazyLoadingManager.Initialize(meeting.Scenario);
					if (!LazyLoadingManager.IsInitialized(meeting.Activity))
						LazyLoadingManager.Initialize(meeting.Activity);
                    meeting.RemovePerson(personMeeting.Person);
                    if (!meeting.MeetingPersons.Any())
                        meetingRepository.Remove(meeting);
                    if (!modifiedMeetings.Contains(meeting)) modifiedMeetings.Add(meeting);
                }
                unitOfWork.PersistAll(_initiatorIdentifier);
            }
            foreach (var meeting in modifiedMeetings)
            {
                NotifyModificationOccured(meeting, false); //Treat every meeting as not deleted. This can be optimized later on.
            }
        }

        /// <summary>
        /// Meetings the remove.
        /// </summary>
        /// <param name="meetingToRemove">The meeting to remove.</param>
        /// <param name="scheduleViewBase">The schedule view base.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-10-26
        /// </remarks>
        internal void MeetingRemove(IMeeting meetingToRemove, ScheduleViewBase scheduleViewBase)
        {
            if (meetingToRemove==null) return;

            var result =
                scheduleViewBase.ShowConfirmationMessage(
                    string.Format(CultureInfo.CurrentUICulture,
                                  UserTexts.Resources.AreYouSureYouWantToDeleteMeeting,
                                  meetingToRemove.GetSubject(new NoFormatting())), UserTexts.Resources.Meeting);

            if (result != DialogResult.Yes) return;

            using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
				IMeetingRepository meetingRepository = _repositoryFactory.CreateMeetingRepository(unitOfWork);
	            var reloadedMeeting = meetingRepository.Get(meetingToRemove.Id.GetValueOrDefault());
				if (reloadedMeeting == null) return;

				if (!LazyLoadingManager.IsInitialized(reloadedMeeting.Scenario))
					LazyLoadingManager.Initialize(reloadedMeeting.Scenario);
				if (!LazyLoadingManager.IsInitialized(reloadedMeeting.Activity))
					LazyLoadingManager.Initialize(reloadedMeeting.Activity);

				unitOfWork.Reassociate(_schedulerStateHolder.SchedulingResultState.LoadedAgents);
				var persons = reloadedMeeting.MeetingPersons.Select(m => m.Person).ToArray();

				if (!_meetingParticipantPermittedChecker.ValidatePermittedPersons(persons, reloadedMeeting.StartDate, scheduleViewBase, PrincipalAuthorization.Current())) return;
				reloadedMeeting.Snapshot();

				meetingRepository.Remove(reloadedMeeting);
                unitOfWork.PersistAll(_initiatorIdentifier);
            }
            NotifyModificationOccured(meetingToRemove, true);
        }

        private void NotifyModificationOccured(IMeeting meeting, bool deleted)
        {
        	var handler = ModificationOccured;
            if (handler!=null)
            {
                handler.Invoke(this,new ModifyMeetingEventArgs(meeting,deleted));
            }
        }

		/// <summary>
		/// start meeting composer with supplied meeting, or null if new meeting should be created
		/// </summary>
		internal void MeetingComposerStart(IMeeting meeting, IScheduleViewBase scheduleViewBase, bool editPermission,
			bool viewSchedulesPermission, IToggleManager toggleManager)
		{
			if (scheduleViewBase == null) return;
			_modifiedMeetingArgs = new List<ModifyMeetingEventArgs>();

			MeetingViewModel meetingViewModel;
			if (meeting == null)
			{
				DateOnly? meetingStart = null;
				IList<IPerson> selectedPersons = new List<IPerson>();
				foreach (var schedulePart in scheduleViewBase.CurrentColumnSelectedSchedules())
				{
					if (!meetingStart.HasValue) meetingStart = schedulePart.DateOnlyAsPeriod.DateOnly;

					if (!selectedPersons.Contains(schedulePart.Person))
						selectedPersons.Add(schedulePart.Person);
				}


				var meetingStartOrToday = meetingStart.GetValueOrDefault(DateOnly.Today);
				IList<IPerson> selectedActivePersons =
					selectedPersons.Where(new PersonIsActiveSpecification(meetingStartOrToday).IsSatisfiedBy).ToList();

				if (!_schedulerStateHolder.CommonStateHolder.ActiveActivities.Any() || selectedActivePersons.Count == 0) return;

				using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					meetingViewModel =
						MeetingComposerPresenter.CreateDefaultMeeting(
							TeleoptiPrincipal.CurrentPrincipal.GetPerson(_repositoryFactory.CreatePersonRepository(unitOfWork)),
							_schedulerStateHolder, meetingStartOrToday,
							selectedActivePersons, new Now());
				}
			}
			else
			{
				using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					var persons = meeting.MeetingPersons.Select(m => m.Person).ToArray();
					unitOfWork.Reassociate(persons);

					var period = meeting.MeetingPeriod(meeting.StartDate);
					var start = period.StartDateTimeLocal(TimeZoneHelper.CurrentSessionTimeZone);
					var end = period.EndDateTimeLocal(TimeZoneHelper.CurrentSessionTimeZone);
					meeting.TimeZone = TimeZoneHelper.CurrentSessionTimeZone;
					meeting.StartTime = start.TimeOfDay;
					meeting.EndTime = end.TimeOfDay;

					meetingViewModel = new MeetingViewModel(meeting, _schedulerStateHolder.CommonNameDescription);
				}
			}

			using (var meetingComposerView = new MeetingComposerView(meetingViewModel, _schedulerStateHolder, editPermission, viewSchedulesPermission, new EventAggregator(), _resourceOptimizationHelper, _skillPriorityProvider,_scheduleStorageFactory, _staffingCalculatorServiceFacade, _resourceCalculationContextFactory))
			{
				showMeetingComposer(meetingComposerView);
			}
		}

		private void showMeetingComposer(MeetingComposerView meetingComposerView)
		{
			meetingComposerView.SetInstanceId(_initiatorIdentifier.InitiatorId);
			meetingComposerView.ModificationOccurred += meetingComposerView_ModificationOccurred;
			meetingComposerView.ShowDialog();
			meetingComposerView.ModificationOccurred -= meetingComposerView_ModificationOccurred;
			_modifiedMeetingArgs.ForEach(eventArg => NotifyModificationOccured(eventArg.ModifiedMeeting, eventArg.Delete));
		}

        /// <summary>
        /// Meetings from list.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="theDate">The date.</param>
        /// <param name="personMeetings">The person meetings.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-10-30
        /// </remarks>
        public IMeeting MeetingFromList(IPerson person, DateTime theDate, IEnumerable<IPersonMeeting> personMeetings)
        {
            IMeeting selectedMeeting = null;

        	var numberOfMeetings = personMeetings.Count();
            if (numberOfMeetings>0)
            {
                if (numberOfMeetings > 1)
                {
                    using (MeetingPicker meetingPicker = new MeetingPicker(_schedulerStateHolder.CommonNameDescription.BuildFor(person) + " " +
                        theDate.ToShortDateString(), personMeetings))
                    {
                        meetingPicker.ShowDialog();
                        selectedMeeting = meetingPicker.SelectedMeeting();
                        meetingPicker.Close();
                    }
                }
                else
                {
                    selectedMeeting = personMeetings.First().BelongsToMeeting;
                }
            }

            return selectedMeeting;
        }

        private void meetingComposerView_ModificationOccurred(object sender, ModifyMeetingEventArgs e)
        {
			_modifiedMeetingArgs.Add(e);
        }
    }
}
