using System;
using DotNetOpenAuth.Messaging;
using log4net;

namespace Teleopti.Ccc.Web.Core.RequestContext.Cookie
{
	public class SessionSpecificDataStringSerializer : ISessionSpecificDataStringSerializer
	{
		private readonly ILog _logger;
		private const char delimiter = '|';
		private const int businessUnitPosition = 0;
		private const int dataSourcePosition = 1;
		private const int personPosition = 2;
		private const int tenantPasswordPosition = 3;
		private const int isTeleoptiApplicationLogonPosition = 4;
		private const string stringFormat = "{1}{0}{2}{0}{3}{0}{4}{0}{5}";

		public SessionSpecificDataStringSerializer(ILog logger)
		{
			_logger = logger;
		}

		public string Serialize(SessionSpecificData data)
		{
			var dataArray = new object[6];
			dataArray[0] = delimiter;
			dataArray[businessUnitPosition+1] = data.BusinessUnitId;
			dataArray[dataSourcePosition+1] = data.DataSourceName;
			dataArray[personPosition+1] = data.PersonId;
			dataArray[tenantPasswordPosition+1] = data.TenantPassword;
			dataArray[isTeleoptiApplicationLogonPosition + 1] = data.IsTeleoptiApplicationLogon;

			return string.Format(stringFormat, dataArray);
		}

		public SessionSpecificData Deserialize(string stringData)
		{
			if (string.IsNullOrEmpty(stringData))
				return null; 
			var split = stringData.Split(delimiter);

			var isTeleoptiApplicationLogon = false;
			if (split.Length >= isTeleoptiApplicationLogonPosition + 1)
			{
				isTeleoptiApplicationLogon = Convert.ToBoolean(split[isTeleoptiApplicationLogonPosition]);
			}

			try
			{
				return new SessionSpecificData(new Guid(split[businessUnitPosition]),
															split[dataSourcePosition],
															new Guid(split[personPosition]), 
															split[tenantPasswordPosition],
															isTeleoptiApplicationLogon);
			}
			catch (FormatException)
			{
				_logger.Warn("Cannot create deserialize string [" + stringData + "] to a [" + typeof(SessionSpecificData).Name + "] object.");
				return null;
			}
		}
	}
}