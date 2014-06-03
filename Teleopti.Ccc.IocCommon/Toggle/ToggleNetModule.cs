using System;
using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Foundation;
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
			builder.Register<IToggleManager>(c =>
			{
				if (_pathToToggle.StartsWith("http://") || _pathToToggle.StartsWith("https://"))
				{
					return new ToggleQuerier(_pathToToggle);
				}
				var ctx = c.Resolve<IComponentContext>();
				var licenseActivator = new Func<ILicenseActivator>(ctx.Resolve<ILicenseActivator>);
				var specMappings = new DefaultSpecificationMappings();
				specMappings.AddMapping("license", new LicenseSpecification(licenseActivator));
				var toggleConfiguration = new ToggleConfiguration(new FileProviderFactory(new FileReader(_pathToToggle), specMappings));
				toggleConfiguration.SetDefaultSpecification(new DefaultSpecification(licenseActivator));
				return new toggleCheckerWrapper(toggleConfiguration.Create());
			})
				.SingleInstance()
				.As<IToggleManager>();

			builder.RegisterType<TogglesActive>()
				.SingleInstance().As<ITogglesActive>();
			builder.RegisterType<AllToggles>()
				.SingleInstance().As<IAllToggles>();
			builder.Register(c =>
			{
				if (DefinedLicenseDataFactory.LicenseActivator == null)
					throw new DataSourceException("Missing datasource (no *.hbm.xml file available)!");
				return DefinedLicenseDataFactory.LicenseActivator;
			})
			.SingleInstance()
			.As<ILicenseActivator>();
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