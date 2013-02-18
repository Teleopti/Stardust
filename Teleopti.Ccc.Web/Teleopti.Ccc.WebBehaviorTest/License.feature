Feature: License
	In order to enforce application license
	As Teleopti
	I want restrict usage based on license

Scenario: Show licensed to information in portal
	Given I am an agent
	And My schedule is published
	When I view my week schedule
	Then I should see licensed to information