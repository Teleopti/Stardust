using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Web.Areas.MultiTenancy;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy.Core
{
	public class TenantUnitOfWorkAttributeTest
	{
		[Test]
		public void MethodMustBeVirtual()
		{
			foreach (var type in typeof(PersonInfoController).Assembly.GetTypes())
			{
				foreach (var method in type.GetMethods()
					.Where(method => method.GetCustomAttribute<TenantUnitOfWorkAttribute>() != null && !method.IsVirtual))
				{
					Assert.Fail("Method [{0}] on type [{1}] is decorated with TenantUnitOfWorkAttribute. This method needs to be virtual otherwise transaction won't be committed! (might be relaxed later if we change proxy handling)", method, type);
				}
			}
		}
	}
}