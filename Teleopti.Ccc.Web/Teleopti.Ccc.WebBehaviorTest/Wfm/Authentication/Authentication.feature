@WFM
Feature: Authentication
	As an agent
	I want to be able to change my password

Background: 
	Given I am an agent with password 'tdemo'
	And I have a role with full access
	And I view people


@OnlyRunIfEnabled('Wfm_Authentication_ChangePasswordMenu_76666')
Scenario: Password changes when changing password
	Given I navigate to change your password
	And I enter 'tdemo' in 'data-test-input-current-password'
	And I enter 'newPassword' in 'data-test-input-new-password'
	And I enter 'newPassword' in 'data-test-input-confirm-password'
	When I click Ok
	Then The password modal should close
	

