using Autofac;
using Autofac.Integration.WebApi;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.IocCommon;

namespace Teleopti.Wfm.Api
{
	public class ApiModule : Module
	{
		private readonly IocConfiguration _config;

		public ApiModule(IocConfiguration config)
		{
			_config = config;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<CommandDtoProvider>().SingleInstance();
			builder.RegisterType<QueryDtoProvider>().SingleInstance();
			builder.RegisterType<DtoProvider>().SingleInstance();
			builder.RegisterType<QueryHandlerProvider>().SingleInstance();

			_config.Args().Cache.This<ITokenVerifier>((c, b) => b.CacheMethod(x => x.TryGetUserAsync("")));
			builder.RegisterType<TokenVerifier>().As<ITokenVerifier>().SingleInstance();

			var assembly = typeof(Startup).Assembly;
			builder.RegisterApiControllers(assembly);
			builder.RegisterAssemblyTypes(assembly).AsClosedTypesOf(typeof(IQueryHandler<,>)).ApplyAspects();
			builder.RegisterAssemblyTypes(assembly).AsClosedTypesOf(typeof(ICommandHandler<>)).ApplyAspects();
		}
	}
}