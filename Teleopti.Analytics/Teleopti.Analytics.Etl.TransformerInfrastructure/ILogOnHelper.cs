using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerInfrastructure
{
    public interface ILogOnHelper : IDisposable
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        IList<IBusinessUnit> GetBusinessUnitCollection();

        bool LogOn(IBusinessUnit businessUnit);
        void LogOff();
    }
}