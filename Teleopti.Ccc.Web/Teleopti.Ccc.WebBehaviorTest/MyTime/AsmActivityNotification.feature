Feature: 18001 Alert agent before change of activity
As an agent 
I need to be alerted before change in activity,
so that I do not forget to switch from Backoffice to Phone at the right time,
so that I can get even better adherence to schedule.

Background:
         Given there is a role with
         | Field                    | Value                 |
         | Name                     | Full access to mytime |
         And there is a role with
         | Field         | Value            |
         | Name          | No access to ASM |
         | Access To Asm | False            |
         And I have a workflow control set with
         | Field                      | Value              |
         | Name                       | Published schedule |
         | Schedule published to date | 2040-06-24         |
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
         | Day  |
         And there is an activity with
         | Field | Value |
         | Name  | Phone |
         | Color | Green |
         And there is an activity with
         | Field | Value  |
         | Name  | Lunch  |
         | Color | Yellow |
         And I have a shift with
         | Field                         | Value            |
         | Shift category                | Day              |
         | Activity                      | Phone            |
         | StartTime                     | 2030-01-01 08:00 |
         | EndTime                       | 2030-01-01 17:00 |
         | Scheduled activity            | Lunch            |
         | Scheduled activity start time | 2030-01-01 11:00 |
         | Scheduled activity end time   | 2030-01-01 12:00 |

@ignore
Scenario: Do not alert agent without permission for ASM
         Given I have the role 'No access to ASM'
        And the current time is '2030-01-01 10:45'
         And Alert Time setting is '15 minutes'                      
         When I am viewing week schedule
         Then I should not see any alert
@ignore
Scenario: Alert agent before next activity happens
         Given I have the role 'Full access to Schedule '
        And the current time is '2030-01-01 10:40'
         And Alert Time setting is '20 minutes'                      
         When I am viewing week schedule
         Then I should not see an alert notifying 'Lunch at 11:00 !'
@ignore
Scenario: Do not alert agent Before Alert Time
         Given I have the role 'Full access to Schedule '
        And the current time is '2030-01-01 10:40'
         And Alert Time setting is '15 minutes'                      
         When I am viewing week schedule
         Then I should not see an alert notifying 'Lunch at 11:00 !'
@ignore
Scenario: Do not alert agent After Alert Time 
         Given I have the role 'Full access to Schedule '
        And the current time is '2030-01-01 11:45'
         And Alert Time setting is '20 minutes'                      
         When I am viewing week schedule
         Then I should not see an alert notifying 'Phone at 12:00 !'
@ignore
Scenario: Alert agent before first activity happens
         Given I have the role 'Full access to Schedule '
        And the current time is '2030-01-01 07:40'
         And Alert Time setting is '20 minutes'                      
         When I am viewing week schedule
         Then I should see an alert notifying 'Phone at 08:00 !'
@ignore
Scenario: Alert agent before last activity happens
         Given I have the role 'Full access to Schedule '
        And the current time is '2030-01-01 16:59:20'
         And Alert Time setting is '40 seconds'                      
         When I am viewing week schedule
         Then I should see one notify message
@ignore
Scenario: Pop up box disappear automaticly
         Given I have the role 'Full access to Schedule '
        And the current time is '2030-01-01 16:59:15'
         And Alert Time setting is '45 seconds' 
         And Auto Disappear setting is '30 seconds'                       
         When I am viewing week schedule
         Then I should not see pop up box
