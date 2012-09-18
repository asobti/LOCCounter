LOCCounter
==========

A multi-threaded WPF application to count the total number of lines of code in a project scattered across files in a directory. Maintains a whitelist of file extensions to consider and provides a pie chart of results.

Introduction
------------

The other day, I finished working on a web development project which had code scattered across numerous PHP, JS, HTML, CS and JS files and I was curious as to how many lines of code I must have written.

That gave me the idea to write an application that can take in a project directory and calculate the total number of lines of code. I had been meaning to learn WPF for a while, so I wrote LOC Counter as a WPF application. 

What I Learnt
-------------

Without a doubt, a similar application already exists. The purpose of me coding LOC Counter wasn't so much to achieve a particular task as it was to learn. Developing LOC Counter taught me the following:

- WPF design principles - XAML code and Expression Blend
- Threading: Most of the work is done on a background thread keeping the UI thread unoccupied, which in turn, keeps the UI responsive.
- Use of AmCharts in WPF. AmCharts is an incredible and free charting library with support for WPF, Flash and HTML5/JS charts.

Requirements
------------

Being a WPF application, LOC Counter will have to be run in a windows environment with the .NET framework installed.

Url
----

http://www.techpowerup.com/forums/showthread.php?t=156001