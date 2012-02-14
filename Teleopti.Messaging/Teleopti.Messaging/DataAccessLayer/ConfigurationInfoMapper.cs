using System;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Messaging.Core;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class ConfigurationInfoMapper : MapperBase<IConfigurationInfo>
    {

        public ConfigurationInfoMapper(string connectionString) : base(connectionString)
        {
        }

        public override bool Insert(SqlConnection connection, IConfigurationInfo domainObject)
        {
            SqlParameter[] paramsToStore = new SqlParameter[7];     
            paramsToStore[0] = new SqlParameter("@ConfigurationId", SqlDbType.Int);
            paramsToStore[0].Value = domainObject.ConfigurationId;
            paramsToStore[1] = new SqlParameter("@ConfigurationType", SqlDbType.NVarChar);
            paramsToStore[1].Value = domainObject.ConfigurationType.TrimEnd(new char[]{' '});
            paramsToStore[1].Size = 50;
            paramsToStore[2] = new SqlParameter("@ConfigurationName", SqlDbType.NVarChar);
            paramsToStore[2].Value = domainObject.ConfigurationName.TrimEnd(new char[] { ' ' });
            paramsToStore[2].Size = 50;
            paramsToStore[3] = new SqlParameter("@ConfigurationValue", SqlDbType.NVarChar);
            paramsToStore[3].Value = domainObject.ConfigurationValue.TrimEnd(new char[] { ' ' });
            paramsToStore[3].Size = 50;
            paramsToStore[4] = new SqlParameter("@ConfigurationDataType", SqlDbType.NVarChar);
            paramsToStore[4].Value = domainObject.ConfigurationDataType.TrimEnd(new char[] { ' ' });
            paramsToStore[4].Size = 50;
            paramsToStore[5] = new SqlParameter("@ChangedBy", SqlDbType.NVarChar);
            paramsToStore[5].Value = (domainObject.ChangedBy.Length > 10 ? domainObject.ChangedBy.Substring(0, 10) : domainObject.ChangedBy);
            paramsToStore[5].Size = 10;
            paramsToStore[6] = new SqlParameter("@ChangedDateTime", SqlDbType.DateTime);
            paramsToStore[6].Value = domainObject.ChangedDateTime;
            ExecuteNonQuery(connection, CommandType.StoredProcedure,"msg.sp_Configuration_Insert", paramsToStore);
            if (domainObject.ConfigurationId == 0)
                domainObject.ConfigurationId = (int)paramsToStore[0].Value;
            return true; 
        }

        protected override IConfigurationInfo Map(IDataRecord record)
        {
            IConfigurationInfo configurationInfo = new ConfigurationInfo();
            
            object item = record["ConfigurationId"];
            configurationInfo.ConfigurationId = (DBNull.Value == item) ? 0 : (int)item;
            
            item = record["ConfigurationType"];
            configurationInfo.ConfigurationType = (DBNull.Value == item) ? string.Empty : (string)item;
            
            item = record["ConfigurationName"];
            configurationInfo.ConfigurationName = (DBNull.Value == item) ? string.Empty : (string)item;
            
            item = record["ConfigurationValue"];
            configurationInfo.ConfigurationValue = (DBNull.Value == item) ? string.Empty : (string)item;
            
            item = record["ConfigurationDataType"];
            configurationInfo.ConfigurationDataType = (DBNull.Value == item) ? string.Empty : (string)item;

            item = record["ChangedBy"];
            configurationInfo.ChangedBy = (DBNull.Value == item) ? string.Empty : (string)item;
            
            item = record["ChangedDateTime"];
            configurationInfo.ChangedDateTime = (DBNull.Value == item) ? Consts.MinDate : (DateTime)item;
            
            return configurationInfo;
        }

    }
}
