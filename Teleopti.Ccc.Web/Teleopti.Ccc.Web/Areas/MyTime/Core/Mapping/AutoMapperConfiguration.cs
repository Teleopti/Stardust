using System.Collections.Generic;
using AutoMapper;
using Autofac;
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

		public void Execute()
		{
			Mapper.Initialize(c =>
			                  	{
									c.ConstructServicesUsing(_container.Resolve);
									_profiles.ForEach(c.AddProfile);
			                  	});
		}
	}
}