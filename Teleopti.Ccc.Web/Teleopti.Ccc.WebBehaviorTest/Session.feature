Feature: Session handling
	In order to be able to work with the application
	As an agent
	I want the application to handle my login session approprietly

Scenario: Stay logged in during server restart
	Given I am signed in
	Then I should be signed in
	When I browse the internet
	And the server restarts
	And I browse to an application page
	Then I should be signed in

Scenario: Logged out when cookie expires
	Given I am signed in
	Then I should be signed in
	When I browse the internet
	And my cookie expires
	And I browse to an application page
	Then I should not be signed in

Scenario: Save preference when cookie is expired
	Given I am signed in
	Then I should be signed in
	When I browse the internet
	And my cookie expires
	And I navigate to the preferences page
	And I select an editable day without preference
	And I select a standard preference
	Then I should not be signed in

Scenario: Navigate to next period when cookie is expired
	Given I am signed in
	Then I should be signed in
	When I browse the internet
	And my cookie expires
	And I navigate to the preferences page
	And I click next virtual schedule period button
	Then I should not be signed in
