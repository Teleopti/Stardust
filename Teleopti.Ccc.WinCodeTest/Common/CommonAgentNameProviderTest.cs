using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class CommonAgentNameProviderTest
    {
        private MockRepository _mocks;
        private ICommonAgentNameProvider _target;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IRepositoryFactory _repositoryFactory;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
            _target = new CommonAgentNameProvider(_unitOfWorkFactory, _repositoryFactory );   
        }

        [Test]
        public void ShouldGetSettingFromRepositoryOnetime()
        {
            var uow = _mocks.StrictMock<IUnitOfWork>();
            var rep = _mocks.StrictMock<ISettingDataRepository>();
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
            Expect.Call(_repositoryFactory.CreateGlobalSettingDataRepository(uow)).Return(rep);
            Expect.Call(rep.FindValueByKey("CommonNameDescription", new CommonNameDescriptionSetting())).Return(
                new CommonNameDescriptionSetting()).IgnoreArguments();
            Expect.Call(uow.Dispose);
            _mocks.ReplayAll();
            var sett1 = _target.CommonAgentNameSettings;
            var sett2 = _target.CommonAgentNameSettings;
            Assert.That(sett1,Is.EqualTo(sett2));
            _mocks.VerifyAll();
        }
    }


}