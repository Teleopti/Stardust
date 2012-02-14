using System;
using Autofac;
using NUnit.Framework;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
    public class UnitOfWorkModuleTest
    {
        private ContainerBuilder containerBuilder;

        [SetUp]
        public void Setup()
        {
            containerBuilder = new ContainerBuilder();
        }


        [Test]
        public void AllRepositoriesAreWired()
        {
            containerBuilder.RegisterModule(new UnitOfWorkModule());
            using (var container = containerBuilder.Build())
            {
                var unitOfWorkFactory = container.Resolve<IUnitOfWorkFactory>();
                Assert.IsInstanceOf<IUnitOfWorkFactory>(unitOfWorkFactory);

                ShouldBeNotImplemented(unitOfWorkFactory);
            }
        }

        private static void ShouldBeNotImplemented(IUnitOfWorkFactory unitOfWorkFactory)
        {
            try
            {
                unitOfWorkFactory.Close();
                Assert.Fail();
            }
            catch (NotImplementedException)
            {
            }
            try
            {
                unitOfWorkFactory.Close();
                Assert.Fail();
            }
            catch (NotImplementedException)
            {
            }
            try
            {
                unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork();
                Assert.Fail();
            }
            catch (NotImplementedException)
            {
            }
            try
            {
                unitOfWorkFactory.CreateAndOpenUnitOfWork();
                Assert.Fail();
            }
            catch (NotImplementedException)
            {
            }
            try
            {
                unitOfWorkFactory.CurrentUnitOfWork();
                Assert.Fail();
            }
            catch (NotImplementedException)
            {
            }
            try
            {
                var name = unitOfWorkFactory.Name;
                Assert.Fail();
            }
            catch (NotImplementedException)
            {
            }
            try
            {
                var number = unitOfWorkFactory.NumberOfLiveUnitOfWorks;
                Assert.Fail();
            }
            catch (NotImplementedException)
            {
            }
            try
            {
                unitOfWorkFactory.Dispose();
                Assert.Fail();
            }
            catch (NotImplementedException)
            {
            }
        }
    }
}