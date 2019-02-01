Feature: Preferences period feedback mobile
	In order to know if my preferences are viable
	As an agent
	I want feedback of my preferences compared to my contract for the period in mobile


@Mobile
Scenario: Period feedback of warning icon
	Given I am an agent
	And I have a scheduling period of 1 week
	And I have a contract schedule with 2 days off
	When I view preferences
	Then I should see a warning icon
@Mobile
Scenario: Period feedback toggle 
	Given I am an agent
	And I have a scheduling period of 1 week
	And I have a contract schedule with 2 days off
	When I view preferences
	Then I should see a warning icon
	When I click the warning icon
	Then I should see a message that I should have 2 days off 
	When I click the warning icon
	Then I should not see any feedback 