Feature: QUnit test
	In order do test driven development in javascript
	As a developer
	I want to run the javascript tests in the browser

@ignore
Scenario: All javascript unit tests
	When I navigate to unit test url Areas/MyTime/Content/Scripts/AllTests.aspx
	Then I should see all tests pass
