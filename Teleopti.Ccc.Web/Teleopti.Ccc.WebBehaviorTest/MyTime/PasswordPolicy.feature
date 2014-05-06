@WatiN
@PasswordPolicy
Feature: Password Policy
	In order to have a good security
	As a user that is trying to sign in or change password
	I have a password policy

Background:
	Given There is a password policy with
	| Field                             | Value              |
	| Max Number Of Attempts            | 3                  |
	| Invalid Attempt Window            | 30                 |
	| Password Valid For Day Count      | 30                 |
	| Password Expire Warning Day Count | 3                  |
	| Rule1                             | PasswordLengthMin8 |
	And I have a role with
	| Field | Value |
	| Name  | Agent |

@ignore
Scenario: Change password fails against the policy
	Given I am a user signed in with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	When I view password setting page
	And I change my password in my profile with
	| Field              | Value     |
	| Password           | aa        |
	| Confirmed Password | aa        |
	| Old Password       | P@ssword1 |
	Then I should see password change failed with message

@ignore
Scenario: Sign in fails after account is locked
	Given I have user logon details with
	| Field    | Value |
	| IsLocked | true  |
	And I have user credential with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	When I try to sign in with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	Then I should not be signed in
	And I should see a log on error 'LogOnFailedAccountIsLocked'

@ignore
Scenario: See change password when password will expire soon
	Given I have user logon details with
	| Field                           | Value |
	| Last Password Change X Days Ago | 29    |
	And I have user credential with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	When I try to sign in with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	Then I should see change password page with warning 'Your password will expire soon'

@ignore
Scenario: Skip change password when password will expire soon
	Given I have user logon details with
	| Field                           | Value |
	| Last Password Change X Days Ago | 29    |
	And I have user credential with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	When I try to sign in with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	And I click skip button
	Then I should be signed in

@ignore
Scenario: See change password when password already expired
	Given I have user logon details with
	| Field                           | Value |
	| Last Password Change X Days Ago | 30    |
	And I have user credential with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	When I try to sign in with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	Then I should see must change password page with warning 'YourPasswordHasAlreadyExpired'
	And I should not see skip button

@ignore
Scenario: Manually navigate to other page when sign in with password already expired
	Given I have user logon details with
	| Field                           | Value |
	| Last Password Change X Days Ago | 30    |
	And I have user credential with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	When I try to sign in with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	And I manually navigate to week schedule page
	Then I should see the sign in page

@ignore
Scenario: Change password successfully
	Given I have user logon details with
	| Field                           | Value |
	| Last Password Change X Days Ago | 30    |
	And I have user credential with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	When I try to sign in with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	And I change my password with
	| Field              | Value        |
	| Password           | NewP@ssword1 |
	| Confirmed Password | NewP@ssword1 |
	| Old Password       | P@ssword1    |
	Then I should be signed in

@ignore
Scenario: Change password fails if new password is weak
	Given I have user logon details with
	| Field                           | Value |
	| Last Password Change X Days Ago | 30    |
	And I have user credential with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	When I try to sign in with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	And I change my password with
	| Field              | Value     |
	| Password           | aa        |
	| Confirmed Password | aa        |
	| Old Password       | P@ssword1 |
	Then I should see an error 'PasswordPolicyWarning'

@ignore
Scenario: Change password fails if old password is wrong
	Given I have user logon details with
	| Field                           | Value |
	| Last Password Change X Days Ago | 30    |
	And I have user credential with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	When I try to sign in with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	And I change my password with
	| Field              | Value         |
	| Password           | P@ssword2     |
	| Confirmed Password | P@ssword2     |
	| Old Password       | wrongPassword |
	Then I should see an error 'InvalidUserNameOrPassword'
