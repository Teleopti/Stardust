using System;

namespace Teleopti.Ccc.Rta.Server
{
    public interface IAlarmMapper
    {
        RtaAlarmLight GetAlarm(Guid activityId, Guid stateGroupId, Guid businessUnit);
        RtaStateGroupLight GetStateGroup(string stateCode, Guid platformTypeId, Guid businessUnitId);
        bool IsAgentLoggedOut(Guid personId, string stateCode, Guid platformTypeId, Guid businessUnitId);
    }
}