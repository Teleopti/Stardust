using System;
using Autofac;
using Teleopti.Ccc.Infrastructure.Aop;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public class SystemImpl : IIsolate, IExtend
	{
		private readonly ContainerBuilder _builder;
		private readonly TestDoubles _testDoubles;

		public SystemImpl(ContainerBuilder builder, TestDoubles testDoubles)
		{
			_builder = builder;
			_testDoubles = testDoubles;
		}

		public virtual ITestDoubleFor UseTestDouble<TTestDouble>() where TTestDouble : class
		{
			return new testDoubleFor<TTestDouble>(_builder, _testDoubles, null);
		}

		public virtual ITestDoubleFor UseTestDouble<TTestDouble>(TTestDouble instance) where TTestDouble : class
		{
			return new testDoubleFor<TTestDouble>(_builder, _testDoubles, instance);
		}

		public virtual ITestDoubleFor UseTestDoubleForType(Type type)
		{
			return new testDoubleFor<object>(_builder, _testDoubles, type);
		}

		public void AddService<TService>(bool instancePerLifeTimeScope = false)
		{
			if (instancePerLifeTimeScope)
			{
				_builder
					.RegisterType<TService>()
					.AsSelf()
					.AsImplementedInterfaces()
					.InstancePerLifetimeScope()
					.ApplyAspects();
			}
			else
			{
				_builder
					.RegisterType<TService>()
					.AsSelf()
					.AsImplementedInterfaces()
					.SingleInstance()
					.ApplyAspects();
			}
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

		private class testDoubleFor<TTestDouble> : ITestDoubleFor where TTestDouble : class
		{
			private readonly ContainerBuilder _builder;
			private readonly TestDoubles _testDoubles;
			private readonly Type _type;
			private readonly object _instance;

			public testDoubleFor(ContainerBuilder builder, TestDoubles testDoubles, object instance)
			{
				_builder = builder;
				_testDoubles = testDoubles;
				_instance = instance;
			}

			public testDoubleFor(ContainerBuilder builder, TestDoubles testDoubles, Type type)
			{
				_builder = builder;
				_testDoubles = testDoubles;
				_type = type;
			}

			public void For<T>()
			{
				register(typeof(T));
			}

			public void For<T1, T2>()
			{
				register(typeof(T1), typeof(T2));
			}

			public void For<T1, T2, T3>()
			{
				register(typeof(T1), typeof(T2), typeof(T3));
			}

			public void For<T1, T2, T3, T4>()
			{
				register(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
			}

			public void For<T1, T2, T3, T4, T5>()
			{
				register(typeof (T1), typeof (T2), typeof (T3), typeof (T4), typeof (T5));
			}

			public void For(Type type)
			{
				register(type);
			}

			private void register(params Type[] asTypes)
			{
				if (_instance == null)
				{
					if (_type != null)
					{
						var registration = _testDoubles.Register(b =>
						{
							b.RegisterType(_type)
								.SingleInstance()
								.AsSelf()
								.As(asTypes);
						}, null);
						_builder
							.RegisterType(_type)
							.SingleInstance()
							.AsSelf()
							.As(asTypes)
							.ExternallyOwned()
							.OnActivated(c =>
							{
								registration.Action = b =>
								{
									b.RegisterInstance(c.Instance)
										.SingleInstance()
										.AsSelf()
										.As(asTypes);
								};
								registration.Instance = c.Instance;
							});
					}
					else
					{
						var registration = _testDoubles.Register(b =>
						{
							b.RegisterType<TTestDouble>()
								.SingleInstance()
								.AsSelf()
								.As(asTypes);
						}, null);
						_builder
							.RegisterType<TTestDouble>()
							.SingleInstance()
							.AsSelf()
							.As(asTypes)
							.ExternallyOwned()
							.OnActivated(c =>
							{
								registration.Action = b =>
								{
									b.RegisterInstance(c.Instance)
										.SingleInstance()
										.AsSelf()
										.As(asTypes);
								};
								registration.Instance = c.Instance;
							});
					}
				}
				else
				{
					_testDoubles.Register(b =>
					{
						b.RegisterInstance(_instance)
							.SingleInstance()
							.AsSelf()
							.As(asTypes);
					}, _instance);
					_builder
						.RegisterInstance(_instance)
						.SingleInstance()
						.AsSelf()
						.As(asTypes)
						.ExternallyOwned()
						;
				}
			}
		}
	}
}