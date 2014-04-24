
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
    class QuestionTextRadio : Question
    {
        //text box

        //fields
        protected TextBox textbox;
        private Label label;

        //reference to a subroutine that will do the validation
        private string validationCodeLabel;

        //reference to a subroutine that will do any needed processing
        private string processCodeLabel;

        

        //the data the user entered, which may be different to the processed data
        private string userData;

        //the value of the selected option
        private string selectedOptionValue;

        //a Radio Button set

        //fields
        List<Option> optionList;

        private GroupBox radioGroup;


        //properties
        //public string OnError { get; set; }

        public string RadioLabel { get; set; }

        public bool CheckPreviousDontKnow { get; set; }



        //constructor
        public QuestionTextRadio(Form form, Form bigMessageBox, GlobalStore gs, GlobalStore specialDataStore, QuestionManager qm)
            : base(form, bigMessageBox, gs, specialDataStore, qm)
        {
            

            //init the optionslist
            optionList = new List<Option>();


        }



        //methods

        public override void load(Dictionary<string, string> pDataDict, Dictionary<string, string> uDataDict)
        {
            //load the processed data via the base class
            load(pDataDict);

            Char[] delim = new Char[] { '\t' };

            //can I find this code in the dictionary
            if (uDataDict.ContainsKey(Code))
            {
                string line = uDataDict[Code];

                string[] parts = line.Split(delim);

                userData = parts[0];
                selectedOptionValue = parts[1];


            }
            else
            {
                userData = null;

            }



        }

        public override void save(TextWriter dhProcessedData, TextWriter dhUserData)
        {

            //call the base method save to save the processed data
            save(dhProcessedData);

            //save the user data

            if (userData != null)
            {

                //save the data stored in this object
                dhUserData.WriteLine(Code + "\t" + userData + "\t" + selectedOptionValue);

            }



        }





        public void addOption(Option op)
        {
            optionList.Add(op);



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

            label.Dispose();
            textbox.Dispose();

            getQM().getPanel().Controls.Remove(radioGroup);


            radioGroup.Dispose();



        }

        public override void configureControls(UserDirection direction)
        {

            //direction is either 'forward' or 'reverse'
            //turn the skip controls on again
            getQM().getMainForm().setSkipControlsVisible();


            //create a label and textbox control
            label = new Label();
            textbox = new TextBox();

            //trap any keypress to deselect the skip-controls
            textbox.KeyPress += new KeyPressEventHandler(button_click);



            //the question Text shown to the user
            label.Text = Val;

            int labelXpos = getWidgetXpos();
            int labelYpos = getWidgetYpos();

            //position the text box under the label, i.e. at the same xpos but an increased ypos
            int textBoxXpos = labelXpos;
            int textBoxYpos = labelYpos + 80;



            //position of the Label
            label.Location = new Point(labelXpos, labelYpos);
            label.Size = new Size(getWidgetWidth(), 80);
            label.ForeColor = GlobalColours.mainTextColour;


            //position of the textbox
            textbox.Location = new Point(textBoxXpos, textBoxYpos);
            textbox.Size = new Size(500, 50);
            textbox.BackColor = GlobalColours.controlBackColour;

            //if userdirection is reverse, populate the control with the previously entered text
            if (PageSeen)
            {
                textbox.Text = userData;


            }

            //add to the form
            getQM().getPanel().Controls.Add(label);
            getQM().getPanel().Controls.Add(textbox);

            textbox.Focus();

            
            //radio buttons
            
            radioGroup = new GroupBox();

            //set font size
            setFontSize(label, textbox, radioGroup);



            //position the group box under the label, i.e. at the same xpos but an increased ypos
            int groupBoxXpos = getWidgetXpos();
            int groupBoxYpos = getWidgetYpos() + 130;



            //position of the groupbox
            radioGroup.Location = new Point(groupBoxXpos, groupBoxYpos);
            radioGroup.Size = new Size(getWidgetWidth(), getWidgetHeight());
            radioGroup.Text = RadioLabel;
            radioGroup.ForeColor = GlobalColours.mainTextColour;



            //create the RadioButton objects that we need, i.e. one for each option.

            RadioButton rb;

            //create a radiobutton for each option
            foreach (Option op in optionList)
            {
                //create a radiobutton
                rb = new RadioButton();

                //trap the click
                rb.Click += new EventHandler(button_click);


                rb.Text = op.getText();
                //rb.Tag = op.getValue();
                rb.Tag = op;
                rb.Location = new Point(op.getWidgetXpos(), op.getWidgetYpos());
                rb.Size = new Size(op.getWidgetWidth(), op.getWidgetHeight());
                rb.ForeColor = GlobalColours.mainTextColour;

                if (!PageSeen)
                {
                    if (op.Default)
                    {
                        rb.Checked = true;
                    }

                }
                else
                {
                    //page has been seen, set the previous data
                    if (op.getValue() == selectedOptionValue)
                    {
                        rb.Checked = true;

                    }

                }




                radioGroup.Controls.Add(rb);





            }

            //add controls to the form


            getQM().getPanel().Controls.Add(radioGroup);

            setSkipSetting();



        }



        public override string processUserData()
        {

            //code for the next question
            string nextCode;


            //get the raw data from the user
            userData = textbox.Text;

            //we have seen this page
            PageSeen = true;

            //did the user skip this question ?
            string skipSetting = getSkipSetting();
            if (skipSetting != null)
            {
                //yes

               
                    processedData = skipSetting;



                    if (processCodeLabel == "CalcYearlyIncomeAndComparePrevious")
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

            string errorMessage="foo";

            bool dataOK;

            string optionData;

            //process the selected option

            //find the selected button
            RadioButton selectedButton = null;
            foreach (RadioButton rb in radioGroup.Controls)
            {

                if (rb.Checked)
                {
                    selectedButton = rb;
                    break;


                }



            }

            //check if the user has not made a selection
            if (selectedButton == null)
            {
                ((Form2)getBigMessageBox()).setLabel("You must select an option");
                getBigMessageBox().ShowDialog();

                return Code;


            }

            //get the tag from the selected button
            Option selectedOption = (Option)(selectedButton.Tag);
            optionData = selectedOption.getValue();
            selectedOptionValue = optionData;

            //process the text data



            //validate the data
            //run the test accoring to the validationCodeLabel


            //if the user selected don't know (if the option exists for this quesiton), then skip any validation

            /*

            if (selectedOptionValue == "Don't know")
            {
                
                //one exception: if this is the second in a question-pair and we want to check that the previous answer was also don't know
                //if this is DONT KNOW:
                if (CheckPreviousDontKnow)
                {

                    string previousData = getPreviousData();
                    
                    if (previousData == "Don't know")
                    {

                        dataOK = true;


                    }
                    else
                    {
                        dataOK = false;
                        errorMessage = "You did not select Don't know previously";


                    }

                }

               
                else
                {
                    dataOK = true;


                }
                
                

            }

            else if (CheckPreviousDontKnow && (getPreviousData() == "Don't know"))
            {
                //this is NOT don't know but it should have been

                

                dataOK = false;
                errorMessage = "You selected don't know previously, but not this time";

                
                


            }


            else
             * 
             * */
            
                //do validation
                if (validationCodeLabel == "TestNullEntry")
                {
                    dataOK = testNullEntry(userData);
                    errorMessage = "Please Try Again";

                }

                else if (validationCodeLabel == "TestNumeric")
                {
                    //must be a real number

                    try
                    {
                        Convert.ToDecimal(userData);
                        dataOK = true;



                    }
                    catch
                    {
                        dataOK = false;
                        errorMessage = "Please enter a Number";

                    }




                }

                else if (validationCodeLabel == "TestDOB")
                {

                    dataOK = testDOB(userData);
                    errorMessage = "The date you entered is not consistent with the age: please try again.";
                }

                else if (validationCodeLabel == "CheckAgeSameAsOrLessThanAGE")
                {

                    dataOK = testCheckAgeSameAsOrLessThanAGE(userData, selectedOptionValue);
                    errorMessage = "The age you entered is not consistent with the previous age (Question 8): please try again.";


                }

                else if (validationCodeLabel == "CheckSameAsPrevious")
                {


                    dataOK = testCheckSameAsPrevious(userData);
                    errorMessage = "The value you entered is not the same as the previous question: please try again";


                }
                else if (validationCodeLabel == "TestBloodSerum" || validationCodeLabel == "TestBloodEDTA" || validationCodeLabel == "TestBloodNAF")
                {
                    //this is the barcode from the serum tube: we need to check that it matches the same group as the master lab barcode
                    string typeSuffix;


                    if (validationCodeLabel == "TestBloodSerum")
                    {
                        typeSuffix = "S1";

                    }
                    else if (validationCodeLabel == "TestBloodEDTA")
                    {

                        typeSuffix = "E1";

                    }
                    else
                    {
                        //TestBloodNAF
                        typeSuffix = "G1";


                    }

                    string masterBarCode = getGS().Get("BLOODMASTER");

                    if (masterBarCode == null)
                    {

                        Form3 warningBox = getQM().getWarningBox();

                        warningBox.setLabel("Warning: Can't compare with the master barcode, which was not entered");
                        warningBox.ShowDialog();

                        dataOK = true;


                    }
                    else
                    {

                        //e.g. master BGZ100
                        //the serum tube should be <master>S1

                        if (string.IsNullOrWhiteSpace(userData))
                        {
                            //no entry
                            dataOK = false;
                            errorMessage = "Please scan a barcode";


                        }
                        else
                        {
                            //compare with master
                            string expectedBarcode = masterBarCode + typeSuffix;

                            if (expectedBarcode == userData)
                            {
                                dataOK = true;

                            }
                            else
                            {
                                dataOK = false;
                                errorMessage = "You have entered the wrong barcode";


                            }



                        }

                    }



                }


                else
                {
                    //unknown code label: error
                    //for now ignore this
                    dataOK = true;
                    errorMessage = "Internal Error: invalid validationCodeLabel";


                }


            



            //if data is OK, process the data if needed
            if (dataOK)
            {

                if (selectedOptionValue == "Don't know")
                {
                    processedData = "Don't know";


                }
                else
                {

                    //process the data
                    if (processCodeLabel == "NoModify")
                    {
                        //make no changes
                        processedData = userData;

                    }

                    else if (processCodeLabel == "CalcYearlyIncome")
                    {


                        processedData = calcYearlyIncome(userData, optionData);

                    }

                    else if (processCodeLabel == "CalcYearlyIncomeAndComparePrevious")
                    {
                        //this is the second try at entering the yearly income and it should give the same result as the first try

                        string previousYearlyIncome = getGS().Get("AverageYearlyEarnings1");
                        string thisYearlyIncome = calcYearlyIncome(userData, optionData);

                        if (previousYearlyIncome == null)
                        {
                            //user has skipped first attempt
                            //issue warning:

                            Form3 warningBox = getQM().getWarningBox();

                            warningBox.setLabel("Warning: Can't compare Yearly Income with first entry (not entered)");
                            warningBox.ShowDialog();

                            processedData = thisYearlyIncome;



                        }
                        else
                        {



                            //are these the same ?
                            if (previousYearlyIncome != thisYearlyIncome)
                            {
                                //failed.
                                ((Form2)getBigMessageBox()).setLabel("Error: Yearly income not the same for both attempts");
                                getBigMessageBox().ShowDialog();
                                nextCode = OnErrorQuestionCompare;
                                return nextCode;


                            }
                            else
                            {

                                processedData = thisYearlyIncome;

                            }

                        }



                    }

                    else if (processCodeLabel == "SetNullEntry")
                    {
                        //if userdata is null save as "No Entry"
                        if (string.IsNullOrEmpty(userData))
                        {
                            processedData = "No User Entry";

                        }
                        else
                        {
                            processedData = userData;

                        }


                    }




                    else
                    {
                        //add calls to specific processing methods here
                        processedData = userData;

                    }


                    //if we have a global setting: save to the global object
                    string globalKey = SetKey;
                    if (globalKey != null)
                    {
                        getGS().Add(globalKey, processedData);

                    }


                    

                }

                //advance to the next question
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


                nextCode = Code;


            }



            return nextCode;



        }



        private string calcYearlyIncome(string userData, string timeOption)
        {

            //option will be W (week), M (month) or Y (year)

            decimal income = Convert.ToDecimal(userData);
            decimal incomePerYear;

            if (timeOption == "W")
            {
                //user entered earnings per week -> convert to per year
                incomePerYear= income * 52.177457M;



            }

            else if(timeOption == "M"){

                //user entered earnings per month -> convert to per year
                incomePerYear= income * 12.0M;


            }
            
            else {

                //user entered earnings per year -> return unchanged
                incomePerYear= income;


            }

            //convert to a string (round to 2 decimal places)
            return Math.Round(incomePerYear, 2).ToString();






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

                //MessageBox.Show("days:" + days + " months:" + months + " years:" + years);

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





    

        private bool testCheckAgeSameAsOrLessThanAGE(string userData, string selectedOptionValue)
        {
            //has the user entered an age or did they choose don't know

            if (selectedOptionValue == "age")
            {
                //is this age the same as or less than the  value entered for AGE ?

                

                int smokingAge = Convert.ToInt32(userData);
                int currentAge= Convert.ToInt32(getGS().Get("AGE"));




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
            else
            {
                //don't know
                //we can't do anything here
                return true;

            }





        }





    }
}
