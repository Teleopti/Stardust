using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
    /// <summary>
    /// Tests for stateholder
    /// </summary>
    [TestFixture]
    [Category("LongRunning")]
    public class StateHolderTest
    {
        private MockRepository mocks;
        private IState stateMock;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            stateMock = mocks.StrictMock<IState>();
            StateHolderProxyHelper.ClearStateHolder();
        }

        /// <summary>
        /// Verifies the IState parameter is not null.
        /// </summary>
        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void VerifyIStateParameterIsNotNull()
        {
            StateHolder.Initialize(null);
        }

        /// <summary>
        /// Verifies the State property is set.
        /// </summary>
        [Test]
        public void VerifyStatePropertyIsSet()
        {
            StateHolder.Initialize(stateMock);
            Assert.IsNotNull(StateHolder.Instance.StateReader);
        }

        /// <summary>
        /// Verifies the state reader in domain has been set.
        /// </summary>
        [Test]
        public void VerifyStateReaderInDomainHasBeenSet()
        {
            StateHolder.Initialize(stateMock);
            Assert.IsNotNull(StateHolderReader.Instance.StateReader);
        }

        /// <summary>
        /// Verifies initialized only allowed to be called once.
        /// </summary>
        [Test]
        [ExpectedException(typeof (StateHolderException))]
        public void VerifyInitializedOnlyCalledOnce()
        {
            StateHolder.Initialize(stateMock);
            StateHolder.Initialize(stateMock);
        }

        /// <summary>
        /// Verifies the isinitialized property.
        /// </summary>
        [Test]
        public void VerifyIsInitialized()
        {
            Assert.IsFalse(StateHolder.IsInitialized);
            Expect.Call(stateMock.ApplicationScopeData)
                .Return(mocks.StrictMock<IApplicationData>())
                .Repeat.Any();
            mocks.ReplayAll();
            StateHolder.Initialize(stateMock);
            Assert.IsTrue(StateHolder.IsInitialized);
        }

        /// <summary>
        /// Verifies the singleton is initialized before called.
        /// </summary>
        [Test]
        [ExpectedException(typeof (InvalidOperationException))]
        public void VerifySingletonInitializedBeforeCalled()
        {
            StateHolder foo = StateHolder.Instance;
        }

        [Test]
        public void VerifyTerminate()
        {
            StateHolder.Initialize(stateMock);
            IApplicationData appData = mocks.StrictMock<IApplicationData>();

            using(mocks.Record())
            {
                Expect.Call(stateMock.ApplicationScopeData)
                    .Return(appData).Repeat.Any();

                appData.Dispose();
            }
            using(mocks.Playback())
            {
                StateHolder.Instance.Terminate();
            }
        }
    }
}