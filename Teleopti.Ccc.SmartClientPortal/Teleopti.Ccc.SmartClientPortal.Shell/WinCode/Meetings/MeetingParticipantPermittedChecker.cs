using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings
{
    public interface IMeetingParticipantPermittedChecker
    {
        bool ValidatePermittedPersons(IEnumerable<IPerson> persons, DateOnly startDate, IViewBase viewBase, IAuthorization authorization);
    }

    public class MeetingParticipantPermittedChecker : IMeetingParticipantPermittedChecker
    {
        public bool ValidatePermittedPersons(IEnumerable<IPerson> persons, DateOnly startDate, IViewBase viewBase, IAuthorization authorization)
        {
            var unPermittedPersons = new List<string>();
            foreach (IPerson person in persons)
            {
                if (!authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyMeetings, startDate, person))
                {
                    unPermittedPersons.Add(person.Name.ToString());
                }
            }
            if (unPermittedPersons.Count > 0)
            {
                var errorPersons = string.Join(", \r\n", unPermittedPersons.ToArray());
                string message = string.Format(System.Globalization.CultureInfo.CurrentUICulture,
                                                 UserTexts.Resources.MeetingErrorMessageWithOneParameter, errorPersons);

                viewBase.ShowErrorMessage(message, UserTexts.Resources.Permissions);
                return false;
            }
            return true;
        }
    }
}