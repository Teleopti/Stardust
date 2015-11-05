@OnlyRunIfEnabled('SeatPlanner_Logon_32003')
@OnlyRunIfEnabled('Wfm_SeatPlan_SeatMapBookingView_32814')

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

Scenario: be able to go back to seat plan
	When I view Seat plan on "2015-01-01"
	And I press seat map booking view button
	And I press back button on seat booking view
	Then I should go back to seat plan

Scenario: the date is the same with I chosed
	When I view Seat plan on "2015-01-01"
	And I press seat map booking view button
	Then the date of datepicker should be correct

Scenario: be able to view booking detail of selected seat
	Given there is a seat booking for me
		| Field         | Value            |
		| BelongsToDate | 2015-01-01       |
		| StartDateTime | 2015-01-01 8:00  |
		| EndDateTime   | 2015-01-01 16:00 |
		| SeatName      | 1                |
	When I view Seat plan on "2015-01-01"
	And I press seat map booking view button
	Then I am able to view booking detail of selected seat

Scenario: be able to delete seat booking for the first seat
	Given there is a seat booking for me
		| Field         | Value            |
		| BelongsToDate | 2015-01-01       |
		| StartDateTime | 2015-01-01 8:00  |
		| EndDateTime   | 2015-01-01 16:00 |
		| SeatName      | 1                |
	When I view Seat plan on "2015-01-01"
	And I press seat map booking view button
	Then I delete the first record under the seat booking details

Scenario: be able to open people search list
	When I view Seat plan on "2015-01-01"
	And I press seat map booking view button
	And I click add agents to seat button
	Then I should see people search list

Scenario: the action button of people is invisible
	When I view Seat plan on "2015-01-01"
	And I press seat map booking view button
	And I click add agents to seat button
	Then I should not see the action buttons of people

Scenario: be able to close people list
	When I view Seat plan on "2015-01-01"
	And I press seat map booking view button
	And I click add agents to seat button
	And I click cancel button after open people search list
	Then I should not see people search list

Scenario: be able to assign agent to selected seat
	When I view Seat plan on "2015-01-01"
	And I press seat map booking view button
	And I click add agents to seat button
	And I select agent 'I' from search list
	And I click assign button
	Then I should see 'I' in the result of seat plan

Scenario: be able to select multiple seats
	Given there are '3' more seats at root location
	When I view Seat plan on "2015-01-01"
	And I press seat map booking view button
	And I select first '2' seats
	Then I should see occupancy detail of two seats in occupancy detail panel

Scenario: be able to delete seatBooking from multiple selection
	Given there is a seat booking for me
		| Field         | Value            |
		| BelongsToDate | 2015-01-01       |
		| StartDateTime | 2015-01-01 8:00  |
		| EndDateTime   | 2015-01-01 16:00 |
		| SeatName      | 1                |
	And there are '2' more seats at root location
	When I view Seat plan on "2015-01-01"
	And I press seat map booking view button
	And I select first '2' seats
	Then I delete the first record under the seat booking details
	And I should not see any seat booking details