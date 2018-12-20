using System;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Overview
{
    public interface IInfoWindowTextFormatter
    {
        string GetInfoText(IMeeting meeting);
    }

    public class InfoWindowTextFormatter : IInfoWindowTextFormatter
    {
        private readonly ISettingDataRepository _settingDataRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly CommonNameDescriptionSetting _commonNameDescription;
        private readonly TimeZoneInfo _userTimeZone;

        public InfoWindowTextFormatter(ISettingDataRepository settingDataRepository, IUnitOfWorkFactory unitOfWorkFactory)
        {
            if(settingDataRepository == null)
                throw new ArgumentNullException("settingDataRepository");

            if(unitOfWorkFactory == null)
                throw new ArgumentNullException("unitOfWorkFactory");

            _settingDataRepository = settingDataRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
            using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                _commonNameDescription = _settingDataRepository.FindValueByKey("CommonNameDescription",
                                                                               new CommonNameDescriptionSetting());
            }
            _userTimeZone = TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.TimeZone;
        }

        public string GetInfoText(IMeeting meeting)
        {
            if (meeting == null) return string.Empty;
            string text;
            try
            {
                using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
                {
                    var person = meeting.Organizer;
                    uow.Reassociate(person);
                    text = _commonNameDescription.BuildFor(person) + "\r\n";
                    text = text + meeting.GetSubject(new NoFormatting()) + "\r\n";
                    text = text + meeting.GetLocation(new NoFormatting()) + "\r\n\r\n";
                    var persons = meeting.MeetingPersons.Select(meetingPerson => meetingPerson.Person).ToList();
                    foreach (var p in persons)
                    {
                        uow.Reassociate(p);
                        text = text + _commonNameDescription.BuildFor(p) + "\r\n";
                    }
                    text = text + "\r\n" + meeting.GetDescription(new NoFormatting()) + "\r\n\r\n";

                    if (meeting.UpdatedOn.HasValue)
                        text = text + TimeZoneInfo.ConvertTimeFromUtc(meeting.UpdatedOn.Value, _userTimeZone);
                }
            }
            catch (DataSourceException)
            {
                text = Resources.ServerUnavailable;
            }
            

            return text;
        }
    }
}