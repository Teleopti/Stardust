using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm 
{
	[Binding]
	class SeatMapStepDefinitions
	{

		[When(@"I click on the Edit Button")]
		public void WhenIClickOnTheEditButton()
		{
			Browser.Interactions.Click (".mfb-component__main-icon--active");
		}

		[When(@"I create a seat")]
		public void WhenICreateASeat()
		{
			Browser.Interactions.Click(".mdi-plus");
		}
		
		[Then(@"I should see a seat in the seat map")]
		public void ThenIShouldSeeASeatInTheSeatMap()
		{
			const string javascript = @"var result = utils.getSeats(canvas); " +
			                          @"return result.length;";

			AssertCanvasUtilityFunctionResultContains ( javascript, "1");
		}
		
		[When(@"I create a location")]
		public void WhenICreateALocation()
		{
			Browser.Interactions.ClickUsingJQuery(".mdi-tab-unselected");
			Browser.Interactions.FillWith("#locationName","Wibble");
			Browser.Interactions.Click("#okButton");
		}

		[Then(@"I should see a location in the seat map")]
		public void ThenIShouldSeeALocationInTheSeatMap()
		{
			const string javascript = @"var result = utils.getLocations(canvas);" +
									  @"return result[0].Name;";
			
			AssertCanvasUtilityFunctionResultContains(javascript, "Wibble");
		}

		public void AssertCanvasUtilityFunctionResultContains (string function, string expectedResult )
		{
			var javascript = @"var injector = angular.element(document.getElementsByClassName('ng-scope')[0]).injector(); " +
			                 @"var utils= injector.get('seatMapCanvasUtilsService');" +
			                 @"var canvas = document.getElementById('c').fabric; " +
							 function;
			
			Browser.Interactions.AssertJavascriptResultContains (javascript, expectedResult);
		}
		

	}
}
