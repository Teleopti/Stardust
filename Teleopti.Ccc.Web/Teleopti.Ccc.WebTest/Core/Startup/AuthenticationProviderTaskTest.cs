using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Teleopti.Ccc.Web.Core.Startup.Booter;

namespace Teleopti.Ccc.WebTest.Core.Startup
{
    [TestFixture]
    public class AuthenticationProviderTaskTest
    {
        private AuthenticationProviderTask _target;

        [Test]
        public void ShouldRegisterProvidersFromConfigFile()
        {
            _target=new AuthenticationProviderTask();
        }
    }

    public class AuthenticationProviderTask : IBootstrapperTask
    {
        public Task Execute()
        {
            throw new NotImplementedException();
        }
    }
}
