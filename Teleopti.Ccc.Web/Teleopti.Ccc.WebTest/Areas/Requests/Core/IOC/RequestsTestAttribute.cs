using System.Globalization;
using Teleopti.Ccc.Domain.ApplicationLayer.PeopleSearch;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Ccc.WebTest.Areas.Global;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC
{
	public class RequestsTestAttribute : DomainTestAttribute
	{
		protected override void Extend(IExtend extend, IIocConfiguration configuration)
		{
			base.Extend(extend, configuration);
			extend.AddModule(new WebModule(configuration, null));
		}

		protected override void Isolate(IIsolate isolate)
		{
			base.Isolate(isolate);

			isolate.UseTestDouble<Global.FakePermissionProvider>().For<IPermissionProvider>();
			isolate.UseTestDouble<FakePeopleSearchProvider>().For<IPeopleSearchProvider>();
			isolate.UseTestDouble<FakeCurrentUnitOfWorkFactory>().For<ICurrentUnitOfWorkFactory>();

			isolate.UseTestDouble(new FakeLoggedOnUser()).For<ILoggedOnUser>();
			isolate.UseTestDouble(new FakeUserCulture(CultureInfo.GetCultureInfo("en-US"))).For<IUserCulture>();
		}
	}
}