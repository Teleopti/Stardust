﻿Feature: Sign in New
	In order to access the site
	As a user that is not signed in
	I want to be able to sign in

Scenario: Sign in with a user with multiple business units by user name
	Given I am a user with multiple business units
	And I am viewing the sign in page
	When I select application logon data source
	And I sign in by user name
	And I select a business unit
	Then I should be signed in
	
Scenario: Sign in with a user with one business unit by user name and I should be directed into that business unit direct without having to select it
	Given I am a user with a single business unit
	And I am viewing the sign in page
	When I select application logon data source
	And I sign in by user name
	Then I should be signed in

Scenario: Sign in with a user with multiple business units by Windows credentials
	Given I am a user with multiple business units
	And I am viewing the sign in page
	When I select windows logon data source
	And I sign in by windows credentials
	And I select a business unit
	Then I should be signed in
	
Scenario: Sign in with a user with one business unit by Windows credentials and I should be directed into that business unit direct without having to select it
	Given I am a user with a single business unit
	And I am viewing the sign in page
	When I select windows logon data source
	And I sign in by windows credentials
	Then I should be signed in
	
Scenario: Sign in with wrong password should give me an informative error
	Given I am a user with a single business unit
	And I am viewing the sign in page
	When I select application logon data source
	And I sign in by user name and wrong password
	Then I should see an log on error

Scenario: Sign in without permission
	Given I dont have permission to sign in
	And I am viewing the sign in page
	When I select application logon data source
	And I sign in by user name
	Then I should not be signed in
