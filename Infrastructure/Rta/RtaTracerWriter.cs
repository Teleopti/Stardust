using System;
using System.Data;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class RtaTracerWriter : IRtaTracerWriter, IDisposable
	{
		private readonly ILog _log = LogManager.GetLogger("Teleopti.RtaTracer");
		private readonly IJsonSerializer serializer;

		public RtaTracerWriter(IConnectionStrings connectionStrings, IConfigReader config, IJsonDeserializer deserializer, IJsonSerializer serializer)
		{
			this.serializer = serializer;
			var appender = new AdoNetAppender()
			{
				Name = "RtaTracer",
				BufferSize = config.ReadValue("RtaTracerBufferSize", 20),
				ConnectionType = typeof(System.Data.SqlClient.SqlConnection).AssemblyQualifiedName,
				ConnectionString = connectionStrings.AnalyticsFor("Teleopti.RtaTracer"),
				CommandText = "INSERT INTO [RtaTracer] ([Time], [MessageType], [Message]) VALUES (@log_date, @messageType, @message)",
			};
			appender.AddParameter(new AdoNetAppenderParameter
			{
				ParameterName = "@log_date",
				DbType = DbType.DateTime,
				Layout = new RawTimeStampLayout()
			});
			appender.AddParameter(new ValueFromJsonSerializedMessageParameter(deserializer)
			{
				ParameterName = "@messageType",
				ValueReader = d => d.Type
			});
			appender.AddParameter(new ValueFromJsonSerializedMessageParameter(deserializer)
			{
				ParameterName = "@message",
				ValueReader = d => serializer.SerializeObject(d.Log)
			});

			appender.ActivateOptions();

			logger().AddAppender(appender);
			logger().Hierarchy.Configured = true;
		}

		public void Dispose()
		{
			logger().RemoveAllAppenders();
		}

		private static Logger logger()
		{
			return (LogManager.GetLogger("Teleopti.RtaTracer").Logger as Logger);
		}

		public void Write<T>(RtaTracerLog<T> log)
		{
			_log.Debug(serializer.SerializeObject(new {Log = log, Type = typeof(T).Name}));
		}
	}

	public class ValueFromJsonSerializedMessageParameter : AdoNetAppenderParameter
	{
		private readonly IJsonDeserializer _deserializer;
		public Func<dynamic, object> ValueReader;
		
		public ValueFromJsonSerializedMessageParameter(IJsonDeserializer deserializer)
		{
			_deserializer = deserializer;
			DbType = DbType.String;
			Size = int.MaxValue;
			Layout = (IRawLayout) new RawLayoutConverter().ConvertFrom(new PatternLayout("%message"));
		}

		public override void FormatValue(IDbCommand command, LoggingEvent loggingEvent)
		{
			var serialized = loggingEvent.RenderedMessage;
			var obj = _deserializer.DeserializeObject<dynamic>(serialized);
			((IDbDataParameter) command.Parameters[ParameterName]).Value = ValueReader.Invoke(obj);
		}
	}
}