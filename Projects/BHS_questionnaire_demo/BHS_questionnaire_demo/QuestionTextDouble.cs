
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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;
using System.IO;




namespace BHS_questionnaire_demo
{
    class QuestionTextDouble : Question
    {
        //text box

        //fields
        private TextBox textbox;
        private TextBox textbox2;
        private Label label;
        private Label hmLabel;      //Hrs Mins label
        

        //reference to a subroutine that will do the validation
        private string validationCodeLabel;

        //reference to a subroutine that will do any needed processing
        private string processCodeLabel;

        

        //the data the user entered, which may be different to the processed data
        private string userData;

        private string userDataFormatted;


        //properties
        

        //constructor
        public QuestionTextDouble(Form form, Form bigMessageBox, GlobalStore gs, GlobalStore specialDataStore, QuestionManager qm)
            : base(form, bigMessageBox, gs, specialDataStore, qm)
        {
            


        }



        //methods

        public override void load(Dictionary<string, string> pDataDict, Dictionary<string, string> uDataDict)
        {
            //load the processed data via the base class
            load(pDataDict);

            


        }

        public override void save(TextWriter dhProcessedData, TextWriter dhUserData)
        {

            //call the base method save to save the processed data
            save(dhProcessedData);

            



        }
        public void setValidation(string validation)
        {
            validationCodeLabel = validation;

        }

        public void setProcess(string process)
        {
            processCodeLabel = process;

        }

        

        public override void removeControls()
        {
            getQM().getPanel().Controls.Remove(label);
            getQM().getPanel().Controls.Remove(textbox);
            getQM().getPanel().Controls.Remove(textbox2);
            getQM().getPanel().Controls.Remove(hmLabel);
            

            textbox2.Dispose();
            label.Dispose();
            textbox.Dispose();
            hmLabel.Dispose();
            



        }

        public override void configureControls(UserDirection direction)
        {

            //direction is either 'forward' or 'reverse'

            //turn the skip controls on 
            getQM().getMainForm().setSkipControlsVisible();


            //create a label and textbox control
            label = new Label();
            label.ForeColor = GlobalColours.mainTextColour;

            textbox = new TextBox();
            textbox.BackColor = GlobalColours.controlBackColour;

            textbox2 = new TextBox();
            textbox2.BackColor = GlobalColours.controlBackColour;

            //trap any keypress to deselect the skip-controls
            textbox.KeyPress += new KeyPressEventHandler(button_click);
            textbox2.KeyPress += new KeyPressEventHandler(button_click);

            hmLabel = new Label();
            hmLabel.ForeColor = GlobalColours.mainTextColour;

            //set font size
            setFontSize(label, textbox, textbox2, hmLabel);


            //the question Text shown to the user
            label.Text = Val;

            int labelXpos = getWidgetXpos();
            int labelYpos = getWidgetYpos();

           
            //position of the Label
            label.Location = new Point(labelXpos, labelYpos);
            label.Size = new Size(getWidgetWidth(), getWidgetHeight());

            //position the hmlabel
            hmLabel.Location = new Point(labelXpos + 100, labelYpos + 80);
            hmLabel.Size = new Size(500, 50);
            hmLabel.Text = "Hrs                   Mins";


            //position of the first textbox
            textbox.Location = new Point(labelXpos + 70, labelYpos+ 110);
            textbox.Size = new Size(100, 50);


            //position the second textbox
            textbox2.Location = new Point(labelXpos + 230, labelYpos + 110);
            textbox2.Size = new Size(100, 50);

            



            //if page seen before, populate the control with the previously entered text
            if (PageSeen)
            {

                if (processedData == null)
                {

                    //leave blank
                    textbox.Text = "";
                    textbox2.Text = "";

                }
                else
                {
                    Match match = Regex.Match(processedData, @"(\d+):(\d+)");

                    if (match.Success)
                    {

                        textbox.Text = match.Groups[1].Value;
                        textbox2.Text = match.Groups[2].Value;

                    }
                    //else
                    //{


                    //    throw new Exception();


                    //}


                }
                
                
                

            }

            //add to the form
            getQM().getPanel().Controls.Add(label);
            getQM().getPanel().Controls.Add(textbox);
            getQM().getPanel().Controls.Add(textbox2);
            getQM().getPanel().Controls.Add(hmLabel);

            textbox.Focus();

            setSkipSetting();

           

        }



