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

@WindowsAndApplicationLogon
Scenario: Sign in without being a windows user in CCC
	Given I have the role 'Role for business unit 1'
	And I am a user with
	| Field                  | Value |
	| Windows authentication | false |
	When I go to mytime web
	Then I should see a log on error 'NoDatasource'
@WindowsAndApplicationLogon
Scenario: Sign in as a windows user in CCC
	Given I have the role 'Role for business unit 1'
	And I am a user with
	| Field                  | Value |
	| Windows authentication | true  |
	When I go to mytime web
	Then I should be signed in

@ExtraDataSource
@WindowsAndApplicationLogon
Scenario: Sign in as a windows user in CCC with multiple data sources
	Given I have the role 'Role for business unit 1'
	And I am a user with
	| Field                  | Value |
	| Windows authentication | true  |
	When I go to mytime web
	And I select one data source
	And I sign in by windows credentials
	Then I should be signed in
@WindowsAndApplicationLogon
Scenario: Sign in as a windows user in CCC with multiple business units
	Given I have the role 'Role for business unit 1'
	And I have the role 'Role for business unit 2'
	And I am a user with
	| Field                  | Value |
	| Windows authentication | true  |
	When I go to mytime web
	And I select business unit 'Business Unit 1'
	Then I should be signed in
@WindowsAndApplicationLogon
@ExtraDataSource
Scenario: Sign in as a windows user in CCC with multiple data sources and multiple business units
	Given I have the role 'Role for business unit 1'
	And I have the role 'Role for business unit 2'
	And I am a user with
	| Field                  | Value |
	| Windows authentication | true  |
	When I go to mytime web
	And I select one data source
	And I sign in by windows credentials
	And I select business unit 'Business Unit 1'
	Then I should be signed in

@ignore
Scenario: Sign in as a windows user in CCC and then choose to sign in as another user
	Given I am an agent with permissions to log on as another user
	And Windows authentication can be used
	And Teleopti application authentication can be used
	And I am a user with
	| Field                  | Value |
	| Windows authentication | true  |
	When I view my week schedule
	And I select to sign in as another user
	Then I should see the sign in page

@ignore
Scenario: No choice for sign in as another user if no permission
	Given I am an agent
	And I am a user with
	| Field                  | Value |
	| Windows authentication | true  |
	When I view my week schedule
	Then I should not see the choice 'sign in as another user'

@ignore
Scenario: Sign out
	Given I am an agent
	When I view my week schedule
	And I sign out
	Then I should see the log out page