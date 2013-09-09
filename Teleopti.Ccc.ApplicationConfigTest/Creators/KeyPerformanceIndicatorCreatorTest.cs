using System.Drawing;
using NHibernate;
using NUnit.Framework;
using Teleopti.Ccc.ApplicationConfig.Creators;
using Teleopti.Ccc.Domain.Kpi;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfigTest.Creators
{
    [TestFixture]
    [Category("LongRunning")]
    public class KeyPerformanceIndicatorTest
    {
        private KeyPerformanceIndicatorCreator _target;
        private ISessionFactory _sessionFactory; 
        private IPerson _person;

        [SetUp]
        public void Setup()
        {
            _person = SetupFixtureForAssembly.Person;
            _sessionFactory = SetupFixtureForAssembly.SessionFactory;
            _target = new KeyPerformanceIndicatorCreator(_person, _sessionFactory);
        }

        [Test]
        public void VerifyCanCreateKeyPerformanceIndicator()
        {
            KeyPerformanceIndicator keyPerformanceIndicator = _target.Create("Very importent KPI",
                                                                                           "TheKPI",
                                                                                           EnumTargetValueType.
                                                                                               TargetValueTypeNumber,
                                                                                           80,
                                                                                           70,
                                                                                           90,
                                                                                           Color.DodgerBlue,
                                                                                           Color.DarkSlateGray,
                                                                                           Color.DimGray);

            Assert.IsNotNull(keyPerformanceIndicator);
            Assert.AreEqual("Very importent KPI", keyPerformanceIndicator.Name);
            Assert.AreEqual("TheKPI", keyPerformanceIndicator.ResourceKey);
            Assert.AreEqual(80, keyPerformanceIndicator.DefaultTargetValue);
            Assert.AreEqual(70, keyPerformanceIndicator.DefaultMinValue);
            Assert.AreEqual(90, keyPerformanceIndicator.DefaultMaxValue);
            Assert.AreEqual(Color.DodgerBlue, keyPerformanceIndicator.DefaultBetweenColor);
            Assert.AreEqual(Color.DimGray, keyPerformanceIndicator.DefaultHigherThanMaxColor);
            Assert.AreEqual(Color.DarkSlateGray, keyPerformanceIndicator.DefaultLowerThanMinColor);
        }

        [Test]
        public void VerifyCanSaveKeyPerformanceIndicator()
        {
            KeyPerformanceIndicator keyPerformanceIndicator = _target.Create("Very importent KPI",
                                                                               "TheKPI",
                                                                               EnumTargetValueType.
                                                                                   TargetValueTypeNumber,
                                                                               80,
                                                                               70,
                                                                               90,
                                                                               Color.DodgerBlue,
                                                                               Color.DarkSlateGray,
                                                                               Color.DimGray);
            Assert.IsTrue(_target.Save(keyPerformanceIndicator), "Kpi Saved");
        }

        [Test]
        public void VerifyCanFetchKpi()
        {          
            KeyPerformanceIndicator keyPerformanceIndicator = _target.Create("Very importent KPI",
                                                                               "KpiAdherenceNew",
                                                                               EnumTargetValueType.
                                                                                   TargetValueTypeNumber,
                                                                               80,
                                                                               70,
                                                                               90,
                                                                               Color.DodgerBlue,
                                                                               Color.DarkSlateGray,
                                                                               Color.DimGray);
            Assert.IsTrue(_target.Save(keyPerformanceIndicator), "Kpi Saved");

            KeyPerformanceIndicator fetchedKpi = _target.Fetch("KpiAdherenceNew");
            Assert.IsNotNull(fetchedKpi);
        }
    }
}
