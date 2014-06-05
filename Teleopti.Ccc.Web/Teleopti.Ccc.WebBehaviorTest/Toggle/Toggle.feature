Feature: Toggle
	In order to allow parallell work
	As a manager
	I want to be able to choose what features should reach customers

@OnlyRunIfEnabled('TestToggle')
Scenario: Only run featuretest if a certain feature is enabled using out of process
	When I query outofprocess toggle service for 'TestToggle'
	Then I should get 'true' back

@OnlyRunIfDisabled('TestToggle')
Scenario: Only run featuretest if a certain feature is disabled using out of process
	When I query outofprocess toggle service for 'TestToggle'
	Then I should get 'false' back
