using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;


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
            _userTimeZone = TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone;
        }
		
        public IList<IMeeting> GetMeetings(DateOnlyPeriod dateOnlyPeriod)
        {
            using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var meetings = _meetingRepository.Find(dateOnlyPeriod.ToDateTimePeriod(_userTimeZone),
                                                               _model.CurrentScenario).Distinct().ToArray();
				var people = meetings.SelectMany(m => m.MeetingPersons.Select(p => p.Person)).Distinct();
				people.ForEach(p =>
				{
					if (!LazyLoadingManager.IsInitialized(p))
					{
						LazyLoadingManager.Initialize(p);
					}
				});
				return meetings;
			}
        }
    }
}