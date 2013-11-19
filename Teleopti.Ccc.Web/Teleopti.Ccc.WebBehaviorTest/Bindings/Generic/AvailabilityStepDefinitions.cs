using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class AvailabilityStepDefinitions
	{
		[StepArgumentTransformation]
		public AvailabilityRotationConfigurable AvailabilityRotationConfigurableTransform(Table table)
		{
			return table.CreateInstance<AvailabilityRotationConfigurable>();
		}

		[StepArgumentTransformation]
		public PersonAvailabilityConfigurable PersonAvailabilityConfigurableTransform(Table table)
		{
			return table.CreateInstance<PersonAvailabilityConfigurable>();
		}

		[Given(@"there is an availability rotation with")]
		public void GivenThereIsAnAvailabilityRotationWith(AvailabilityRotationConfigurable rotation)
		{
			DataMaker.Data().Apply(rotation);
		}

		[Given(@"I have an availability with")]
		public void GivenIHaveAnAvailabilityWith(PersonAvailabilityConfigurable availability)
		{
			DataMaker.Data().Apply(availability);
		}
	}
}