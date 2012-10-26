Feature: Password Policy
	In order to have a good security
	As a user that is trying to sign in or change password
	I have a password policy

Background:
	Given I am an agent with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	And I have a password policy with
	| Field                         | Value                  |
	| MaxNumberOfAttempts           | 3                      |
	| InvalidAttemptWindow          | 30                     |
	| PasswordValidForDayCount      | 30                     |
	| PasswordExpireWarningDayCount | 3                      |
	| Rule1                         | PasswordLengthMin8     |


Scenario: Change password successfully against the policy
	Given I view change password page
	When I change my password with
	| Field             | Value        |
	| Password          | NewP@ssword1 |
	| ConfirmedPassword | NewP@ssword1 |
	| OldPassword       | P@ssword1    |
	Then I should see information with
	| Field           | Value |
	| Success updated | true  |

Scenario: Change password failed against the policy
	Given I view change password page
	When I change my password with
	| Field             | Value     |
	| Password          | aa        |
	| ConfirmedPassword | aa        |
	| OldPassword       | P@ssword1 |
	Then I should see information with
	| Field           | Value |
	| Success updated | false |
	| Reason          | Rule1 |

Scenario: Sign in failed after wrong password many times
	Given I input wrong password 3 times in 30 minutes
	And I am viewing the sign in page
	When I sign in with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	Then I should not be signed in
	And I should see an error message

Scenario: Sign in successfully after wrong password many times
	Given I input wrong password 3 times 30 minutes before
	And I am viewing the sign in page
	When I sign in with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	Then I should be signed in

Scenario: Sign in with password will expire soon 
	Given My password will expire in 2 days
	And I am viewing the sign in page
	When I sign in with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	Then I should be signed in
	And I should see a warning message with 
	| Field                 | Value       |
	| Message               | will expire |
	| Will expire in X days | 2           |

Scenario: Sign in with password already expired
	Given My password has already expired
	And I am viewing the sign in page
	When I sign in with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	Then I should not be signed in
	And I should be redirected to the changing password page
	And I should see a warning message with 
	| Field   | Value       |
	| Message | has expired |

Scenario: Navigate to week schedule page when sign in with password already expired
	Given My password has already expired
	And I am viewing the sign in page
	When I sign in with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	And I navigate to week schedule page
	Then I should be redirected to the sign in page

Scenario: Change password successfully when password already expired
	Given My password has already expired
	And I am viewing the sign in page
	When I sign in with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	And I change my password with
	| Field             | Value        |
	| Username          | aa           |
	| Password          | NewP@ssword1 |
	| ConfirmedPassword | NewP@ssword1 |
	| OldPassword       | P@ssword1    |
	Then I should be signed in

Scenario: Change password failed when password already expired
	Given My password has already expired
	And I am viewing the sign in page
	When I sign in with
	| Field    | Value     |
	| UserName | aa        |
	| Password | P@ssword1 |
	And I change my password with
	| Field             | Value     |
	| Username          | aa        |
	| Password          | aa        |
	| ConfirmedPassword | aa        |
	| OldPassword       | P@ssword1 |
	Then I should see information with
	| Field           | Value |
	| Success updated | false |
	| Reason          | Rule1 |
	And I should not be signed in

Scenario: Fail to change password with wrong old password
	Given My password has already expired
	And I am viewing the change password page
	When I change my password with
	| Field             | Value			|
	| Username          | aa			|
	| Password          | P@sswordNew	|
	| ConfirmedPassword | P@sswordNew	|
	| OldPassword       | P@sswordWrong	|
	Then I should see information with
	| Field           | Value |
	| Success updated | false |
	And I should not be signed in
	And I should remain on the screen
	And I should see the input fields get empty
	And I should see an error message

Scenario: Lock person after trying to change password many times
	Given I input wrong password 3 times in 30 minutes
	And I am viewing the change password page
	When I change my password with
	| Field             | Value			|
	| Username          | aa			|
	| Password          | P@sswordNew	|
	| ConfirmedPassword | P@sswordNew	|
	| OldPassword       | P@sswordWrong	|
	Then I should see information with
	| Field           | Value |
	| Success updated | false |
	And I should not be signed in
	And I should remain on the screen
	And I should see an error message

# scenario> Redirect to change password page if agent tries to navigate from the change password page after password has expired 



