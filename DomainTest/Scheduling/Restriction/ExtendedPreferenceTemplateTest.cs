using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.DomainTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Restriction
{
    [TestFixture]
    public class ExtendedPreferenceTemplateTest
    {
        private ExtendedPreferenceTemplate target;
        private const string name = "ExtendedPreferenceTemplate";
        private readonly Color color = Color.ForestGreen;
        private IPerson person;
        private MockRepository mocks;
        private IPreferenceRestrictionTemplate preferenceRestriction;
        private IActivity activity;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            person = mocks.StrictMock<IPerson>();
            activity = mocks.StrictMock<IActivity>();
            preferenceRestriction = new PreferenceRestrictionTemplate();
            preferenceRestriction.AddActivityRestriction(new ActivityRestrictionTemplate(activity));
            target = new ExtendedPreferenceTemplate(person, preferenceRestriction, name, color);
        }

        /// <summary>
        /// Verifies that properties are set correctly
        /// </summary>
        [Test]
        public void CanSetProperties()
        {
            Assert.AreEqual(preferenceRestriction,target.Restriction);
            Assert.AreEqual(target,target.Restriction.Parent);
            Assert.AreEqual(name, target.Name);
            Assert.AreEqual(color, target.DisplayColor);
            Assert.AreEqual(person,target.Person);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyActivityRestrictionTemplateHasDefaultConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(typeof(ActivityRestrictionTemplate),true));
        }
    }
}