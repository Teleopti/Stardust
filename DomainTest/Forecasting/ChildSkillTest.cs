using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
    /// <summary>
    /// Tests for ChildSkill
    /// </summary>
    [TestFixture]
    public class ChildSkillTest
    {
        private ChildSkill target;
        private MultisiteSkill _multiSiteSkill;
        private SkillType _skillTypePhone;
        private string _name = "Skill - Name";
        private string _description = "Skill - Description";
        private Color _displayColor = Color.FromArgb(234);
        
        /// <summary>
        /// Setup
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _skillTypePhone = SkillTypeFactory.CreateSkillTypePhone();
            _multiSiteSkill = new MultisiteSkill("Parent", "Parent", _displayColor, 15, _skillTypePhone);
			target = new ChildSkill(_name, _description, _displayColor, _multiSiteSkill);
        }

		/// <summary>
		/// Verifies the empty constructor.
		/// </summary>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-04-21
		/// </remarks>
		[Test]
        public void VerifyEmptyConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(target.GetType()));
        }

        /// <summary>
        /// Verifies the type of the instance is created of correct.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-18
        /// </remarks>
        [Test]
        public void VerifyInstanceIsCreatedOfCorrectType()
        {
            Assert.IsNotNull(target);
            Assert.IsInstanceOf<Skill>(target);
        }
    }
}