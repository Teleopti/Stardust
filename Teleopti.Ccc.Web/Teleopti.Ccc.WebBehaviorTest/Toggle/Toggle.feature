Feature: Toggle
	In order to allow parallell work
	As a manager
	I want to be able to choose what features should reach customers

Scenario: Enabled feature in process
	When I query inprocess toggle service for 'EnabledFeature'
	Then I should get 'true' back

Scenario: Enabled feature out of process
	When I query outofprocess toggle service for 'EnabledFeature'
	Then I should get 'true' back
