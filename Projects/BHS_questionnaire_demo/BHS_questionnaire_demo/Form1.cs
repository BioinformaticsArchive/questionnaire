
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
using System.IO;

namespace BHS_questionnaire_demo
{

    enum UserDirection
    {
        forward,
        reverse


    };
    
    
    
    
    
    
    public partial class Form1 : Form
    {

        //dir to store data files
        private String dataDir = "C:\\Program Files\\BHS_survey_data";
        //private TextBox myTextBox;
        //private Label myLabel;
        //private GroupBox myRadioGroup;  //container for a radio button set

        //a big version of a message box for errors
        private Form2 bigMessageBox;

        //a message box for warnings
        private Form3 warningMessageBox;

        //an info box
        private MessageForm stdMessageBox;

        //advice box
        private AdviceForm adviceBox;

        //confirm box (has yes/No buttons)
        private ConfirmForm confirmBox;


        private QuestionManager qm;

        private string xmlFileName;

        //map section name to first QuestionCode in that section
        private Dictionary<string, string> sectionToCodeMap;

        private string latitude = null;
        private string longitude = null;

        //reference to parent form
        private BaseForm baseForm = null;

        //is this a new or existing user?
        private bool newUser;

        //used by confirm window to send back result
        public string confirmResult
        {
            get;
            set;

        }

        
        
        
        
        public Form1()
        {
            InitializeComponent();

            

            
            //set colours of main widgets
            BackColor = GlobalColours.mainFormColour;

            panel1.BackColor = GlobalColours.mainPanelColour;

            button1.BackColor = GlobalColours.mainButtonColour;

            button2.BackColor = GlobalColours.mainButtonColour;

            button3.BackColor = GlobalColours.altButtonColour;

            GPSconnectButton.BackColor = GlobalColours.altButtonColour;

            button4.BackColor = GlobalColours.altButtonColour;

            //populate the map for sections
            sectionToCodeMap = new Dictionary<string, string>();

            //South Africa
            sectionToCodeMap.Add("Consents", "START");
            sectionToCodeMap.Add("Personal History", "PERSONAL");
            sectionToCodeMap.Add("Zulu", "ZULU");
            sectionToCodeMap.Add("Family History", "FAMILY");
            sectionToCodeMap.Add("Diabetes", "DIABETES");
            sectionToCodeMap.Add("Clinical Information", "CLINICAL");
            sectionToCodeMap.Add("Clinical Measurements", "MEASURE");




            //Malawi
            /*
            sectionToCodeMap.Add("Consents", "START");
            sectionToCodeMap.Add("Demographic Information", "DEM0");
            sectionToCodeMap.Add("Education, Occupation and Livelihood", "EDLEV0");
            sectionToCodeMap.Add("Health: Tobacco Use", "TOB0");
            sectionToCodeMap.Add("Health: Alcohol Consumption", "ALC0");
            sectionToCodeMap.Add("Health: Diet", "DIET0");
            sectionToCodeMap.Add("Physical Activity: Work", "PHYS00");
            sectionToCodeMap.Add("Physical Activity: Travel to and from places", "PHYS02");
            sectionToCodeMap.Add("Physical Activity: Recreational Activities", "PHYS03");
            sectionToCodeMap.Add("Physical Activity: Sedentary Behaviour", "PHYS04");
            sectionToCodeMap.Add("History of Raised Blood Pressure", "HBP00");
            sectionToCodeMap.Add("History of Diabetes", "HD0");
            sectionToCodeMap.Add("History of High Cholesterol", "HC0");
            sectionToCodeMap.Add("History of Immunisation", "HI0");
            sectionToCodeMap.Add("Physical Measurements: Blood Pressure", "BP0");
            sectionToCodeMap.Add("Physical Measurements: Anthropometry", "HTID");
            sectionToCodeMap.Add("Blood Sample", "NURCODE");
            sectionToCodeMap.Add("Advice", "ADVICE_BMI");
            sectionToCodeMap.Add("Final Comments", "THANKYOU");
             */ 


            bigMessageBox = new Form2();

            warningMessageBox = new Form3();

            stdMessageBox = new MessageForm();

            adviceBox = new AdviceForm();

            confirmBox = new ConfirmForm();
            
            



        }

