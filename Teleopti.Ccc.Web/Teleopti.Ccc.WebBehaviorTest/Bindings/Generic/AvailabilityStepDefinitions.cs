using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;

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
			DataMaker.Data().Setup(rotation);
		}

		[Given(@"I have an availability with")]
		public void GivenIHaveAnAvailabilityWith(PersonAvailabilityConfigurable availability)
		{
			DataMaker.Data().Setup(availability);
		}
	}
}