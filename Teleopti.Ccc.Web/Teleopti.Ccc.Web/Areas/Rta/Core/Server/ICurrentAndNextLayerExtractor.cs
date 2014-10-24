﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Rta;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
    public interface ICurrentAndNextLayerExtractor
    {
        Tuple<ScheduleLayer, ScheduleLayer> CurrentLayerAndNext(DateTime onTime, IList<ScheduleLayer> layers);
    }
}