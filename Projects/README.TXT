Instructions for building the "EQ" program, for running an Electronic Questionnaire. 

This program has been designed for and run on computers/tablets
running versions of the Microsoft Windows operating system. It is known to work with Windows XP, Windows 7 and Windows 8. The .Net framework from Microsoft
must be installed on a computer before using EQ. See this link for more information: http://www.microsoft.com/en-gb/download/details.aspx?id=30653

1. Install Microsoft Visual Studio for Windows Desktop (denoted hereafter as "VS". Note: the free "Express" version works as of this writing, i.e. the 2013 version)
2. Open VS and choose File -> Open Project
3. Open file: Projects -> BHS_questionaire_demo -> BHS_questionaire_demo.sln
4. Click "Start" in the main menu and VS should compile and run the project.
5. In the window that appears, click on the "settings" tab, then click the "Set Data Directory" button. Choose the directory where the configuration file (EQuestionnaire_English.xml) is located.
6. Click the "Save Settings" button. This will save the Data Directory location in the Windows Registry.
7. To start a new questionnaire, go to the "New Questionnaire" tab, enter a participant ID and click the "Start" button. 
8. A new window will appear. Click the "Last Question" and "Next Question" buttons to navigate between questions. Answers are saved when the "Next Question" button is clicked.
9. To exit the questionnaire form, close the window. The form can be re-opened for further editing by clicking the "Existing Questionnaire" tab, selecting the questionnaire from the list of participant IDs, then clicking the "Edit Selected Questionnaire" button.
10. Data is stored in the Data Directory. A separate directory is created for each participant with the name: participant_data_English_<participant-id>
11. It is important not to interfere with the files in these directories, as doing so may stop EQ from working properly.
12. There are a number of files in these directories, but the most important from a user perspective is the file "final_data_<participant-id>.txt".
13. This file contains the saved data for the questionnaire. Each line is formatted as <Question-code>\t<answer>


Instructions for building the "EQdbUploader" program for transferring EQ-generated data to a MySQL database.

You must have a MySQL database available. The free "community" version is sufficient. This document assumes basic knowledge of MySQL.
Login to the MySQL command line shell. Your MySQL account must have create, read and write permissions.
Create a new database with an appropriate name, e.g. to create the database 'eq_results', at the MySQL command prompt you would type:

    mysql> create database eq_results;

Load the new database:

    mysql> use eq_results;

Create the following table in this database:

    mysql> create table participant_data(
    row_id int unsigned auto_increment,
    participant_id varchar(100),
    question_code varchar(100),
    question_response text,
    primary key(row_id)
    ) engine=InnoDB;

Before you can export the EQ data to a database, you must first export it out of EQ. To do this (in EQ), select the "Existing Questionnaire" tab and click the "Export All Data" button. Select an empty directory to copy the data into.

To build the VS project:

1. In VS, open the project: Projects -> EQdbUploader -> EQdbUploader.sln
2. Click "Start" in the main menu and VS should compile and run the project.
3. You will see a couple of messages about the database not being connected. These can be ignored.
4. In the EQ uploader window, click the "Settings" tab. Enter the database connection parameters, i.e. the name of the host machine ("localhost" if MySQL is running on your desktop or laptop machine),
the name of the database ("eq_results" in this example) and the user-name and password that you normally use to login to MySQL.
5. Click the "connect" button. You should see a message that connection was successful.
6. Click the "transfer data" tab and you should see a list of participants in the database. This will be empty initially.
7. Click the "load data from directory" button and select the directory that you used to export data out of EQ (above).
8. Once the data has been transferred, you should see a message saying that the data has been transferred successfully and you will now see a list of participants in the database.
9. Close the window.

The EQdbUploader makes use of the file MySql.Data.dll for database connection. This file must be in the same directory as the executable program, which is important if the executable is moved
from the VS project elsewhere.



1. The usage of a range of years within a copyright statement contained within
this distribution should be interpreted as being equivalent to a list of years
including the first and last year specified and all consecutive years between
them. For example, a copyright statement that reads 'Copyright (c) 2005, 2007-
2009, 2011-2012' should be interpreted as being identical to a statement that
reads 'Copyright (c) 2005, 2007, 2008, 2009, 2011, 2012' and a copyright
statement that reads "Copyright (c) 2005-2012' should be interpreted as being
identical to a statement that reads 'Copyright (c) 2005, 2006, 2007, 2008,
2009, 2010, 2011, 2012'."