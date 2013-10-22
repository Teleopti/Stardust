using System;
using Teleopti.Ccc.Domain.ApplicationRtaQueue;
using Teleopti.Ccc.Rta.WebService;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;

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