using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Data;
using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling
{
	/// <summary>
	/// Provides the transient error detection logic for transient faults that are specific to SQL Database.
	/// </summary>
	public sealed class SqlDatabaseTransientErrorDetectionStrategyEx : ITransientErrorDetectionStrategy
	{

        /// <summary>
        /// Error codes reported by the DBNETLIB module.
        /// </summary>
        private enum ProcessNetLibErrorCode
		{
			ZeroBytes = -3,
			Timeout,
			Unknown,
			InsufficientMemory = 1,
			AccessDenied,
			ConnectionBusy,
			ConnectionBroken,
			ConnectionLimit,
			ServerNotFound,
			NetworkNotFound,
			InsufficientResources,
			NetworkBusy,
			NetworkAccessDenied,
			GeneralError,
			IncorrectMode,
			NameNotFound,
			InvalidConnection,
			ReadWriteError,
			TooManyHandles,
			ServerError,
			SSLError,
			EncryptionError,
			EncryptionNotSupported
		}



		/// <summary>
		/// Determines whether the specified exception represents a transient failure that can be compensated by a retry.
		/// </summary>
		/// <param name="ex">The exception object to be verified.</param>
		/// <returns>true if the specified exception is considered as transient; otherwise, false.</returns>
		public bool IsTransient(Exception ex)
		{
			return IsOrdinaryTransient(ex) 
				|| IsTransientTimeout(ex) 
				|| IsNetworkTimeout(ex) 
				|| IsInternalDotNetFrameworkDataProviderErrorSix(ex);
		}

		private bool IsOrdinaryTransient(Exception ex)
		{
			if (ex != null)
			{
				SqlException ex2;
				if ((ex2 = (ex as SqlException)) != null)
				{
					IEnumerator enumerator = ex2.Errors.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							SqlError sqlError = (SqlError)enumerator.Current;
							int number = sqlError.Number;
							bool result;
							if (number <= 10060)
							{
								if (number <= 64)
								{
									if (number != 20 && number != 64)
									{
										continue;
									}
								}
								else if (number != 233)
								{
									switch (number)
									{
										case 10053:
										case 10054:
											break;
										default:
											if (number != 10060)
											{
												continue;
											}
											break;
									}
								}
							}
							else if (number <= 40197)
							{
								switch (number)
								{
									case 10928:
									case 10929:
										break;
									default:
										if (number != 40143 && number != 40197)
										{
											continue;
										}
										break;
								}
							}
							else
							{
								if (number == 40501)
								{
									ThrottlingCondition throttlingCondition = ThrottlingCondition.FromError(sqlError);
									ex2.Data[throttlingCondition.ThrottlingMode.GetType().Name] = throttlingCondition.ThrottlingMode.ToString();
									ex2.Data[throttlingCondition.GetType().Name] = throttlingCondition;
									result = true;
									return result;
								}
								if (number != 40540 && number != 40613)
								{
									continue;
								}
							}
							result = true;
							return result;
						}
						return false;
					}
					finally
					{
						IDisposable disposable = enumerator as IDisposable;
						if (disposable != null)
						{
							disposable.Dispose();
						}
					}
				}
				if (ex is TimeoutException)
				{
					return true;
				}
				EntityException ex3;
				if ((ex3 = (ex as EntityException)) != null)
				{
					return this.IsTransient(ex3.InnerException);
				}
			}
			return false;
		}

		bool IsTransientTimeout(Exception ex)
		{
			if (IsConnectionTimeout(ex))
				return true;


			return ex.InnerException != null && IsTransientTimeout(ex.InnerException);
		}
		private bool IsInternalDotNetFrameworkDataProviderErrorSix(Exception ex)
		{
			var invalidOperationException = ex as InvalidOperationException;
			if (invalidOperationException != null)
			{
				return invalidOperationException.Message.Contains(".Net Framework Data Provider error 6");
			}
			return false;
		}

		private bool IsNetworkTimeout(Exception ex)
		{
			var sqlException = ex as SqlException;
			if (sqlException != null)
			{
				return sqlException.Errors.Cast<SqlError>().Any(error => error.Number == 40);
			}
			return false;
		}

		bool IsConnectionTimeout(Exception ex)
		{
			// Timeout exception: error code -2 
			// http://social.msdn.microsoft.com/Forums/en-US/ssdsgetstarted/thread/7a50985d-92c2-472f-9464-a6591efec4b3/ 


			// Timeout exception: error code 121 
			// http://social.msdn.microsoft.com/Forums/nl-NL/ssdsgetstarted/thread/5e195f94-d4d2-4c2d-8a4e-7d66b4761510 


			SqlException sqlException;
			return ex != null
				   && (sqlException = ex as SqlException) != null
				   && sqlException.Errors.Cast<SqlError>().Any(error => error.Number == -2 || error.Number == 121);
		}

	}
}
