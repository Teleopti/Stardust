using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Autofac;
using Autofac.Core;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.IocCommon;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public abstract class IocRegistrationTest
	{
		private IocConfiguration _iocConfig;
		private ContainerBuilder _builder;
		private IContainer _container;
		
		protected IContainer InitContainer(string toggleMode)
		{
			_builder = new ContainerBuilder();
			var iocArgs = new IocArgs(new FakeConfigReader());
			var currentAssemblyPath = new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath;
			var currentDirectory = new FileInfo(currentAssemblyPath).Directory?.FullName;
			if (currentDirectory == null)
				throw new InvalidOperationException("Current directory is not");
					
			iocArgs.FeatureToggle = Path.Combine(currentDirectory, "FeatureFlags/toggles.txt");
			iocArgs.ToggleMode = toggleMode;
			var toggleManager = CommonModule.ToggleManagerForIoc(iocArgs);
			_iocConfig = new IocConfiguration(iocArgs, toggleManager);
			DefineModules(_iocConfig).ForEach(m => _builder.RegisterModule(m));
			_container = _builder.Build();
			return _container;
		}
		
		[TearDown]
		public void Teardown()
		{
			if (_container != null)
			{
				_container.Dispose();
				_container = null;
			}
		}

		protected abstract IEnumerable<IModule>  DefineModules(IocConfiguration iocConfiguration);
	}
}