using System;
using System.Collections.Generic;
using System.Data;
using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class RtaTracerReader : IRtaTracerReader
	{
		private readonly WithAnalyticsUnitOfWork _unitOfWork;

		public RtaTracerReader(WithAnalyticsUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IEnumerable<RtaTraceLog> Read()
		{
			return _unitOfWork.Get(uow =>
				uow.Current().Session()
					.CreateSQLQuery($@"SELECT Time, Message FROM RtaTracer")
					.SetResultTransformer(Transformers.AliasToBean<RtaTraceLog>())
					.List<RtaTraceLog>()
			);
		}
	}
	
	public class RtaTracerWriter : IRtaTracerWriter, IDisposable
	{
		private readonly ILog log = LogManager.GetLogger("Teleopti.RtaTracer");

		public RtaTracerWriter(IConnectionStrings connectionStrings)
		{
			var appender = new AdoNetAppender()
			{
				Name = "RtaTracer",
				BufferSize = 0,
				ConnectionType = typeof(System.Data.SqlClient.SqlConnection).AssemblyQualifiedName,
				ConnectionString = connectionStrings.Analytics(),
				CommandText = "INSERT INTO [RtaTracer] ([Time],[Message]) VALUES (@log_date, @message)",
			};
			appender.AddParameter(new AdoNetAppenderParameter
			{
				ParameterName = "@log_date",
				DbType = DbType.DateTime,
				Layout = new RawTimeStampLayout()
			});
			appender.AddParameter(new AdoNetAppenderParameter
			{
				ParameterName = "@message",
				DbType = DbType.String,
				Size = 4000,
				Layout = (IRawLayout) new RawLayoutConverter().ConvertFrom(new PatternLayout("%message"))
			});
			appender.ActivateOptions();

			logger().AddAppender(appender);
			logger().Hierarchy.Configured = true;		
		}
		
		public void Write(string message)
		{
			log.Debug(message);
		}
		
		public void Dispose()
		{
			logger().RemoveAllAppenders();
		}

		private static Logger logger()
		{
			return (LogManager.GetLogger("Teleopti.RtaTracer").Logger as Logger);
		}
	}
	
}