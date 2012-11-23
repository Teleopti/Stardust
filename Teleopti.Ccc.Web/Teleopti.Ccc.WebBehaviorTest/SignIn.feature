﻿Feature: Sign in
	In order to access the site
	As a user that is not signed in
	I want to be able to sign in

Background:
	
	Given there is a business unit with
	| Field | Value           |
	| Name  | Business Unit 1 |
	And there is a business unit with
	| Field | Value           |
	| Name  | Business Unit 2 |
	And there is a scenario
	| Field         | Value           |
	| Name          | Scenario 1      |
	| Business Unit | Business Unit 1 |
	And there is a scenario
	| Field         | Value           |
	| Name          | Scenario 2      |
	| Business Unit | Business Unit 2 |
	And there is a role with
	| Field                    | Value                    |
	| Name                     | Role for business unit 1 |
	| Business Unit            | Business Unit 1          |
	| Access to mytime web     | true                     |
	And there is a role with
	| Field                    | Value                    |
	| Name                     | Role for business unit 2 |
	| Business Unit            | Business Unit 2          |
	| Access to mytime web     | true                     |


Scenario: Sign in with a user with multiple business units by user name
	Given I have the role 'Role for business unit 1'
	And I have the role 'Role for business unit 2'
	And I am viewing the sign in page
	When I sign in by user name
	And I select a business unit
	Then I should be signed in
	
Scenario: Sign in with a user with one business unit by user name and I should be directed into that business unit direct without having to select it
	Given I have the role 'Role for business unit 1'
	And I am viewing the sign in page
	When I sign in by user name
	Then I should be signed in

Scenario: Sign in with a user with multiple business units by Windows credentials
	Given Windows user have the role 'Role for business unit 1'
	And Windows user have the role 'Role for business unit 2'
	And I am viewing the sign in page
	When I sign in by windows credentials
	And I select a business unit
	Then I should be signed in
	
@ignore
Scenario: Sign in with a user with one business unit by Windows credentials and I should be directed into that business unit direct without having to select it
	Given Windows user have the role 'Role for business unit 2'
	And I am viewing the sign in page
	When I sign in by windows credentials
	Then I should be signed in
	
Scenario: Sign in with wrong password should give me an informative error
	Given I have the role 'Role for business unit 1'
	And I am viewing the sign in page
	When I sign in by user name and wrong password
	Then I should see a log on error

Scenario: Sign in without permission
	Given I dont have permission to sign in
	And I am viewing the sign in page
	When I sign in by user name
	Then I should not be signed in
