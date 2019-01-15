@RTA
Feature: Team changes
  In order to easier find the team leader to blame
  As a real time analyst
  I want agents changing teams calculated correctly

  Background:
	Given there is a switch
	And there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And there is a team named 'Green' on site 'Paris'
	And there is a site named 'London'
	And there is a team named 'Orange' on site 'London'
	And I have a role with full access
	And Ashely Andeen has a person period with
	  | Field      | Value      |
	  | Team       | Red        |
	  | Start Date | 2016-02-01 |
	And Pierre Baldi has a person period with
	  | Field      | Value      |
	  | Team       | Red        |
	  | Start Date | 2016-02-01 |
	And Pierre Baldi has a person period with
	  | Field      | Value      |
	  | Team       | Green      |
	  | Start Date | 2016-02-02 |
	And Ashely Andeen has a shift with
	  | Field      | Value            |
	  | Activity   | Phone            |
	  | Start time | 2016-02-01 08:00 |
	  | End time   | 2016-02-01 17:00 |
	And Pierre Baldi has a shift with
	  | Field      | Value            |
	  | Activity   | Phone            |
	  | Start time | 2016-02-01 08:00 |
	  | End time   | 2016-02-01 17:00 |
	And Ashely Andeen has a shift with
	  | Field      | Value            |
	  | Activity   | Phone            |
	  | Start time | 2016-02-02 08:00 |
	  | End time   | 2016-02-02 17:00 |
	And Pierre Baldi has a shift with
	  | Field      | Value            |
	  | Activity   | Phone            |
	  | Start time | 2016-02-02 08:00 |
	  | End time   | 2016-02-02 17:00 |
	And there is a rule with
	  | Field       | Value |
	  | Activity    | Phone |
	  | Phone state | Pause |
	  | Is Alarm    | true  |
	  | Adherence   | Out   |

  Scenario: Exclude person changed team
	Given the time is '2016-02-01 09:00:00'
	When I view Real time adherence for teams on site 'Paris'
	And 'Ashely Andeen' sets her phone state to 'Pause'
	And 'Pierre Baldi' sets his phone state to 'Pause'
	Then I should see team 'Red' with 2 agents in alarm
	When 'Pierre Baldi' changes team to 'Green'
	Then I should see team 'Red' with 1 agents in alarm

  Scenario: Exclude person that changed team to different site
	Given the time is '2016-02-01 09:00:00'
	When I view Real time adherence sites
	And 'Ashely Andeen' sets her phone state to 'Pause'
	And 'Pierre Baldi' sets his phone state to 'Pause'
	Then I should see site 'Paris' with 2 agents in alarm
	When 'Pierre Baldi' changes team to 'Orange'
	Then I should see site 'Paris' with 1 agents in alarm

  Scenario: Exclude person changed team over time
	Given the time is '2016-02-01 09:00:00'
	When I view Real time adherence for teams on site 'Paris'
	And 'Ashely Andeen' sets her phone state to 'Pause'
	And 'Pierre Baldi' sets his phone state to 'Pause'
	Then I should see team 'Red' with 2 agents in alarm
	When the time is '2016-02-02 09:00:00'
	And I view Real time adherence for teams on site 'Paris'
	Then I should see team 'Red' with 1 agents in alarm
