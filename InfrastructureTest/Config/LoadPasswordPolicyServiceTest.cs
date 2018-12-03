using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Config;


namespace Teleopti.Ccc.InfrastructureTest.Config
{
    [TestFixture]
    public class LoadPasswordPolicyServiceTest
    {
        private LoadPasswordPolicyService _target;
        private int _maxNumberOfAttempts;
        private int _invalidAttemptWindow;
        private int _passwordValidForDayCount;
        private int _passwordExpireWarningDayCount;

        [SetUp]
        public void Setup()
        {
            _maxNumberOfAttempts = 3;
            _invalidAttemptWindow = 30;
            _passwordValidForDayCount = 32;
            _passwordExpireWarningDayCount = 11;
            _target = new LoadPasswordPolicyService(createCompleteTestDocument());
        }

        [Test]
        public void VerifyLoadFromFile()
        {
            _target = new LoadPasswordPolicyService(string.Empty);
            Assert.AreEqual(1, _target.LoadMaxAttemptCount());
            Assert.AreEqual(TimeSpan.Zero, _target.LoadInvalidAttemptWindow());
            Assert.AreEqual(0, _target.LoadPasswordExpireWarningDayCount());
            Assert.AreEqual(int.MaxValue-1, _target.LoadPasswordValidForDayCount());
            Assert.AreEqual(2, _target.LoadPasswordStrengthRules().Count);
        }

        [Test]
        public void VerifyCanLoadMaxAttemptCountFromFile()
        {
            Assert.AreEqual(_maxNumberOfAttempts, _target.LoadMaxAttemptCount());
        }

        [Test]
        public void VerifyCanLoadAttemptWindowFromFile()
        {
            Assert.AreEqual(TimeSpan.FromMinutes(_invalidAttemptWindow), _target.LoadInvalidAttemptWindow());
        }

        [Test]
        public void VerifyCanLoadPasswordStrengthRulesFromFile()
        {
            Assert.IsNotNull(_target.File);
            IList<IPasswordStrengthRule> result = _target.LoadPasswordStrengthRules();
            Assert.AreEqual(2, result.Count, "There are two rules in the xml-file");
        }


        [Test]
        public void VerifyCanLoadPasswordValidForDayCount()
        {
            Assert.AreEqual(_passwordValidForDayCount, _target.LoadPasswordValidForDayCount());
        }

        [Test]
        public void VerifyCanLoadPasswordExpireWarningDayCount()
        {
            Assert.AreEqual(_passwordExpireWarningDayCount, _target.LoadPasswordExpireWarningDayCount());
        }

        [Test]
        public void VerifyDefaultWhenFileNotFound()
        {
            File.Move("PasswordPolicy.xml", "PasswordPolicy.xml.notinuse");
            _target = new LoadPasswordPolicyService(string.Empty);
            Assert.AreEqual(3, _target.LoadMaxAttemptCount(), "default value is 3");
            Assert.AreEqual(TimeSpan.Zero, _target.LoadInvalidAttemptWindow(), "If the window is zero, the user will get a new attempt directly after trying to log on");
            //Assert.AreEqual(0, _target.LoadPasswordStrengthRules().Count, "default value is no passwordstrengthrules");
            Assert.AreEqual(int.MaxValue, _target.LoadPasswordValidForDayCount(), "default value for PasswordValidForDayCount is int.max");
            Assert.AreEqual(0, _target.LoadPasswordExpireWarningDayCount(), "default value for ExpireWarningDayCount is 0");
				_target.LoadPasswordStrengthRules().First().VerifyPasswordStrength("").Should().Be.False();
				_target.LoadPasswordStrengthRules().First().VerifyPasswordStrength("a").Should().Be.True();
            File.Move("PasswordPolicy.xml.notinuse", "PasswordPolicy.xml");
			  
        }

        [Test]
        public void VerifyBadMaxAttemptCountReturnsDefaultValue()
        {
            _target = new LoadPasswordPolicyService(DocWithBadMaxAttemptCount());
            Assert.AreEqual(3, _target.LoadMaxAttemptCount());
        }

        [Test]
        public void VerifyBadInvalidAttemptWindowReturnsDefaultValue()
        {
            _target = new LoadPasswordPolicyService(DocWithBadInvalidAttemptWindow());
            Assert.AreEqual(TimeSpan.Zero, _target.LoadInvalidAttemptWindow());
        }

        [Test]
        public void VerifyDoesNotAddRuleIfXmlIsIncorrect()
        {
            _target = new LoadPasswordPolicyService(AddOkRule(TestDocument()));
            Assert.AreEqual(1, _target.LoadPasswordStrengthRules().Count, "Just to check that a rule gets added");

            _target = new LoadPasswordPolicyService(AddBadRule(TestDocument()));
            Assert.AreEqual(0, _target.LoadPasswordStrengthRules().Count, "Rule that cannot be created does not get added");

        }
        [Test]
        public void VerifyDoesNotAddAnyRuleIfAnyXmlIsIncorrect()
        {

            _target = new LoadPasswordPolicyService(AddOkRule(AddBadRule(TestDocument())));
            Assert.AreEqual(0, _target.LoadPasswordStrengthRules().Count, "If any of the rules are wrong, dont add any rules at all for now");

        }

