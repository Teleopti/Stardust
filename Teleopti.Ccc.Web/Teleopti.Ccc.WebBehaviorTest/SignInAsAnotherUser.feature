Feature: Sign in as another user
	In order to access the site
	As a user other than myself
	I want to be able to sign in as another user

Background:
	Given there is a business unit with
	| Field | Value           |
	| Name  | Business Unit 1 |
	And there is a role with
	| Field                     | Value                     |
	| Name                      | Can logon as another user |
	| Business Unit             | Business Unit 1           |
	| Access to mytime web      | true                      |
	| Can logon as another user | true                      |
	And there is a role with
	| Field                     | Value                        |
	| Name                      | Cannot logon as another user |
	| Business Unit             | Business Unit 1              |
	| Access to mytime web      | true                         |
	| Can logon as another user | false                        |

@WindowsAndApplicationLogon
Scenario: Not permitted to logon as another user
	Given I have the role 'Cannot logon as another user'
	And I am a user with
	| Field                  | Value |
	| Windows authentication | true  |
	When I go to mytime web
	Then I should not be able to logon as another user

@WindowsAndApplicationLogon
Scenario: Logon as another user
	Given I have the role 'Can logon as another user'
	And there is a user with
	| Field    | Value    |
	| UserName | aa       |
	| Password | password |
	And I am a user with
	| Field                  | Value |
	| Windows authentication | true  |
	When I go to mytime web
	And I choose to logon as another user
	And I choose teleopti identity provider
	When I try to sign in with
	| Field    | Value    |
	| UserName | aa       |
	| Password | password |
	Then I should be signed in
