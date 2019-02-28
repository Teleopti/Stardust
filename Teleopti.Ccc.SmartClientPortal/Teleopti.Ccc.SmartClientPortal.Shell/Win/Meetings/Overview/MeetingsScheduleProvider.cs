using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Syncfusion.Schedule;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.ExceptionHandling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Overview;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Meetings.Overview
{
    public class MeetingsScheduleProvider : ScheduleDataProvider
    {
        private readonly IScheduleAppointmentFromMeetingCreator _scheduleAppointmentFromMeetingCreator;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IRepositoryFactory _repositoryFactory;

    	private DateOnlyPeriod _lastPeriod;
        private readonly IScheduleAppointmentList _lastList = new ScheduleAppointmentList();
        private readonly AppointmentFromMeetingCreator _appointmentFromMeetingCreator;
        private readonly TimeZoneInfo _userTimeZone;
    	private readonly IMeetingChangerAndPersister _meetingChangerAndPersister;
        private readonly IMeetingOverviewViewModel _model;
    	private readonly IOverlappingAppointmentsHelper _overlappingAppointmentsHelper;
    	private IList<IScenario> _allowedScenarios;

        public MeetingsScheduleProvider(IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory, 
            IScheduleAppointmentFromMeetingCreator scheduleAppointmentFromMeetingCreator, IMeetingChangerAndPersister meetingChangerAndPersister, 
            IMeetingOverviewViewModel model, IOverlappingAppointmentsHelper overlappingAppointmentsHelper)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _repositoryFactory = repositoryFactory;
            _scheduleAppointmentFromMeetingCreator = scheduleAppointmentFromMeetingCreator;
            _meetingChangerAndPersister = meetingChangerAndPersister;
            _model = model;
        	_overlappingAppointmentsHelper = overlappingAppointmentsHelper;
        	_userTimeZone = TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone;
            _appointmentFromMeetingCreator = new AppointmentFromMeetingCreator(new MeetingTimeZoneHelper(_userTimeZone));
            
            SaveOnCloseBehaviorAction = SaveOnCloseBehavior.DoNotSave;
        }

       public void CreateListObjectList()
        {
            var listObjects = new ListObjectList();
          
            using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                unitOfWork.DisableFilter(QueryFilter.Deleted);
                var activityRepository = _repositoryFactory.CreateActivityRepository(unitOfWork);
                var activities = activityRepository.LoadAllSortByName();
                var index = 0;
                foreach (var activity in activities)
                {
                    var listObject = new ActivityListObject(index, activity.Name, activity.DisplayColor, activity);
                    listObjects.Add(listObject);
                    index += 1;
                }
            }
           
            LabelList = listObjects;

       		var markers = new ListObjectList
       		              	{
       		              		new MarkerListObject(0, "", Color.Empty),
       		              		new MarkerListObject(1, Resources.CannotDisplayAllMeetings, Color.Red)
       		              	};
       		MarkerList = markers;
        }

       public IScenario Scenario { get { return _model.CurrentScenario; } 
           set
           {
               ResetLoadedPeriod();
               _model.CurrentScenario = value;
           } 
       }

        public IList<IScenario> AllowedScenarios()
        {
            if(_allowedScenarios == null)
            {
               
                using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    _allowedScenarios = _repositoryFactory.CreateScenarioRepository(uow).FindAllSorted();
                }

                var authorization = PrincipalAuthorization.Current_DONTUSE();
                if (!authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewRestrictedScenario))
                {
                    for (var i = _allowedScenarios.Count - 1; i > -1; i--)
                    {
                        if (_allowedScenarios[i].Restricted)
                            _allowedScenarios.RemoveAt(i);
                    }
                }
            }
            return _allowedScenarios;
        }

        public override IScheduleAppointmentList GetSchedule(DateTime startDate, DateTime endDate)
        {
            // when using CustomWeek the dates gets mixed up
        	var period = endDate < startDate
        	             	? new DateOnlyPeriod(new DateOnly(endDate), new DateOnly(startDate))
        	             	: new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));
            if (period!=_lastPeriod)
            {
                _lastPeriod = period;
                try
                {
                    using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
                    {
                        var meetingRepository = _repositoryFactory.CreateMeetingRepository(unitOfWork);
                        var personRepository = _repositoryFactory.CreatePersonRepository(unitOfWork);

                    	var persons = new List<Guid>(_model.FilteredPersonsId);
						var person = TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.PersonId;
						if (!persons.Contains(person) && _model.IncludeForOrganizer)
						{
							persons.Add(person);
						}

                    	ScenarioRepository.DONT_USE_CTOR(unitOfWork).LoadAll();

                    	var loadedPeople = personRepository.FindPeople(persons);
						var meetings = meetingRepository.Find(loadedPeople, period, _model.CurrentScenario, _model.IncludeForOrganizer);
                    	meetings = meetings.OrderBy(m => m.StartDate).ThenBy(m => m.StartTime).ToList();
                    	IEnumerable<ISimpleAppointment> tempAppointments = _appointmentFromMeetingCreator.GetAppointments(meetings,
                    	                                                                      period.StartDate.AddDays(-1),
                    	                                                                      period.EndDate);

						tempAppointments = _overlappingAppointmentsHelper.ReduceOverlappingToFive(tempAppointments.OrderBy(t => t.StartDateTime));

						_lastList.Clear();
                        _scheduleAppointmentFromMeetingCreator.GetAppointmentList(tempAppointments, LabelList).OfType<IScheduleAppointment>().ForEach(_lastList.Add);
                    }
                }
                catch (DataSourceException dataSourceException)
                {
                	showDataSourceExceptionDialog(dataSourceException);

                	return new ScheduleAppointmentList();
                }
            }
            _lastList.SortStartTime();
            removeAppointmentsOutsidePeriod(period.StartDate, period.EndDate);
            return _lastList;
        }

    	private static void showDataSourceExceptionDialog(DataSourceException dataSourceException)
    	{
    		using (var view = new SimpleExceptionHandlerView(dataSourceException,
    		                                                 Resources.MeetingOverview,
    		                                                 Resources.ServerUnavailable))
    		{
    			view.ShowDialog();
    		}
    	}

    	private void removeAppointmentsOutsidePeriod(DateOnly startDate, DateOnly endDate)
        {
            var toRemove = new List<IScheduleAppointment>();
            foreach (IScheduleAppointment appointment in _lastList)
            {
                if(appointment.StartTime < startDate.Date || appointment.StartTime.Date > endDate.Date)
                    toRemove.Add(appointment);
            }
            foreach (var scheduleAppointment in toRemove)
            {
                _lastList.Remove(scheduleAppointment);
            }
        }

        public override IScheduleAppointmentList GetScheduleForDay(DateTime day)
        {
        	var dateOnly = new DateOnly(day);
            var period = new DateOnlyPeriod(dateOnly, dateOnly);
            try
            {
                using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
                {
                    var meetingRepository = _repositoryFactory.CreateMeetingRepository(unitOfWork);
                	var people = _repositoryFactory.CreatePersonRepository(unitOfWork).FindPeople(_model.FilteredPersonsId);
                    var meetings = meetingRepository.Find(people, period, _model.CurrentScenario, _model.IncludeForOrganizer);
                    var tempAppointments = _appointmentFromMeetingCreator.GetAppointments(meetings, period.StartDate, period.EndDate);
                    return _scheduleAppointmentFromMeetingCreator.GetAppointmentList(tempAppointments, LabelList);
                }
            }
            catch (DataSourceException dataSourceException)
            {
                using (var view = new SimpleExceptionHandlerView(dataSourceException,
                                                                    Resources.MeetingOverview,
                                                                    Resources.ServerUnavailable))
                {
                    view.ShowDialog();
                }
                return new ScheduleAppointmentList();
            }
        }

        //this is only called on drag in grid
        public void SaveAppointment(IScheduleAppointment appointment, IScheduleAppointment currentItem, IViewBase viewBase )
        {
            if (appointment == null) return;
            if(currentItem == null) return;
            // real meeting on tag
            var meeting = appointment.Tag as IMeeting;
            if (meeting == null)
                return;
            
            var startDifference = currentItem.StartTime - appointment.StartTime;
            var endDifference = appointment.EndTime - currentItem.EndTime;
			if (startDifference == TimeSpan.FromMinutes(0))
			{
				//we could do a check here if arabic culture and jump out because syncfusion behave strange changing end time in arabic (and other??)
				_meetingChangerAndPersister.ChangeDurationAndPersist(meeting, endDifference, viewBase);
			}
			else
			{
				var oldDuration = currentItem.EndTime - currentItem.StartTime;
				var diff = (appointment.EndTime - appointment.StartTime) - oldDuration;
				var app = getAppointmentFromChainIfOverMidnight(appointment, currentItem);
				_meetingChangerAndPersister.ChangeStartDateTimeAndPersist(meeting, app.StartTime, diff, _userTimeZone, viewBase);
			}
            ResetLoadedPeriod();
        }

        public void ResetLoadedPeriod()
        {
            _lastPeriod = new DateOnlyPeriod();
        }

        private static IScheduleAppointment getAppointmentFromChainIfOverMidnight(IScheduleAppointment appointment, IScheduleAppointment currentItem)
        {
            var simple = ((TeleoptiScheduleAppointment)appointment).SimpleAppointment;
            var first = simple.FirstAppointment;
            var last = simple.LastAppointment;

            if (first.Equals(last))
                return appointment;

            var diffStart = appointment.StartTime - currentItem.StartTime;
            var diffEnd = appointment.EndTime - currentItem.EndTime;
            
            return new TeleoptiScheduleAppointment
                          {
                              StartTime = first.StartDateTime.Add(diffStart),
                              EndTime = last.EndDateTime.Add(diffEnd)
                          };
        }
    }

    public class ActivityListObject: ListObject
    {
        public ActivityListObject(int valueMember, string displayMember, Color colorMember, IActivity activity)
            : base(valueMember, displayMember, colorMember)
        {
            Activity = activity;
        }

        public IActivity Activity{get; set;}
    }

	public class MarkerListObject : ListObject
	{
		public MarkerListObject(int valueMember, string displayMember, Color colorMember)
			: base(valueMember, displayMember, colorMember)
		{}
	}
}