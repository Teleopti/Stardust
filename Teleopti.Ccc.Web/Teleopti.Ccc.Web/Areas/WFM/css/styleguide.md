Living Styleguide for Teleopti wfm
=================

These guidelines are strongly inspired by http://www.google.com/design/spec/material-design/introduction.html
which have been implemented in https://www.google.com/drive/

If something is missing, please refer to it.


Main concepts
---

1. Maintain consistency
2. Avoid unnecessary clicks
3. Automatic tasks in background 
4. "Nature doesn't like empty space, and so do users". 
5. Learning from users habits
6. Adapt to scale


Other things to keep in mind
---

1. Try to avoid too many actions displayed in same time. Maybe some of them could be hidden until needed?
For example when a list is displayed : actions icons can be displayed only when an item is selected. See list section for an example.
2. If an action takes time (ajax request...) => display a loading bar (or a spinner). See loading bar section
3. Tables, lists... etc must be displayed in alphabetical order, except for specific use case.
4. Giving visual identity to objects (by icons for example)
5. In general, providing ways to filter and sort on tables, lists, grid is good.

Help
---

Three levels of help must be always available through the whole application :
1. Tooltips : Add useful tooltips on elements providing *more* information (3/4 words max => "create a user" for an icon "+" for example).
2. Help panel : (currently bottom of the right panel) Display contextual informations(only 2 or 3 sentences) on the page or on a selected element and a link to the wiki.
3. Wiki page

General layout
---

TODO