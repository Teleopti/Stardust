using log4net;
using System;

namespace Teleopti.Ccc.Web.Core.RequestContext.Cookie
{
	public class SessionSpecificDataStringSerializer : ISessionSpecificDataStringSerializer
	{
		private readonly ILog _logger;
		private const char delim = '|';
		private const int businessUnitPosition = 0;
		private const int dataSourcePosition = 1;
		private const int personPosition = 2;
		private const int tenantPasswordPosition = 3;
		private const int isTeleoptiApplicationLogonPosition = 4;

		public SessionSpecificDataStringSerializer(ILog logger)
		{
			_logger = logger;
		}

		public string Serialize(SessionSpecificData data)
		{
			return $"{data.BusinessUnitId}{delim}{data.DataSourceName}{delim}{data.PersonId}{delim}{data.TenantPassword}{delim}{data.IsTeleoptiApplicationLogon}";
		}

		public SessionSpecificData Deserialize(string stringData)
		{
			if (string.IsNullOrEmpty(stringData))
				return null;

			try
			{
				var split = stringData.Split(delim);
				var isTeleoptiApplicationLogon = false;
				if (split.Length >= isTeleoptiApplicationLogonPosition + 1)
				{
					isTeleoptiApplicationLogon = Convert.ToBoolean(split[isTeleoptiApplicationLogonPosition]);
				}

				return new SessionSpecificData(Guid.Parse(split[businessUnitPosition]),
											   split[dataSourcePosition],
											   Guid.Parse(split[personPosition]),
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