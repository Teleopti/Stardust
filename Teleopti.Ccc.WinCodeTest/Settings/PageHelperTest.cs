using NUnit.Framework;
using Teleopti.Interfaces.Domain;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings;

namespace Teleopti.Ccc.WinCodeTest.Settings
{
    [TestFixture]
    public class PageHelperTest
    {
        IList<IScenario> scenarioList;
        const string newSccenarioText = "New Scenario";
        const string propertyName = "Description.Name";

        [SetUp]
        public void Init()
        {
			scenarioList = new List<IScenario>();
            scenarioList.Add(new Scenario("Work-aholic Scenario"));
            scenarioList.Add(new Scenario("Lazy-worker Scenario"));
            scenarioList.Add(new Scenario("New Scenario"));
            scenarioList.Add(new Scenario("<New Scenario>"));
            scenarioList.Add(new Scenario("Default But Not Scenario"));
        }

        [Test]
        public void VerifyDescriptionWhenEmptyOrNull()
        {
            string expected = string.Format(CultureInfo.InvariantCulture, "<{0}>", newSccenarioText); // <New Scenario>

            Assert.IsNotNull(scenarioList);

            scenarioList.Clear();
            Description description1 = PageHelper.CreateNewName(scenarioList, propertyName, newSccenarioText);
            Assert.AreEqual(expected, description1.Name); // <New Scenario>

            scenarioList = null;
            Description description2 = PageHelper.CreateNewName(scenarioList, propertyName, newSccenarioText);
            Assert.AreEqual(expected, description2.Name);
        }

        [Test]
        public void VerifyDescription1()
        {
            string expected1 = string.Format(CultureInfo.InvariantCulture, "<{0} {1}>", newSccenarioText, 1); // <New Scenario 1>

            Assert.IsNotNull(scenarioList);
            Description description1 = PageHelper.CreateNewName(scenarioList, propertyName, newSccenarioText);
            Assert.AreEqual(expected1, description1.Name); // <New Scenario 1>
        }

        [Test]
        public void VerifyDescription2()
        {
            string expected2 = string.Format(CultureInfo.InvariantCulture, "<{0} {1}>", newSccenarioText, 2); // <New Scenario 2>
            Assert.IsNotNull(scenarioList);

            Description description1 = PageHelper.CreateNewName(scenarioList, propertyName, newSccenarioText);
            scenarioList.Add(new Scenario(description1.Name));

            Description description2 = PageHelper.CreateNewName(scenarioList, propertyName, newSccenarioText);
            Assert.AreEqual(expected2, description2.Name); // <New Scenario 2>}
        }
    }
}
