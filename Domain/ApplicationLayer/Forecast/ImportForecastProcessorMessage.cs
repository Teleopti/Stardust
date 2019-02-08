using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages.General;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Forecast
{

    public class ImportForecastProcessorMessage : EventWithInfrastructureContext
    {

        public Guid JobId { get; set; }

        public Guid Identity
        {
            get { return JobId; }
        }

        public Guid TargetSkillId { get; set; }

        public Guid OwnerPersonId { get; set; }

        public DateTime Date { get; set; }

        public ICollection<ForecastsRow> Forecasts { get; set; }

        public ImportForecastsMode ImportMode { get; set; }
    }
}
