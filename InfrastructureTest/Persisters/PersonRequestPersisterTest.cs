using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters
{
    [TestFixture]
    public class PersonRequestPersisterTest
    {
        private MockRepository _mocks;
        private IPersonRequestPersister _target;
        private IClearReferredShiftTradeRequests _clearReferredShiftTradeRequests;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _clearReferredShiftTradeRequests = _mocks.StrictMock<IClearReferredShiftTradeRequests>();
            _target = new PersonRequestPersister(_clearReferredShiftTradeRequests);
        }

        [Test]
        public void ShouldPersist()
        {
            var personRequestRepository = _mocks.StrictMock<IPersonRequestRepository>();
            var personRequest1 = _mocks.StrictMock<IPersonRequest>();
            var personRequest2 = _mocks.StrictMock<IPersonRequest>();
            var personRequests = new List<IPersonRequest> {personRequest1, personRequest2};

            _mocks.Record();

            Expect.Call(personRequest1.Changed).Return(true);
            Expect.Call(() => personRequestRepository.Add(personRequest1));
            Expect.Call(personRequest2.Changed).Return(false);
            Expect.Call(personRequest1.Persisted);
            Expect.Call(personRequest2.Persisted);
            Expect.Call(_clearReferredShiftTradeRequests.ClearReferredShiftTradeRequests);

            _mocks.ReplayAll();

            _target.MarkForPersist(personRequestRepository, personRequests);

            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldNotPersistIfNotPermitted()
        {
            _mocks.ReplayAll();
            using(new CustomAuthorizationContext(new PrincipalAuthorizationWithNoPermission()))
            {
                _target.MarkForPersist(null, null);
            }
            _mocks.VerifyAll();
        }
    }
}