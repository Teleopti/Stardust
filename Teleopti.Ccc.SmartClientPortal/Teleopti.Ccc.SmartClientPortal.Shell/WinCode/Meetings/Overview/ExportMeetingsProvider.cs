using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Overview
{
    public interface IExportMeetingsProvider
    {
        IList<IMeeting> GetMeetings(DateOnlyPeriod dateOnlyPeriod);
    }

    public class ExportMeetingsProvider : IExportMeetingsProvider
    {
        private readonly IMeetingRepository _meetingRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IMeetingOverviewViewModel _model;
        private readonly TimeZoneInfo _userTimeZone;

        public ExportMeetingsProvider(IMeetingRepository meetingRepository,
            IUnitOfWorkFactory unitOfWorkFactory, IMeetingOverviewViewModel model)
        {
            _meetingRepository = meetingRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _model = model;
            _userTimeZone = TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone;
        }


        public IList<IMeeting> GetMeetings(DateOnlyPeriod dateOnlyPeriod)
        {
            var lst = new HashSet<IMeeting>();
            using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var meetingsToExport = _meetingRepository.Find(dateOnlyPeriod.ToDateTimePeriod(_userTimeZone),
                                                               _model.CurrentScenario);
                foreach (var meeting in meetingsToExport)
                {
                    _meetingRepository.LoadAggregate(meeting.Id.GetValueOrDefault());
                    var dates = meeting.GetRecurringDates();
                    foreach (var dateOnly in dates)
                    {
                        if (dateOnlyPeriod.Contains(dateOnly))
                        {
                            lst.Add(meeting);
                            break;
                        }
                    }
                }
            }
            return new List<IMeeting>(lst);
        }
    }

}