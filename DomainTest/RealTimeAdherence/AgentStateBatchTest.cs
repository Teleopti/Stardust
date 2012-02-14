using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence
{
    [TestFixture]
    public class AgentStateBatchTest
    {
        private AgentStateBatch target;
        private readonly BatchIdentifier _batchIdentifier = new BatchIdentifier { BatchTimestamp = DateTime.UtcNow, DataSourceId = 1 };

        [SetUp]
        public void Setup()
        {
            target = new AgentStateBatch(_batchIdentifier);
        }

        [Test]
        public void VerifyCreateInstance()
        {
            Assert.IsNotNull(target);
        }

        [Test]
        public void VerifyCanGetBatchIdentifier()
        {
            Assert.AreEqual(_batchIdentifier,target.BatchIdentifier);
        }

        [Test]
        public void VerifyCanAddPerson()
        {
            MockRepository mocks = new MockRepository();
            IPerson person = mocks.StrictMock<IPerson>();
            mocks.ReplayAll();
            target.AddPerson(person);
            target.AddPerson(person);
            Assert.IsTrue(target.PersonCollection.Contains(person));
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanComparePersons()
        {
            MockRepository mocks = new MockRepository();
            IPerson person1 = mocks.StrictMock<IPerson>();
            IPerson person2 = mocks.StrictMock<IPerson>();

            Expect.Call(person1.Equals(person1)).Return(true).Repeat.Any();
            Expect.Call(person2.Equals(person1)).Return(false).Repeat.Any();
            Expect.Call(person1.Equals(person2)).Return(false).Repeat.Any();
            Expect.Call(person2.Equals(person2)).Return(true).Repeat.Any();

            mocks.ReplayAll();

            IAgentStateBatch oldBatch =
                new AgentStateBatch(new BatchIdentifier
                                        {
                                            BatchTimestamp = _batchIdentifier.BatchTimestamp.AddHours(-1),
                                            DataSourceId = _batchIdentifier.DataSourceId
                                        });
            oldBatch.AddPerson(person1);
            oldBatch.AddPerson(person2);

            target.AddPerson(person2);

            IEnumerable<IPerson> diffCollection = target.CompareWithPreviousBatch(oldBatch);
            Assert.AreEqual(1,diffCollection.Count());
            Assert.IsTrue(diffCollection.Contains(person1));

            mocks.VerifyAll();
        }
    }
}
