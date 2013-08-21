﻿Feature: View confidential absence
	In order to keep privacy of persons absences
	As a team leader
	I have permissions to see confidential absence names for agents in my team
	
Background:
	Given there is a team with
	| Field | Value      |
	| Name  | Team green |
	And there is a role with
	| Field                      | Value                 |
	| Name                       | Can View Confidential |
	| Access to team             | Team green            |
	| Access to Anywhere         | true                  |
	| View unpublished schedules | true                  |
	| View confidential          | true                  |
	And there is a role with
	| Field                      | Value                    |
	| Name                       | Cannot View Confidential |
	| Access to team             | Team green               |
	| Access to Anywhere         | true                     |
	| View unpublished schedules | true                     |
	| View confidential          | false                    |
	And 'Pierre Baldi' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2013-08-01 |
	And there is an absence with
	| Field        | Value    |
	| Name         | Vacation |
	| Confidential | true     |
	| Color        | Red      |
	
#team schedule view
Scenario: Cannot view confidential absence in team view when no permission
	Given I have the role 'Cannot View Confidential'
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-08-10 00:00 |
	| End time   | 2013-08-10 23:59 |
	When I view schedules for '2013-08-10'
	Then I should see 'Pierre Baldi' with absence
	| Field | Value |
	| Color | gray  |

Scenario: View confidential absence in team view when permitted
	Given I have the role 'Can View Confidential'
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-08-10 00:00 |
	| End time   | 2013-08-10 23:59 |
	When I view schedules for '2013-08-10'
	Then I should see 'Pierre Baldi' with absence
	| Field | Value |
	| Color | Red   |


#person schedule view
Scenario: Cannot view confidential absence in person view when no permission
	Given I have the role 'Cannot View Confidential'
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-08-10 00:00 |
	| End time   | 2013-08-10 23:59 |
	When I view person schedule for 'Pierre Baldi' on '2013-08-10'
	Then I should see a shift layer with
	| Field      | Value |
	| Start time | 08:00 |
	| End time   | 16:00 |
	| Color      | gray  |
	And I should see an absence in the absence list with
	| Field      | Value            |
	| Name       | Övrigt           |
	| Color      | gray             |
	| Start time | 2013-08-10 00:00 |
	| End time   | 2013-08-10 23:59 |

Scenario: View confidential absence in person view when permitted
	Given I have the role 'Can View Confidential'
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-08-10 00:00 |
	| End time   | 2013-08-10 23:59 |
	When I view person schedule for 'Pierre Baldi' on '2013-08-10'
	Then I should see a shift layer with
	| Field      | Value |
	| Start time | 08:00 |
	| End time   | 16:00 |
	| Color      | Red   |
	And I should see an absence in the absence list with
	| Field      | Value            |
	| Name       | Vacation         |
	| Color      | Red              |
	| Start time | 2013-08-10 00:00 |
	| End time   | 2013-08-10 23:59 |