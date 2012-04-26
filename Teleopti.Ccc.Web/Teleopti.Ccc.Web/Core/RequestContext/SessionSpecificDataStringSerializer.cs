using System;
using Teleopti.Interfaces.Domain;
using log4net;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public class SessionSpecificDataStringSerializer : ISessionSpecificDataStringSerializer
	{
		private readonly ILog _logger;
		private const char delimiter = '|';
		private const int businessUnitPosition = 0;
		private const int dataSourcePosition = 1;
		private const int personPosition = 2;
		private const int authenticateTypePosition = 3;
		private static readonly string stringFormat = "{0}" + delimiter + "{1}" + delimiter + "{2}" + delimiter + "{3}";

		public SessionSpecificDataStringSerializer(ILog logger)
		{
			_logger = logger;
		}

		public string Serialize(SessionSpecificData data)
		{
			var dataArray = new object[4];
			dataArray[businessUnitPosition] = data.BusinessUnitId;
			dataArray[dataSourcePosition] = data.DataSourceName;
			dataArray[personPosition] = data.PersonId;
			dataArray[authenticateTypePosition] = (int)data.AuthenticationType;
			return string.Format(stringFormat, dataArray);
		}

		public SessionSpecificData Deserialize(string stringData)
		{
			if (string.IsNullOrEmpty(stringData))
				return null; 
			var split = stringData.Split(delimiter);
			try
			{
				return new SessionSpecificData(new Guid(split[businessUnitPosition]),
															split[dataSourcePosition],
															new Guid(split[personPosition]),
															(AuthenticationTypeOption)Convert.ToInt32(split[authenticateTypePosition]));

			}
			catch (FormatException)
			{
				_logger.Warn("Cannot create deserialize string [" + stringData + "] to a [" + typeof(SessionSpecificData).Name + "] object.");
				return null;
			}
		}
	}
}