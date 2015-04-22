using System;
using System.Data.SqlClient;
using NHibernate.Exceptions;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate
{
	public class TenantNhibernateExceptionConverter : ISQLExceptionConverter
	{
		public Exception Convert(AdoExceptionContextInfo adoExceptionContextInfo)
		{
			var sqle = ADOExceptionHelper.ExtractDbException(adoExceptionContextInfo.SqlException) as SqlException;
			if (sqle != null)
			{
				if (sqle.Number == 2601)
				{
					if (sqle.Message.Contains("UQ_PersonInfo_ApplicationLogonName"))
					{
						return new DuplicateApplicationLogonNameException();
					}
					if (sqle.Message.Contains("UQ_PersonInfo_Identity"))
					{
						return new DuplicateIdentityException();
					}
				}
			}
			return SQLStateConverter.HandledNonSpecificException(adoExceptionContextInfo.SqlException, adoExceptionContextInfo.Message, adoExceptionContextInfo.Sql);
		}
	}
}