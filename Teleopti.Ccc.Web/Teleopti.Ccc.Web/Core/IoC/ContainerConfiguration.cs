using System;
using System.Collections.Generic;
using System.Web.Http;
using Autofac;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Core.Resolving;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.IocCommon;

namespace Teleopti.Ccc.Web.Core.IoC
{
	public class ContainerConfiguration : IContainerConfiguration
	{
		public ILifetimeScope Configure(string featureTogglePath, HttpConfiguration httpConfiguration)
		{
			var builder = new ContainerBuilder();

			var args = new IocArgs(new ConfigOverrider(new ConfigReader(), new Dictionary<string, string> {{"FCM", "key=AAAANvMkWNA:APA91bG1pR8ZVsp-S98uWsFUE5lnQiC8UnsQL3DgN6Vyw5HyaKuqVt86kdeurfLfQkWt_7kZTgXcTuAaxvcVUkjtE8jFo72loTy6UYrLrVbYnqCXVI4mWCYhvLQnU3Sv0sIfW1k-eZCu"}}))
			{
				FeatureToggle = featureTogglePath,
				DataSourceApplicationName = DataSourceApplicationName.ForWeb()
			};
			var configuration = new IocConfiguration(args, CommonModule.ToggleManagerForIoc(args));
			builder.RegisterModule(new WebAppModule(configuration, httpConfiguration));

			return new preventDisposalOfContainer(builder.Build());
		}

		// Experimental fix for 48412, and exceptions like it
		// Read more about it here:
		// http://challenger:8080/tfs/web/UI/Pages/WorkItems/WorkItemEdit.aspx?id=48412
		private class preventDisposalOfContainer : ILifetimeScope
		{
			private readonly IContainer _container;

			public preventDisposalOfContainer(IContainer container)
			{
				_container = container;
			}

			public void Dispose()
			{
			}

			public object ResolveComponent(IComponentRegistration registration, IEnumerable<Parameter> parameters) => _container.ResolveComponent(registration, parameters);
			public IComponentRegistry ComponentRegistry => _container.ComponentRegistry;
			public ILifetimeScope BeginLifetimeScope() => _container.BeginLifetimeScope();
			public ILifetimeScope BeginLifetimeScope(object tag) => _container.BeginLifetimeScope(tag);
			public ILifetimeScope BeginLifetimeScope(Action<ContainerBuilder> configurationAction) => _container.BeginLifetimeScope(configurationAction);
			public ILifetimeScope BeginLifetimeScope(object tag, Action<ContainerBuilder> configurationAction) => _container.BeginLifetimeScope(tag, configurationAction);
			public IDisposer Disposer => _container.Disposer;
			public object Tag => _container.Tag;

			public event EventHandler<LifetimeScopeBeginningEventArgs> ChildLifetimeScopeBeginning
			{
				add => _container.ChildLifetimeScopeBeginning += value;
				remove => _container.ChildLifetimeScopeBeginning -= value;
			}

			public event EventHandler<LifetimeScopeEndingEventArgs> CurrentScopeEnding
			{
				add => _container.CurrentScopeEnding += value;
				remove => _container.CurrentScopeEnding -= value;
			}

			public event EventHandler<ResolveOperationBeginningEventArgs> ResolveOperationBeginning
			{
				add => _container.ResolveOperationBeginning += value;
				remove => _container.ResolveOperationBeginning -= value;
			}
		}
	}
}