using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class Tenant
	{
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
		public virtual int Id { get; protected set; }

		private static string generateRtaKey(string name)
		{
			var hash = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(name));
			return string.Join("", hash.Select(b => b.ToString("x2")).ToArray()).Substring(0, 10);
		}
	}
}