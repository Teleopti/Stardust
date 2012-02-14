using NUnit.Framework;
using Teleopti.Ccc.PayrollFormatter;

namespace Teleopti.Ccc.PayrollFormatterTest
{
    [TestFixture]
    public class ItemFormatTest
    {
        private ItemFormat target;

        [SetUp]
        public void Setup()
        {
            target = new ItemFormat("ItemElement","xs:string");
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual("ItemElement",target.Element);
            Assert.AreEqual("xs:string",target.XmlType);
            Assert.AreEqual(string.Empty,target.Format);
            Assert.AreEqual(' ',target.Fill);
            Assert.IsFalse(target.Quote);
            Assert.AreEqual(Align.Left, target.Align);
            Assert.AreEqual(0,target.Length);

            target.Format = "yyyyMMdd";
            target.Fill = '#';
            target.Quote = true;
            target.Align = Align.Right;
            target.Length = 8;

            Assert.AreEqual("yyyyMMdd",target.Format);
            Assert.AreEqual('#',target.Fill);
            Assert.IsTrue(target.Quote);
            Assert.AreEqual(Align.Right,target.Align);
            Assert.AreEqual(8,target.Length);
        }
    }
}
