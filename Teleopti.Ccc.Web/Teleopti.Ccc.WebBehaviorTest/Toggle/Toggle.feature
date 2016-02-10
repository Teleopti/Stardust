﻿Feature: Toggle
	In order to allow parallell work
	As a manager
	I want to be able to choose what features should reach customers

@OnlyRunIfEnabled('TestToggle')
@NoBrowser
Scenario: Only run featuretest if a certain feature is enabled when loading on demand
	When I query toggle service for 'TestToggle'
	Then I should get 'true' back

@OnlyRunIfDisabled('TestToggle')
@NoBrowser
Scenario: Only run featuretest if a certain feature is disabled when loading on demand
	When I query toggle service for 'TestToggle'
	Then I should get 'false' back

@OnlyRunIfEnabled('TestToggle')
@NoBrowser
Scenario: Only run featuretest if a certain feature is enabled when loading them all
	When I query toggle service for 'TestToggle' by loading them all
	Then I should get 'true' back

@OnlyRunIfDisabled('TestToggle')
@NoBrowser
Scenario: Only run featuretest if a certain feature is disabled when loading them all
	When I query toggle service for 'TestToggle' by loading them all
	Then I should get 'false' back


## When running locally, one of these tests should fail
## They are here so build server can verify it's running in correct mode
@IsInDevMode
@NoBrowser
Scenario: Verify environment is set correctly to DEV mode
	When I query toggle service for 'TestToggle'
	Then I should get 'true' back

@IsInCustomerMode
@NoBrowser
Scenario: Verify environment is set correctly to CUSTOMER mode
	When I query toggle service for 'TestToggle'
	Then I should get 'false' back
##########################################################################
