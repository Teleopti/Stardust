using System;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration.LegacyTransientErrorHandling
{
	[RemoveMeWithToggle(Toggles.Tech_Moving_ResilientConnectionLogic_76181)]
	public sealed class SqlDatabaseTransientErrorDetectionStrategy : ITransientErrorDetectionStrategy
	{
		public bool IsTransient(Exception ex)
		{
			if (ex != null)
			{
				SqlException sqlException;
				if ((sqlException = ex as SqlException) != null)
				{
					foreach (SqlError error in sqlException.Errors)
					{
						switch (error.Number)
						{
							case 20:
							case 64:
							case 233:
							case 10053:
							case 10054:
							case 10060:
							case 10928:
							case 10929:
							case 40143:
							case 40197:
							case 40540:
							case 40613:
								return true;
							case 40501:
								ThrottlingCondition throttlingCondition = ThrottlingCondition.FromError(error);
								sqlException.Data[(object)throttlingCondition.ThrottlingMode.GetType().Name] = (object)throttlingCondition.ThrottlingMode.ToString();
								sqlException.Data[(object)throttlingCondition.GetType().Name] = (object)throttlingCondition;
								return true;
							default:
								continue;
						}
					}
				}
				else
				{
					if (ex is TimeoutException)
						return true;
					EntityException entityException;
					if ((entityException = ex as EntityException) != null)
						return this.IsTransient(entityException.InnerException);
				}
			}
			return false;
		}

		private enum ProcessNetLibErrorCode
		{
			ZeroBytes = -3,
			Timeout = -2,
			Unknown = -1,
			InsufficientMemory = 1,
			AccessDenied = 2,
			ConnectionBusy = 3,
			ConnectionBroken = 4,
			ConnectionLimit = 5,
			ServerNotFound = 6,
			NetworkNotFound = 7,
			InsufficientResources = 8,
			NetworkBusy = 9,
			NetworkAccessDenied = 10, // 0x0000000A
			GeneralError = 11, // 0x0000000B
			IncorrectMode = 12, // 0x0000000C
			NameNotFound = 13, // 0x0000000D
			InvalidConnection = 14, // 0x0000000E
			ReadWriteError = 15, // 0x0000000F
			TooManyHandles = 16, // 0x00000010
			ServerError = 17, // 0x00000011
			SSLError = 18, // 0x00000012
			EncryptionError = 19, // 0x00000013
			EncryptionNotSupported = 20, // 0x00000014
		}
	}
}