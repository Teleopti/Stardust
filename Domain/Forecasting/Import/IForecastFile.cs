using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Import
{
    public interface IForecastFile : IAggregateRoot, IChangeInfo
    {
        string FileName { get; }
        byte[] FileContent { get; }
    }
}
