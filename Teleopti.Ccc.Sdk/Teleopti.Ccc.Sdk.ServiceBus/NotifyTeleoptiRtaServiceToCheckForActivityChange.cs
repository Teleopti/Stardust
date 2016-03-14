using System;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.PulseLoop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Rta.WebService;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class NotifyTeleoptiRtaServiceToCheckForActivityChange : INotifyRtaToCheckForActivityChange
    {
        private readonly IChannelCreator _channelCreator;
	    private readonly ICurrentDataSource _dataSource;

	    public NotifyTeleoptiRtaServiceToCheckForActivityChange(IChannelCreator channelCreator, ICurrentDataSource dataSource)
        {
	        _channelCreator = channelCreator;
	        _dataSource = dataSource;
        }

	    public Task CheckForActivityChange(Guid personId, Guid businessUnitId, DateTime timestamp)
	    {
		    return Task.Factory.StartNew(() =>
		    {
				var channel = _channelCreator.CreateChannel<ITeleoptiRtaService>();
				channel.GetUpdatedScheduleChange(personId, businessUnitId, timestamp, _dataSource.CurrentName());
		    });
	    }
    }
}