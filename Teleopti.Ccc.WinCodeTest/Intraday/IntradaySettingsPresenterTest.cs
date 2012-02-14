using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSettings;
using Teleopti.Ccc.WinCode.Intraday;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Intraday
{
    [TestFixture]
    public class IntradaySettingsPresenterTest
    {
        private IntradaySettingsPresenter _target;

        [SetUp]
        public void Setup()
        {
            _target = new IntradaySettingsPresenter();
        }

        [Test]
        public void VerifyDefaultConstructor()
        {
            _target = new IntradaySettingsPresenter();
            Assert.IsNotNull(_target);
        }
    }
}
