using System;
using System.Web;

namespace Teleopti.Ccc.Web.Filters
{
	public interface IAuthenticationModule
	{
		Uri Issuer(HttpContextBase request);
		string Realm { get; }
	}
}