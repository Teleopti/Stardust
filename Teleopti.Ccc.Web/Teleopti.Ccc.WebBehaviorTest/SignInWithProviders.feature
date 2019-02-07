Feature: Sign in with different providers
	In order to access the site
	As a user that is not signed in
	I want to be able to sign in by selecting providers

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
	| Field                | Value                    |
	| Name                 | Role for business unit 1 |
	| Business Unit        | Business Unit 1          |
	| Access to mytime web | true                     |
	And there is a role with
	| Field                | Value                    |
	| Name                 | Role for business unit 2 |
	| Business Unit        | Business Unit 2          |
	| Access to mytime web | true                     |

@WindowsAsDefaultIdentityProviderLogon
Scenario: Sign in without being a windows user in CCC
	Given I have the role 'Role for business unit 1'
	And I am a user with
	| Field                  | Value |
	| Windows authentication | false |
	When I go to mytime web
	Then I should see a log on error 'IdentityLogonMissing'
@WindowsAsDefaultIdentityProviderLogon
Scenario: Sign in as a windows user in CCC
	Given I have the role 'Role for business unit 1'
	And I am a user with
	| Field                  | Value |
	| Windows authentication | true  |
	When I go to mytime web
	Then I should be signed in

@WindowsAsDefaultIdentityProviderLogon
Scenario: Sign in as a windows user in CCC with multiple business units
	Given I have the role 'Role for business unit 1'
	And I have the role 'Role for business unit 2'
	And I am a user with
	| Field                  | Value |
	| Windows authentication | true  |
	When I go to mytime web
	And I select business unit 'Business Unit 1'
	Then I should be signed in