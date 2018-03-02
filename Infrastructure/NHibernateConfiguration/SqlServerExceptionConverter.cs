﻿using System;
using System.Data.SqlClient;
using NHibernate.Exceptions;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Infrastructure.Foundation;
using ConstraintViolationException = Teleopti.Ccc.Infrastructure.Foundation.ConstraintViolationException;


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
					case 2627:
					case 2601:
						return new ConstraintViolationException(adoExceptionContextInfo.Message, sqle, adoExceptionContextInfo.Sql);
					case 547:
						return new ForeignKeyException(adoExceptionContextInfo.Message, sqle, adoExceptionContextInfo.Sql);
					case 10053:
					case 10054:
						return new DatabaseConnectionLostException(adoExceptionContextInfo.Message, sqle, adoExceptionContextInfo.Sql);
					case 1205:
						return new DeadLockVictimException(adoExceptionContextInfo.Message, sqle);
					default:
						return new DataSourceException(adoExceptionContextInfo.Message, sqle);
				}
			}
			var nhEx = SQLStateConverter.HandledNonSpecificException(adoExceptionContextInfo.SqlException, adoExceptionContextInfo.Message, adoExceptionContextInfo.Sql);
			return new DataSourceException(nhEx.Message, nhEx);
		}
	}
}
