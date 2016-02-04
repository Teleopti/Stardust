﻿using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{

    public class ImportForecastsToSkill : EventWithInfrastructureContext
    {

        public Guid JobId { get; set; }

        public Guid Identity
        {
            get { return JobId; }
        }

        public Guid TargetSkillId { get; set; }

        public Guid OwnerPersonId { get; set; }

        public DateTime Date { get; set; }

        public ICollection<IForecastsRow> Forecasts { get; set; }

        public ImportForecastsMode ImportMode { get; set; }
    }
}
