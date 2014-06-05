using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Licensing;

namespace Teleopti.Ccc.Web.Core.Licensing
{
	public class LicenseActivatorReadFromQueryString : ILicenseActivatorProvider
	{
		private readonly IQueryStringReader _querystringReader;
		private readonly ICheckLicenseExists _checkLicense;
		public const string QuerystringKey = "datasource";

		public LicenseActivatorReadFromQueryString(IQueryStringReader querystringReader, ICheckLicenseExists checkLicense)
		{
			_querystringReader = querystringReader;
			_checkLicense = checkLicense;
		}

		public ILicenseActivator Current()
		{
			var name = _querystringReader.GetValue(QuerystringKey);
			_checkLicense.Check(name);
			return DefinedLicenseDataFactory.GetLicenseActivator(name);
		}
	}
}