using NHibernate.Cfg;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Teleopti.Ccc.Domain.MultiTenancy;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class Tenant
	{

		private readonly IDictionary<string, string> _applicationConfig;

		protected Tenant()
		{
			_applicationConfig = new Dictionary<string, string> { { Environment.CommandTimeout, "60" } };
		}

		public Tenant(string tenantName) : this()
		{
			Name = tenantName;
			DataSourceConfiguration = new DataSourceConfiguration();
			RtaKey = generateRtaKey(Name);
			Active = true;
		}

		public virtual DataSourceConfiguration DataSourceConfiguration { get; protected set; }

		public virtual IDictionary<string, string> ApplicationConfig => new Dictionary<string, string>(_applicationConfig);
		public virtual string Name { get; protected set; }
		public virtual string RtaKey { get; set; }
		public virtual int Id { get; protected set; }
		public virtual bool Active { get; set; }

		public virtual string GetApplicationConfig(string key)
		{
			if (_applicationConfig.TryGetValue(key, out var value))
			{
				return value;
			}
			return string.Empty;
		}

		public virtual void SetApplicationConfig(string key, string value)
		{
			_applicationConfig[key] = value;
		}
		private static string generateRtaKey(string name)
		{
			var hash = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(name));
			return string.Join("", hash.Select(b => b.ToString("x2")).ToArray()).Substring(0, 10);
		}
	}
}