using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Foundation;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;


namespace Teleopti.Ccc.WinCodeTest.Common
{
    /// <summary>
    /// Tests for the LanguageResourceHelper class
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-12-27
    /// </remarks>
    [TestFixture]
    public class LanguageResourceHelperTest
    {
        LanguageResourceHelper target;
        private string _OK;
        private string _xxOK;
        /// <summary>
        /// Setups this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-27
        /// </remarks>
        [SetUp]
        public void Setup()
        {
            _OK = "OK";
            _xxOK = "xxOK";
            target = new LanguageResourceHelper();
        }

        /// <summary>
        /// Verifies the translation cancels when state holder not is initialized.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-27
        /// </remarks>
        [Test]
        public void VerifyTranslationCancelsWhenStateHolderNotIsInitialized()
        {
            MockRepository mocks = new MockRepository();
            ILocalized localized = mocks.StrictMock<ILocalized>();
            IState state = (IState)StateHolder.Instance.StateReader;
            StateHolderProxyHelper.ClearStateHolder();

            target.SetTexts(localized);

            Assert.IsNotNull(localized);

            StateHolderProxyHelper.ClearAndSetStateHolder(state);
        }

        /// <summary>
        /// Verifies the translation is done with empty class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-27
        /// </remarks>
        [Test]
        public void VerifyTranslationIsDoneWithEmptyClass()
        {
            TestLocalization localized = new TestLocalization();
            
            target.SetTexts(localized);

            Assert.IsNotNull(localized);
        }

        /// <summary>
        /// Verifies the translation is done.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-27
        /// </remarks>
        [Test]
        public void VerifyTranslationIsDone()
        {
            TestLocalizationWithMember localized = new TestLocalizationWithMember();

            target.SetTexts(localized);

            Assert.IsNotNull(localized);

            //This assertion requires the key to exist in resource file
            Assert.AreEqual("OK", localized.ButtonText);
        }

        /// <summary>
        /// Verifies the translation is not done with non component member.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-27
        /// </remarks>
        [Test]
        public void VerifyTranslationIsNotDoneWithNonComponentMember()
        {
            TestLocalizationWithFalseMember localized = new TestLocalizationWithFalseMember();

            target.SetTexts(localized);

            Assert.IsNotNull(localized);
        }

        /// <summary>
        /// Verifies the enums can be translated.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-28
        /// </remarks>
        [Test]
        public void VerifyEnumWithNoTranslationCannotBeTranslated()
        {
            Assert.Throws<ArgumentNullException>(() => LanguageResourceHelper.TranslateEnum<TaskOwnerPeriodType>());
        }

        /// <summary>
        /// Verifies the enum can be translated.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-28
        /// </remarks>
        [Test]
        public void VerifyEnumCanBeTranslated()
        {
            //Enum as key, string as value
            IDictionary<EmploymentType, string> enumTranslation = LanguageResourceHelper.TranslateEnum<EmploymentType>();
            Assert.AreEqual(3, enumTranslation.Count);
            Assert.IsTrue(enumTranslation.ContainsKey(EmploymentType.FixedStaffNormalWorkTime));
            Assert.IsFalse(string.IsNullOrEmpty(enumTranslation[EmploymentType.FixedStaffNormalWorkTime]));
        }

        [Test]
        public void VerifyEnumCanBeTranslatedToList()
        {
            IList<KeyValuePair<EmploymentType, string>> enumTranslation = LanguageResourceHelper.TranslateEnumToList<EmploymentType>();
            Assert.AreEqual(3, enumTranslation.Count);
            Assert.IsFalse(string.IsNullOrEmpty(enumTranslation[0].Value));
        }


        /// <summary>
        /// Verifies the enum value can be translated.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-10
        /// </remarks>
        [Test]
        public void VerifyEnumValueCanBeTranslated()
        {
            //Enum as key, string as value
            string enumTranslation = LanguageResourceHelper.TranslateEnumValue(EmploymentType.FixedStaffNormalWorkTime);
            Assert.IsFalse(string.IsNullOrEmpty(enumTranslation));
        }

        /// <summary>
        /// Verifies the not enum cannot be translated.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-28
        /// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyNotEnumCannotBeTranslated()
        {
			Assert.Throws<ArgumentException>(() => LanguageResourceHelper.TranslateEnum<LanguageResourceHelper>());
        }

        [Test]
        public void VerifyNotEnumValueCannotBeTranslated()
        {
			Assert.Throws<ArgumentException>(() => LanguageResourceHelper.TranslateEnumValue(new object()));
        }

        [Test]
        public void VerifyTranslationOnStringIsDone()
        {
            string toTranslate = _xxOK;
            Assert.AreEqual(_OK,LanguageResourceHelper.Translate(toTranslate));
         
        }

	    [Test]
	    public void ShouldHandleNullAsTargetControlBug36008()
	    {
		    new LanguageResourceHelper().SetTexts(null);
	    }

        #region Internal classes for test use
        internal class TestLocalization : System.ComponentModel.Component, ILocalized
        {
            #region ILocalized Members

            public void SetTexts()
            {
                throw new NotImplementedException();
            }

            public string Name
            {
                get { return GetType().Name; }
            }

            public RightToLeft RightToLeft
            {
                get;
                set;
            }

            #endregion
        }

        internal class TestLocalizationWithMember : TestLocalization
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TestLocalizationWithMember"/> class.
            /// </summary>
            /// <remarks>
            /// Created by: robink
            /// Created date: 2007-12-27
            /// </remarks>
            internal TestLocalizationWithMember()
            {
                buttonToTranslate = new Button();
                buttonToTranslate.Name = "myButton";
                buttonToTranslate.Text = "xxOk";
            }

            private Button buttonToTranslate;

            /// <summary>
            /// Gets the button text.
            /// </summary>
            /// <value>The button text.</value>
            /// <remarks>
            /// Created by: robink
            /// Created date: 2007-12-27
            /// </remarks>
            internal string ButtonText
            {
                get
                {
                    if (buttonToTranslate != null)
                        return buttonToTranslate.Text;
                    
                    return String.Empty;
                }
            }
        }

        internal class TestLocalizationWithFalseMember : TestLocalization
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TestLocalizationWithFalseMember"/> class.
            /// </summary>
            /// <remarks>
            /// Created by: robink
            /// Created date: 2007-12-27
            /// </remarks>
            internal TestLocalizationWithFalseMember()
            {
                person = new Person();
                person.WithName(new Name("foo", "foo"));
            }

            private Person person;
        }
        #endregion
    }
}
