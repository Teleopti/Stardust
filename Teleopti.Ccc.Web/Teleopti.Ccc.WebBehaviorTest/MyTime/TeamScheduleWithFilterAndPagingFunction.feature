Feature: Team Schedule with filter and paging function
In order to know when my colleagues work
As an agent
I want to see my team mates' schedules

Background: 
	Given there is an absence with
	| Field | Value    |
	| Name  | Vacation |
	| Color | Red      |
	And there is a dayoff with
	| Field | Value  |
	| Name  | DayOff |

Scenario: Team schedule tab
	Given I am an agent
	When I am viewing an application page
	Then I should see the team schedule tab

Scenario: View team schedule
	Given I am an agent in a team
	And I have an assigned shift with
	| Field | Value      |
	| Date  | 2014-05-02 |
	And I have a colleague
	And My colleague has an assigned shift with
	| Field | Value      |
	| Date  | 2014-05-02 |
	And the time is '2014-05-02 08:00'
	When I view team schedule
	Then I should see my colleague's schedule

Scenario: View only my team's schedule
	Given I am an agent in a team with access to the whole site
	And I have an assigned shift with
	| Field | Value      |
	| Date  | 2014-05-02 |
	And I have a colleague
	And My colleague has an assigned shift with
	| Field | Value      |
	| Date  | 2014-05-02 |
	And I have a colleague in another team
	And The colleague in the other team has an assigned shift with
	| Field | Value      |
	| Date  | 2014-05-02 |
	When I view group schedule for '2014-05-02'
	Then I should see my colleague's schedule
	And I should not see the other colleague's schedule

 

Scenario: View team schedule, day off
	Given I am an agent in a team
	And I have a colleague
	And 'Team Colleague' has a day off with
	| Field | Value      |
	| Name  | DayOff     |
	| Date  | 2014-05-02 |
	When I view group schedule for '2014-05-02'
	Then I should see my colleague's day off

Scenario: Should not see unpublished schedule
	Given I am an agent in a team
	And I have a colleague 'Unpublished Yet'
	And 'Unpublished Yet' has a day off with
	| Field | Value      |
	| Name  | DayOff     |
	| Date  | 2014-05-02 |
	When I view group schedule for '2014-05-02'
	Then Agent 'Unpublished Yet' schedule should be empty

Scenario: View team schedule, absence 
	Given I am an agent in a team
	And I have a colleague
	
	And 'Team Colleague' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2014-05-02 00:00 |
	| End time   | 2014-05-02 23:59 |
	When I view group schedule for '2014-05-02'
	Then I should see my colleague's absence

Scenario: View team schedule, no shift
	Given I am an agent in a team
	And I have a colleague
	When I view team schedule
	Then I should see my colleague without schedule

Scenario: Can't see confidential absence
	Given I am an agent in a team
	And I have a colleague
	And there is an absence with
	| Field        | Value        |
	| Name         | Confidential |
	| Confidential | true         |
	| Color        | GreenYellow  |
	And My colleague has a confidential absence with
	| Field   | Value        |
	| Date    | 2014-05-02   |
	| Absence | Confidential |
	When I view group schedule for '2014-05-02'
	Then I should see my colleague's schedule
	And I should see the absence with color Gray
 
Scenario: Can't see the team schedule tab without permission 
	Given I have a role with
         | Field                   | Value |
         | Access To Team Schedule | false |
	When I am viewing an application page
	Then I should not see the team schedule tab

Scenario: Can't navigate to team schedule without permission 
	Given I have a role with
         | Field                   | Value |
         | Access To Team Schedule | false |
	When I am viewing an application page
	And I navigate to the team schedule
	Then I should see an error message

Scenario: Can't see colleagues schedule without permission
	Given I have a role with
	| Field            | Value |
	| Access to my own | true  |
	And I am in a team with published schedule
	And I have a colleague
	And My colleague has an assigned shift with 
	| Field | Value      |
	| Date  | 2014-05-02 |
	When I view group schedule for '2014-05-02'
	Then I should not see my colleagues schedule



Scenario: View time line +/- whole quarter of an hour
	Given I am an agent in a team
	And I have an assigned shift with
	| Field     | Value      |
	| Date      | 2014-05-02 |
	| StartTime | 7:56       |
	| EndTime   | 17:00      |
	And I have a colleague
	And My colleague has an assigned shift with 
	| Field     | Value      |
	| Date      | 2014-05-02 |
	| StartTime | 9:00       |
	| EndTime   | 18:01      |
	When I view group schedule for '2014-05-02'
	Then The time line should span from 8:00 to 18:00

Scenario: View time line default
	Given I am an agent in my own team
	When I view team schedule
	Then The time line should span from 08:00 to 17:00

Scenario: View time line default in hawaii
	Given I am an agent in my own team
	And I am located in hawaii
	When I view team schedule
	Then The time line should span from 08:00 to 17:00

Scenario: Navigate to the next day
	Given I am an agent
	And I am viewing team schedule for '2014-05-02'
	When I click the next day button in datepicker
	Then I should see the next day

Scenario: Navigate to the previous day
	Given I am an agent
	And I am viewing team schedule for '2014-05-02'
	When I click the previous day button in datepicker
	Then I should see the previous day

Scenario: Show team-picker with multiple teams
	Given I am an agent in a team with access to the whole site
	And the site has another team
	And I am viewing team schedule
	When I open the team-picker
	Then I should see the team-picker with both teams
	And the teams should be sorted alphabetically
	
Scenario: Show default team when no access to a team on a date
	Given I am an agent in a team with access to the whole site
	And I belong to another site's team on '2014-05-03'
	And the other site has 2 teams
	And I am viewing team schedule for '2014-05-02'
	When I click the next day button in datepicker
	Then I should see the other site's team

Scenario: Don't show team-picker with no team access
	Given I have a role with
         | Field            | Value |
         | Access to my own | true  |
  And I am in a team with published schedule
	When I view team schedule
	Then I should not see the team-picker

Scenario: Should show date-picker with no team access
	Given I have a role with
         | Field            | Value |
         | Access to my own | true  |
  And I am in a team with published schedule
	When I view team schedule
	Then I should see the date-picker

Scenario: Default team when no own team but everyone access
	Given I have a role with
         | Field              | Value |
         | Access To Everyone | True  |
	And the site has another team
	When I view team schedule
	Then I should see the team-picker

Scenario: Show error message when acces to my team but no own team
	Given I am an agent in no team with access to my team
	When I view team schedule
	Then I should see a user-friendly message explaining I dont have anything to view

Scenario: Show friendly message when after leaving date
	Given I am an agent in a team that leaves on '2030-05-02'
	When I am viewing team schedule for '2030-05-03'
	Then I should see a user-friendly message explaining I dont have anything to view

Scenario: Navigate next date without team
	Given I have a role with
         | Field            | Value |
         | Access to my own | true  |
	And I am in a team with published schedule
	And I am viewing team schedule for '2014-05-02'
	When I click the next day button in datepicker
	Then I should see the next day
