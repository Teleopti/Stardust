Feature: MobileReports
 In order to keep track on my CC
 As a Supervisor on the move
 I want to see reports on my mobile

Background:
	Given I browse with a mobile
	
Scenario: Enter Application 
	Given I am a supervisor
	When I view MobileReports
	Then I should see ReportSettings

Scenario: Enter Application without permission
	Given I am user without permission to MobileReports
	When I view MobileReports
	Then I should see friendly error message

Scenario: Logout from application
	Given I am a supervisor
	And I view MobileReports
	And I click Signout button
	Then I should be signed out

Scenario: Enter Application with report preference
	Given I am a supervisor
	And I have previously viewed reports
	When I enter MobileReports
	Then I should see a report 
	And date of should be today

Scenario: View Report
	Given I am a supervisor
	When I view ReportSettings
	And I select a report
	And I check type Graph
	And I check type Table
	And I click View Report Button
	Then I should se a report
	And I should see a graph 
	And I should see a table

Scenario: Select date in date-picker
	Given I am a supervisor
	When I view ReportSettings
	And I open the date-picker
	And I click on any date
	Then the date-picker should close
	And I should see the selected date

Scenario: Select skill in skill-picker
	Given I am a supervisor
	And I have skill statistic data
	When I view ReportSettings
	And I open the skill-picker
	And I select a skill
	And I close the skill-picker
	Then I should see the selected skill

Scenario: Select all skills item in skill-picker
	Given I am a supervisor
	When I view ReportSettings
	And I open the skill-picker
	And I select the all skills item
	And I close the skill-picker
	Then I should see the all skill item selected

Scenario: Navigate within report view to previous day
	Given I am a supervisor
	When I am view a Report
	And I click previous date
	Then I should see a report for previous date

Scenario: Navigate within report view to next day
	Given I am a supervisor
	When I am view a Report
	And I click next date
	Then I should see a report for next date

Scenario: Enter Application with partial access to reports
	Given I am user with partial access to reports
	When I view ReportSettings
	Then I should only see reports i have access to

Scenario: Tabledata shows sunday as first day of week for US culture
	Given I am a supervisor
	And I am american
	When I am view a Report with week data
	Then I should see sunday as the first day of week in tabledata