        public override string processUserData()
        {

            //code for the next question
            string nextCode;


            //get the raw data from the user
            string userData1= textbox.Text;
            string userData2= textbox2.Text;

            

            //we have seen this page
            PageSeen = true;

            //did the user skip this question ?
            string skipSetting = getSkipSetting();
            if (skipSetting != null)
            {
                //yes

               
                    processedData = skipSetting;


                    if (validationCodeLabel == "CheckSameAsPrevious")
                    {
                        
                        //get the previous value.
                        bool thisDataOK = testCheckSameAsPrevious(skipSetting);

                        if (thisDataOK)
                        {

                            return ToCode;



                        }
                        else
                        {
                            

                            ((Form2)getBigMessageBox()).setLabel("The value you entered is not the same as the previous question: please try again");
                            getBigMessageBox().ShowDialog();

                            return OnErrorQuestionCompare;

                           
                        }
                        


                    }
                    else
                    {

                        return ToCode;

                    }

                




            }




            string errorMessage = null;

            bool dataOK = true;

            //validate the data

            //1 or both fields must contain something
            if(validationCodeLabel== "HrsMinsTest"){

                bool hasData1;
                bool hasData2;

                if(string.IsNullOrWhiteSpace(userData1)){
                    hasData1= false;

                }
                else{

                    hasData1= true;
                }

                if(string.IsNullOrWhiteSpace(userData2)){
                    hasData2= false;

                }
                else{

                    hasData2= true;
                }


                //convert to Hrs:Mins format
                string checkedData1;
                string checkedData2;

                if (hasData1)
                {
                    checkedData1 = userData1;

                }
                else
                {
                    checkedData1 = "0";


                }

                if (hasData2)
                {
                    checkedData2 = userData2;

                }
                else
                {
                    checkedData2 = "0";


                }


                userDataFormatted = checkedData1 + ":" + checkedData2;
                


                if ((!hasData1) && (!hasData2))
                {
                    dataOK = false;
                    errorMessage = "You must enter either Hours or Minutes or both";

                }
                else
                {
                    //check that the entered field(s) contain numbers

                    //check the total hours + mins is not > 24 hours
                    int hours = 0;
                    int mins = 0;
                    int totalMins = 0;

                    if (hasData1)
                    {

                        try
                        {

                            hours= Convert.ToInt32(userData1);


                        }
                        catch(FormatException e){

                            dataOK = false;
                            errorMessage = "The data must be numbers";

                        }


                    }

                    if (hasData2)
                    {

                        try
                        {

                            mins= Convert.ToInt32(userData2);


                        }
                        catch (FormatException e)
                        {

                            dataOK = false;
                            errorMessage = "The data must be numbers";

                        }


                    }

                    totalMins = hours * 60 + mins;
                    if (totalMins > 1440)
                    {
                        dataOK = false;
                        errorMessage = "You must have less than 24 hours in total";



                    }

                    if (mins > 59)
                    {
                        dataOK = false;
                        errorMessage = "You must have less than 60 in the minutes field";


                    }


                }

            }


            else if (validationCodeLabel == "CheckSameAsPrevious")
            {
                userDataFormatted = formatTime(userData1, userData2);


                dataOK = testCheckSameAsPrevious(userDataFormatted);
                errorMessage = "The value you entered is not the same as the previous question: please try again";


            }

            else
            {

                //should not get to here:
                throw new Exception();


            }

            //if data is OK, process the data if needed
            if (dataOK)
            {

                processedData = userDataFormatted;
                nextCode = ToCode;
                
                

            }
            else
            {
                //validation has failed.
                //increment the number of tries
                
                
                
                //no
                //the next code is the same as this one
                //MessageBox.Show("Error: please try again");
                ((Form2)getBigMessageBox()).setLabel(errorMessage);
                getBigMessageBox().ShowDialog();

                if (validationCodeLabel == "CheckSameAsPrevious")
                {
                    nextCode = OnErrorQuestionCompare;
                }
                else
                {
                    nextCode = Code;

                }



            }

            return nextCode;



        }

        private string formatTime(string userData1, string userData2)
        {

            bool hasData1;
            bool hasData2;

            if (string.IsNullOrWhiteSpace(userData1))
            {
                hasData1 = false;

            }
            else
            {

                hasData1 = true;
            }

            if (string.IsNullOrWhiteSpace(userData2))
            {
                hasData2 = false;

            }
            else
            {

                hasData2 = true;
            }


            //convert to Hrs:Mins format
            string checkedData1;
            string checkedData2;

            if (hasData1)
            {
                checkedData1 = userData1;

            }
            else
            {
                checkedData1 = "0";


            }

            if (hasData2)
            {
                checkedData2 = userData2;

            }
            else
            {
                checkedData2 = "0";


            }


            return checkedData1 + ":" + checkedData2;




        }

        private bool testDOB(string userData)
        {
            //is this age consistent with the previously entered Date of Birth
            string dobAsStr = getGS().Get("DOB");

            DateTime today = DateTime.Now;
            DateTime dob;

            //extract the day, months, years.
            Match match = Regex.Match(dobAsStr, @"(\d+)/(\d+)/(\d+)");

            int days, months, years;

            if (match.Success)
            {
                days = Convert.ToInt32(match.Groups[1].Value);
                months = Convert.ToInt32(match.Groups[2].Value);
                years = Convert.ToInt32(match.Groups[3].Value);

                //if days==0 or months==0 => values are unknown
                //use a default day/month as 1 for this usage

                if (days == 0)
                {
                    days = 1;
                }

                if (months == 0)
                {
                    months = 1;

                }

                dob = new DateTime(years, months, days);



            }
            else
            {
                throw new Exception("date parsing failed");

            }

            //calc the elapsed time
            TimeSpan elapsedTime = today - dob;
            double elapsedYears = elapsedTime.TotalDays / 365.25;

            //MessageBox.Show("years:" + elapsedYears);

            //is this approx the same as the userData ?
            //lets leave a window of 1 year

            //the number of years entered by the user
            double userYears = Convert.ToDouble(userData);


            double yearsLowerBound = userYears - 1;
            double yearsUpperBound = userYears + 2;

            if ((elapsedYears >= yearsLowerBound) && (elapsedYears <= yearsUpperBound))
            {

                //OK
                return true;

            }
            else
            {
                //failed: outside allowed range
                return false;


            }




        }





        

        private bool testCheckAgeSameAsOrLessThanAGE(string userData)
        {


            //is this age the same as or less than the  value entered for AGE ?



            int smokingAge = Convert.ToInt32(userData);
            int currentAge = Convert.ToInt32(getGS().Get("AGE"));




            if (smokingAge <= currentAge)
            {

                //both are the same
                return true;

            }
            else
            {
                //these differ
                return false;


            }



        }





    }
}

