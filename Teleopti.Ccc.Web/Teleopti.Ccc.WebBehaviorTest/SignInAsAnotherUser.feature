Feature: Sign in as another user
	In order to access the site
	As a user other than myself
	I want to be able to sign in as another user

Background:

@WindowsAndApplicationLogon
Scenario: Not permitted to logon as another user
	Given I have a role with
	| Field                     | Value |
	| Access to mytime web      | true  |
	| Can logon as another user | false |
	And I am a user with
	| Field                  | Value |
	| Windows authentication | true  |
	When I go to mytime web
	Then I should not be able to logon as another user

@WindowsAndApplicationLogon
Scenario: Logon as another user
	Given I have a role with
	| Field                     | Value |
	| Access to mytime web      | true  |
	| Can logon as another user | true  |
	And I am a user with
	| Field                  | Value |
	| Windows authentication | true  |
	And there is a role with
	| Field                | Value      |
	| Name                 | Agent role |
	| Access to mytime web | true       |
	And 'Ashley' is a user with
	| Field    | Value      |
	| UserName | Ashley     |
	| Password | password   |
	| Role     | Agent role |
	When I go to mytime web
	And I choose to logon as another user
	And I choose teleopti identity provider
	And I try to signin with
	| Field    | Value    |
	| UserName | Ashley   |
	| Password | password |
	Then I should be signed in as another user 'Ashley'
