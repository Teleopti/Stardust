using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemCheck;

namespace Teleopti.Ccc.InfrastructureTest.SystemCheck
{
    [TestFixture]
    public class SystemCheckerValidatorTest
    {
        private SystemCheckerValidator target;
        private ISystemCheck systemCheck;
        private MockRepository mocks;

        [SetUp]
        public void Setup()
        {
            mocks =new MockRepository();
            systemCheck = mocks.StrictMock<ISystemCheck>();
            target = new SystemCheckerValidator(new List<ISystemCheck> { systemCheck });
        }

        [Test]
        public void VerifySystemCheckOk()
        {
            using(mocks.Record())
            {
                Expect.Call(systemCheck.IsRunningOk())
                    .Return(true);
            }
            using(mocks.Playback())
            {
                Assert.IsTrue(target.IsOk());
            }
            CollectionAssert.IsEmpty(target.Result);
        }

        [Test]
        public void VerifySystemCheckWarning()
        {
            const string warning = "gurka";
            using (mocks.Record())
            {
                Expect.Call(systemCheck.IsRunningOk())
                    .Return(false);
                Expect.Call(systemCheck.WarningText)
                    .Return(warning);
            }
            using (mocks.Playback())
            {
                Assert.IsFalse(target.IsOk());
            }
            Assert.AreEqual(1, target.Result.Count());
            Assert.AreEqual(warning, target.Result.First());
        }

        [Test]
        public void VerifyMultipleWarnings()
        {
            var systemCheck2 = mocks.StrictMock<ISystemCheck>();
            target = new SystemCheckerValidator(new List<ISystemCheck>{systemCheck,systemCheck2});
            const string warning1 = "gurka";
            const string warning2 = "banan";
            using (mocks.Record())
            {
                Expect.Call(systemCheck.IsRunningOk()).Return(false);
                Expect.Call(systemCheck.WarningText).Return(warning1);
                Expect.Call(systemCheck2.IsRunningOk()).Return(false);
                Expect.Call(systemCheck2.WarningText).Return(warning2);
            }
            using (mocks.Playback())
            {
                Assert.IsFalse(target.IsOk());
            }
            Assert.AreEqual(2, target.Result.Count());
            CollectionAssert.Contains(target.Result, "gurka");
            CollectionAssert.Contains(target.Result, "banan");
        }

        [Test]
        public void VerifyMultipleChecks()
        {
            const string warning = "gurka";
            using (mocks.Record())
            {
                Expect.Call(systemCheck.IsRunningOk())
                    .Return(false).Repeat.Any();
                Expect.Call(systemCheck.WarningText)
                    .Return(warning).Repeat.Any();
            }
            using (mocks.Playback())
            {
                Assert.IsFalse(target.IsOk());
                Assert.IsFalse(target.IsOk());
                Assert.IsFalse(target.IsOk());
            }
            Assert.AreEqual(1, target.Result.Count());
            Assert.AreEqual(warning, target.Result.First());
        }
    }
}