using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;
using Teleopti.Ccc.Web.Areas.Rta;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class GetUpdatedScheduleChangeFromTeleoptiRtaService : IGetUpdatedScheduleChangeFromTeleoptiRtaService
    {
        private readonly IChannelCreator _channelCreator;

        public GetUpdatedScheduleChangeFromTeleoptiRtaService(IChannelCreator channelCreator)
        {
            _channelCreator = channelCreator;
        }

        public void GetUpdatedScheduleChange(Guid personId, Guid businessUnitId, DateTime timestamp)
        {
            var channel = _channelCreator.CreateChannel<ITeleoptiRtaService>();
            channel.GetUpdatedScheduleChange(personId, businessUnitId, timestamp);
        }
    }
}