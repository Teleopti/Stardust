using System;
using System.Data.SqlClient;
using NHibernate.Exceptions;
using Teleopti.Ccc.Domain.MultiTenancy;

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
						return new DuplicateApplicationLogonNameException(Guid.Empty);
					}
					if (sqle.Message.Contains("UQ_PersonInfo_Identity"))
					{
						return new DuplicateIdentityException(Guid.Empty);
					}
				}
			}
			return SQLStateConverter.HandledNonSpecificException(adoExceptionContextInfo.SqlException, adoExceptionContextInfo.Message, adoExceptionContextInfo.Sql);
		}
	}
}