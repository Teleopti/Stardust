using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using log4net;
using Newtonsoft.Json;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class ExtensiveLogRepository : IExtensiveLogRepository
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly INow _now;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly ILog _logger = LogManager.GetLogger(typeof(ExtensiveLogRepository));

		public ExtensiveLogRepository(ICurrentUnitOfWork currentUnitOfWork, INow now, ICurrentBusinessUnit currentBusinessUnit, ILoggedOnUser loggedOnUser)
		{
			_currentUnitOfWork = currentUnitOfWork;
			_now = now;
			_currentBusinessUnit = currentBusinessUnit;
			_loggedOnUser = loggedOnUser;
		}

		public void Add(object obj,Guid objId, string entityType)
		{
			try
			{
				//assuming we only have one setting
				
				var settingValue = _currentUnitOfWork.Session().CreateSQLQuery(
						@"select value,TimeoutInMin,StartedLoggingAt from ExtensiveLogsSettings where Setting = 'EnableRequestLogging'")
					.SetResultTransformer(Transformers.AliasToEntityMap)
					.List<Hashtable>().First();

				if (Convert.ToBoolean(settingValue["value"]))
				{
					if (loggingNotTimedout(settingValue["TimeoutInMin"], settingValue["StartedLoggingAt"]))
					{
						var rawObject = JsonConvert.SerializeObject(obj, Formatting.Indented,
							new JsonSerializerSettings
							{
								ReferenceLoopHandling = ReferenceLoopHandling.Ignore
							});
						string hostName = Dns.GetHostName();

						var networkinfo = Dns.GetHostAddresses(hostName);
						var myIp = "";
						if (networkinfo.Any(x => x.AddressFamily.ToString() == ProtocolFamily.InterNetwork.ToString()))
						{
							myIp = networkinfo.First(x => x.AddressFamily.ToString() == ProtocolFamily.InterNetwork.ToString()).ToString();
						}

						_currentUnitOfWork.Session().CreateSQLQuery(
								@"INSERT INTO [ExtensiveLogs] 
						(
							[Id]
							,[ObjectId]
							,[Person]
							,[BusinessUnit]
							,[UpdatedOn]	
							,[RawData]
							,[EntityType]
							,[IpAddress]
							,[HostName]
						)
					VALUES
						(
							:Id,
							:ObjectId, 
							:Person, 
							:BusinessUnit, 
							:UpdatedOn, 
							:RawData,
							:EntityType,
							:IpAddress,
							:HostName
						)"
							)
							.SetGuid("Id", Guid.NewGuid())
							.SetParameter<Guid?>("ObjectId", objId)
							.SetParameter<Guid?>("Person", _loggedOnUser.CurrentUser().Id)
							.SetParameter<Guid?>("BusinessUnit", _currentBusinessUnit.CurrentId())
							.SetDateTime("UpdatedOn", _now.UtcDateTime())
							.SetParameter("RawData",rawObject,NHibernateUtil.StringClob)
							.SetString("EntityType", entityType)
							.SetString("IpAddress", myIp)
							.SetString("HostName", hostName)
							.ExecuteUpdate();
					}
					
				}
				
			}
			catch (Exception e)
			{
				_logger.Error("Exception in extensive logging",e);
			}
			
		}

		private bool loggingNotTimedout(object to,object ls)
		{
			var timeoutInMinutes = Convert.ToInt32(to);
			var timeoutAt = _now.UtcDateTime().AddMinutes(timeoutInMinutes);
			if (ls != null)
			{
				var logStartedAt = Convert.ToDateTime(ls);
				if (timeoutAt > logStartedAt.AddMinutes(timeoutInMinutes))
				{
					var command = $@"UPDATE ExtensiveLogsSettings SET Value = 'false',StartedLoggingAt = null  WHERE Setting = 'EnableRequestLogging' ";
					_currentUnitOfWork.Session().CreateSQLQuery(command).ExecuteUpdate();
					return false;
				}
			}
			else
			{
				var command = $@"UPDATE ExtensiveLogsSettings SET StartedLoggingAt = :StartedLoggingAt  WHERE Setting = 'EnableRequestLogging' ";
				_currentUnitOfWork.Session().CreateSQLQuery(command)
					.SetDateTime("StartedLoggingAt", timeoutAt)
					.ExecuteUpdate();
			}

			return true;
		}

		public IList<ExtensiveLog> LoadAll()
		{
			var result = _currentUnitOfWork.Session().CreateSQLQuery(
					@"SELECT [Id] ,[ObjectId],[Person],[BusinessUnit],[UpdatedOn],[RawData],[EntityType],[IpAddress],[HostName]  FROM [dbo].[ExtensiveLogs]")
				.SetResultTransformer(Transformers.AliasToBean<ExtensiveLog>())
				.List<ExtensiveLog>();
			return result;
		}

		public string LoadSetting(string setting)
		{
			throw new NotImplementedException();
		}
	}
}