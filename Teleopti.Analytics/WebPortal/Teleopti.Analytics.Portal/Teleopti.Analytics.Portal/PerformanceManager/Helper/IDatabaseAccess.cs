using System;
using System.Data;

namespace Teleopti.Analytics.Portal.PerformanceManager.Helper
{
    public interface IDatabaseAccess : IDisposable
    {
        // Methods
        void AddProcParameter(IDbDataParameter parameter);
    }
}