Feature: Routing
	In order make it easy to browse to the site
	As a user
	I want to be redirected to the correct locations

Scenario: Browse to root
	Given I am not signed in
	When I navigate to the site's root
	Then I should see the global sign in page

Scenario: Browse to MyTime
	Given I am not signed in
	When I navigate to MyTime
	Then I should see MyTime's sign in page

Scenario: Browse to Mobile Reports
	Given I am not signed in
	When I navigate to Mobile Reports
	Then I should see Mobile Report's sign in page
	
Scenario: Browse to root and sign in to menu
	Given I am a user with access to all areas
	When I navigate to the site's root
	And I sign in
	Then I should see the global menu

Scenario: Browse to root and sign in to mobile menu
	Given I am a user with access to all areas
	When I navigate to the site's root mobile signin page
	And I sign in 
	Then I should see the mobile global menu
 
Scenario: Browse to root and sign in to MyTime
	Given I am a user with access only to MyTime
	When I navigate to the site's root
	And I sign in
	Then I should see MyTime

Scenario: Browse to root and sign in to Mobile Reports
	Given I am a user with access only to Mobile Reports
	When I navigate to the site's root
	And I sign in
	Then I should see MyTime

Scenario: Browse to MyTime and sign in
	Given I am a user with access to all areas
	When I navigate to MyTime
	And I sign in
	Then I should see MyTime

Scenario: Browse to Mobile Reports and sign in
	Given I am a user with access to all areas
	When I navigate to Mobile Reports
	And I sign in
	Then I should see Mobile Reports
