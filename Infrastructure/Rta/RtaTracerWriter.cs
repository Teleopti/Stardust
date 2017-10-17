using System;
using System.Data;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class RtaTracerWriter : IRtaTracerWriter, IDisposable
	{
		private readonly ILog _log = LogManager.GetLogger("Teleopti.RtaTracer");
		private readonly IJsonSerializer _serializer;

		public RtaTracerWriter(IConfigReader config, IJsonDeserializer deserializer, IJsonSerializer serializer)
		{
			_serializer = serializer;
			var appender = new AdoNetAppender()
			{
				Name = "RtaTracer",
				BufferSize = config.ReadValue("RtaTracerBufferSize", 20),
				ConnectionType = typeof(System.Data.SqlClient.SqlConnection).AssemblyQualifiedName,
				ConnectionString = config.ConnectionString("RtaTracer"),
				CommandText = "INSERT INTO RtaTracer.[Logs] ([Time], [Tenant], [MessageType], [Message]) VALUES (@log_date, @tenant, @messageType, @message)",
			};
			appender.AddParameter(new AdoNetAppenderParameter
			{
				ParameterName = "@log_date",
				DbType = DbType.DateTime,
				Layout = new RawTimeStampLayout()
			});
			appender.AddParameter(new ValueFromJsonSerializedMessageParameter(deserializer)
			{
				ParameterName = "@tenant",
				ValueReader = d => d.Tenant
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

			// not really tested
			logger().Level = Level.All;

			logger().AddAppender(appender);
			logger().Hierarchy.Configured = true;
		}

		public void Dispose()
		{
			logger().RemoveAllAppenders();
		}

		private static Logger logger()
		{
			return LogManager.GetLogger("Teleopti.RtaTracer").Logger as Logger;
		}

		public void Write<T>(RtaTracerLog<T> log)
		{
			_log.Debug(_serializer.SerializeObject(new { Log = log, log.Tenant, Type = typeof(T).Name }));
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
			Layout = (IRawLayout)new RawLayoutConverter().ConvertFrom(new PatternLayout("%message"));
		}

		public override void FormatValue(IDbCommand command, LoggingEvent loggingEvent)
		{
			var serialized = loggingEvent.RenderedMessage;
			var obj = _deserializer.DeserializeObject<dynamic>(serialized);
			((IDbDataParameter)command.Parameters[ParameterName]).Value = ValueReader.Invoke(obj);
		}
	}
}