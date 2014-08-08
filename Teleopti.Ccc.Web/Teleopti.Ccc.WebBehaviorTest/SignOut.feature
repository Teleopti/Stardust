Feature: Sign out
	In order to leave the site
	As a user
	I want to be able to sign out

@WindowsAsDefaultIdentityProviderLogon
Scenario: Logon as another user
	Given I have a role with
	| Field                     | Value |
	| Access to mytime web      | true  |
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
	And I choose to sign out
	And I choose teleopti identity provider
	And I try to signin with
	| Field    | Value    |
	| UserName | Ashley   |
	| Password | password |
	Then I should be signed in as another user 'Ashley'
