Feature: 21211 View Monthly schedule
As an agent 
I need to see my schedule for a whole month at a time, 
so that I get a good overview of shifts, full day absences and days off (i.e. full day events), 
so that I can easier plan my weekends, holidays etc.

Background: 
Given there is a role with
	| Field                           | Value                               |
	| Name                            | AgentCouldNotSeeUnpublishedSchedule |
	| CanSeeUnpublishedSchedule       | false                               |
	And there are shift categories 
	| Name  |
	| Early |
	And there is a dayoff with
	| Field | Value  |
	| Name  | DayOff |
	And there is an activity with
	| Field | Value |
	| Name  | Phone |
	| Color | Green |
	And I have a workflow control set with
	| Field                      | Value              |
	| Name                       | Published schedule |
	| Schedule published to date | 2014-02-01         |
	And I have a schedule period with 
	| Field      | Value      |
	| Start date | 2013-08-19 |
	| Type       | Week       |
	| Length     | 1          |
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2013-08-19 |
    And I have the workflow control set 'Published schedule'
       
Scenario: View full month with full first and last week
Given I am an agent
When I navigate to month view for '2014-01-07'
Then I should see '2013-12-30' as the first day 
And I should see '2014-02-02' as the last day

Scenario: View when you are working
Given I am an agent
And I have a shift with
	| Field          | Value            |
	| StartTime      | 2014-01-07 09:00 |
	| EndTime        | 2014-01-07 18:00 |
	| Shift category | Early            |
When I navigate to month view for '2014-01-07'
Then I should see an indication implying I should work on '2014-01-07'

Scenario: View when you are not working
Given I am an agent
	And I have a day off with
	| Field | Value      |
	| Name  | DayOff     |
	| Date  | 2014-01-07 |
When I navigate to month view for '2014-01-07'
Then I should see an indication implying I should not work on '2014-01-07'

Scenario: View when you have full day absence 
Given I am an agent
	And I have a full-day absence today with
	| Field         | Value      |
	| Date          | 2014-01-07 |
When I navigate to month view for '2014-01-07'
Then I should see an indication implying I should not work on '2014-01-07'

Scenario: Distinguish day out of current month 
Given I am an agent 
When I navigate to month view for '2014-01-07'
Then I should see the day '2013-12-30' is not part of current month 
And I should see the day '2014-02-01' is not part of current month


Scenario: Do not show unpublished schedule 
Given I am an agent 
And I have a shift with
	| Field          | Value            |
	| StartTime      | 2014-02-02 09:00 |
	| EndTime        | 2014-02-02 18:00 |
	| Shift category | Early            |
When I navigate to month view for '2014-02-02'
Then I should not see any indication for day '2014-02-02'

Scenario: Do not show indication for an empty day 
Given I am an agent 
When I navigate to month view for '2014-01-02'
Then I should not see any indication for day '2014-01-02'


Scenario: Language setting
Given I am an agent
And I have my language set to German
When I navigate to month view for '2014-01-07'
Then I should see the month name 'Januar'

Scenario: First day of week
Given I am an agent
And I am american
When I navigate to month view for '2014-01-07'
Then I should see '2013-12-29' as the first day 
And I should see '2014-02-01' as the last day

Scenario: Can switch to monthly schedule when showing weekly schedule 
Given I am an agent 
When I view my week schedule for date '2014-01-07'
And I choose to go to month view
Then I should end up in month view for '2014-01-07'

Scenario: View current month
Given I am an agent
When I navigate to month view
Then I should end up in month view for current month

Scenario: Can switch to weekly schedule when showing monthly schedule 
Given I am an agent 
When I navigate to month view for '2014-01-07'
And I choose to go to week view
Then I should end up in week view for '2014-01-07'

Scenario: Navigate to next month
Given I am an agent
And I navigate to month view for '2014-01-07'
When I choose to go to next month
Then I should end up in month view for '2014-02-01' 

Scenario: Navigate to previous month
Given I am an agent
And I navigate to month view for '2014-01-07'
When I choose to go to previous month
Then I should end up in month view for '2013-12-01' 

Scenario: Pick a month in the calendar
Given I am an agent
And I navigate to month view for '2014-01-07'
When I select a month '2014-05' in the calendar 
Then I should end up in month view for '2014-05-01'
