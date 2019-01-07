@PasswordPolicy
Feature: ResetPassword
	In order to change password
	As a user
	I want to be able to get an email about further instructions

Background:
	Given there is a business unit with
	| Field | Value           |
	| Name  | Business Unit 1 |
	And there is a role with
	| Field                | Value                    |
	| Name                 | Role for business unit 1 |
	| Business Unit        | Business Unit 1          |
	| Access to mytime web | true                     |

@OnlyRunIfEnabled('Wfm_User_Password_Reset_74957')
Scenario: Canceling when at the sending in user information
	Given I am viewing the sign in page
	And I have user logon details with
	| Field    | Value |
	| IsLocked | false  |
	And I have user credential with
	| Field    | Value     |
	| UserName | ashley.andeen@insurance.com |
	| Password | P@ssword1 |
	When I choose to click reset password
	And I click the cancel button at the user submit form
	Then I should be redirected to the sign in page

@OnlyRunIfEnabled('Wfm_User_Password_Reset_74957')
Scenario: Submit user information
	Given I am viewing the sign in page
	And I have user logon details with
	| Field    | Value |
	| IsLocked | false  |
	And I have user credential with
	| Field    | Value     |
	| UserName | ashley.andeen@insurance.com |
	| Password | P@ssword1 |
	When I choose to click reset password
	And I choose to enter username in reset password form with 'Ashley'
	And I click send password button
	Then I should see a notification about an email about further instructions

@OnlyRunIfEnabled('Wfm_User_Password_Reset_74957')
Scenario: Viewing reset page with invalid token
	Given I am viewing the reset password form with invalid token
	Then I should see an error message about an invalid token

@OnlyRunIfEnabled('Wfm_User_Password_Reset_74957')
Scenario: Viewing reset page with valid token and submiting mismatch of passwords
	Given I have user credential with
	| Field    | Value     |
	| UserName | bill.andeen@insurance.com |
	| Password | P@ssword1 |
	When I view the reset password form as 'bill.andeen@insurance.com' with password 'P@ssword1'
	And I fill the reset password form with mismatch of passwords
	And press the reset password submit form button
	Then I should see a error message for the reset password form

@OnlyRunIfEnabled('Wfm_User_Password_Reset_74957')
Scenario: Viewing reset page with valid token and submiting passwords not matching the password policy
	Given I have user credential with
	| Field    | Value     |
	| UserName | bill.andeen@insurance.com |
	| Password | P@ssword1 |
	When I view the reset password form as 'bill.andeen@insurance.com' with password 'P@ssword1'
	And I fill the reset password form with shorter password than the policy
	And press the reset password submit form button
	Then I should see a policy error message for the reset password form

@OnlyRunIfEnabled('Wfm_User_Password_Reset_74957')
Scenario: Viewing reset page with valid token and then canceling
	Given I have user credential with
	| Field    | Value     |
	| UserName | bill.andeen@insurance.com |
	| Password | P@ssword1 |
	When I view the reset password form as 'bill.andeen@insurance.com' with password 'P@ssword1'
	And press the reset password cancel button
	Then I should be redirected to the sign in page

@OnlyRunIfEnabled('Wfm_User_Password_Reset_74957')
Scenario: Viewing reset page with valid token and then submiting correct information
	Given I have user logon details with
	| Field    | Value |
	| IsLocked | false  |
	And I have user credential with
	| Field    | Value     |
	| UserName | bill.andeen@insurance.com |
	| Password | P@ssword1 |
	And I have the role 'Role for business unit 1'
	When I view the reset password form as 'bill.andeen@insurance.com' with password 'P@ssword1'
	And I fill the reset password form with password 'password'
	And press the reset password submit form button
	Then I should see a success message for the reset password form
	When I press the reset password form logon button
	Then I should be redirected to the sign in page
	When I sign in by username 'bill.andeen@insurance.com' and password 'password'
	Then I should be signed in
