using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Licensing;

namespace Teleopti.Ccc.Web.Core.Licensing
{
	public class LicenseActivatorDecoratorReadFromQueryString : ILicenseActivatorProvider
	{
		private readonly ILicenseActivatorProvider _fallbackActivatorProvider;
		private readonly IQueryStringReader _querystringReader;
		private readonly ICheckLicenseExists _checkLicense;
		public const string QuerystringKey = "datasource";

		public LicenseActivatorDecoratorReadFromQueryString(ILicenseActivatorProvider fallbackActivatorProvider, 
															IQueryStringReader querystringReader, 
															ICheckLicenseExists checkLicense)
		{
			_fallbackActivatorProvider = fallbackActivatorProvider;
			_querystringReader = querystringReader;
			_checkLicense = checkLicense;
		}

		public ILicenseActivator Current()
		{
			var name = _querystringReader.GetValue(QuerystringKey);
			if (name == null)
				return _fallbackActivatorProvider.Current();
			_checkLicense.Check(name);
			return DefinedLicenseDataFactory.GetLicenseActivator(name);
		}
	}
}