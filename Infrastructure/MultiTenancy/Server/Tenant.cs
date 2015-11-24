using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class Tenant
	{
#pragma warning disable 169
#pragma warning disable 649
		private int id;
#pragma warning restore 649
#pragma warning restore 169

		protected Tenant(){}

		public Tenant(string tenantName)
		{
			Name = tenantName;
			DataSourceConfiguration = new DataSourceConfiguration();
			RtaKey = generateRtaKey(Name);
		}

		public virtual DataSourceConfiguration DataSourceConfiguration { get; protected set; }
		public virtual string Name { get; protected set; }
		public virtual string RtaKey { get; set; }

		private string generateRtaKey(string name)
		{
			var hash = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(name));
			return string.Join("", hash.Select(b => b.ToString("x2")).ToArray()).Substring(0, 10);
		}

		public virtual int GetId()
		{
			return id;
		}
	}
}