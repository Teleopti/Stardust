using System;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
    public interface IDatabaseWriter
    {
        RtaStateGroupLight AddAndGetNewRtaState(string stateCode, Guid platformTypeId, Guid businessUnit);
        void PersistActualAgentState(IActualAgentState actualAgentState);
    }
}