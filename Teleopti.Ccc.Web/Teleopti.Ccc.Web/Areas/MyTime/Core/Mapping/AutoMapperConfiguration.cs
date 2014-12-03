using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Autofac;
using Owin;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Web.Core.Startup.Booter;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Mapping
{
	[TaskPriority(999)]
	public class AutoMapperConfiguration : IBootstrapperTask
	{
		private readonly IEnumerable<Profile> _profiles;
		private readonly IComponentContext _container;

		public AutoMapperConfiguration(IEnumerable<Profile> profiles, IComponentContext container)
		{
			_profiles = profiles;
			_container = container;
		}

		public Task Execute(IAppBuilder application)
		{
			Mapper.Initialize(c =>
			                  	{
			                  		c.AllowNullCollections = true;
									c.ConstructServicesUsing(_container.Resolve);
									_profiles.ForEach(c.AddProfile);
			                  	});
			return null;
		}
	}
}