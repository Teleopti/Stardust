Feature: Toggle
	In order to allow parallell work
	As a manager
	I want to be able to choose what features should reach customers

@OnlyRunIfEnabled('EnabledFeature')
Scenario: Only run featuretest if a certain feature is enabled using in process
	When I query inprocess toggle service for 'EnabledFeature'
	Then I should get 'true' back

@OnlyRunIfDisabled('EnabledFeature')
Scenario: Only run featuretest if a certain feature is disabled using in process
	When I query inprocess toggle service for 'EnabledFeature'
	Then I should get 'false' back

@OnlyRunIfEnabled('EnabledFeature')
Scenario: Only run featuretest if a certain feature is enabled using out of process
	When I query outofprocess toggle service for 'EnabledFeature'
	Then I should get 'true' back

@OnlyRunIfDisabled('EnabledFeature')
Scenario: Only run featuretest if a certain feature is disabled using out of process
	When I query outofprocess toggle service for 'EnabledFeature'
	Then I should get 'false' back
