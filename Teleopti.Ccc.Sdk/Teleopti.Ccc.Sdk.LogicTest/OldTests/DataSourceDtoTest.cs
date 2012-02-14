using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class DataSourceDtoTest
    {
        private DataSourceDto target;
        private MockRepository mocks;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            IDataSource dataSource = mocks.StrictMock<IDataSource>();
            IUnitOfWorkFactory unitOfWorkFactory = mocks.StrictMock<IUnitOfWorkFactory>();

            Expect.Call(dataSource.Application).Return(unitOfWorkFactory).Repeat.AtLeastOnce();
            Expect.Call(unitOfWorkFactory.Name).Return("name").Repeat.AtLeastOnce();

            mocks.ReplayAll();
            target = new DataSourceDto(dataSource, AuthenticationTypeOptionDto.Application);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual("name",target.Name);
            Assert.AreEqual(AuthenticationTypeOptionDto.Application,target.AuthenticationTypeOptionDto);
            mocks.VerifyAll();
        }
    }
}