Feature: Preferences Extended
	In order to view and submit when I prefer to work in more detail
	As an agent
	I want to view and submit extended preferences

Scenario: See indication of an extended preference
	Given I am an agent
	And I have an existing extended preference
	When I view preferences
	Then I should see that I have an existing extended preference

Scenario: See extended preference
	Given I am an agent
	And I have an existing extended preference
	When I view preferences
	And I click the extended preference indication
	Then I should see my existing extended preference

Scenario: See extended preference without permission
	Given I am an agent without access to extended preferences
	And I have an existing extended preference
	When I view preferences
	And I click the extended preference indication
	Then I should see my existing extended preference
