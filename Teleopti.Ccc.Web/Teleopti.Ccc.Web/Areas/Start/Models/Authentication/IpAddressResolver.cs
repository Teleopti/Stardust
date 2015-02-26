using Teleopti.Ccc.Infrastructure.Web;

namespace Teleopti.Ccc.Web.Areas.Start.Models.Authentication
{
	public class IpAddressResolver : IIpAddressResolver
	{
		private readonly ICurrentHttpContext _currentHttpContext;

		public IpAddressResolver(ICurrentHttpContext currentHttpContext )
		{
			_currentHttpContext = currentHttpContext;
		}

		public string GetIpAddress()
		{
			return _currentHttpContext.Current().Request.UserHostAddress;
		}
	}
}