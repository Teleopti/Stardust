using System;
using System.Data;

namespace Teleopti.Analytics.Etl.TransformerInfrastructure
{
    public interface IDB : IDisposable
    {
        void AddProcParameter(IDbDataParameter parameter);
        int ExecuteNonQuery();
        DataSet ExecuteDataSet();
    }
}
