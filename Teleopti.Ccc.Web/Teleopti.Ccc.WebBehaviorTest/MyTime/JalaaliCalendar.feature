@OnlyRunIfEnabled('MyTimeWeb_JalaaliCalendar_32997')
Feature: JalaaliCalendar
	In order to improve usability for Iranian Customers
	As an Agent
	I want to see dates displayed according to the Jalaali calendar

Background:
	Given I am iranian
	And there is a role with
	| Field                    | Value                 |
	| Name                     | Full access to mytime |

Scenario: Open Jalaali Date Picker
	Given I am an agent
	And the time is '2015-05-17 08:00'
	And I am editing an existing Text Request
	When I open the date picker
	Then I should see a jalaali date picker with date '1394/02/27', 'اردیبهشت'

Scenario: Check Number of Days in Month
	Given I am an agent
	And the time is '2015-05-17 08:00'
	And I am editing an existing Text Request
	When I open the date picker
	Then I should see a jalaali date picker with 31 days
	
Scenario: Open Jalaali Time Picker
	Given I am an agent
	And the time is '2015-05-17 02:15Z'
	And I am editing an existing Text Request
	When I open the time picker
	Then I should see a jalaali time picker with '02','15'

