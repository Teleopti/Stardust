Feature: Application start recovery
	In order to be able to access the site after a failed application startup
	As a user
	I want it to recover when the problem is fixed

Scenario: Application startup failure
	Given the database isnt running
	And I access the site for the first time
	Then I should get a error message
