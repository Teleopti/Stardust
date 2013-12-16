using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.WinCode.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Forecasting
{
    [TestFixture]
    public class TextManagerTest
    {
        private TextManager _target;
        private ISkillType _skillType;
        private MockRepository mocks;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            _skillType = mocks.StrictMock<ISkillType>();
        }
        [Test]
        public void CanGetCorrectString()
        {
            using (mocks.Record())
            {
                Expect.Call(_skillType.ForecastSource).Return(ForecastSource.Email);
                Expect.Call(_skillType.ForecastSource).Return(ForecastSource.Backoffice);
                Expect.Call(_skillType.ForecastSource).Return(ForecastSource.Facsimile);
                Expect.Call(_skillType.ForecastSource).Return(ForecastSource.Time);
                Expect.Call(_skillType.ForecastSource).Return(ForecastSource.Facsimile);
                Expect.Call(_skillType.ForecastSource).Return(ForecastSource.InboundTelephony);
                Expect.Call(_skillType.ForecastSource).Return(ForecastSource.Retail);
            }
            using (mocks.Playback())
            {
                _target = new TextManager(_skillType);
                Assert.IsTrue(_target.WordDictionary.ContainsKey("Tasks"));
                _target = new TextManager(_skillType);
                Assert.IsTrue(_target.WordDictionary.ContainsKey("Tasks"));
                _target = new TextManager(_skillType);
                Assert.IsTrue(_target.WordDictionary.ContainsKey("Tasks"));
                _target = new TextManager(_skillType);
                Assert.IsTrue(_target.WordDictionary.ContainsKey("Tasks"));
                _target = new TextManager(_skillType);
                Assert.IsTrue(_target.WordDictionary.ContainsKey("Tasks"));
                _target = new TextManager(_skillType);
                Assert.IsTrue(_target.WordDictionary.ContainsKey("Tasks"));
                _target = new TextManager(_skillType);
                Assert.IsTrue(_target.WordDictionary.ContainsKey("Tasks"));
            }
        }

		[Test]
		public void ShouldShowTalkTimeForInboundTelephony()
		{
			using (mocks.Record())
			{
				Expect.Call(_skillType.ForecastSource).Return(ForecastSource.InboundTelephony);
			}
			using (mocks.Playback())
			{
				_target = new TextManager(_skillType);
				Assert.That(_target.WordDictionary["AverageTaskTime"], Is.EqualTo(UserTexts.Resources.TalkTime));
				Assert.That(_target.WordDictionary["TotalStatisticAverageTaskTime"], Is.EqualTo(UserTexts.Resources.TalkTime));
			}
		}
    }
}
