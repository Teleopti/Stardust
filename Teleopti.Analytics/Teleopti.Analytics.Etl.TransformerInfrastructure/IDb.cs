using System;
using System.Data;

namespace Teleopti.Analytics.Etl.TransformerInfrastructure
{
    public interface IDB : IDisposable
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