        [Test]
        public void VerifyDoesNotGoBoomIfPasswordStrengthRuleNotValid()
        {
            //Just checks that we dont get a crash if there is something wrong with the PasswordStrength tabs
            XDocument doc = createCompleteTestDocument();
            XElement root = doc.Element("PasswordPolicy");
            XElement ruleWithoutRegEx = new XElement("Rule", new XAttribute("MinAccepted", 1),
                new XElement("PasswordStrengthRule"));
            root.Add(ruleWithoutRegEx);
            _target=new LoadPasswordPolicyService(doc);
            Assert.IsNotNull(_target.LoadPasswordStrengthRules()); 

            //Add a rule without MinAccepted
            root.Add(
                new XElement("Rule")
                );
            _target = new LoadPasswordPolicyService(doc);
            Assert.IsNotNull(_target.LoadPasswordStrengthRules()); 
        }

        [Test]
        public void ShouldGetSetPath()
        {
            _target.Path = "path";
            Assert.AreEqual("path", _target.Path);
        }

		//[Test]
		//public void ShouldClearFile()
		//{
		//	_target = new LoadPasswordPolicyService(AddOkRule(TestDocument()));
		//		Assert.AreEqual(1, _target.LoadPasswordStrengthRules().Count, "Just to check that a rule gets added");
		//	_target.ClearFile();
		//	_target.Path = "notExist";
		//	Assert.AreEqual(0, _target.LoadPasswordStrengthRules().Count, "Should clear _file");
		//}

        #region createXml
        private static XDocument DocWithBadMaxAttemptCount()
        {
            XDocument ret = TestDocument();
            XElement element = ret.Element("PasswordPolicy");
            element.SetAttributeValue("MaxNumberOfAttempts", "NotParsableToTimeSpan");
            return ret;
        }

        private static XDocument DocWithBadInvalidAttemptWindow()
        {
            XDocument ret = TestDocument();
            XElement element = ret.Element("PasswordPolicy");
            element.SetAttributeValue("InvalidAttemptWindow", "NotPasrsableToInt");
            return ret;
        }

        private static XDocument AddBadRule(XDocument doc)
        {
            XDocument ret = doc;

            XElement passwordPolicyRoot = ret.Element("PasswordPolicy");

            XElement badRule = new XElement("Rule");
            badRule.SetAttributeValue("MinAccepted", "NotParsableToInt");


            passwordPolicyRoot.Add(badRule);

            return ret;
        }

        private static XDocument AddOkRule(XDocument doc)
        {
            XDocument ret = doc;
            XElement passwordPolicyRoot = ret.Element("PasswordPolicy");
            XElement badRule = new XElement("Rule");
            badRule.SetAttributeValue("MinAccepted", "2");
            passwordPolicyRoot.Add(badRule);

            return ret;
        }



        private static XDocument TestDocument()
        {
            return new XDocument(
             new XDeclaration("1.0", "utf-8", "yes"),
             new XComment("Default config data"),
             new XElement("PasswordPolicy", new XAttribute("MaxNumberOfAttempts", "12"), new XAttribute("InvalidAttemptWindow", "5"))

             );




        }

        private  XDocument createCompleteTestDocument()
        {
            XDocument d = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("PasswordPolicy",
                             new XAttribute("MaxNumberOfAttempts", _maxNumberOfAttempts),
                             new XAttribute("InvalidAttemptWindow", _invalidAttemptWindow),
                             new XAttribute("PasswordValidForDayCount", _passwordValidForDayCount),
                             new XAttribute("PasswordExpireWarningDayCount", _passwordExpireWarningDayCount),
                             new XElement("Rule",
                                          new XAttribute("MinAccepted", 1),
                                          new XAttribute("Description", "PasswordLength"),
                                          new XElement("PasswordStrengthRule", "", new XAttribute("RegEx", "stuff")),
                                          new XElement("PasswordStrengthRule", "", new XAttribute("RegEx", "stuff")),
                                          new XElement("PasswordStrengthRule", "", new XAttribute("RegEx", "stuff"))),

                             new XElement("Rule",
                                          new XAttribute("MinAccepted", 2),
                                          new XAttribute("Description", "AnotherRule"),
                                          new XElement("PasswordStrengthRule", "", new XAttribute("RegEx", "stuff")),
                                          new XElement("PasswordStrengthRule", "", new XAttribute("RegEx", "stuff")),
                                          new XElement("PasswordStrengthRule", "", new XAttribute("RegEx", "stuff")))));

          
            return d;
        }

        #endregion //createXml



    }
}
