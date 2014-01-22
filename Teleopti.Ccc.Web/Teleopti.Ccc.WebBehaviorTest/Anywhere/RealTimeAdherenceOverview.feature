Feature: Real time adherence overview
	In order to easier find the team leader to blame
	As a real time analyst
	I want to see which parts of the organization currently not adhering to the schedule

Scenario: View sum of employees not adhering to schedule for each site
	Given there is a shift category named 'Late'
	And there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Green' on site 'Paris'
	And there is a site named 'London'
	And there is a team named 'Red' on site 'London'
	And Pierre Baldi has a person period with
	| Field      | Value      |
	| Team       | Green      |
	| Start Date | 2014-01-21 |
	And Ashley Andeen has a person period with
         | Field      | Value      |
         | Team       | Red        |
         | Start Date | 2014-01-21 |
	 And Pierre Baldi has a shift with
         | Field          | Value      |
         | Shift Category | Late       |
         | Start time     | 12:00      |
         | End time       | 19:00      |
         | Activity       | Phone      |
         | Date           | 2014-01-21 |
	 And Ashley Andeen has a shift with
         | Field          | Value      |
         | Shift Category | Late       |
         | Start time     | 12:00      |
         | End time       | 19:00      |
         | Activity       | Phone      |
         | Date           | 2014-01-21 |
	 When the current time is '2014-01-21 13:00'
	 And Pierre Baldi has his phone state set to Pause
	 And Ashley Andeen has her phone state set to Ready for taking call
	 And I am viewing Real time adherence overview
	 Then I should see site 'Paris' with 1 of 1 employees out of adherence
	 And I should see site 'London' with 0 of 1 employees out of adherence

Scenario: View sum of employees not adhering to schedule for each team within a site

Scenario: Update sum of employees not adhering to schedule for one site when employee press break

Scenario: Update sum of employees not adhering to schedule for one team when employee press break

Scenario: Should not be able to view Real time adherence overview when not permitted
