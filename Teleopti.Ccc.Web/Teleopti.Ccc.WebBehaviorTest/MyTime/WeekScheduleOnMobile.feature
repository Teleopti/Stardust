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
	| Field                      | Value                               |
	| Name                       | Published schedule until 2014-04-30 |
	| Schedule published to date | 2014-04-30                          |
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
	And there is an absence with
	| Field      | Value   |
	| Name       | Illness |
	| Short name | IL      |
	| Color      | Red     |

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

Scenario: View when you have a day off
	Given I have the role 'Full access to mytime'
	And the current time is '2014-04-22 12:00'
	And I have the workflow control set 'Published schedule'
	And I have a day off with
	| Field | Value      |
	| Name  | DayOff     |
	| Date  | 2014-04-22 |
		And I have a shift with
	| Field          | Value            |
	| StartTime      | 2014-04-21 09:00 |
	| EndTime        | 2014-04-21 18:00 |
	| Shift category | Early            |
	When I view my mobile week schedule
	Then I should see the day off on '2014-04-22'

Scenario: View when you have full day absence
   Given I have the role 'Full access to mytime'
   And I have the workflow control set 'Published schedule'
   And I have an absence with
   | Field      | Value            |
   | Name       | Illness          |
   | Start time | 2014-04-15 00:00 |
   | End time   | 2014-04-15 23:59 |
   When I view my mobile week schedule for date '2014-04-15'
   Then I should see the absence with
   | Field | Value      |
   | Name  | Illness    |
   | Date  | 2014-04-15 |

Scenario: View when you have a full day absence on working day
   Given I have the role 'Full access to mytime'
   And I have the workflow control set 'Published schedule'
   And I have a shift with
   | Field          | Value            |
   | StartTime      | 2014-04-15 09:00 |
   | EndTime        | 2014-04-15 18:00 |
   | Shift category | Early            |
   And I have an absence with
   | Field                   | Value            |
   | Name      | Illness          |
   | StartTime | 2014-04-15 09:00 |
   | EndTime   | 2014-04-15 18:00 |
   When I view my mobile week schedule for date '2014-04-15'
   Then I should not see a shift on date '2014-04-15'
   And I should see the absence with
   | Field | Value      |
   | Name  | Illness    |
   | Date  | 2014-04-15 |

Scenario: View when you are in absence on day off
   Given I have the role 'Full access to mytime'
   And I have the workflow control set 'Published schedule'
   And I have a day off with
   | Field | Value      |
   | Name  | DayOff     |
   | Date  | 2014-04-15 |
   And I have an absence with
   | Field     | Value            |
   | Name      | Illness          |
   | StartTime | 2014-04-15 00:00 |
   | EndTime   | 2014-04-15 23:59 |
   When I view my mobile week schedule for date '2014-04-15'
	Then I should not see dayoff on date '2014-04-15'
   And I should see the absence with
   | Field | Value      |
   | Name  | Illness    |
   | Date  | 2014-04-15 |

Scenario: Do not show unpublished schedule
   Given I have the role 'Full access to mytime'
   And I have the workflow control set 'Published schedule until 2014-04-30'
   And I have a shift with
   | Field          | Value            |
   | StartTime      | 2014-05-01 09:00 |
   | EndTime        | 2014-05-01 18:00 |
   | Shift category | Early            |
   When I view my mobile week schedule for date '2014-05-01'
   Then I should not see any shift for day '2014-05-01'

Scenario: Language setting
   Given I have the role 'Full access to mytime'
   And I have the workflow control set 'Published schedule'
   And I am german
   When I view my mobile week schedule for date '2014-01-01'
   Then I should see a day name being 'MONTAG'

Scenario: First day of week
   Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Published schedule'
   And I am american
   When I view my mobile week schedule for date '2014-04-15'
   Then I should see '2014-04-13' as the first day
   And I should see 'SUNDAY' as the first day of week label
