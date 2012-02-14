using System;
using System.Data;

namespace Teleopti.Analytics.Portal.PerformanceManager.Helper
{
    public interface IDatabaseAccess : IDisposable
    {
        // Methods
        void AddProcParameter(IDbDataParameter parameter);
        void AddProcParameter(string parameterName, object value);
        void Close();
        int ExecuteNonQuery();
        IDataReader ExecuteDataReader();
        DataSet ExecuteDataSet();
    }
}