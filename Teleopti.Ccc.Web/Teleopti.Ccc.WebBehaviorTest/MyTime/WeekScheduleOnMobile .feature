@WeekSchedule
Feature: View week schedule on mobile
	In order to know how to work this week
	As an agent
	I want to see my schedule details on mobile phone
	
Background:
	Given there is a role with
	| Field                    | Value                 |
	| Name                     | Full access to mytime |
	Given there is a role with
	| Field                          | Value                   |
	| Name                           | Only access to Anywhere |
	| Access To Anywhere             | true                    |
	| Access To Mytime Web           | false                   |
	| Access To Asm                  | false                   |
	| Access To Text Requests        | false                   |
	| Access To Absence Requests     | false                   |
	| Access To Shift Trade Requests | false                   |
	| Access To Text Requests        | false                   |
	| Access To Extended Preferences | false                   |
	| Access To Preferences          | false                   |
	| Access To Team Schedule        | false                   |
	And there is a workflow control set with
	| Field                      | Value              |
	| Name                       | Published schedule |
	| Schedule published to date | 2040-06-24         |
	And there is a workflow control set with
	| Field                                 | Value                            |
	| Name                                  | Published schedule to 2012-08-28 |
	| Schedule published to date            | 2012-08-28                       |
	| Preference period is closed           | true                             |
	| Student availability period is closed | true                             |
	And I have a schedule period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Type       | Week       |
	| Length     | 1          |
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	And there are shift categories
	| Name  |
	| Night |
	| Day   |
	| Late  |
	And there are activities
	| Name           | Color  |
	| Lunch          | Yellow |
	| White activity | White  |
	| Black activity | Black  |

@ignore
Scenario: No access to schedule page
	Given I have the role 'Only access to Anywhere'
	And I navigate to Anywhere
	And I select application logon data source
	And I sign in
	And I should see Anywhere
	When I manually navigate to mobile week schedule page
	Then I should see an error message

@ignore
Scenario: View current week
	Given I have the role 'Full access to mytime'
	And the current time is '2030-10-03 12:00'
	When I view my mobile week schedule for date '2030-10-03'
	Then I should see mobile view of the week with for date '2030-10-03'
