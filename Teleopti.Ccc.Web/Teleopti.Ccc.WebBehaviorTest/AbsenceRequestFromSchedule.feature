﻿@ignore
#Ignored for now because this is going to be redesigned before merged to main
Feature: Absence request from schedule
	In order to make requests to my superior
	As an agent
	I want to be able to submit absence requests

Background:
	Given there is a role with
	| Field                    | Value                 |
	| Name                     | Full access to mytime |
	And there is a role with
	| Field						 | Value						 |
	| Name						 | No access to absence requests |
	| Access to absence requests | False					     |
	
Scenario: Open add absence request form from day summary
	Given I have the role 'Full access to mytime'
	And I view my week schedule for date '2013-10-03'
	When I click on the day summary for date '2013-10-03'
	And I click to add a new absence request
	Then I should see the add absence request form

Scenario: Add absence request from week schedule view
	Given I have the role 'Full access to mytime'
	And I have a requestable absence called Vacation
	And I view my week schedule for date '2013-10-03'
	When I click on the day symbol area for date '2013-10-03'
	And I click absence request tab
	And I input absence request values with 'Vacation' for date '2013-10-03'
	And I click the OK button
	Then I should see a symbol at the top of the schedule for date '2013-10-03'
	
Scenario: Can not add absence request from day symbol area if no permission
	Given I have the role 'No access to absence requests'
	And I view my week schedule for date '2013-10-03'
	When I click on the day symbol area for date '2013-10-03'
	Then I should not see the add absence request form

Scenario: Can not add absence request from day summary if no permission
	Given I have the role 'No access to absence requests'
	And I view my week schedule for date '2013-10-03'
	When I click on the day summary for date '2013-10-03'
	Then I should not see the add absence request form
	
Scenario: Default absence request values from week schedule
	Given I have the role 'Full access to mytime'
	And I view my week schedule for date '2013-10-03'
	When I click on the day summary for date '2013-10-03'
	And I click absence request tab
	Then I should see the request form with '2013-10-03' as default date
	And I should see 00:00 - 23:59 as the default times
	
Scenario: Default absence request values from week schedule when unchecked Fullday
	Given I have the role 'Full access to mytime'
	And I view my week schedule for date '2013-10-03'
	When I click on the day summary for date '2013-10-03'
	And I click absence request tab
	And I unchecked the full day checkbox
	Then I should see the request form with '2013-10-03' as default date
	And I should see 08:00 - 17:00 as the default times
	
Scenario: Default absence request values from week schedule when checked Fullday
	Given I have the role 'Full access to mytime'
	And I view my week schedule for date '2013-10-03'
	When I click on the day summary for date '2013-10-03'
	And I click absence request tab
	And I checked the full day checkbox
	Then I should see the request form with '2013-10-03' as default date
	And I should see 00:00 - 23:59 as the default times
	
Scenario: Cancel adding absence request
	Given I have the role 'Full access to mytime'
	And I have a requestable absence called Vacation
	And I view my week schedule for date '2013-10-03'
	When I click on the day summary for date '2013-10-03'
	And I click absence request tab
	And I input absence request values with 'Vacation' for date '2013-10-03'
	When I click the Cancel button
	Then I should not see a symbol at the top of the schedule for date '2013-10-03'
	
Scenario: Adding invalid absence request values
	Given I have the role 'Full access to mytime'
	And I view my week schedule for date '2013-10-03'
	When I click on the day symbol area for date '2013-10-03'
	And I click absence request tab
	And I input empty subject
	And I input later start time than end time for date '2013-10-03'
	And I click the OK button
	Then I should see texts describing my errors
	And I should not see a symbol at the top of the schedule for date '2013-10-03'
	
Scenario: View absence types
	Given I have the role 'Full access to mytime'
	And I have a requestable absence called Vacation
	And I view my week schedule for date '2013-10-03'
	When I click on the day symbol area for date '2013-10-03'
	And I click absence request tab
	Then I should see an absence type called Vacation in droplist
	
Scenario: Switch request type
	Given I have the role 'Full access to mytime'
	And I have a requestable absence called Vacation
	And I view my week schedule for date '2013-10-03'
	When I click on the day symbol area for date '2013-10-03'
	And I input text request values for date '2013-10-03'
	And I click absence request tab
	Then I should see my existing inputs for date '2013-10-03'
	And I should see an absence type called Vacation in droplist

Scenario: Add absence request from week schedule view with multiple absences
	Given I have the role 'Full access to mytime'
	And I have a requestable absence called Vacation
	And I have a requestable absence called Time in lieu
	And I have a requestable absence called Unpaid Holiday
	And I view my week schedule for date '2013-10-03'
	When I click on the day symbol area for date '2013-10-03'
	And I click absence request tab
	And I input absence request values with 'Time in lieu' for date '2013-10-03'
	And I click the OK button
	And I navigate to the requests page
	Then I should see my existing absence request with absence 'Time in lieu'


