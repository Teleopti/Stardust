Feature: JalaaliCalendar
	In order to improve usability for Iranian Customers
	As an Agent
	I want to see dates displayed according to the Jalaali calendar

Background:
	Given I am iranian
	And there is a role with
	| Field                    | Value                 |
	| Name                     | Full access to mytime |
	And there is a role with
	| Field                           | Value                           |
	| Name                            | Access to overtime availability |
	| Access to overtime availability | true                            |
	And I have a schedule period with 
	| Field      | Value      |
	| Start date | 2015-05-01 |
	| Type       | Week       |
	| Length     | 4          |
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2015-05-01 |
	
@OnlyRunIfEnabled('MyTimeWeb_MobileResponsive_43826')
Scenario: Open Jalaali Date Picker
	Given I am an agent
	And the time is '2015-05-17 08:00'
	And I am editing an existing Text Request
	When I open the date picker
	Then I should see a jalaali date picker with date '1394/02/27', 'اردیبهشت'
	
@OnlyRunIfEnabled('MyTimeWeb_MobileResponsive_43826')
Scenario: Check Number of Days in Month
	Given I am an agent
	And the time is '2015-05-17 08:00'
	And I am editing an existing Text Request
	When I open the date picker
	Then I should see a jalaali date picker with 31 days
	
@OnlyRunIfEnabled('MyTimeWeb_MobileResponsive_43826')
Scenario: Open Jalaali Time Picker
	Given I am an agent
	And the time is '2015-05-17 02:15'
	And I am editing an existing Text Request
	When I open the time picker
	Then I should see a jalaali time picker with '02','15'

@Ignore
Scenario: Show Correctly Formatted Text Request Date
	Given I am an agent
	And I have an existing text request with
	| Field     | Value            |
	| StartTime | 2015-05-17 10:00 |
	| End Time  | 2015-05-17 14:00 |
	And I am viewing requests
	Then I should see request page
	And I should see a request with date period '1394/02/27 10:00 - 14:00'
		
@Ignore
Scenario: Show Correctly Formatted Absence Request Date
	Given I am an agent
	And I have an existing absence request with
	| Field     | Value            |
	| StartTime | 2015-05-17 10:00 |
	| End Time  | 2015-05-17 14:00 |
	And I am viewing requests
	Then I should see request page
	And I should see a request with date period '1394/02/27 10:00 - 14:00'

Scenario: Open Schedule 
	Given I am an agent
	When I view my week schedule for date '2015-05-17'
	Then I should see the day header text for date '2015-05-17' is 'یک‌شنبه'


Scenario: Open Preference
	Given I am an agent
	And the time is '2014-05-02 08:00'
	When I view preferences
	Then I should see the period to be '1394/02/11 - 1394/02/11'

@Ignore
Scenario: Show correctly formatted Text Request Date From Schedule
	Given I am an agent
	And I have an existing text request with
	| Field     | Value            |
	| StartTime | 2015-05-17 10:00 |
	| End Time  | 2015-05-17 14:00 |
	When I view my week schedule for date '2015-05-17'
	And I click the request symbol for date '2015-05-17'
	Then I should see request page
	And I should see a request with date period '1394/02/27 10:00 - 14:00'

@Ignore
Scenario: Show correctly formatted Absence Request Date From Schedule
	Given I am an agent
	And I have an existing absence request with
	| Field     | Value            |
	| StartTime | 2015-05-17 10:00 |
	| End Time  | 2015-05-17 14:00 |
	When I view my week schedule for date '2015-05-17'
	And I click the request symbol for date '2015-05-17'
	Then I should see request page
	And I should see a request with date period '1394/02/27 10:00 - 14:00'
	
Scenario: Show correctly formatted Overtime Availability dates
	Given I am an agent
	And I have the role 'Access to overtime availability'
	And I have a schedule period with 
	| Field      | Value      |
	| Start date | 2017-05-17 |
	| Type       | Week       |
	| Length     | 1          |
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2017-05-01 |
	And there are shift categories
	| Name  |
	| Day   |
	And there are activities
	| Name           | Color  |
	| Phone          | Yellow |
	And I have a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| Start time       | 2015-05-17 09:00 |
	| End time         | 2015-05-17 18:00 |
	And I view my week schedule for date '2015-05-17'
	When I click on the day summary for date '2015-05-17'
	And I click add new overtime availability
	Then I should see the overtime availability form with a start date of '1394/02/27' and an end date of '1394/02/27'