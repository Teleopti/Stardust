using System;
using System.Linq;
using System.Reflection;
using Autofac;
using NUnit.Framework;
using NUnit.Framework.Interfaces;


namespace ManagerTest.Attributes
{
	public class BaseTestsAttribute : Attribute, ITestAction
	{
		private IContainer _container;
		private object _fixture;
		private Type _fixtureType;

		public void BeforeTest(ITest test)
		{
			_fixture = test.Fixture;
			_fixtureType = _fixture.GetType();

			buildContainer();
			injectMembers();
		}

		public void AfterTest(ITest test)
		{
		}

		public ActionTargets Targets => ActionTargets.Test;

		private void injectMembers()
		{
			injectMembers(GetType(), this);
			injectMembers(_fixtureType, _fixture);
		}

		private void injectMembers(IReflect type, object instance)
		{
			var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(x => x.CanWrite);
			foreach (var propertyInfo in properties)
			{
				propertyInfo.SetValue(instance, _container.Resolve(propertyInfo.PropertyType), null);
			}
			var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
			foreach (var fieldsInfo in fields)
			{
				fieldsInfo.SetValue(instance, _container.Resolve(fieldsInfo.FieldType));
			}
		}

		private void buildContainer()
		{
			var builder = new ContainerBuilder();
			SetUp(builder);
			_container = builder.Build();
		}

		protected virtual void SetUp(ContainerBuilder builder)
		{
		}
	}
}