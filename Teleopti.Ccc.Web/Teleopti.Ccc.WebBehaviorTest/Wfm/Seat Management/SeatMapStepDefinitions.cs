using System.ComponentModel;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;
using Navigation = Teleopti.Ccc.WebBehaviorTest.Bindings.NavigationStepDefinitions;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm 
{
	[Binding]
	class SeatMapStepDefinitions
	{

		[When(@"I create a seat")]
		public void WhenICreateASeat()
		{
			Browser.Interactions.Click("#menuFile");
			Browser.Interactions.Click("#menuFileAdd");
			Browser.Interactions.Click("#menuFileAddSeat");
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
			Browser.Interactions.Click("#menuFile");
			Browser.Interactions.Click("#menuFileAdd");
			Browser.Interactions.Click("#menuFileAddLocation");

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
			var javascript =	@"var scopeElement = angular.element(document.getElementsByClassName('ng-scope')[0]); " +	
								@"var canvasElement = angular.element(document.getElementsByClassName('upper-canvas')[0]); "+
								@"var injector = scopeElement.injector(); " +
								@"var vm = canvasElement.scope().vm; " +
								@"var utils= injector.get('seatMapCanvasUtilsService');" +
								@"var canvas = vm.getCanvas();" +
								function;
			
			Browser.Interactions.AssertJavascriptResultContains (javascript, expectedResult);
		}
		

	}
}
