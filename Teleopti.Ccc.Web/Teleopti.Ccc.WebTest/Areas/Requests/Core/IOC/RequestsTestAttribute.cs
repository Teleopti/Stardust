using System;
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


namespace Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC
{
	public class WebTestAttribute : Attribute, IExtendSystem
	{
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddModule(new WebModule(configuration, null));
		}
	}

	public class RequestsTestAttribute : Attribute, IIsolateSystem
	{
		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakePeopleSearchProvider>().For<IPeopleSearchProvider>();
			isolate.UseTestDouble<FakeCurrentUnitOfWorkFactory>().For<ICurrentUnitOfWorkFactory>();
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			isolate.UseTestDouble(new FakeUserCulture(CultureInfo.GetCultureInfo("en-US"))).For<IUserCulture>();
		}
	}
}