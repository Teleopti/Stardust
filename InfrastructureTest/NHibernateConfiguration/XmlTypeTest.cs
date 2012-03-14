using System;
using System.Data;
using System.Xml;
using System.Xml.XPath;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;

namespace Teleopti.Ccc.InfrastructureTest.NHibernateConfiguration
{
    [TestFixture]
    public class XmlTypeTest
    {
        private XmlType target;
        private IXPathNavigable document1;
        private IXPathNavigable document2;
        private IXPathNavigable document3;

        [SetUp]
        public void Setup()
        {
            document1 = new XmlDocument();
            document1.CreateNavigator().AppendChildElement(string.Empty,"fel",string.Empty,string.Empty);
            document2 = new XmlDocument();
            document2.CreateNavigator().AppendChildElement(string.Empty, "testar", string.Empty, string.Empty);
            document3 = new XmlDocument();
            document3.CreateNavigator().AppendChildElement(string.Empty, "testar", string.Empty, string.Empty);

            target = new XmlType();
        }

        [Test]
        public void VerifyEquals()
        {
            Assert.IsFalse(target.Equals(null, null));
            Assert.IsFalse(target.Equals(null, document1));
            Assert.IsFalse(target.Equals(document1, null));
            Assert.IsFalse(target.Equals(document1,document2));
            Assert.IsTrue(target.Equals(document3,document2));
        }

        [Test]
        public void VerifyGetHashCode()
        {
            Assert.AreEqual(document1.GetHashCode(),target.GetHashCode(document1));
        }

        [Test]
        public void VerifyNullSafeGet()
        {
            var names = new[] {"name1"};

            MockRepository mocks = new MockRepository();
            IDataReader dataReader = mocks.StrictMock<IDataReader>();

            Expect.Call(dataReader[names[0]]).Return(null);
            Expect.Call(dataReader[names[0]]).Return(document1.CreateNavigator().OuterXml);

            mocks.ReplayAll();

            Assert.IsNull(target.NullSafeGet(dataReader,names,null));

            IXPathNavigable document = (IXPathNavigable)target.NullSafeGet(dataReader, names, null);
            Assert.AreEqual(document1.CreateNavigator().OuterXml,document.CreateNavigator().OuterXml);

            mocks.VerifyAll();
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void VerifyNullSafeGetExceptionWithTooManyNames()
        {
            target.NullSafeGet(null, new[] {"name1", "name2"},null);
        }

        [Test]
        public void VerifyNullSafeSet()
        {
            MockRepository mocks = new MockRepository();
            IDbCommand dbCommand = mocks.StrictMock<IDbCommand>();
            IDataParameterCollection dataParameterCollection = mocks.StrictMock<IDataParameterCollection>();
            IDataParameter dataParameter = mocks.StrictMock<IDataParameter>();

            Expect.Call(dbCommand.Parameters).Return(dataParameterCollection).Repeat.Twice();
            Expect.Call(dataParameterCollection[1]).Return(dataParameter).Repeat.Twice();
            Expect.Call(dataParameter.Value = DBNull.Value);
            Expect.Call(dataParameter.Value = document2.CreateNavigator().OuterXml);

            mocks.ReplayAll();

            target.NullSafeSet(dbCommand, null, 1);
            target.NullSafeSet(dbCommand, document2, 1);

            mocks.VerifyAll();
        }

        [Test]
        public void VerifyDeepCopy()
        {
            Assert.IsNull(target.DeepCopy(null));
            IXPathNavigable document = (IXPathNavigable)target.DeepCopy(document1);
            Assert.AreNotEqual(document1,document);
            Assert.AreEqual(document1.CreateNavigator().OuterXml,document.CreateNavigator().OuterXml);
        }

        [Test,ExpectedException(typeof(NotImplementedException))]
        public void VerifyReplaceNotImplemented()
        {
            target.Replace(null, null, null);
        }

        [Test]
        public void VerifyAssemble()
        {
            Assert.IsNull(target.Assemble(null,null));
            IXPathNavigable document = (IXPathNavigable)target.Assemble(document2.CreateNavigator().OuterXml, null);
            Assert.AreEqual(document2.CreateNavigator().OuterXml,document.CreateNavigator().OuterXml);
        }

        [Test]
        public void VerifyDisassemble()
        {
            Assert.IsNull(target.Disassemble(null));
            string xmlString = (string)target.Disassemble(document2);
            Assert.AreEqual(document2.CreateNavigator().OuterXml, xmlString);
        }

        [Test]
        public void VerifyProperties()
        {
           
            Assert.IsTrue(target.IsMutable);
            Assert.AreEqual(typeof(IXPathNavigable), target.ReturnedType);
        }
    }
}