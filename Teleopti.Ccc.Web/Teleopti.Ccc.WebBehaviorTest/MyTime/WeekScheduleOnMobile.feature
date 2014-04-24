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
	And I have a schedule period with 
	| Field      | Value      |
	| Start date | 2014-04-14 |
	| Type       | Week       |
	| Length     | 1          |
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2014-04-14 |
	And there are shift categories
	| Name  |
	| Early |
	And there is a dayoff with
	| Field | Value  |
	| Name  | DayOff |

Scenario: No access to schedule page
	Given I have the role 'Only access to Anywhere'
	And I am viewing Anywhere
	When I manually navigate to mobile week schedule page
	Then I should see an error message

Scenario: View current week
	Given I have the role 'Full access to mytime'
	And the current time is '2030-10-03 12:00'
	When I view my mobile week schedule
	Then I should see my mobile week schedule for date '2030-10-03'

Scenario: Navigate to desktop view
	Given I have the role 'Full access to mytime'
	And the current time is '2014-04-15 12:00'
	And I am viewing my mobile week schedule
	When I click the desktop link
	Then I should see my week schedule for date '2014-04-15'

@ignore	
Scenario: Navigate from desktop
	Given I have the role 'Full access to mytime'
	And the current time is '2014-04-15 12:00'
	And I am viewing my week schedule
	When I click the mobile link
	Then I should see my mobile week schedule for date '2014-04-15'

Scenario: View when you are working
	Given I have the role 'Full access to mytime'
	And the current time is '2014-04-21 12:00'
	And I have the workflow control set 'Published schedule'
	And I have a shift with
	| Field          | Value            |
	| StartTime      | 2014-04-21 09:00 |
	| EndTime        | 2014-04-21 18:00 |
	| Shift category | Early            |
	When I view my mobile week schedule
	Then I should see the shift with
	| Field          | Value         |
	| Date           | 2014-04-21    |
	| Time span      | 09:00 - 18:00 |
	| Shift category | Early         |

	@ignore
Scenario: View when you have a day off
	Given I have the role 'Full access to mytime'
	And the current time is '2014-04-22 12:00'
	And I have the workflow control set 'Published schedule'
	And I have a day off with
	| Field | Value      |
	| Name  | DayOff     |
	| Date  | 2014-04-22 |
	When I view my mobile week schedule
	Then I should see the day off on '2014-04-22'
