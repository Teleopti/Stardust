using Autofac;
using Autofac.Integration.WebApi;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.IocCommon;

namespace Teleopti.Wfm.Api
{
	public class ApiModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<CommandDtoProvider>();
			builder.RegisterType<QueryDtoProvider>();
			builder.RegisterType<DtoProvider>();
			builder.RegisterType<QueryHandlerProvider>();
			builder.RegisterType<TokenVerifier>().As<ITokenVerifier>();
			builder.RegisterApiControllers(typeof(Startup).Assembly);
			builder.RegisterAssemblyTypes(typeof(Startup).Assembly).AsClosedTypesOf(typeof(IQueryHandler<,>)).ApplyAspects();
			builder.RegisterAssemblyTypes(typeof(Startup).Assembly).AsClosedTypesOf(typeof(ICommandHandler<>)).ApplyAspects();
		}
	}
}