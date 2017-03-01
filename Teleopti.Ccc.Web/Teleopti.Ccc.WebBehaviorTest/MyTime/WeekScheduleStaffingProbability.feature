@ignore("For_function_description_only")
@OnlyRunIfEnabled('MyTimeWeb_ViewIntradayStaffingProbability_41608')
Feature: View staffing probability within week schedule
	In order to check if there is probability to get absence / overtime
	As an agent
	I want to see an indication of probability to get absence / overtime

Scenario: Hide staffing probability by default
	Given the time is '2017-02-20 20:00'
	And I have the role 'Full access to mytime'
	And I have the workflow control set 'Intraday head count check'
	When I view my week schedule for date '2017-02-20'
	Then I should see the probability type "Hide staffing probability" selected
	And I should not see staffing probability

Scenario: Show probability type option for current week only
	Given the time is '2017-02-20 20:00'
	And I have the role 'Full access to mytime'
	And I have the workflow control set 'Intraday head count check'
	When I view my week schedule for date '2017-02-28'
	Then I should see the probability type "Hide staffing probability" selected
	And I should not see staffing probability

Scenario: Show absence probability option only when check staffing by intraday or by intraday with shrinkage
	Given the time is '2017-02-20 20:00'
	And I have the role 'Full access to mytime'
	And I have the workflow control set with staffing check by "Intraday" or "Intraday With Shrinkage"
	When I view my week schedule for date '2017-02-20'
	Then I should not see the probability type "Absence probability" in probability type selection list

Scenario: Show absence probability within schedule time range
	Given the time is '2017-02-20 20:00'
	And I have the role 'Full access to mytime'
	And I have the workflow control set 'Intraday head count check'
	And I have schedule from "2017-02-20 08:30:00" to "2017-02-20 16:30:00"
	When I view my week schedule for date '2017-02-20'
	And I selected "Show absence probability"
	Then I should see absence probability from "08:30:00" to "16:30:00" for "2017-02-20"

Scenario: Show no absence probability for day off or full day absence
	Given the time is '2017-02-20 20:00'
	And I have the role 'Full access to mytime'
	And I have the workflow control set 'Intraday head count check'
	And I was scheduled day off or full day absence for '2017-02-20'
	When I view my week schedule for date '2017-02-20'
	And I selected "Show absence probability"
	Then I should see no absence probability for "2017-02-20"

Scenario: Show correct absence probability for cross day schedule
	Given the time is '2017-02-20 20:00'
	And I have the role 'Full access to mytime'
	And I have the workflow control set 'Intraday head count check'
	And I have schedule from "2017-02-19 18:30:00" to "2017-02-20 02:30:00"
	And I have schedule from "2017-02-20 15:00:00" to "2017-02-20 23:00:00"
	When I view my week schedule for date '2017-02-20'
	And I selected "Show absence probability"
	Then I should see absence probability from "00:00:00" to "02:30:00" for "2017-02-20"
	And I should see no absence probability from "02:30:00" to "15:30:00" for "2017-02-20"
	And I should see absence probability from "15:00:00" to "23:30:00" for "2017-02-20"

Scenario: Show overtime probability within open hour period
	Given the time is '2017-02-20 20:00'
	And There is site open hour from "10:00" to "15:00" for my site
	And I have the role 'Full access to mytime'
	And I have schedule from "2017-02-20 08:30:00" to "2017-02-20 16:30:00"
	When I view my week schedule for date '2017-02-20'
	And I selected "Show overtime probability"
	Then I should see overtime probability from "10:30" to "15:00" for "2017-02-20"

Scenario: Show correct overtime probability for cross day schedule
	Given the time is '2017-02-20 20:00'
	And There is site open hour from "10:00" to "15:00" for my site
	And I have the role 'Full access to mytime'
	And I have schedule from "2017-02-19 18:30:00" to "2017-02-20 02:30:00"
	And I have schedule from "2017-02-20 15:00:00" to "2017-02-20 23:00:00"
	When I view my week schedule for date '2017-02-20'
	And I selected "Show overtime probability"
	Then I should see overtime probability from "10:30" to "15:00" for "2017-02-20"

Scenario: Show absence probability after current time masked
	Given the time is '2017-02-20 20:00'
	And I have the role 'Full access to mytime'
	And I have the workflow control set 'Intraday head count check'
	And I have schedule from "2017-02-20 08:30:00" to "2017-02-20 16:30:00"
	And current time is "2017-02-20 10:08:00"
	When I view my week schedule for date '2017-02-20'
	And I selected "Show absence probability"
	Then I should see absence probability from "08:30:00" to "16:30:00" for "2017-02-20"
	And I should see absence probability from "08:30:00" to "10:00:00" for "2017-02-20" are masked
