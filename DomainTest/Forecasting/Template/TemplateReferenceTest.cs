using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Template
{
    [TestFixture, SetUICulture("en-US")]
    public class TemplateReferenceTest
    {
        private DayOfWeek? _dayOfWeek; 
        private ITemplateReference _target;

        [SetUp]
        public void Setup()
        {
            _dayOfWeek = DayOfWeek.Monday;
            _target = new TemplateReference(Guid.Empty, 0, "TUE", null);
        }

        [Test]
        public void VerifyConstructor()
        {
            var templateId = Guid.NewGuid();
            const int versionNumber = 5;
            const string templateName = "Juldagen";
            _dayOfWeek = null;
            ITemplateReference templateReference = new TemplateReference(templateId, versionNumber, templateName, _dayOfWeek);
            Assert.AreEqual(templateId, templateReference.TemplateId);
            Assert.AreEqual(versionNumber, templateReference.VersionNumber);
            Assert.AreEqual(templateName, templateReference.TemplateName);
            Assert.AreEqual(_dayOfWeek, templateReference.DayOfWeek);
        }

        [Test]
        public void VerifyGetTemplateName()
        {
            _dayOfWeek = DayOfWeek.Monday;
            const string name = "Monday";
            const string abbreviatedName = "<MON>";
            Assert.AreEqual(abbreviatedName, _target.DisplayName(_dayOfWeek, name, false));
            Assert.AreEqual("<OLDMON>", _target.DisplayName(_dayOfWeek, name, true));

        }

        const string BaseName = "name";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyTrimNameDecorations()
        {
            var testTemplateReference = new TestTemplateReference();
            Assert.AreEqual(testTemplateReference.VersionNumber, testTemplateReference.AvoidCodaAnalysisComplaint());

			string decoratedName = "<" + BaseName + ">";
            string trimmedName = TestTemplateReference.GetTrimNameDecorations(decoratedName);
			Assert.AreEqual(BaseName, trimmedName);
        }

		[Test]
		public void ShouldGetUpdatedOnInfo()
		{
			var templateId = Guid.NewGuid();
			const int versionNumber = 5;
			const string templateName = "Juldagen";
			_dayOfWeek = null;
			ITemplateReference templateReference = new TemplateReference(templateId, versionNumber, templateName, _dayOfWeek);
			var updatedDate = new DateTime(2010, 12, 2);
			templateReference.UpdatedDate = updatedDate;
			Assert.AreEqual(updatedDate, templateReference.UpdatedDate);
		}

        private class TestTemplateReference : TemplateReference
        {
            public int AvoidCodaAnalysisComplaint()
            {
                return VersionNumber;
            }

            public static string GetTrimNameDecorations(string decoratedName)
            {
                return TrimNameDecorations(decoratedName);
            }
        }


    }
}
