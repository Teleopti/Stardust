﻿Feature: Password Policy
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

Scenario: Change password failed against the policy
	Given I am a user with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	And I am signed in with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	When I view password setting page
	And I change my password with
	| Field              | Value     |
	| Password           | aa        |
	| Confirmed Password | aa        |
	| Old Password       | P@ssword1 |
	Then I should see password changed failed with message

Scenario: Sign in failed after account is locked
	Given I have user logon details with
	| Field    | Value |
	| IsLocked | true  |
	And I am a user with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	And I am viewing the sign in page
	When I try to sign in with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	Then I should not be signed in
	And I should see an log on error

Scenario: Sign in with password will expire soon
	Given Current time is '2012-01-30'
	And I have user logon details with
	| Field                | Value      |
	| Last Password Change | 2012-01-01 |
	And I am a user with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	And I am viewing the sign in page
	When I try to sign in with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	Then I should be signed in
	And I should see a warning message that password will be expired

Scenario: Sign in with password already expired
	Given Current time is '2012-01-31'
	And I have user logon details with
	| Field                | Value      |
	| Last Password Change | 2012-01-01 |
	And I am a user with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	And I am viewing the sign in page
	When I try to sign in with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	Then I should not be signed in
	And I should be see the must change password page
	And I should see an error message password has already expired

Scenario: Navigate to other page when sign in with password already expired
	Given Current time is '2012-01-31'
	And I have user logon details with
	| Field                | Value      |
	| Last Password Change | 2012-01-01 |
	And I am a user with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	And I am viewing the sign in page
	When I try to sign in with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	And I navigate to week schedule page
	Then I should see the sign in page

Scenario: Change password successfully when password already expired
	Given Current time is '2012-01-31'
	And I have user logon details with
	| Field                | Value      |
	| Last Password Change | 2012-01-01 |
	And I am a user with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	And I am viewing the sign in page
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

Scenario: Change password failed when password already expired
	Given Current time is '2012-01-31'
	And I have user logon details with
	| Field                | Value      |
	| Last Password Change | 2012-01-01 |
	And I am a user with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	And I am viewing the sign in page
	When I try to sign in with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	And I change my password with
	| Field              | Value     |
	| Password           | aa        |
	| Confirmed Password | aa        |
	| Old Password       | P@ssword1 |
	Then I should see an error message password changed failed
	And I should not be signed in
