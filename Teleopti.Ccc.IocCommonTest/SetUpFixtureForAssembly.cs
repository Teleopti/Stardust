using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommonTest
{
    [SetUpFixture]
    public class SetupFixtureForAssembly
    {

        [SetUp]
        public void RunBeforeAnyTest()
        {
            var mocks = new MockRepository();
            var stateMock = mocks.StrictMock<IState>();

            var ds = mocks.StrictMock<IDataSource>();
            var uowFactory = mocks.StrictMock<IUnitOfWorkFactory>();
            StateHolderProxyHelper.ClearAndSetStateHolder(mocks, ds, stateMock);

            Expect.Call(ds.Application)
                .Return(uowFactory)
                .Repeat.Any();

            mocks.ReplayAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), TearDown]
        public void Teardown()
        {
            StateHolderProxyHelper.ClearStateHolder();
        }
    }
}