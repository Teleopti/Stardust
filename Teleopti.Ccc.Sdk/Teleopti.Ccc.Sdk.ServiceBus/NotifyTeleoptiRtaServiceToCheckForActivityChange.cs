using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.PulseLoop;
using Teleopti.Ccc.Rta.WebService;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class NotifyTeleoptiRtaServiceToCheckForActivityChange : INotifyRtaToCheckForActivityChange
    {
        private readonly IChannelCreator _channelCreator;

        public NotifyTeleoptiRtaServiceToCheckForActivityChange(IChannelCreator channelCreator)
        {
            _channelCreator = channelCreator;
        }

        public void CheckForActivityChange(Guid personId, Guid businessUnitId, DateTime timestamp, string dataSource)
        {
            var channel = _channelCreator.CreateChannel<ITeleoptiRtaService>();
            channel.GetUpdatedScheduleChange(personId, businessUnitId, timestamp, dataSource);
        }
    }
}