using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    ///<summary>
    /// Tests UserDetailRepository
    ///</summary>
    [TestFixture]
    [Category("LongRunning")]
    public class ExtendedPreferenceTemplateRepositoryTest : RepositoryTest<IExtendedPreferenceTemplate>
    {
        private IPerson person;

        protected override void ConcreteSetup()
        {
            person = PersonFactory.CreatePerson();
            PersistAndRemoveFromUnitOfWork(person);
        }

        /// <summary>
        /// Creates an aggregate using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IExtendedPreferenceTemplate CreateAggregateWithCorrectBusinessUnit()
        {
            IExtendedPreferenceTemplate extendedPreferenceTemplate = new ExtendedPreferenceTemplate(person, new PreferenceRestrictionTemplate(), "Test", Color.DimGray);
            return extendedPreferenceTemplate;
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(IExtendedPreferenceTemplate loadedAggregateFromDatabase)
        {
            IExtendedPreferenceTemplate org = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(org.Person, loadedAggregateFromDatabase.Person);
            Assert.AreEqual(org.DisplayColor.ToArgb(), loadedAggregateFromDatabase.DisplayColor.ToArgb());
            Assert.AreEqual(org.Name, loadedAggregateFromDatabase.Name);
        }

		[Test]
		public void VerifyCanFindById()
		{
			IExtendedPreferenceTemplate extendedPreferenceTemplate = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(extendedPreferenceTemplate);

			IExtendedPreferenceTemplate foundTemplate = new ExtendedPreferenceTemplateRepository(UnitOfWork).Find(extendedPreferenceTemplate.Id.Value);
			Assert.AreEqual("Test", foundTemplate.Name);
		}

		[Test]
		public void ReturnNullIfNotFind()
		{
			IExtendedPreferenceTemplate extendedPreferenceTemplate = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(extendedPreferenceTemplate);

			IExtendedPreferenceTemplate foundTemplate = new ExtendedPreferenceTemplateRepository(UnitOfWork).Find(Guid.NewGuid());
			Assert.IsNull(foundTemplate);
		}

        [Test]
        public void VerifyCanFindByPerson()
        {
            IExtendedPreferenceTemplate extendedPreferenceTemplate = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(extendedPreferenceTemplate);

            IList<IExtendedPreferenceTemplate> foundTemplates = new ExtendedPreferenceTemplateRepository(UnitOfWork).FindByUser(person);
            Assert.AreEqual(1, foundTemplates.Count);
        }

        [Test]
        public void VerifyCanFindByUserIsSorted()
        {
            IExtendedPreferenceTemplate temp1 = new ExtendedPreferenceTemplate(person, new PreferenceRestrictionTemplate(), "Xerxes", Color.Blue);
            IExtendedPreferenceTemplate temp2 = new ExtendedPreferenceTemplate(person, new PreferenceRestrictionTemplate(), "Anders", Color.Blue);
            IExtendedPreferenceTemplate temp3 = new ExtendedPreferenceTemplate(person, new PreferenceRestrictionTemplate(), "Tommys", Color.Blue);
            IExtendedPreferenceTemplate temp4 = new ExtendedPreferenceTemplate(person, new PreferenceRestrictionTemplate(), "zalg", Color.Blue);
            PersistAndRemoveFromUnitOfWork(temp1);
            PersistAndRemoveFromUnitOfWork(temp2);
            PersistAndRemoveFromUnitOfWork(temp3);
            PersistAndRemoveFromUnitOfWork(temp4);

            IList<IExtendedPreferenceTemplate> foundTemplates = new ExtendedPreferenceTemplateRepository(UnitOfWork).FindByUser(person);
            Assert.AreEqual(4, foundTemplates.Count);
            Assert.AreEqual("Anders", foundTemplates[0].Name);
            Assert.AreEqual("Tommys", foundTemplates[1].Name);
            Assert.AreEqual("Xerxes", foundTemplates[2].Name);
            Assert.AreEqual("zalg", foundTemplates[3].Name);
        }

        protected override Repository<IExtendedPreferenceTemplate> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new ExtendedPreferenceTemplateRepository(currentUnitOfWork);
        }
    }
}
