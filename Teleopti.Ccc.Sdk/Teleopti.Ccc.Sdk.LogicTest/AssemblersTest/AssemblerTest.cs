using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Sdk.Logic.Assemblers;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class AssemblerTest
    {
        private testForCollectionMethods target;

        [SetUp]
        public void Setup()
        {
            target = new testForCollectionMethods();
        }

        [Test]
        public void VerifyDtosToDomainEntities()
        {
            IList<int> res = new List<int>(target.DtosToDomainEntities(new[] {1, 2}));
            CollectionAssert.AreEqual(new[] {2, 3}, res);
        }

        [Test]
        public void VerifyDomainEntitiesToDtos()
        {
            IList<int> res = new List<int>(target.DomainEntitiesToDtos(new[] { 1, 2 }));
            CollectionAssert.AreEqual(new[] { 0, 1 }, res);
        }

        private class testForCollectionMethods : Assembler<int, int>
        {
            public override int DomainEntityToDto(int entity)
            {
                return entity-1;
            }

            public override int DtoToDomainEntity(int dto)
            {
                return dto+1;
            }
        }
    }
}