        public void startSurvey(string xmlFileName, string userDir, string userID, bool newUser, string gpsPort, string gpsBaud, BaseForm baseForm, string gpsCountry)
        {
            //called after loading the form: passes in needed vars from baseform
            //open a file dialog box

            button1.Enabled = false;

            //reference the the parent form
            this.baseForm = baseForm;

            this.newUser = newUser;


            //application title
            Text = "Questionnaire for: " + userID;



            qm = new QuestionManager(bigMessageBox, userDir, panel1, label2, userID, label3, label1, adviceBox, confirmBox, this, gpsCountry);
            qm.setWarningBox(warningMessageBox);

            try
            {

                qm.ParseConfigXML(xmlFileName, this);



            }
            catch(Exception e)
            {
                //parsing error
                
                bigMessageBox.setLabel("Error: There is an error in the XML configuration file");
                bigMessageBox.ShowDialog();

                MessageBox.Show(e.StackTrace);

                //exit form

                Close();


            }
            

            

            string startQuestionCode= null;


            //load in previous data if this is not a new user
            if (!newUser)
            {
                //old user

                try
                {

                    qm.load();

                    //pop the stack to get the start question
                    startQuestionCode = qm.getCodeAtTopOfStack();


                }
                catch
                {
                    //probably a Questionnaire that was terminated on the start page, i.e. counts as existing in the parent form
                    //but has no data files
                    //startQuestionCode = "START";

                    bigMessageBox.setLabel("Error: Cannot load data: please delete this Questionnaire and start a new one");
                    bigMessageBox.ShowDialog();

                    

                    //exit form

                    Close();

                    return;


                }
                

            }
            else
            {
                //new user
                startQuestionCode = "START";


            }



            //set the question manager to point to the first question: Code= Name
            //qm.setCurrentQuestion("START");

            qm.setCurrentQuestion(startQuestionCode);

            //configure the controls appropriately for the first question:

            qm.configureControls(UserDirection.forward);

            setMainButtonStatus();

            

            //set serial port properties

            //comm port for ViewSonic
            //serialPort1.PortName = "COM3";

            //comm port for laptop
            //serialPort1.PortName = "COM4";



            if (Utils.GPSenabled)
            {

                serialPort1.PortName = gpsPort;
                serialPort1.BaudRate = Convert.ToInt32(gpsBaud);

                //start the timer which will update the GPS data each half second
                timer1.Enabled = true;


                //try and open the serial port for GPS comms
                try
                {
                    serialPort1.Open();



                }
                catch
                {

                    //show warning screen
                    timer1.Enabled = false;

                    warningMessageBox.setLabel("Warning: Can't open serial port connection to the GPS unit. Make sure the unit is plugged in.");
                    warningMessageBox.ShowDialog();

                }




            }
            else
            {

                //disable GPS related features
                longitudeLabel.Visible = false;
                latitudeLabel.Visible = false;
                //button that jumps to the GPS view
                button3.Enabled = false;

                //GPS connect button
                GPSconnectButton.Enabled = false;





            }



            //qm.showDebug();


        }

        private void Form1_Load(object sender, EventArgs e)
        {

            
            





        }

        private void button2_MouseClick(object sender, MouseEventArgs e)
        {

            //user has clicked the 'next' button.
            

            //enable the previous-page button
            button1.Enabled = true;

            //we need to process the data entered by the user
            qm.processUserData();

            //save the current data to disk
            try
            {
                qm.save();


            }
            catch
            {

                //save failed: exit
                bigMessageBox.setLabel("Save Failed: Data for this Questionnaire may be corrupt !");
                bigMessageBox.ShowDialog();

                //exit the window.
                this.Close();




            }
            


            //delete the current widgets on the form
            qm.removeControls();

            //display the next question
            qm.setNextQuestion();

            qm.configureControls(UserDirection.forward);

            //make sure the forward and reverse buttons have the correct enabled/disabled
            setMainButtonStatus();

            



        }

        private void button1_Click(object sender, EventArgs e)
        {

            //user has clicked the 'previous' button, i.e. we need to reverse to the previous question

            //delete the current widgets on the form
            qm.removeControls();

            //display the previous question
            qm.setPreviousQuestion();

            qm.configureControls(UserDirection.reverse);

            //make sure the forward and reverse buttons have the correct enabled/disabled
            setMainButtonStatus();

           




        }

        private void setMainButtonStatus()
        {
            //is this the first page?
            if (qm.isFirstPage())
            {

                button1.Enabled = false;
                button2.Enabled = true;


            }
                //is this the last page ?

            else if (qm.isLastPage())
            {
                button1.Enabled = true;
                button2.Enabled = false;

            }
            else
            {
                //both enabled for middle pages
                button1.Enabled = true;
                button2.Enabled = true;


            }


        }

