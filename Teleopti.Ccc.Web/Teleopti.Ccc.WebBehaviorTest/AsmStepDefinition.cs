using TechTalk.SpecFlow;

namespace Teleopti.Ccc.WebBehaviorTest
{
    public class AsmStepDefinition
    {
        [When(@"I am viewing asm gant")]
        public void WhenIAmViewingAsmGant()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"I should see a schedule in popup")]
        public void ThenIShouldSeeAScheduleInPopup()
        {
            ScenarioContext.Current.Pending();
        }

    }
}
