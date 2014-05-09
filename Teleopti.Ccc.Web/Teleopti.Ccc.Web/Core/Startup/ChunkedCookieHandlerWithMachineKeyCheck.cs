using System;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Security;
using Microsoft.IdentityModel.Web;

namespace Teleopti.Ccc.Web.Core.Startup
{
	public class ChunkedCookieHandlerWithMachineKeyCheck : CookieHandler
	{
		private readonly CookieHandler innerHandler = new ChunkedCookieHandler();
		private readonly MethodInfo methodDeleteCore = typeof(ChunkedCookieHandler).GetMethod("DeleteCore",
			BindingFlags.Instance | BindingFlags.NonPublic);
		private readonly MethodInfo methodReadCore = typeof(ChunkedCookieHandler).GetMethod("ReadCore",
			BindingFlags.Instance | BindingFlags.NonPublic);
		private readonly MethodInfo methodWriteCore = typeof(ChunkedCookieHandler).GetMethod("WriteCore",
			BindingFlags.Instance | BindingFlags.NonPublic);

		protected override void DeleteCore(string name, string path, string domain, HttpContext context)
		{
			methodDeleteCore.Invoke(innerHandler, new object[]{name, path, domain, context});
		}

		protected override byte[] ReadCore(string name, HttpContext context)
		{
			var result = (byte[]) methodReadCore.Invoke(innerHandler, new object[] { name, context });
			try
			{
				MachineKey.Decode(Encoding.UTF8.GetString(result), MachineKeyProtection.All);
			}
			catch (Exception)
			{
				return null;
			}
			return result;
		}

		protected override void WriteCore(byte[] value, string name, string path, string domain, DateTime expirationTime, bool secure,
			bool httpOnly, HttpContext context)
		{
			methodWriteCore.Invoke(innerHandler, new object[] { value, name, path, domain, expirationTime, secure, httpOnly, context });
		}
	}
}