        private void button3_MouseClick(object sender, MouseEventArgs e)
        {

            //skip this question

            //set a flag in the QuestionManager, the process the forward button

            qm.SkipThisQuestion = true;
            
            button2_MouseClick(sender, e);

            //reset flag
            qm.SkipThisQuestion = false;


        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {

            string selectedSection = (string) comboBox1.SelectedItem;
            
           //map the section name to the question code at the start of that section

            string qCode = sectionToCodeMap[selectedSection];

            //delete the current widgets on the form
            qm.removeControls();

            //display the next question
            qm.setNextQuestion(qCode);

            qm.configureControls(UserDirection.forward);

            //make sure the forward and reverse buttons have the correct enabled/disabled
            setMainButtonStatus();




        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                string data = serialPort1.ReadExisting();
                string[] strArr = data.Split('$');
                for (int i = 0; i < strArr.Length; i++)
                {
                    string strTemp = strArr[i];
                    string[] lineArr = strTemp.Split(',');
                    if (lineArr[0] == "GPGGA")
                    {
                        try
                        {

                            latitude = Utils.getPosition(lineArr[2], lineArr[3], true);
                            longitude = Utils.getPosition(lineArr[4], lineArr[5], true);

                            
                            
                            
                            /*
                            
                            //Latitude
                            Double dLat = Convert.ToDouble(lineArr[2]);
                            dLat = dLat / 100;
                            string[] lat = dLat.ToString().Split('.');
                            latitude = lineArr[3].ToString() +
                            lat[0].ToString() + "." +
                            ((Convert.ToDouble(lat[1]) /
                            60)).ToString("#####");

                            //Longitude
                            Double dLon = Convert.ToDouble(lineArr[4]);
                            dLon = dLon / 100;
                            string[] lon = dLon.ToString().Split('.');
                            longitude = lineArr[5].ToString() +
                            lon[0].ToString() + "." +
                            ((Convert.ToDouble(lon[1]) /
                            60)).ToString("#####");
                             * 
                             * 
                             * */

                            //Display
                            label3.Text = latitude;
                            label1.Text = longitude;


                        }
                        catch
                        {
                            //Cannot Read GPS values


                            label3.Text = "GPS unavailable";
                            label1.Text = "GPS unavailable";



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

                label1.Text = "GPS disconnected";
                label3.Text = "GPS disconnected";


                

            }







        }


        public bool isGPSconnected()
        {

            //true if we have an open serial port
            if (serialPort1.IsOpen)
            {
                
                //device connected to serial port e.g. GPS dongle
                return true;


            }
            else
            {
                
                //no device connected, i.e. stand-alone GPS unit e.g. Garmin with its own display
                return false;


            }



        }



        private void GPSconnectButton_Click(object sender, EventArgs e)
        {

            //user is trying to connect to GPS
            try
            {
                
                //make sure that it is not already connected
                if (serialPort1.IsOpen)
                {
                    //already open
                    stdMessageBox.setMainLabel("GPS connection is already open");
                    stdMessageBox.ShowDialog();




                }
                else
                {
                    //not open
                    serialPort1.Open();

                    timer1.Enabled = true;

                    stdMessageBox.setMainLabel("GPS connection was opened OK");
                    stdMessageBox.ShowDialog();




                }
                
                

            }
            catch
            {

                //show warning screen
                timer1.Enabled = false;

                warningMessageBox.setLabel("Warning: Can't open serial port connection to the GPS unit. Make sure the unit is plugged in.");
                warningMessageBox.ShowDialog();

                label1.Text = "GPS disconnected";
                label3.Text = "GPS disconnected";

            }






        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //fires when the form is closing, but not yet closed

            //stop the timer
            timer1.Enabled = false;

            //close the serial port
            serialPort1.Close();

            //tell the baseForm that we are closing so it can let other forms open
            baseForm.questionFormClosing();





        }


        public string getSkipSetting()
        {

            //return the currently selected skip radio button or null if none have been selected

            string selectedText = null;
            
            foreach (RadioButton rb in groupBox1.Controls)
            {

                if (rb.Checked)
                {
                    selectedText= rb.Text;

                }



            }

            return selectedText;



        }


        public void clearControlButtons()
        {
            //set all control buttons (no answer, don't know, etc) to unselected

            foreach (RadioButton rb in groupBox1.Controls)
            {

                rb.Checked = false;

            }


        }




        public void setSkipSetting(string buttonText)
        {
            //set the correct radiobutton
            foreach (RadioButton rb in groupBox1.Controls)
            {

                if (rb.Text == buttonText)
                {
                    rb.Checked = true;

                }
                else
                {
                    rb.Checked = false;

                }
                
                
              


            }

            //clear controls if this is not the OK button
            if (buttonText != "OK")
            {

                clearMainControls();

            }



        }

        private void clearMainControls()
        {

            //remove text and disable main controls
            bool hasAsked = false;  //true if we have already warned the user about deleting text
            
            foreach (Control control in panel1.Controls)
            {


                if (control is TextBox)
                {

                    //did the user type something in the box ?
                    if ((control.Text.Length > 0) && (! hasAsked))
                    {

                        hasAsked = true;
                        
                        //warn the user in case this is not what was intended.

                        string confLabel = "You will delete what you typed, are you sure?";
                        confirmBox.setFormLabel(confLabel, this);
                        confirmBox.ShowDialog();

                        //the confirmbox calls back to the mainForm which button was pressed
                        string buttonResult = confirmResult;

                        if (buttonResult == "yes")
                        {
                            //go ahead and delete
                            control.Text = "";

                        }
                        else
                        {
                            //user has changed mind: cancel
                            clearControlButtons();
                            return;


                        }



                    }
                    else if (control.Text.Length > 0)
                    {
                        control.Text = "";

                    }
                    
                   
                }
                else if (control is GroupBox)
                {

                    foreach (RadioButton rb in control.Controls)
                    {
                        rb.Checked = false;
                        //rb.Enabled = false;



                    }



                }
                else if (control is ComboBox)
                {

                    object selectedItem= ((ComboBox)control).SelectedItem;
                    
                    if ((selectedItem != null) && (!hasAsked))
                    {

                        hasAsked = true;

                        //warn the user in case this is not what was intended.

                        string confLabel = "You will delete what you selected, are you sure?";
                        confirmBox.setFormLabel(confLabel, this);
                        confirmBox.ShowDialog();

                        //the confirmbox calls back to the mainForm which button was pressed
                        string buttonResult = confirmResult;

                        if (buttonResult == "yes")
                        {
                            //go ahead and delete
                            ((ComboBox)control).SelectedItem = null;
                            

                        }
                        else
                        {
                            //user has changed mind: cancel
                            clearControlButtons();
                            return;


                        }



                    }
                    else 
                    {
                        ((ComboBox)control).SelectedItem = null;

                    }




                }
                else if (control is Button)
                {
                    //control.Enabled = false;


                }



            }


        }





        private void radioButton2_Click(object sender, EventArgs e)
        {
            //user has clicked the "don't know" radio button on console.
            //make sure anything that has been selected or typed in is cleared, then disable the control

            clearMainControls();





        }

        private void radioButton3_Click(object sender, EventArgs e)
        {
            //user has clicked "No answer" radio button
            clearMainControls();



        }

        private void radioButton4_Click(object sender, EventArgs e)
        {
            //user has clicked "not applicable" radiobutton
            clearMainControls();



        }

        private void radioButton1_Click(object sender, EventArgs e)
        {
            //user has clicked "OK" radiobutton on console

            //enable main controls.

            foreach (Control control in panel1.Controls)
            {

                control.Enabled= true;
               
                if (control is GroupBox)
                {

                    foreach (RadioButton rb in control.Controls)
                    {

                        rb.Enabled = true;



                    }



                }



            }



        }


        public void setSkipControlsInvisible()
        {
            //turn off for labels which have no use of them
            groupBox1.Visible = false;




        }

        public void setSkipControlsVisible()
        {
            groupBox1.Visible = true;



        }

        private void button3_Click(object sender, EventArgs e)
        {

            //jump to the GPS question

            //first check if that question exists in the current version of the form
            

            try
            {

                Question gpsQuestion = qm.getQuestion("GPS");


            }
            catch
            {

                warningMessageBox.setLabel("Warning: GPS is not enabled for this set of questions");
                warningMessageBox.ShowDialog();

                return;

            }

            



            //enable the previous-page button
            button1.Enabled = true;

            
            //delete the current widgets on the form
            qm.removeControls();

            //display the next question
            qm.setNextQuestion("GPS");

            qm.configureControls(UserDirection.forward);

            //make sure the forward and reverse buttons have the correct enabled/disabled
            setMainButtonStatus();




        }

        private void button4_Click(object sender, EventArgs e)
        {

            //debug: 
            qm.showDebug();






        }

        private void button4_Click_1(object sender, EventArgs e)
        {

            //show the form-status window, i.e. the completeness of each section
            //disable locking

            baseForm.checkCompleteness(false, newUser);






        }

        
    }
}
