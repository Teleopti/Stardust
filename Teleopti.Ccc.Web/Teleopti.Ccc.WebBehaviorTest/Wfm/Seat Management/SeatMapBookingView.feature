@WFM
Feature: SeatMapBookingView
	I want to view the seat bookings and assign agents to seat

Background: 
	
	Given there is a site named 'London'
	And there is a team named 'Team Red' on site 'London'
	And there is a team named 'Team Blue' on site 'London'

	And there is a workflow control set with
		| Field                      | Value						  |
		| Name                       | Schedule published to 20150102 |
		| Schedule published to date | 2015-01-01                     |
	And there are shift categories
		| Name  |
		| Night |
		| Day   |
		| Late  |
	And there are activities
		| Name           | Color  |
		| Lunch          | Yellow |
		| White activity | White  |
		| Black activity | Black  |
	And I have a role with
		| Field                 | Value        |
		| Name                  | Seat Planner |
		| Access to seatplanner | True         |
		| Access to people   | true        |
		| Access to everyone    | true         |
	And there is a seat in root location with
		| Field    | Value |
		| Name     | 1     |
		| Priority | 1     |
	And I have a person period with
		| Field      | Value      |
		| Team       | Team Red   |
		| Start Date | 2014-01-01 |
	And I have a schedule period with 
		| Field      | Value      |
		| Start date | 2015-01-01 |
		| Type       | Week       |
		| Length     | 1          |
	And I have the workflow control set 'Schedule published to 20150102'
	And I have a shift with
		| Field          | Value            |
		| StartTime      | 2015-01-01 8:00  |
		| EndTime        | 2015-01-01 17:00 |
		| Shift category | Day              |


Scenario: be able to open the seat map booking view
	When I view Seat plan on "2015-01-01"
	And I press seat map booking view button
	Then the seat map booking should be opened

Scenario: the date is the same with I chose
	When I view Seat plan on "2015-01-01"
	And I press seat map booking view button
	Then the date of datepicker should be correct
