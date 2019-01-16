using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
    /// <summary>
    /// Tests for QueueSource
    /// </summary>
    [TestFixture]
    public class QueueSourceTest
    {
        private IQueueSource _target;
        private IQueueSource _target2;
        private const string Name = "QueueSource - Name";
        private const string Description = "QueueSource - Description";
        private const string QueueOriginalId = "14";
        private const string QueueAggId = "111";
        private const string LogObjectName = "Log object name";
        private const int DataSourceId = 2;
        private const int QueueMartId = 999;

        /// <summary>
        /// Setup
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _target = new QueueSource(Name, Description, QueueOriginalId);
            _target2 = new QueueSource(Name, Description, QueueOriginalId, QueueAggId, QueueMartId, DataSourceId);
        }

        /// <summary>
        /// Constructor works.
        /// </summary>
        [Test]
        public void CanCreateQueueObject()
        {
            _target = new QueueSource(Name, Description, QueueOriginalId);
            Assert.IsNotNull(_target);
        }

        /// <summary>
        /// Verifies that properties are set correctly
        /// </summary>
        [Test]
        public void CanSetProperties()
        {
            _target.Name = Name;
            _target.Description = Description;
            _target.QueueAggId = QueueAggId;
            _target.DataSourceId = DataSourceId;
            _target.LogObjectName = LogObjectName;
            _target.QueueOriginalId = QueueOriginalId;
            _target.QueueMartId = QueueMartId;

            Assert.AreEqual(Name, _target.Name);
            Assert.AreEqual(Description, _target.Description);
            Assert.AreEqual(QueueOriginalId, _target.QueueOriginalId);
            Assert.AreEqual(QueueAggId, _target.QueueAggId);
            Assert.AreEqual(DataSourceId, _target.DataSourceId);
            Assert.AreEqual(LogObjectName, _target.LogObjectName);
            Assert.AreEqual(QueueMartId, _target.QueueMartId);
        }

        [Test]
        public void VerifyPropertiesWorks()
        {
            Assert.AreEqual(Name, _target.Name);
            Assert.AreEqual(Description, _target2.Description);
            Assert.AreEqual(QueueOriginalId, _target2.QueueOriginalId);
            Assert.AreEqual(DataSourceId, _target2.DataSourceId);
            Assert.AreEqual(QueueAggId, _target2.QueueAggId);
            Assert.AreEqual(string.Empty, _target2.LogObjectName);
            Assert.AreEqual(false, _target2 is IBelongsToBusinessUnit);
        }

        /// <summary>
        /// Public constructor works.
        /// </summary>
        [Test]
        public void PublicConstructorWorks()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_target.GetType(), true));
        }
    }
}