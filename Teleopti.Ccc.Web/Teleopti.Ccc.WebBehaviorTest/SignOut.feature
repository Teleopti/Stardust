Feature: Sign out
	In order to leave the site
	As a user
	I want to be able to sign out

@WindowsAsDefaultIdentityProviderLogon
Scenario Outline: Logon as another user
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
	When I logon to mytime web
	And I choose to sign out
	And I choose teleopti identity provider
	And I try to signin with
	| Field    | Value    |
	| UserName | Ashley   |
	| Password | password |
	Then I should be signed in as another user 'Ashley'
		Examples: 
	| Key |
	| 1   |
	| 2   |
	| 3   |
	| 4   |
	| 5   |
	| 6   |
	| 7   |
	| 8   |
	| 9   |
	| 10   |
	| 11   |
	| 12   |
	| 13   |
	| 14   |
	| 15   |
	| 16   |
	| 17   |
	| 18   |
	| 19   |
	| 20   |
	| 21   |
	| 22   |
	| 23   |
	| 24   |
	| 25   |
	| 26   |
	| 27   |
	| 28   |
	| 29   |
	| 30   |
	| 31   |
	| 32   |
	| 33   |
	| 34   |
	| 35   |
	| 36   |
	| 37   |
	| 38   |
	| 39   |
	| 40   |
	| 41   |
	| 42   |
	| 43   |
	| 44   |
	| 45   |
	| 46   |
	| 47   |
	| 48   |
	| 49   |
	| 50   |
	| 51   |
	| 52   |
	| 53   |
	| 54   |
	| 55   |
	| 56   |
	| 57   |
	| 58   |
	| 59   |
