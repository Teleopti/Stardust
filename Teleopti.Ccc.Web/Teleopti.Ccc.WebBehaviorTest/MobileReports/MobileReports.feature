Feature: MobileReports
 In order to keep track on my CC
 As a Supervisor on the move
 I want to see reports on my mobile

Background:
	Given there is a role with
    | Field                    | Value                    |
    | Name                     | Access to mobile reports |
    | Access to mobile reports | true                     |
	And there is a role with
	| Field                    | Value                       |
	| Name                     | No access to mobile reports |
	| Access to mobile reports | false                       |

Scenario: Enter Application 
	Given I have the role 'Access to mobile reports'
	When I view MobileReports
	Then I should see ReportSettings

Scenario: Default report settings
	Given I have the role 'Access to mobile reports'
	When I view ReportSettings
	Then I should see ReportSettings with default value

Scenario: Enter Application without permission
	Given I have the role 'No access to mobile reports'
	When I view MobileReports
	Then I should see friendly error message

Scenario: View Report
	Given I have the role 'Access to mobile reports'
	And I have analytics data for today
	And I have analytics fact queue data
	When I view ReportSettings
	And I select a report
	And I select date today
	And I check type Graph
	And I check type Table
	And I click View Report Button
	Then I should see a report
	And I should see a graph 
	And I should see a table

Scenario: Select date in date-picker
	Given I have the role 'Access to mobile reports'
	When I view ReportSettings
	And I open the date-picker
	And I click on any date
	Then the date-picker should close
	And I should see the selected date

@Chrome
Scenario: Select skill in skill-picker
	Given I have the role 'Access to mobile reports'
	And I have analytics data for today
	And I have skill analytics data
	When I view ReportSettings
	And I open the skill-picker
	And I select a skill
	And I close the skill-picker
	Then I should see the selected skill

@Chrome
Scenario: Select all skills item in skill-picker
	Given I have the role 'Access to mobile reports'
	When I view ReportSettings
	And I open the skill-picker
	And I select the all skills item
	And I close the skill-picker
	Then I should see the all skill item selected

Scenario: Navigate within report view to previous day
	Given I have the role 'Access to mobile reports'
	And I have analytics data for the current week
	And I am viewing a report
	When I click previous date
	Then I should see a report for previous date

Scenario: Navigate within report view to next day
	Given I have the role 'Access to mobile reports'
	And I have analytics data for the current week
	And I am viewing a report
	When I click next date
	Then I should see a report for next date

Scenario: Enter Application with partial access to reports
	Given I am user with partial access to reports
	When I view ReportSettings
	Then I should only see reports i have access to

Scenario: Tabledata shows sunday as first day of week for US culture
	Given I have the role 'Access to mobile reports'
	And I am american
	And I have analytics data for the current week
	And I have analytics fact queue data
	When I view a report with week data
	Then I should see sunday as the first day of week in tabledata

