using System;
using System.Data.SqlClient;
using NHibernate.Exceptions;
using Teleopti.Ccc.Infrastructure.Foundation;
using ConstraintViolationException=Teleopti.Ccc.Infrastructure.Foundation.ConstraintViolationException;


namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
    public class SqlServerExceptionConverter : ISQLExceptionConverter
    {
        public Exception Convert(AdoExceptionContextInfo adoExceptionContextInfo)
        {
            var sqle = ADOExceptionHelper.ExtractDbException(adoExceptionContextInfo.SqlException) as SqlException;
            if (sqle != null)
            {
                switch (sqle.Number)
                {
					case -2:
						return new DataSourceException(adoExceptionContextInfo.Message, sqle);
                    case 2627:
                        return new ConstraintViolationException(adoExceptionContextInfo.Message, sqle, adoExceptionContextInfo.Sql);
					case 547:
                		return new ForeignKeyException(adoExceptionContextInfo.Message, sqle, adoExceptionContextInfo.Sql);
                }

				return new Exception(adoExceptionContextInfo.Message, sqle);
            }
			var nhEx = SQLStateConverter.HandledNonSpecificException(adoExceptionContextInfo.SqlException, adoExceptionContextInfo.Message, adoExceptionContextInfo.Sql);
			return new DataSourceException(nhEx.Message, nhEx);
        }
    }
}
