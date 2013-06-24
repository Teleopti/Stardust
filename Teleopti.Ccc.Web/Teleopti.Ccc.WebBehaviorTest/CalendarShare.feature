Feature: Calendar Share
	In order to share my calendar with myself and others on the internet
	As an agent
	I want to 
		be able to activate calendar sharing on my settings
		be able to have a unique url to share my calendar


Scenario: Activate calendar sharing
	Given I am an agent
	When I view my settings
	And I click 'activate calendar sharing'
	Then I should see a unique url

