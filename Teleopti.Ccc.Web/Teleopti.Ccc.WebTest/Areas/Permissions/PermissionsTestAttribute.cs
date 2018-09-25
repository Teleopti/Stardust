using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Core.IoC;

namespace Teleopti.Ccc.WebTest.Areas.Permissions
{
	public class PermissionsTestAttribute : DomainTestAttribute
	{
		protected override void Extend(IExtend extend, IocConfiguration configuration)
		{
			base.Extend(extend, configuration);
			extend.AddModule(new WebModule(configuration, null));
		}

		protected override void Isolate(IIsolate isolate)
		{
			base.Isolate(isolate);
			isolate.UseTestDouble<FakePersonInRoleQuerier>().For<IPersonInRoleQuerier>();
			isolate.UseTestDouble<FakeApplicationFunctionsToggleFilter>().For<IApplicationFunctionsToggleFilter>();
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
		}
	}

	public class FakePersonInRoleQuerier : IPersonInRoleQuerier
	{
		private List<Guid> _personIdList = new List<Guid>();

		public void AddFakeData(List<Guid> personIdList)
		{
			_personIdList = personIdList;
		}

		public IEnumerable<Guid> GetPersonInRole(Guid roleId)
		{
			return _personIdList;
		}
	}
}