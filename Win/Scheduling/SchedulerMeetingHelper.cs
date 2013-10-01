using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Win.Meetings;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Meetings;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Scheduling
{
    internal class SchedulerMeetingHelper
    {
        private readonly IMessageBrokerIdentifier _messageBrokerIdentifier;
        private readonly ISchedulerStateHolder _schedulerStateHolder;
        private readonly IRepositoryFactory _repositoryFactory = new RepositoryFactory();

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulerMeetingHelper"/> class.
        /// </summary>
        /// <param name="messageBrokerIdentifier">The message broker module.</param>
        /// <param name="schedulerStateHolder">The scheduler state holder.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-10-26
        /// </remarks>
        internal SchedulerMeetingHelper(IMessageBrokerIdentifier messageBrokerIdentifier, ISchedulerStateHolder schedulerStateHolder)
        {
            _messageBrokerIdentifier = messageBrokerIdentifier;
            _schedulerStateHolder = schedulerStateHolder;
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
                IMeetingRepository meetingRepository = _repositoryFactory.CreateMeetingRepository(unitOfWork);
                foreach (IPersonMeeting personMeeting in personMeetings)
                {
                    var meeting = meetingRepository.Get(personMeeting.BelongsToMeeting.Id.Value);
                    if (meeting == null) // recurrent already deleted
                        continue;
                    meeting.RemovePerson(personMeeting.Person);
                    if (meeting.MeetingPersons.Count() == 0)
                        meetingRepository.Remove(meeting);
                    if (!modifiedMeetings.Contains(meeting)) modifiedMeetings.Add(meeting);
                }
                unitOfWork.PersistAll(_messageBrokerIdentifier);
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
                var persons = meetingToRemove.MeetingPersons.Select(m => m.Person);
                unitOfWork.Reassociate(persons);
                if (!new MeetingParticipantPermittedChecker().ValidatePermittedPersons(persons, meetingToRemove.StartDate, scheduleViewBase, PrincipalAuthorization.Instance()))
                    return;
				meetingToRemove.Snapshot();

                IMeetingRepository meetingRepository = _repositoryFactory.CreateMeetingRepository(unitOfWork);
                meetingRepository.Remove(meetingToRemove);
                unitOfWork.PersistAll(_messageBrokerIdentifier);
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
        internal void MeetingComposerStart(IMeeting meeting, IScheduleViewBase scheduleViewBase, bool editPermission, bool viewSchedulesPermission)
        {
            if(scheduleViewBase == null) return;

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

                if (_schedulerStateHolder.CommonStateHolder.Activities.Count == 0 || selectedPersons.Count == 0) return;

                using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    meetingViewModel =
                        MeetingComposerPresenter.CreateDefaultMeeting(
                            TeleoptiPrincipal.Current.GetPerson(_repositoryFactory.CreatePersonRepository(unitOfWork)),
                            _schedulerStateHolder, meetingStart.GetValueOrDefault(DateOnly.Today),
                            selectedPersons, new Now());
                }
            }
            else
            {
                var persons = meeting.MeetingPersons.Select(meetingPerson => meetingPerson.Person).ToList();
                using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    unitOfWork.Reassociate(persons);

                    var period  = meeting.MeetingPeriod(meeting.StartDate);
                    var start = period.StartDateTimeLocal(TimeZoneHelper.CurrentSessionTimeZone);
                    var end = period.EndDateTimeLocal(TimeZoneHelper.CurrentSessionTimeZone);
                    meeting.TimeZone = TimeZoneHelper.CurrentSessionTimeZone;
                    meeting.StartTime = start.TimeOfDay;
                    meeting.EndTime = end.TimeOfDay;

                    meetingViewModel = new MeetingViewModel(meeting, _schedulerStateHolder.CommonNameDescription);
                }
            }

            using (MeetingComposerView meetingComposerView = new MeetingComposerView(meetingViewModel, _schedulerStateHolder, editPermission, viewSchedulesPermission, new EventAggregator()))
            {
                meetingComposerView.ModificationOccurred += meetingComposerView_ModificationOccurred;
                meetingComposerView.ShowDialog();
                meetingComposerView.ModificationOccurred -= meetingComposerView_ModificationOccurred;
            }
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
                    using (MeetingPicker meetingPicker = new MeetingPicker(_schedulerStateHolder.CommonNameDescription.BuildCommonNameDescription(person) + " " +
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
            NotifyModificationOccured(e.ModifiedMeeting, e.Delete);
        }
    }
}
