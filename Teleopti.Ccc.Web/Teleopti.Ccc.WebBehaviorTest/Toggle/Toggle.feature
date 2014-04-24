Feature: Toggle
	In order to allow parallell work
	As a manager
	I want to be able to choose what features should reach customers

Scenario: Enabled feature
	When I query toggle service for 'EnabledFeature'
	Then I should get 'true' back
