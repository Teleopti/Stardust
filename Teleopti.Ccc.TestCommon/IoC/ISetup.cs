using Autofac;
using Autofac.Builder;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.IocCommon;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public interface ISetup
	{
		void Setup(ISystem system, IIocConfiguration configuration);
	};

	public interface ISystem
	{
		ITestDoubleFor UseTestDouble<TTestDouble>();
		ITestDoubleFor UseTestDouble<TTestDouble>(TTestDouble instance);

		void AddService<TService>();
		void AddService<TService>(TService instance) where TService : class;

		void AddModule(Module module);
	}

	public interface ITestDoubleFor
	{
		void For<T>();
		void For<T1, T2>();
		void For<T1, T2, T3>();
		void For<T1, T2, T3, T4, T5, T6, T7>();
	}

	public class ContainerBuilderWrapper : ISystem
	{
		private readonly ContainerBuilder _builder;

		public ContainerBuilderWrapper(ContainerBuilder builder)
		{
			_builder = builder;
		}

		public ITestDoubleFor UseTestDouble<TTestDouble>()
		{
			return new TestDoubleFor<TTestDouble>(_builder, null);
		}

		public ITestDoubleFor UseTestDouble<TTestDouble>(TTestDouble instance)
		{
			return new TestDoubleFor<TTestDouble>(_builder, instance);
		}

		public void AddService<TService>()
		{
			_builder
				.RegisterType<TService>()
				.AsSelf()
				.AsImplementedInterfaces()
				.SingleInstance()
				.ApplyAspects();
		}

		public void AddService<TService>(TService instance) where TService : class
		{
			_builder
				.RegisterInstance(instance)
				.AsSelf()
				.AsImplementedInterfaces()
				.SingleInstance();
		}

		public void AddModule(Module module)
		{
			_builder.RegisterModule(module);
		}
	}

	public class TestDoubleFor<TTestDouble> : ITestDoubleFor
	{
		private readonly ContainerBuilder _builder;
		private readonly object _instance;

		public TestDoubleFor(ContainerBuilder builder, object instance)
		{
			_builder = builder;
			_instance = instance;
		}

		private IRegistrationBuilder<TTestDouble, ConcreteReflectionActivatorData, SingleRegistrationStyle> registerType()
		{
			return
				_builder
				.RegisterType<TTestDouble>()
				.AsSelf()
				.SingleInstance();
		}

		private IRegistrationBuilder<object, SimpleActivatorData, SingleRegistrationStyle> registerInstance()
		{
			return _builder
				.RegisterInstance(_instance)
				.AsSelf()
				.SingleInstance();
		}

		public void For<T>()
		{
			if (_instance == null)
				registerType().As<T>();
			else
				registerInstance().As<T>();
		}

		public void For<T1, T2>()
		{
			if (_instance == null)
				registerType().As<T1>().As<T2>();
			else
				registerInstance().As<T1>().As<T2>();
		}

		public void For<T1, T2, T3>()
		{
			if (_instance == null)
				registerType().As<T1>().As<T2>().As<T3>();
			else
				registerInstance().As<T1>().As<T2>().As<T3>();
		}

		public void For<T1, T2, T3, T4, T5, T6, T7>()
		{
			if (_instance == null)
				registerType().As<T1>().As<T2>().As<T3>().As<T4>().As<T5>().As<T6>().As<T7>();
			else
				registerInstance().As<T1>().As<T2>().As<T3>().As<T4>().As<T5>().As<T6>().As<T7>();
		}
	}

}