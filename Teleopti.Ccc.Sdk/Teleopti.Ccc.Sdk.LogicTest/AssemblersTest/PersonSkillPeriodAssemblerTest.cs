using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class PersonSkillPeriodAssemblerTest
    {
        private PersonSkillPeriodAssembler _target;
        private IPerson _person;
        private ReadOnlyCollection<IPersonPeriod> _personPeriodCollection;
        private IPersonPeriod _period;
        private ISkill _skill;

        [SetUp]
        public void Setup()
        {
            _target = new PersonSkillPeriodAssembler();
        }

        /// <summary>
        /// Creates the person period collection.
        /// </summary>
        /// <param name="mocks">The mocks.</param>
        /// <param name="dateTimeUtc">The date time UTC.</param>
        /// <param name="dateOnly">The date only.</param>
        /// <param name="doPeriod">The DateOnlyPeriod.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-12-10
        /// </remarks>
        private void CreatePersonPeriodCollection(MockRepository mocks, out DateTime dateTimeUtc, out DateOnly dateOnly, out DateOnlyPeriod doPeriod)
        {
            IList<IPersonPeriod> personPeriodCollection = new List<IPersonPeriod>();

            _period = mocks.StrictMock<IPersonPeriod>();
            dateOnly = new DateOnly(2005, 1, 1);
            dateTimeUtc = new DateTime(2005, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            doPeriod = new DateOnlyPeriod(dateOnly, new DateOnly(2005, 2, 1));

            _skill = SkillFactory.CreateSkill("test");
            _skill.SetId(Guid.NewGuid());

            Expect.Call(_period.PersonSkillCollection).Return(new List<IPersonSkill>
                                                                  {PersonSkillFactory.CreatePersonSkill(_skill, 1)});
            _person.AddPersonPeriod(_period);
            personPeriodCollection.Add(_period);
            _personPeriodCollection = new ReadOnlyCollection<IPersonPeriod>(personPeriodCollection);
        }

        [Test]
        public void VerifyDomainEntityToDto()
        {
            MockRepository mocks = new MockRepository();
            _person = mocks.StrictMock<IPerson>();

            DateTime dateTimeUtc;
            DateOnly dateOnly;
            DateOnlyPeriod doPeriod;
            CreatePersonPeriodCollection(mocks, out dateTimeUtc, out dateOnly, out doPeriod);

            using (mocks.Record())
            {
                Expect.Call(_person.Id)
                    .Return(Guid.NewGuid())
                    .Repeat.Any();

                Expect.Call(_period.Id)
                    .Return(Guid.NewGuid())
                    .Repeat.Any();

                Expect.Call(_person.Name)
                    .Return(new Name("aaa", "bbb"))
                    .Repeat.Any();

                Expect.Call(_person.Email)
                    .Return("mail")
                    .Repeat.Any();

                Expect.Call(_person.EmploymentNumber)
                    .Return("mail")
                    .Repeat.Any();

                Expect.Call(_period.Parent)
                    .Return(_person)
                    .Repeat.AtLeastOnce();

                Expect.Call(_period.StartDate)
                    .Return(dateOnly)
                    .Repeat.AtLeastOnce();

                Expect.Call(_period.EndDate())
                    .Return(dateOnly)
                    .Repeat.AtLeastOnce();

                Expect.Call(_person.PersonPeriodCollection)
                    .Return(_personPeriodCollection)
                    .Repeat.AtLeastOnce();
            }

            mocks.ReplayAll();

            var personDto = _target.DomainEntityToDto(_period);
            Assert.AreEqual(_period.Id, personDto.Id);
            Assert.AreEqual(_skill.Id,personDto.SkillCollection[0]);
        }

        [Test, ExpectedException(typeof(NotSupportedException))]
        public void VerifyDtoToDomainEntity()
        {
            _target.DtoToDomainEntity(new PersonSkillPeriodDto());
        }
    }
}
