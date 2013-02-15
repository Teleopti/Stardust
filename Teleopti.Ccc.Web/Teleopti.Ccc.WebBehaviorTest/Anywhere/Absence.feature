@Ignore 
Feature: Report absence on agents
	In order to keep track of agents absences
	As a team leader
	I want to add absence for an agent

Background:
	Given there is a role with
	| Field                      | Value                    |
	| Name                       | Full access to Anywhere  |
	| Access to Anywhere         | true                     |
	| View unpublished schedules | true                     |
	And there is a team with
	| Field | Value            |
	| Name  | Team green       |
	Given there is a team member with
	| Field        | Value        |
	| Name         | Pierre Baldi |
	| TerminalDate | 2012-12-31   |
	And there is a person period for 'Pierre Baldi' with
	| Field           | Value            |
	| Team            | Team green       |
	| StartDate       | 2012-12-01       |
	And there is an activity with
	| Field | Value |
	| Name  | Lunch |
	And there is an activity with
	| Field | Value |
	| Name  | Phone |

Scenario: Open add absence form from tab add absence
	Given I am a team leader for 'Team green' with role 'Full access to Anywhere'
	And there is a shift with
	| Field          | Value        |
	| Person         | Pierre Baldi |
	| Date           | 2012-12-02   |
	| StartTime      | 08:00        |
	| EndTime        | 17:00        |
	| Activity       | Phone        |
	| LunchStartTime | 11:30        |
	| LunchEndTime   | 12:15        |
	| LunchActivity  | Lunch        |
	And I view agent schedule for 'Pierre Baldi' for date '2012-12-02'
	When I click on the add absence tab
	Then I should see the add absence form
	
Scenario: Default absence start and end time values from agents schedule
	Given I am a team leader for 'Team green' with role 'Full access to Anywhere'
	And there is a shift with
	| Field          | Value        |
	| Person         | Pierre Baldi |
	| Date           | 2012-12-02   |
	| StartTime      | 08:00        |
	| EndTime        | 17:00        |
	| Activity       | Phone        |
	| LunchStartTime | 11:30        |
	| LunchEndTime   | 12:15        |
	| LunchActivity  | Lunch        |
	And I view agent schedule for 'Pierre Baldi' for date '2012-12-02'
	When I click on the add absence tab
	Then I should see the add absence form with 08:00 - 17:00 as the default times
	
#Is there supposted to be a Cancel button or should absence form be hidden when click outside of form?
Scenario: Cancel adding absence
	Given I am a team leader for 'Team green' with role 'Full access to Anywhere'
	And there is a shift with
	| Field          | Value        |
	| Person         | Pierre Baldi |
	| Date           | 2012-12-02   |
	| StartTime      | 08:00        |
	| EndTime        | 17:00        |
	| Activity       | Phone        |
	| LunchStartTime | 11:30        |
	| LunchEndTime   | 12:15        |
	| LunchActivity  | Lunch        |
	And I view agent schedule for 'Pierre Baldi' for date '2012-12-02'
	When I click on the add absence tab
	And I see the add absence form
	And I click the cancel button
	Then I should not see an absence for 'Pierre Baldi' for date '2012-12-02'

Scenario: Adding invalid absence values
	Given I am a team leader for 'Team green' with role 'Full access to Anywhere'
	And there is a shift with
	| Field          | Value        |
	| Person         | Pierre Baldi |
	| Date           | 2012-12-02   |
	| StartTime      | 08:00        |
	| EndTime        | 17:00        |
	| Activity       | Phone        |
	| LunchStartTime | 11:30        |
	| LunchEndTime   | 12:15        |
	| LunchActivity  | Lunch        |
	And I view agent schedule for 'Pierre Baldi' for date '2012-12-02'
	When I click on the add absence tab
	And I see the add absence form
	And I input later start time than end time
	And I click the apply button
	Then I should not see an absence for 'Pierre Baldi' for date '2012-12-02'

Scenario: View absence type
	Given I am a team leader for 'Team green' with role 'Full access to Anywhere'
	And there is a shift with
	| Field          | Value        |
	| Person         | Pierre Baldi |
	| Date           | 2012-12-02   |
	| StartTime      | 08:00        |
	| EndTime        | 17:00        |
	| Activity       | Phone        |
	| LunchStartTime | 11:30        |
	| LunchEndTime   | 12:15        |
	| LunchActivity  | Lunch        |
	And I view agent schedule for 'Pierre Baldi' for date '2012-12-02'
	And there is an absence with
	| Field		| Value            |
	| Name      | Vacation         |
	| StartTime | 2012-12-02 00:00 |
	| EndTime   | 2012-12-02 23:59 |
	Then I should see an absence for 'Pierre Baldi' for date '2012-12-02' and with start time '00:00' and end time '23:59' in schedule with type called 'Vacation'

Scenario: Add absence of type illness
	Given I am a team leader for 'Team green' with role 'Full access to Anywhere'
	And there is a shift with
	| Field          | Value        |
	| Person         | Pierre Baldi |
	| Date           | 2012-12-02   |
	| StartTime      | 08:00        |
	| EndTime        | 17:00        |
	| Activity       | Phone        |
	| LunchStartTime | 11:30        |
	| LunchEndTime   | 12:15        |
	| LunchActivity  | Lunch        |
	And I view agent schedule for 'Pierre Baldi' for date '2012-12-02'
	And I click on the add absence tab
	And I add an absence with
	| Field		| Value            |
	| Name      | Illness          |
	| StartTime | 2012-12-02 08:00 |
	| EndTime   | 2012-12-02 17:00 |
	And I click on the apply button
	Then I should see an absence for 'Pierre Baldi' for date '2012-12-02' and with start time '08:00' and end time '17:00' in schedule with type called 'Illness'


