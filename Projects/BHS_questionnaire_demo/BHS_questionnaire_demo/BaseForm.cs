
/*
Copyright (c) 14 April, 2014 Genome Research Ltd.
Author: Stephen Rice <sr7@sanger.ac.uk>
This file is part of EQ.
EQ is free software: you can redistribute it and/or modify it under
the terms of the GNU Affero General Public License as published by the Free
Software Foundation; either version 3 of the License, or (at your option) any
later version.
This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more
details.
You should have received a copy of the GNU Affero General Public License
along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.IO;
using System.Text.RegularExpressions;




namespace BHS_questionnaire_demo
{
    public partial class BaseForm : Form
    {
        
        //do we have any Qforms open ?
        bool qFormIsOpen = false;

        //current status of the logger
        //can be 'on', 'off', 'suspended' (suspended= was on, but has been disabled while the user has a questionnaire window open
        private string loggerStatus = "off";
        
        
        //current form for the survey
        Form1 currentSurvey = null;

        //current survey opened for checking completeness but not editing
        CompletenessForm currentSurveyComp = null;

        //the participant ID for the currently open Questionnaire
        string openParticipantID = null;
        
        
        //dir for storing all questionnaires
        private string dataDir;

        //a message box for warnings
        private Form3 warningMessageBox;

        //an info box
        private MessageForm stdMessageBox;

        //error box
        private Form2 errorBox;

        //a confirm (yes/no) box
        private ConfirmForm confirmForm;

        private string portNum;
        private string baudRate;
        private string gpsCountry;

        //Registry Key
        private string keyName = @"Software\EQuestionnaire\Settings";

        //used by confirm window to send back result
        public string confirmResult
        {
            get; set;

        }
        
        
        
        public BaseForm()
        {
            InitializeComponent();


            




        }

        private void BaseForm_Load(object sender, EventArgs e)
        {

            //set English as default language
            listBox1.SelectedItem = "English";


            groupBox1.ForeColor = GlobalColours.mainTextColour;
            
            
            //set active cursor on the partID box
            textBox1.Focus();
            
            
            //init dialogs
            warningMessageBox = new Form3();
            stdMessageBox = new MessageForm();
            errorBox = new Form2();
            confirmForm = new ConfirmForm();



            //get saved values from registry if they exist
            //readonly access
            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(keyName);
            if (regKey != null)
            {
               
                //get values
                dataDir = (string)regKey.GetValue("DATA_DIR");
                portNum = (string)regKey.GetValue("GPS_PORT");
                baudRate = (string)regKey.GetValue("GPS_BAUD");
                gpsCountry = (string)regKey.GetValue("GPS_COUNTRY");

                string GPSenabled = (string)regKey.GetValue("GPS_ENABLED");

                if (GPSenabled == "TRUE")
                {

                    Utils.GPSenabled = true;

                    //enable checkbox
                    checkBox1.Checked = true;



                }
                else
                {
                    Utils.GPSenabled = false;

                    checkBox1.Checked = false;


                }


                if (dataDir != null)
                {
                    label7.Text = dataDir;


                }

                if (portNum != null)
                {

                    comboBox1.SelectedItem = portNum;


                }

                if (baudRate != null)
                {

                    comboBox2.SelectedItem = baudRate;


                }

                if (gpsCountry != null)
                {

                    comboBox3.SelectedItem = gpsCountry;

                }

                

                regKey.Close();


            }

            if (dataDir != null)
            {

                //does the base dir still exist, i.e. its possible that the user has moved or deleted it.
                if (Directory.Exists(dataDir))
                {
                    //populate the current questionnaires, if any
                    updateExistingQuestionList();

                    //load help text file
                    loadHelp();

                }
                else
                {
                    warningMessageBox.setLabel("Warning: The Data Directory has been changed: please update the settings");
                    warningMessageBox.ShowDialog();


                }



            }

           
        }


        private void loadHelp()
        {

            //load the help text into the help tab
            StreamReader dh = null;

            try
            {
                string helpFileName = dataDir + "\\EQ help.txt";

                dh = new StreamReader(helpFileName);

                StringBuilder sb = new StringBuilder();

                while (dh.EndOfStream == false)
                {
                    string line = dh.ReadLine();

                    sb.Append(line);
                    sb.Append("\n");


                }

                dh.Close();

                label8.Text = sb.ToString();



            }
            catch(Exception e)
            {
                warningMessageBox.setLabel("Warning: Could not load the help Text file, although this is not essential");
                warningMessageBox.ShowDialog();

                //MessageBox.Show("error:" + e.Message + e.StackTrace);


            }
            finally
            {

                if (dh != null)
                {
                    dh.Close();

                } 


            }




        }

        private void button8_Click(object sender, EventArgs e)
        {

            //user has clicked the set data directory button

            //open a file dialog to get the name of the dir

            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Select the folder to store questionnaire data";

            //set root to My Documents
            //folderBrowserDialog.RootFolder = Environment.SpecialFolder.Personal;

            // Show the FolderBrowserDialog.
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                dataDir = folderBrowserDialog.SelectedPath;

                //update label:
                label7.Text = dataDir;

                
            }







        }

        private void button7_Click(object sender, EventArgs e)
        {
            //test GPS connection

            //make sure the logger is off
            if (loggerStatus != "off")
            {

                //the port is already in use
                stdMessageBox.setMainLabel("Connection already in use (GPS logger)");
                stdMessageBox.ShowDialog();

                return;

            }


            portNum = (string)comboBox1.SelectedItem;
            baudRate = (string)comboBox2.SelectedItem;

            if ((portNum == null) || (baudRate == null))
            {

                //show fail
                warningMessageBox.setLabel("Warning: Please select Port and Baud Rate");
                warningMessageBox.ShowDialog();



            }
            else
            {
                //test serial port connection

                serialPort1.PortName = portNum;

                //comm port for laptop
                //serialPort1.PortName = "COM4";

                serialPort1.BaudRate = Convert.ToInt32(baudRate);


                //try and open the serial port for GPS comms
                try
                {
                    serialPort1.Open();

                    //show OK dialog
                    stdMessageBox.setMainLabel("Connection Successfull");
                    stdMessageBox.ShowDialog();

                    //disconnect
                    serialPort1.Close();




                }
                catch
                {

                   //show fail dialog

                    warningMessageBox.setLabel("Warning: Connection failed. You might need different Port/Baud Rate values");
                    warningMessageBox.ShowDialog();

                }



            }








        }

        private void button9_Click(object sender, EventArgs e)
        {

            //save gps and data dir values to the registry
            //check for valid values

            portNum = (string)comboBox1.SelectedItem;
            baudRate = (string)comboBox2.SelectedItem;
            gpsCountry = (string)comboBox3.SelectedItem;

            

            //if ((dataDir == null) || (portNum == null) || (baudRate == null) || (gpsCountry == null))
            //{
                //warningMessageBox.setLabel("Warning: Please select settings before saving");
                //warningMessageBox.ShowDialog();

            //}
            //else
            //{
                RegistryKey regKey = Registry.CurrentUser.OpenSubKey(keyName, RegistryKeyPermissionCheck.ReadWriteSubTree);
                if (regKey == null)
                {
                    //not present: create the key
                    regKey = Registry.CurrentUser;
                    regKey= regKey.CreateSubKey(keyName);

                }


                    //save data in this key


                if (dataDir != null)
                {
                    regKey.SetValue("DATA_DIR", dataDir);

                }

                if (portNum != null)
                {
                    regKey.SetValue("GPS_PORT", portNum);

                }

                if (baudRate != null)
                {

                    regKey.SetValue("GPS_BAUD", baudRate);

                }

                if (gpsCountry != null)
                {

                    regKey.SetValue("GPS_COUNTRY", gpsCountry);


                }
                
                    
                    
                    

                    if (Utils.GPSenabled)
                    {
                        regKey.SetValue("GPS_ENABLED", "TRUE");

                    }
                    else
                    {
                        regKey.SetValue("GPS_ENABLED", "FALSE");


                    }

                    regKey.Close();


               

           // }


            


        }

        public void questionFormClosing()
        {

            // a questionnaire is calling this to tell us it is closing
            qFormIsOpen = false;

            //this might have been a new question, so rebuild our list of existing Qs
            updateExistingQuestionList();

            openParticipantID = null;

            //turn GPS logging on if it was on previously:
            /*
            if (loggerStatus == "suspended")
            {

                portNum = (string)comboBox1.SelectedItem;
                baudRate = (string)comboBox2.SelectedItem;

                if ((portNum == null) || (baudRate == null))
                {

                    //show fail
                    warningMessageBox.setLabel("Warning: Please select Port and Baud Rate");
                    warningMessageBox.ShowDialog();


                }
                else
                {
                    //test serial port connection

                    serialPort1.PortName = portNum;

                    //comm port for laptop
                    //serialPort1.PortName = "COM4";

                    serialPort1.BaudRate = Convert.ToInt32(baudRate);

                    //start the timer which will update the GPS data each half second
                    timer1.Enabled = true;


                    //try and open the serial port for GPS comms
                    try
                    {
                        serialPort1.Open();

                        label11.Text = "Starting...";
                        label12.Text = "Starting...";

                        loggerStatus = "on";


                    }
                    catch
                    {

                        //show warning screen
                        timer1.Enabled = false;

                        warningMessageBox.setLabel("Warning: Can't open serial port connection to the GPS unit. Make sure the unit is plugged in.");
                        warningMessageBox.ShowDialog();

                    }


                }




            }
             * */


        }




        private void button1_Click(object sender, EventArgs e)
        {

            //new questionnaire

            //do we have a questionnaire open already ?
            if (qFormIsOpen)
            {
                //still open
                errorBox.setLabel("Error: You have a Questionnaire already open: you must close it first.");
                errorBox.ShowDialog();
                return;



            }

            //has the user selected a language and participant ID

            string language = (string)listBox1.SelectedItem;

            if (language == null)
            {
                errorBox.setLabel("Error: Please select language");
                errorBox.ShowDialog();
                return;



            }

            string xmlFilePath;

            //what is the filePath for the xml config file
            if(language == "English"){

                xmlFilePath= dataDir + "\\EQuestionnaire_English.xml";

            }

            else if (language == "Chewa")
            {

                xmlFilePath = dataDir + "\\EQuestionnaire_Chewa.xml";


            }




            else
            {

                //language not supported
                errorBox.setLabel("Error: Language '" + language + " is not currently supported");
                errorBox.ShowDialog();
                return;


            }

            
            //check that the xml file exists
            if(! File.Exists(xmlFilePath)){

                errorBox.setLabel("Error: Cannot find expected configuration file:" + xmlFilePath);
                errorBox.ShowDialog();
                return;


            }




            //participantID
            string partID = textBox1.Text;

            if (string.IsNullOrWhiteSpace(partID))
            {

                errorBox.setLabel("Error: Please enter a Participant ID");
                errorBox.ShowDialog();
                return;


            }

            //check that the partID does not contain any underscore characters (which will cause problems as its gets embedded into dir/file names which use underscores as separators
            if (partID.Contains("_"))
            {

                errorBox.setLabel("Error: The Participant ID contains underscore character(s), which is not allowed.");
                errorBox.ShowDialog();
                return;

            }


            //does this participant already exist (in which case they should be using the existing participant page)?
            foreach (Participant part in listBox2.Items)
            {
                if (partID == part.getID())
                {

                    errorBox.setLabel("Error: This participant already exists");
                    errorBox.ShowDialog();
                    return;

                }




            }








            //has the user set the GPS baud rate, port and the main data dir ?

            portNum = (string)comboBox1.SelectedItem;
            baudRate = (string)comboBox2.SelectedItem;
            gpsCountry = (string)comboBox3.SelectedItem;

            if (dataDir == null)
            {
                errorBox.setLabel("Error: Please set the data directory (settings)");
                errorBox.ShowDialog();
                return;

            }

            if (portNum == null && Utils.GPSenabled)
            {

                errorBox.setLabel("Error: Please set the GPS port Number (settings)");
                errorBox.ShowDialog();
                return;

            }

            if (baudRate == null && Utils.GPSenabled)
            {

                errorBox.setLabel("Error: Please set the GPS baudrate (settings)");
                errorBox.ShowDialog();
                return;

            }

            if (gpsCountry == null && Utils.GPSenabled)
            {
                errorBox.setLabel("Error: Please set the GPS country (settings)");
                errorBox.ShowDialog();
                return;

            }


            //check that this participant does not already exist, i.e. that the new datadir does not exist

            string partDataDir = dataDir + "\\participant_data_" + language + "_" + partID;

            

            if (Directory.Exists(partDataDir))
            {
                //user already exists
                errorBox.setLabel("Error: This Participant already exists.");
                errorBox.ShowDialog();
                return;


            }


            //create the new dir.
            try
            {

                Directory.CreateDirectory(partDataDir);


            }
            catch
            {

                errorBox.setLabel("Error: Could not create directory for this participant. You may not have permission to write to this location");
                errorBox.ShowDialog();
                return;

            }



            //if the logger is running: suspend it
            //i.e. disconnect from the serial port
            /*
            if (loggerStatus == "on")
            {

                //turn off until user exits the Q window
                //disable the timer
                timer1.Enabled = false;

                //close the serial port
                if (serialPort1.IsOpen)
                {
                    serialPort1.Close();

                }

                label11.Text = "suspended while Questionnaire is open";
                label12.Text = "suspended while Questionnaire is open";

                loggerStatus = "suspended";



            }
             * */



            

            //open the form
            currentSurvey = new Form1();

            try
            {

                //start the survey
                currentSurvey.startSurvey(xmlFilePath, partDataDir, partID, true, portNum, baudRate, this, gpsCountry);

                currentSurvey.Show();

                qFormIsOpen = true;

                openParticipantID = partID;




            }
            catch (ObjectDisposedException e2)
            {

                //something went wrong with startup, e.g. XML parsing exception

                qFormIsOpen = false;


            }

            

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //edit selected questionnaire

            //do we have a questionnaire open already ?
            if (qFormIsOpen)
            {
                //still open
                errorBox.setLabel("Error: You have a Questionnaire already open: you must close it first.");
                errorBox.ShowDialog();
                return;



            }

            Participant selectedPart = (Participant)listBox2.SelectedItem;

            if (selectedPart == null)
            {

                errorBox.setLabel("Error: Please select a Participant from the list");
                errorBox.ShowDialog();
                return;



            }

            //is the Part locked
            if (selectedPart.Locked)
            {
                errorBox.setLabel("Error: Cannot edit a locked Participant");
                errorBox.ShowDialog();
                return;

            }

            if (dataDir == null)
            {
                errorBox.setLabel("Error: Please set the data directory (settings)");
                errorBox.ShowDialog();
                return;

            }


            //open the form
            

            //has the user selected a language and participant ID

            string language = selectedPart.getLanguage();

            string xmlFilePath;

            //what is the filePath for the xml config file
            if (language == "English")
            {

                xmlFilePath = dataDir + "\\EQuestionnaire_English.xml";

            }
            else if (language == "Chewa")
            {

                xmlFilePath = dataDir + "\\EQuestionnaire_Chewa.xml";


            }
            else
            {

                //language not supported
                errorBox.setLabel("Error: Language '" + language + " is not currently supported");
                errorBox.ShowDialog();
                return;


            }


            //check that the xml file exists
            if (!File.Exists(xmlFilePath))
            {

                errorBox.setLabel("Error: Cannot find expected configuration file:" + xmlFilePath);
                errorBox.ShowDialog();
                return;


            }




            //participantID
            string partID = selectedPart.getID();

           


            //has the user set the GPS baud rate, port and the main data dir ?

            portNum = (string)comboBox1.SelectedItem;
            baudRate = (string)comboBox2.SelectedItem;
            gpsCountry = (string)comboBox3.SelectedItem;


            if (portNum == null && Utils.GPSenabled)
            {

                errorBox.setLabel("Error: Please set the GPS port Number (settings)");
                errorBox.ShowDialog();
                return;

            }

            if (baudRate == null && Utils.GPSenabled)
            {

                errorBox.setLabel("Error: Please set the GPS baudrate (settings)");
                errorBox.ShowDialog();
                return;

            }

            if (gpsCountry == null && Utils.GPSenabled)
            {
                errorBox.setLabel("Error: Please set the GPS country (settings)");
                errorBox.ShowDialog();
                return;

            }


            //check that this participant does not already exist, i.e. that the new datadir does not exist

            string partDataDir = selectedPart.getPath();


            //turn off the GPS logger if on
            /*
            if (loggerStatus == "on")
            {

                //turn off until user exits the Q window
                //disable the timer
                timer1.Enabled = false;

                //close the serial port
                if (serialPort1.IsOpen)
                {
                    serialPort1.Close();

                }

                label11.Text = "suspended while Questionnaire is open";
                label12.Text = "suspended while Questionnaire is open";

                loggerStatus = "suspended";



            }
             * */
            


            //open the form
            currentSurvey = new Form1();


           

            try
            {

                //start the survey
                currentSurvey.startSurvey(xmlFilePath, partDataDir, partID, false, portNum, baudRate, this, gpsCountry);

                currentSurvey.Show();

                qFormIsOpen = true;

                openParticipantID = partID;




            }
            catch(ObjectDisposedException e2){

                //something went wrong with startup, e.g. XML parsing exception
                
                qFormIsOpen = false;


            }

            
            


        }

        private void updateExistingQuestionList()
        {
            //update the list of questions shown in the listbox

            if (dataDir != null)
            {

                //delete current contents of the listbox
                listBox2.Items.Clear();

                List<Participant> partList = new List<Participant>();
                


                
                //get all the directories from the base dir of the form: participant_data_
                DirectoryInfo di = new DirectoryInfo(dataDir);
                string subDirName;
                string subDirPath;
                string partID;
                string language;


                foreach (DirectoryInfo subDir in di.GetDirectories())
                {
                    subDirName = subDir.Name;

                    Match match = Regex.Match(subDirName, "^participant_data_([^_]+)_([^_]+)$");

                    if (match.Success)
                    {


                        language = match.Groups[1].Value;
                        partID = match.Groups[2].Value;
                        subDirPath = subDir.FullName;

                        //does the lockfile exist, i.e. meaning the Participant should be treated as locked
                        string lockFileName = subDirPath + "\\lockfile.txt";

                        bool partIsLocked;

                        if (File.Exists(lockFileName))
                        {
                            partIsLocked = true;

                        }
                        else
                        {
                            partIsLocked = false;

                        }

                        Participant part = new Participant(subDirPath, partID, language);
                        part.Locked = partIsLocked;



                        //a valid participant dir
                        //listBox2.Items.Add(part);

                        partList.Add(part);



                    }




                }

                //order the participants

                //are all of these items numeric?
                

                int partIDasInt;

                bool allAreNumeric = true;

                foreach (Participant part in partList)
                {
                    //is this ID numeric
                    string partid = part.getID();

                    try
                    {
                        partIDasInt = Convert.ToInt32(partid);
                        part.setPartIDint(partIDasInt);




                    }
                    catch
                    {

                        allAreNumeric = false;

                    }




                }

                IEnumerable<Participant> partListSorted = null;

                if (allAreNumeric)
                {
                    //order as numbers
                    partListSorted= partList.OrderBy(n => n.getPartIDint());




                }
                else
                {
                    //order as strings
                    partListSorted= partList.OrderBy(n => n.getID());




                }


                //add these to the listBox
                foreach (Participant part in partListSorted)
                {

                    listBox2.Items.Add(part);

                }




            }



        }

        private void button3_Click(object sender, EventArgs e)
        {

            //delete the selected Questionnaire

            Participant selectedPart = (Participant)listBox2.SelectedItem;

            if (selectedPart == null)
            {

                errorBox.setLabel("Error: Please select a Participant from the list");
                errorBox.ShowDialog();
                return;



            }

            //is the Part locked
            if (selectedPart.Locked)
            {
                errorBox.setLabel("Error: Cannot delete a locked Participant");
                errorBox.ShowDialog();
                return;

            }




            //do we have a questionnaire open already ?
            //if we have an open questionnaire, make sure it is not the one we want to delete
            if (qFormIsOpen)
            {
                
                //is this the same as the one we want to delete
                if (openParticipantID == selectedPart.getID())
                {
                    //yes: stop deletion
                    errorBox.setLabel("Error: You must close a Questionnaire before you can delete it.");
                    errorBox.ShowDialog();
                    return;



                }
                
                

            }

            
            //make sure that the user really does want to delete this
            confirmForm.setFormLabel("Are you sure you want to delete this participant ?", this);
            confirmForm.ShowDialog();

            if (confirmResult == "yes")
            {
                //get the path to the dir
                string dirPath = selectedPart.getPath();

                //delete the dir
                try
                {
                    Directory.Delete(dirPath, true);

                    //refresh the list of Parts
                    updateExistingQuestionList();

                    //stdMessageBox.setMainLabel("Participant deleted");
                    //stdMessageBox.ShowDialog();


                }
                catch
                {
                    errorBox.setLabel("Error: The Participant could not be deleted");
                    errorBox.ShowDialog();

                }
                





            }









        }

        public void lockQuestionnaire()
        {

             
            
            //user wants to lock the selected Questionnaire
            Participant selectedPart = (Participant)listBox2.SelectedItem;

            if (selectedPart == null)
            {

                errorBox.setLabel("Error: Please select a Participant from the list");
                errorBox.ShowDialog();
                return;



            }

            //is it locked already ?

            if (selectedPart.Locked)
            {

                errorBox.setLabel("Error: This Participant is locked already");
                errorBox.ShowDialog();
                return;


            }


            //do we have a questionnaire open already ?
            //if we have an open questionnaire, make sure it is not the one we want to lock
            if (qFormIsOpen)
            {

                //is this the same as the one we want to delete
                if (openParticipantID == selectedPart.getID())
                {
                    //yes: stop deletion
                    errorBox.setLabel("Error: You must close a Questionnaire before you can lock it.");
                    errorBox.ShowDialog();
                    return;



                }



            }

            //place a lockfile inside the dir for this participant as a marker
            string partDir = selectedPart.getPath();
            string lockFileName = partDir + "\\lockfile.txt";

            try
            {

                FileStream fh = File.Create(lockFileName);
                fh.Close();



            }
            catch
            {

                errorBox.setLabel("Error: Could not lock this Participant");
                errorBox.ShowDialog();
                return;

            }




            //redraw the list
            updateExistingQuestionList();



        }



        private void button4_Click(object sender, EventArgs e)
        {
            
            

            
            


        }

        private void button5_Click(object sender, EventArgs e)
        {

            //unlock the selected Participant
            Participant selectedPart = (Participant)listBox2.SelectedItem;

            if (selectedPart == null)
            {

                errorBox.setLabel("Error: Please select a Participant from the list");
                errorBox.ShowDialog();
                return;



            }

            //is the particpant locked
            if (! selectedPart.Locked)
            {
                errorBox.setLabel("Error: This Participant is NOT locked");
                errorBox.ShowDialog();
                return;


            }

            //delete the  lockfile 
            string partDir = selectedPart.getPath();
            string lockFileName = partDir + "\\lockfile.txt";

            try
            {

                File.Delete(lockFileName);
                
               

            }
            catch
            {

                errorBox.setLabel("Error: Could not unlock this Participant");
                errorBox.ShowDialog();
                return;

            }


            //redraw the list
            updateExistingQuestionList();








        }

        private void button6_Click(object sender, EventArgs e)
        {
            //copy all data

            //ask the user to specify a dir to copy to (e.g. on a memory stick)

            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Select the folder to copy data files to";

            string toDataDir;

      

            // Show the FolderBrowserDialog.
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                toDataDir = folderBrowserDialog.SelectedPath;

                //for each Participant, fetch each result file and copy to the toDataDir
                foreach (Participant part in listBox2.Items)
                {

                    string partDir = part.getPath();
                    string partID = part.getID();

                    string finalDataSourcePath = partDir + "\\final_data_" + partID + ".txt";

                    //if the data is locked, add this to the dest file name
                    string finalDataDestPath = null;
                    if (part.Locked)
                    {
                        //locked
                        finalDataDestPath = toDataDir + "\\locked_final_data_" + partID + ".txt";

                    }
                    else
                    {
                        //not locked
                        finalDataDestPath = toDataDir + "\\final_data_" + partID + ".txt";


                    }



                    

                    try
                    {

                        File.Copy(finalDataSourcePath, finalDataDestPath, true);

                    }
                    //ignore problems where the source file does not exist, which can sometimes happen for legitimate reasons
                    catch (FileNotFoundException e2) { }

                    catch(Exception e3)
                    {

                        errorBox.setLabel("Error: Could not copy file");
                        errorBox.ShowDialog();
                        return;

                    }

                    

                }



            }





        }


        /*

        private void button10_Click(object sender, EventArgs e)
        {

            //start the GPS logger

            //check its not already running or in use by the Questionnaire window
            if (loggerStatus == "on")
            {

                errorBox.setLabel("Error: The logger is already in use");
                errorBox.ShowDialog();
                return;


            }


            if (loggerStatus == "suspended")
            {

                errorBox.setLabel("Error: The GPS connection is already in use (Questionnaire Window)");
                errorBox.ShowDialog();
                return;


            }

            if (qFormIsOpen)
            {
                errorBox.setLabel("Error: The GPS connection is already in use (Questionnaire Window)");
                errorBox.ShowDialog();
                return;


            }

            //check that the data directory is set and exists

            if (dataDir == null || (!Directory.Exists(dataDir)))
            {
                errorBox.setLabel("Error: Please set the data directory (settings)");
                errorBox.ShowDialog();
                return;

            }



            portNum = (string)comboBox1.SelectedItem;
            baudRate = (string)comboBox2.SelectedItem;

            if ((portNum == null) || (baudRate == null))
            {

                //show fail
                warningMessageBox.setLabel("Warning: Please select Port and Baud Rate");
                warningMessageBox.ShowDialog();


            }
            else
            {
                //test serial port connection

                serialPort1.PortName = portNum;

                //comm port for laptop
                //serialPort1.PortName = "COM4";

                serialPort1.BaudRate = Convert.ToInt32(baudRate);

                //start the timer which will update the GPS data each half second
                timer1.Enabled = true;


                //try and open the serial port for GPS comms
                try
                {
                    serialPort1.Open();

                    label11.Text = "Starting...";
                    label12.Text = "Starting...";

                    loggerStatus = "on";






                }
                catch
                {

                    //show warning screen
                    timer1.Enabled = false;

                    warningMessageBox.setLabel("Warning: Can't open serial port connection to the GPS unit. Make sure the unit is plugged in.");
                    warningMessageBox.ShowDialog();

                }


            }


        }
         * 
         */

        private void timer1_Tick(object sender, EventArgs e)
        {

            string latitude;
            string longitude;



            if (serialPort1.IsOpen)
            {
                string data = serialPort1.ReadExisting();
                string[] strArr = data.Split('$');
                for (int i = 0; i < strArr.Length; i++)
                {
                    string strTemp = strArr[i];

                    //test
                    //string strTemp = "GPGGA,124053.000,5204.6890,N,00011.1347,E,1,05,3.2,38.0,M,47.0,M,,0000*64";



                    string[] lineArr = strTemp.Split(',');
                    if (lineArr[0] == "GPGGA")
                    {
                        try
                        {
                            //Latitude

                            /*
                            Double dLat = Convert.ToDouble(lineArr[2]);
                            dLat = dLat / 100;
                            string[] lat = dLat.ToString().Split('.');
                            latitude = lineArr[3].ToString() +
                            lat[0].ToString() + "." +
                            ((Convert.ToDouble(lat[1]) /
                            60)).ToString("#####");
                            */

                            //writeLog(strTemp);

                            latitude = Utils.getPosition(lineArr[2], lineArr[3], true);




                            //Longitude

                            /*
                            Double dLon = Convert.ToDouble(lineArr[4]);
                            dLon = dLon / 100;
                            string[] lon = dLon.ToString().Split('.');
                            longitude = lineArr[5].ToString() +
                            lon[0].ToString() + "." +
                            ((Convert.ToDouble(lon[1]) /
                            60)).ToString("#####");

                            */
                             
                            longitude = Utils.getPosition(lineArr[4], lineArr[5], true);


                            //Display
                            //label11.Text = latitude;
                            //label12.Text = longitude;

                            //writeLog("latitude:" + latitude + ", longitude:" + longitude);
                            writeLog(latitude + "\t" + longitude);
                            


                        }
                        catch
                        {
                            //Cannot Read GPS values


                            //label11.Text = "unavailable (no position fix)";
                            //label12.Text = "unavailable (no position fix)";

                            //writeLog("latitude: unavailable, longitude: unavailable");
                            writeLog("unavailable");



                        }
                    }
                }
            }
            else
            {

                //disable the timer
                timer1.Enabled = false;

                warningMessageBox.setLabel("Warning: Can't open serial port connection to the GPS unit. Make sure the unit is plugged in.");

                warningMessageBox.ShowDialog();

                //label11.Text = "disconnected";
                //label12.Text = "disconnected";




            }






        }

        


        private void writeLog(string data)
        {
            //check the logfile:

            string logfile = dataDir + "\\gps_logger.txt";

            //append to log file
            if (!File.Exists(logfile))
            {
                

                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(logfile))
                try
                {
                    //StreamWriter sw = File.CreateText(logfile);
                    sw.WriteLine(data);
                }
                catch (Exception ee)
                {
                    MessageBox.Show(ee.StackTrace);

                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(logfile))
                {
                    sw.WriteLine(data);



                }



            }




        }




        /*
        private void button11_Click(object sender, EventArgs e)
        {
            //stop logging

            //disable the timer
            timer1.Enabled = false;

            //close the serial port
            if (serialPort1.IsOpen)
            {
                serialPort1.Close();

            }

            label11.Text = "disconnected";
            label12.Text = "disconnected";

            loggerStatus = "off";



        }
         */ 

        private void button12_Click(object sender, EventArgs e)
        {
            //copy GPS logging data.

            //make sure logging is disabled

            if (loggerStatus == "on")
            {
                warningMessageBox.setLabel("Warning: Turn off logging before copying data");

                warningMessageBox.ShowDialog();

                return;



            }

            //list of GPSposition objects
            List<GPSposition> posList = new List<GPSposition>();


            //read the data into memory
            string logfile = dataDir + "\\gps_logger.txt";

            char[] splitChars = {'\t'};

            if (File.Exists(logfile))
            {
                StreamReader dh = null;
                StreamWriter sr = null;

                try
                {

                    //read the data from the logfile
                    
                    dh = new StreamReader(logfile);

                    while (dh.EndOfStream == false)
                    {
                        string line = dh.ReadLine();

                        //ignore unavaiable lines
                        if (line != "unavailable")
                        {

                            string[] items = line.Split(splitChars);

                            //first will be latitude, second will be longitude
                            posList.Add(new GPSposition(items[0], items[1]));


                        }



                    }

                    dh.Close();

                    //ask the user for a file to write the kml to

                    string saveToFile = null;

                    SaveFileDialog sfd = new SaveFileDialog();

                    //must be a kml file
                    sfd.Filter = "kml files (*.kml)|*.kml";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {

                        saveToFile = sfd.FileName;



                    }


                    //write the KML
                    sr = new StreamWriter(saveToFile);

                    //KML headers

                    sr.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sr.WriteLine("<kml xmlns=\"http://www.opengis.net/kml/2.2\">");
                    sr.WriteLine("<Document>");
                    sr.WriteLine("<name>Points with TimeStamps</name>");

                    //icons
                    sr.WriteLine("<Style id=\"hiker-icon\">");
                    sr.WriteLine("<IconStyle>");
                    sr.WriteLine("<Icon>");
                    sr.WriteLine("<href>http://maps.google.com/mapfiles/ms/icons/hiker.png</href>");
                    sr.WriteLine("</Icon>");
                    sr.WriteLine("<hotSpot x=\"0\" y=\".5\" xunits=\"fraction\" yunits=\"fraction\"/>");
                    sr.WriteLine("</IconStyle>");
                    sr.WriteLine("</Style>");

                    //placemarks

                    foreach (GPSposition pos in posList)
                    {

                        sr.WriteLine(pos.getXMLposition("#hiker-icon"));

                    }


                    // final elements

                    sr.WriteLine("</Document></kml>");
                    




                }
                catch(Exception ex)
                {

                    errorBox.setLabel("Error: Could not load log data");
                    errorBox.ShowDialog();

                    MessageBox.Show(ex.Message + ": " + ex.StackTrace);
                    
                    


                }

                finally
                {

                    if (dh != null)
                    {
                        dh.Close();


                    }

                    if (sr != null)
                    {

                        sr.Close();

                    }

                }
                




            }
            else
            {

                warningMessageBox.setLabel("Warning: No logging data was found.");

                warningMessageBox.ShowDialog();


            }






        }

        public void checkCompleteness(bool lockingEnabled, bool newUser)
        {
            //locking should only be enabled if this is called from the baseform, but not from the Qform

            //check the completeness of the selected questionnaire
            //do we have a questionnaire open already ?

            /*
            if (qFormIsOpen)
            {
                //still open
                errorBox.setLabel("Error: You have a Questionnaire already open: you must close it first.");
                errorBox.ShowDialog();
                return;



            }
             * 
             * */

            Participant selectedPart = null;

            if (newUser)
            {

                //we might be using a new participant, which won't be selected or even in this list

                
                    //update the question list
                    updateExistingQuestionList();

                    //get the participant from the list
                    foreach (Participant part in listBox2.Items)
                    {

                        //MessageBox.Show(part.getID());


                        if (openParticipantID == part.getID())
                        {
                            selectedPart = part;
                            break;



                        }

                    }

                    if (selectedPart == null)
                    {
                        //something has gone wrong
                        errorBox.setLabel("Error: Can't show status: " + openParticipantID);
                        errorBox.ShowDialog();
                        return;



                    }



            }
            else
            {

                selectedPart = (Participant)listBox2.SelectedItem;

                if (selectedPart == null)
                {

                    errorBox.setLabel("Error: Please select a Participant from the list");
                    errorBox.ShowDialog();
                    return;


                }



            }



            if (dataDir == null)
            {
                errorBox.setLabel("Error: Please set the data directory (settings)");
                errorBox.ShowDialog();
                return;

            }


            //open the form


            //has the user selected a language and participant ID

            string language = selectedPart.getLanguage();

            string xmlFilePath;

            //what is the filePath for the xml config file
            if (language == "English")
            {

                xmlFilePath = dataDir + "\\EQuestionnaire_English.xml";

            }
            else if (language == "Chewa")
            {

                xmlFilePath = dataDir + "\\EQuestionnaire_Chewa.xml";


            }
            else
            {

                //language not supported
                errorBox.setLabel("Error: Language '" + language + " is not currently supported");
                errorBox.ShowDialog();
                return;


            }


            //check that the xml file exists
            if (!File.Exists(xmlFilePath))
            {

                errorBox.setLabel("Error: Cannot find expected configuration file:" + xmlFilePath);
                errorBox.ShowDialog();
                return;


            }




            //participantID
            string partID = selectedPart.getID();



            //check that this participant does not already exist, i.e. that the new datadir does not exist

            string partDataDir = selectedPart.getPath();


            //open the form
            currentSurveyComp = new CompletenessForm(lockingEnabled);




            try
            {

                //start the survey
                currentSurveyComp.startCheck(xmlFilePath, partDataDir, partID, this);

                currentSurveyComp.Show();

                //qFormIsOpen = true;

                //openParticipantID = partID;




            }
            catch (ObjectDisposedException e2)
            {

                //something went wrong with startup, e.g. XML parsing exception

                //qFormIsOpen = false;


            }




        }



        private void button13_Click(object sender, EventArgs e)
        {

            checkCompleteness(true, false);



        }

        private void button4_Click_1(object sender, EventArgs e)
        {

            //delete any data present in the log file by deleting the file.
            string logfile = dataDir + "\\gps_logger.txt";

            //check that the data directory is set and exists

            if (dataDir == null || (!Directory.Exists(dataDir)))
            {
                errorBox.setLabel("Error: Please set the data directory (settings)");
                errorBox.ShowDialog();
                return;

            }

            //is the user sure ?

            

            confirmForm.setFormLabel("Are you sure you want to delete all previous locations ?", this);
            confirmForm.ShowDialog();

            if (confirmResult == "yes")
            {
                //delete the file
                try
                {
                    File.Delete(logfile);


                }
                catch
                {

                    errorBox.setLabel("Error: Could not delete locations");
                    errorBox.ShowDialog();
                    return;

                }
                




            }





        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

            //is this checked ?

            if (checkBox1.Checked)
            {
                //enable GPS controls

                groupBox1.Enabled = true;

                Utils.GPSenabled = true;





            }
            else
            {
                //disable GPS controls
                groupBox1.Enabled = false;

                Utils.GPSenabled = false;



            }





        }


       

       


    }
}
