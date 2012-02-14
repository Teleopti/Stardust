using System;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Messaging.Core;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class AddressMapper : MapperBase<IAddressInformation>
    {
        public AddressMapper(string connectionString) : base(connectionString)
        {
        }

        public override bool Insert(SqlConnection connection, IAddressInformation domainObject)
        {
            SqlParameter[] paramsToStore = new SqlParameter[3];
            paramsToStore[0] = new SqlParameter("@AddressId", SqlDbType.Int);
            paramsToStore[0].Value = domainObject.AddressId;
            paramsToStore[0].Direction = ParameterDirection.InputOutput;
            paramsToStore[1] = new SqlParameter("@Address", SqlDbType.NVarChar);
            paramsToStore[1].Value = domainObject.Address;
            paramsToStore[1].Size = 255;
            paramsToStore[2] = new SqlParameter("@Port", SqlDbType.Int);
            paramsToStore[2].Value = domainObject.Port;
            ExecuteNonQuery(connection, CommandType.StoredProcedure, "msg.sp_Address_Insert", paramsToStore);
            if (domainObject.AddressId == 0)
                domainObject.AddressId = (int)paramsToStore[0].Value;
            return true;
        }

        protected override IAddressInformation Map(IDataRecord record)
        {
            IAddressInformation addressInfo = new MessageInformation();
            addressInfo.AddressId = (DBNull.Value == record["AddressId"]) ? 0 : (int)record["AddressId"];
            addressInfo.Address = (DBNull.Value == record["Address"]) ? string.Empty : (string)record["Address"];
            addressInfo.Port = (DBNull.Value == record["Port"]) ? 0 : (int)record["Port"];
            return addressInfo;
        }

    }
}
