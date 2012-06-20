Feature: QUnit test
	In order do test driven development in javascript
	As a developer
	I want to run the javascript tests in the browser

Scenario: Preferences tests
	When I navigate to unit test url Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Preference.Tests.html
	Then I should see the tests run
	And I should see all tests pass
