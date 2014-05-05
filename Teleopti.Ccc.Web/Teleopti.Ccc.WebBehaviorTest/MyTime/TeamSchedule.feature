@WatiN
Feature: Team schedule
In order to know when my colleagues work
As an agent
I want to see my team mates' schedules
 
Scenario: Team schedule tab
	Given I am an agent
	When I am viewing an application page
	Then I should see the team schedule tab

Scenario: View team schedule
	Given I am an agent in a team
	And I have a shift today
	And I have a colleague
	And My colleague has a shift today
	When I view team schedule
	Then I should see my schedule
	And I should see my colleague's schedule

Scenario: View only my team's schedule
	Given I am an agent in a team with access to the whole site
	And I have a shift today
	And I have a colleague
	And My colleague has a shift today
	And I have a colleague in another team
	And The colleague in the other team has a shift today
	When I view team schedule
	Then I should see my schedule
	And I should see my colleague's schedule
	And I should not see the other colleague's schedule

Scenario: View team schedule, day off
	Given I am an agent in a team
	And I have a colleague
	And My colleague has a dayoff today
	When I view team schedule
	Then I should see my colleague's day off

Scenario: View team schedule, absence 
	Given I am an agent in a team
	And I have a colleague
	And My colleague has an absence today
	When I view team schedule
	Then I should see my colleague's absence

Scenario: View team schedule, no shift
	Given I am an agent in a team
	And I have a colleague
	When I view team schedule
	Then I should see myself without schedule
	And I should see my colleague without schedule

Scenario: Can't see confidential absence
	Given I am an agent in a team
	And I have a colleague
	And My colleague has a confidential absence
	When I view team schedule
	Then I should see my colleague's schedule
	And I should not see the absence's color
 
Scenario: Can't see the team schedule tab without permission 
	Given I am an agent with no access to team schedule
	When I am viewing an application page
	Then I should not see the team schedule tab

Scenario: Can't navigate to team schedule without permission 
	Given I am an agent with no access to team schedule
	When I am viewing an application page
	And I navigate to the team schedule
	Then I should see an error message

Scenario: Can't see colleagues schedule without permission
	Given I am an agent in a team with access only to my own data
	And I have a colleague
	And My colleague has a shift today
	When I view team schedule
	Then I should not see my colleagues schedule

Scenario: View time line +/- whole quarter of an hour
	Given I am an agent in a team
	And I have a shift from 7:56 to 17:00
	And I have a colleague
	And My colleague has a shift from 9:00 to 18:01
	When I view team schedule
	Then The time line should span from 7:30 to 18:30

Scenario: View time line default
	Given I am an agent in my own team
	When I view team schedule
	Then The time line should span from 7:45 to 17:15

Scenario: View time line default in hawaii
	Given I am an agent in my own team
	And I am located in hawaii
	When I view team schedule
	Then The time line should span from 7:45 to 17:15

Scenario: Navigate to the next day
	Given I am an agent
	And I view team schedule
	When I click the next day button
	Then I should see the next day

Scenario: Navigate to the previous day
	Given I am an agent
	And I view team schedule
	When I click the previous day button
	Then I should see the previous day
 
Scenario: Sort late shifts after early shifts
	Given I am an agent in a team
	And I have a shift from 9:00 to 17:00
	And I have a colleague
	And My colleague has a shift from 8:00 to 18:00
	When I view team schedule
	Then I should see my colleague before myself

Scenario: Sort full-day absences after shifts
	Given I am an agent in a team
	And I have a full-day absence today
	And I have a colleague
	And My colleague has a shift from 9:00 to 17:00
	When I view team schedule
	Then I should see my colleague before myself

Scenario: Sort day offs after the absences
	Given I am an agent in a team
	And I have a dayoff today
	And I have a colleague
	And My colleague has a full-day absence today
	When I view team schedule
	Then I should see my colleague before myself

Scenario: Sort no schedule last
	Given I am an agent in a team
	And I have a colleague
	And My colleague has a dayoff today
	When I view team schedule
	Then I should see my colleague before myself

Scenario: Show tooltip with activity name and times
	Given I am an agent in a team
	And I have an activity from 8:00 to 12:00
	When I view team schedule
	Then The layer's start time attibute value should be 08:00
	And The layer's end time attibute value should be 12:00
	And The layer's activity name attibute value should be Phone

Scenario: Show team-picker with multiple teams
	Given I am an agent in a team with access to the whole site
	And the site has another team
	And I am viewing team schedule
	When I open the team-picker
	Then I should see the team-picker with both teams
	And the teams should be sorted alphabetically
	
@ignore
Scenario: Show other team's schedule
	Given I am an agent in a team with access to the whole site
	And I have a colleague in another team
	And I am viewing team schedule
	When I select the other team in the team picker
	Then I should see my colleague
	And I should not see myself

@ignore
Scenario: Keep selected date when changing team
	Given I am an agent in a team with access to the whole site
	And the site has another team
	And I am viewing team schedule for tomorrow
	When I select the other team in the team picker
	Then I should see tomorrow

@ignore
Scenario: Show team-picker with teams for my site for another day
	Given I am an agent in a team with access to the whole site
	And I belong to another site's team tomorrow
	And the other site has 2 teams
	And I am viewing team schedule for today
	When I click the next day button
	Then I should see the team-picker with the other site's team

Scenario: Show default team when no access to a team on a date
	Given I am an agent in a team with access to the whole site
	And I belong to another site's team tomorrow
	And the other site has 2 teams
	And I am viewing team schedule for today
	When I click the next day button
	Then I should see the other site's team

@ignore
Scenario: Default to my team
	Given I am an agent in a team with access to the whole site
	And the site has another team
	And I am viewing team schedule
	Then the team-picker should have my team selected

@ignore
Scenario: Default to first team if no access to my team
	Given I am an agent in a team with access to another site
	And the other site has 2 teams
	And I am viewing team schedule
	Then the team-picker should have the first of the other site's teams selected

Scenario: Don't show team-picker with no team access
	Given I am an agent in a team with access only to my own data
	When I view team schedule
	Then I should not see the team-picker

Scenario: Don't show team-picker with only one team
	Given I am an agent in a team with access to my team
	When I view team schedule
	Then I should not see the team-picker

Scenario: Default team when no own team but everyone access
	Given I am a user with everyone access
	And the site has another team
	When I view team schedule
	Then I should see the team-picker

Scenario: Show error message when acces to my team but no own team
	Given I am an agent in no team with access to my team
	When I view team schedule
	Then I should see a user-friendly message explaining I dont have anything to view

Scenario: Show friendly message when after leaving date
	Given I am an agent in a team that leaves tomorrow
	And I am viewing team schedule for today
	When I click the next day button
	Then I should see a user-friendly message explaining I dont have anything to view

Scenario: Navigate next date without team
	Given I am an agent in a team with access only to my own data
	And I view team schedule
	When I click the next day button
	Then I should see the next day