using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon.Configuration;
using Toggle.Net;
using Toggle.Net.Configuration;
using Toggle.Net.Providers.TextFile;

namespace Teleopti.Ccc.IocCommon.Toggle
{
	public class ToggleNetModule : Module
	{
		public const string DeveloperLicenseName = "Teleopti_RD";
		public const string RcLicenseName = "Teleopti_RC";
		
		private readonly string _pathToToggle;
		
		public ToggleNetModule(string pathToToggle)
		{
			_pathToToggle = pathToToggle.Trim();
		}

		protected override void Load(ContainerBuilder builder)
		{
			if (togglePathIsAnUrl())
			{
				builder.Register(c => new ToggleQuerier(c.Resolve<ICurrentDataSource>(), _pathToToggle))
					.SingleInstance()
					.As<IToggleManager>()
					.As<IToggleFiller>();
			}
			else
			{
				builder.Register<IToggleManager>(c =>
				{
					if (togglePathIsNotDefined())
						return new FalseToggleManager();

					const string licenseActivatorReadingFromQuerystring = "querystring";
					var licenseActivatorProvider = c.IsRegisteredWithName<ILicenseActivatorProvider>(licenseActivatorReadingFromQuerystring) ?
								c.ResolveNamed<ILicenseActivatorProvider>(licenseActivatorReadingFromQuerystring) : 
								c.Resolve<ILicenseActivatorProvider>();
					var specMappings = new DefaultSpecificationMappings();
					specMappings.AddMapping("license", new LicenseSpecification(licenseActivatorProvider));
					var toggleConfiguration = new ToggleConfiguration(new FileProviderFactory(new FileReader(_pathToToggle), specMappings));
					toggleConfiguration.SetDefaultSpecification(new DefaultSpecification(licenseActivatorProvider));
					return new toggleCheckerWrapper(toggleConfiguration.Create());
				})
				.SingleInstance()
				.As<IToggleManager>();
			}

			builder.RegisterType<TogglesActive>()
				.SingleInstance().As<ITogglesActive>();
			builder.RegisterType<AllToggles>()
				.SingleInstance().As<IAllToggles>();
		}

		public static void RegisterDependingModules(ContainerBuilder builder)
		{
			//if using in scenarios where not all modules are registered
			builder.RegisterModule<UnitOfWorkModule>();
			builder.RegisterModule<AuthenticationModule>();
		}

		private bool togglePathIsAnUrl()
		{
			return _pathToToggle.StartsWith("http://") || _pathToToggle.StartsWith("https://");
		}

		private bool togglePathIsNotDefined()
		{
			return string.IsNullOrEmpty(_pathToToggle);
		}

		private class toggleCheckerWrapper : IToggleManager
		{
			private readonly IToggleChecker _toggleChecker;

			public toggleCheckerWrapper(IToggleChecker toggleChecker)
			{
				_toggleChecker = toggleChecker;
			}

			public bool IsEnabled(Toggles toggle)
			{
				return _toggleChecker.IsEnabled(toggle.ToString());
			}
		}
	}
}