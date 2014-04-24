
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
    class QuestionText : Question
    {
        //text box

        //fields
        private TextBox textbox;
        private Label label;

        //reference to a subroutine that will do the validation
        private string validationCodeLabel;

        //reference to a subroutine that will do any needed processing
        private string processCodeLabel;


        //the data the user entered, which may be different to the processed data
        private string userData= null;


        //properties
        public int LabelToBoxGap { get; set; }

        public bool HasTextArea { get; set; }


        //constructor
        public QuestionText(Form form, Form bigMessageBox, GlobalStore gs, GlobalStore specialDataStore, QuestionManager qm)
            : base(form, bigMessageBox, gs, specialDataStore, qm)
        {



        }



        //methods
       



        public override void save(TextWriter dhProcessedData, TextWriter dhUserData)
        {

            //call the base method save to save the processed data
            save(dhProcessedData);

            //save the user data

            if (userData != null)
            {

                //save the data stored in this object
                dhUserData.WriteLine(Code + "\t" + userData);

            }
            
            
        }

        public override void load(Dictionary<string, string> pDataDict, Dictionary<string, string> uDataDict)
        {
            //load the processed data via the base class
            load(pDataDict);

            //can I find this code in the dictionary
            if (uDataDict.ContainsKey(Code))
            {
                userData = uDataDict[Code];

            }
            else
            {
                userData = null;

            }



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
            //getForm().Controls.Remove(label);
            //getForm().Controls.Remove(textbox);

            getQM().getPanel().Controls.Remove(label);
            getQM().getPanel().Controls.Remove(textbox);

            label.Dispose();
            textbox.Dispose();



        }

        public override void configureControls(UserDirection direction)
        {

            //direction is either 'forward' or 'reverse'
            //turn the skip controls on again
            getQM().getMainForm().setSkipControlsVisible();


            //create a label and textbox control
            label = new Label();
            textbox = new TextBox();

            //set font size
            setFontSize(label, textbox);

 

            
            //trap any keypress to deselect the skip-controls
            textbox.KeyPress += new KeyPressEventHandler(button_click);



            //the question Text shown to the user
            label.Text = Val;

            int labelXpos = getWidgetXpos();
            int labelYpos = getWidgetYpos();

            //position the text box under the label, i.e. at the same xpos but an increased ypos
            int textBoxXpos = labelXpos;
            //int textBoxYpos = labelYpos + 50;

            //MessageBox.Show("gap is:" + LabelToBoxGap);

            int textBoxYpos = labelYpos + LabelToBoxGap + 50;



            //position of the Label
            label.Location = new Point(labelXpos, labelYpos);
            label.Size = new Size(getWidgetWidth(), getWidgetHeight());


            //position of the textbox
            textbox.Location = new Point(textBoxXpos, textBoxYpos);
            


            //is this a textarea ?
            if (HasTextArea)
            {
                textbox.Multiline = true;
                textbox.Size = new Size(700, 300);



            }
            else
            {
                //1 line textbox
                textbox.Size = new Size(500, 50);


            }





            

            if (PageSeen)
            {
                textbox.Text = userData;


            }

            //add to the form
            //getForm().Controls.Add(label);
            //getForm().Controls.Add(textbox);


            //colours for controls
            label.ForeColor = GlobalColours.mainTextColour;


            textbox.BackColor = GlobalColours.controlBackColour;



            //add controls to the panel
            getQM().getPanel().Controls.Add(label);
            getQM().getPanel().Controls.Add(textbox);

            textbox.Focus();


            //set the console radio buttons
            setSkipSetting();



        }



        public override string processUserData()
        {

            //code for the next question
            string nextCode;

            //these are used to avoid a standard error box when height or weight are shown as unusal in warning box.
            bool heightBad = false;
            bool weightBad = false;
            bool armCircBad = false;


            //get the raw data from the user
            userData = textbox.Text;

            if (HasTextArea)
            {
                //userData might contain line-breaks, which will mess up the data formatting
                userData = userData.Replace("\r\n", " ");
                




            }

            //we have seen this page
            PageSeen = true;

            //did the user skip this question ?
            //if (getQM().SkipThisQuestion)

            string skipSetting = getSkipSetting();
            if(skipSetting != null)
            {
                //yes

                    processedData = skipSetting;
                    //userData = skipSetting;
                    userData = "";

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

            bool dataOK = false;

            //validate the data
            //run the test accoring to the validationCodeLabel
            if (validationCodeLabel == "TestNullEntry")
            {
                dataOK = testNullEntry(userData);
                errorMessage = "Error: please try again";

            }


            else if (validationCodeLabel == "TestDiagnosisAge")
            {

                dataOK = testDiagnosisAge(userData);
                errorMessage = "Error: age is not consistent with years of birth and diagnosis";


            }




            else if (validationCodeLabel == "AllowNullEntry")
            {
                //anything is allowed even nothing
                dataOK = true;



            }
            else if (validationCodeLabel == "TestNumeric")
            {
                //must be a number

                dataOK = TestNumeric(userData);
                errorMessage = "Please enter a Number";



            }

            else if(validationCodeLabel == "TestNumberBetween0and100"){

                try
                {
                    decimal amount= Convert.ToDecimal(userData);

                    if (amount >= 0 && amount <= 100)
                    {
                        dataOK = true;

                    }
                    else
                    {
                        dataOK = false;
                        errorMessage = "Error: Please enter a number between 0 and 100";

                    }


                }
                catch
                {
                    dataOK = false;
                    errorMessage = "Error: Please enter a number between 0 and 100";

                }







            }
            else if (validationCodeLabel == "TestNumericNotZero")
            {
                //must be a number > 0

                dataOK = TestNumericNotZero(userData);
                errorMessage = "Please enter a Number larger than zero";


            }
            else if (validationCodeLabel == "TestNoNumbers")
            {

                //first, check we have something
                dataOK = testNullEntry(userData);
                if (dataOK)
                {


                    //string cannot contain any numbers
                    dataOK = TestNoNumbers(userData);
                    errorMessage = "Entry must not contain number(s)";



                }
                else
                {

                    errorMessage = "Error: please try again";
                }





            }

            else if (validationCodeLabel == "TestOnlyNumbers")
            {

                //first, check we have something
                dataOK = testNullEntry(userData);
                if (dataOK)
                {


                    //string must contain only numbers
                    dataOK = TestOnlyNumbers(userData);
                    errorMessage = "Entry must contain ONLY numbers";



                }
                else
                {

                    errorMessage = "Error: please try again";
                }





            }
            else if (validationCodeLabel == "TestArmCirc")
            {

                //arm circumference (no smaller than 10 cm and no larger than 45cm)

                try
                {
                    decimal circ = Convert.ToDecimal(userData);

                    if (circ < 10)
                    {
                        dataOK = false;
                        errorMessage = "arm circumference must be between 10 cm and 60 cm";



                    }
                    else
                    {
                        dataOK = true;

                        if (circ > 60)
                        {
                            //warning
                            Form1 mainForm = getQM().getMainForm();
                            ConfirmForm confirmBox = getQM().getConfirmBox();
                            string confLabel = "The arm circumference is unusal (" + userData + "). Is this correct ?";
                            confirmBox.setFormLabel(confLabel, mainForm);
                            confirmBox.ShowDialog();

                            //the confirmbox calls back to the mainForm which button was pressed
                            string buttonResult = mainForm.confirmResult;



                            if (buttonResult == "no")
                            {
                                armCircBad = true;

                            }




                        }

                    }



                }
                catch
                {
                    dataOK = false;
                    errorMessage = "Please enter a Number";

                }



            }


            else if (validationCodeLabel == "TestNumericInt")
            {
                //must be an integer number

                try
                {
                    Convert.ToInt32(userData);
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

                //make sure that a number was entered
                try
                {
                    Convert.ToDouble(userData);

                    //OK it is an integer
                    dataOK = testDOB(userData);
                    errorMessage = "The age you entered is not consistent with the date: please try again.";


                }
                catch
                {

                    //not a number

                    dataOK = false;
                    errorMessage = "Please enter a number";

                }



            }

            else if (validationCodeLabel == "TestSameAsRES")
            {
                //the answer must be the same as RES
                string previousData = getGS().Get("RES");

                if (userData != previousData)
                {
                    errorMessage = "This answer is not the same as the previous one: please try again";

                    ((Form2)getBigMessageBox()).setLabel(errorMessage);
                    getBigMessageBox().ShowDialog();
                    return "RES";



                }
                else
                {
                    dataOK = true;
                }



            }

            else if (validationCodeLabel == "TestSameAsRMS")
            {
                //the answer must be the same as RMS
                string previousData = getGS().Get("RMS");

                if (userData != previousData)
                {
                    errorMessage = "This answer is not the same as the previous one: please try again";

                    ((Form2)getBigMessageBox()).setLabel(errorMessage);
                    getBigMessageBox().ShowDialog();
                    return "RMS";



                }
                else
                {
                    dataOK = true;

                }



            }

            else if (validationCodeLabel == "TestLessThanTotalSibs")
            {
                //this value must be an integer and be <= total siblings
                try
                {
                    int numSibsThis= Convert.ToInt32(userData);

                    string numSibsStr = getGS().Get("NUMSIBS");

                    if (numSibsStr == null)
                    {
                        Form3 warningBox = getQM().getWarningBox();

                        warningBox.setLabel("Warning: Can't compare with total number of siblings, as the total was not entered previously");
                        warningBox.ShowDialog();

                        dataOK = true;


                    }
                    else
                    {
                        int numSibsTotal = Convert.ToInt32(numSibsStr);

                        if (numSibsThis <= numSibsTotal)
                        {
                            //OK
                            dataOK = true;


                        }
                        else
                        {
                            //error
                            dataOK = false;
                            errorMessage = "Error: this number must be <= the total number of siblings";

                        }



                    }

                    



                }
                catch
                {
                    dataOK = false;
                    errorMessage = "Please enter a whole Number";

                }



            }

            else if (validationCodeLabel == "TestBrotherPlusSistersLessThanTotalSibs")
            {
                //this value is the total sisters (when both brothers and sisters effected)
                //check that the total of this and the num brothers is <= total siblings

                try
                {
                    int numSisters = Convert.ToInt32(userData);

                    //total siblings
                    string numSibsStr = getGS().Get("NUMSIBS");

                    //total brothers
                    string numBrothersStr = getGS().Get("NUM_AFFECTED_BROTHERS");

                    if (numSibsStr == null || numBrothersStr == null)
                    {
                        Form3 warningBox = getQM().getWarningBox();

                        warningBox.setLabel("Warning: Can't compare with total number of siblings, as neccessary data was not entered previously");
                        warningBox.ShowDialog();

                        dataOK = true;


                    }
                    else
                    {
                        int numSibsTotal = Convert.ToInt32(numSibsStr);
                        int numBrothers = Convert.ToInt32(numBrothersStr);

                        if ((numSisters + numBrothers) <= numSibsTotal)
                        {
                            //OK
                            dataOK = true;


                        }
                        else
                        {
                            //error
                            dataOK = false;
                            errorMessage = "Error: affected sisters + affected bothers must be <= total siblings";

                        }



                    }





                }
                catch
                {
                    dataOK = false;
                    errorMessage = "Please enter a whole Number";

                }



            }

            else if (validationCodeLabel == "CheckSameAsPrevious")
            {


                dataOK = testCheckSameAsPrevious(userData);
                errorMessage = "The value you entered is not the same as the previous question: please try again";


            }

            else if ((validationCodeLabel == "CheckAgeSameAsOrLessThanAGE") || (validationCodeLabel == "CheckAgeSmokingStartLessThanAgeSmokingStop"))
            {


                //did the user enter anything ?
                if (string.IsNullOrWhiteSpace(userData))
                {

                    dataOK = false;
                    errorMessage = "Error: please try again";

                }
                else
                {

                    //did they enter an integer ?
                    try
                    {
                        Convert.ToInt32(userData);

                        //skip the test if the AGE was not entered previously
                        string currentAge = getGS().Get("AGE");

                        if (currentAge == null)
                        {

                            Form3 warningBox = getQM().getWarningBox();

                            warningBox.setLabel("Warning: Can't compare with AGE as AGE was not entered");
                            warningBox.ShowDialog();

                            dataOK = true;


                        }
                        else
                        {
                            dataOK = testCheckAgeSameAsOrLessThanAGE(userData);
                            errorMessage = "The age entered here is not consistent with the previously entered age";

                            if (dataOK && (validationCodeLabel == "CheckAgeSmokingStartLessThanAgeSmokingStop"))
                            {
                                //do an extra check to make sure that age started smoking < age stopped smoking

                                string ageStopped = getGS().Get("AGE_STOPPED_SMOKING");

                                if (ageStopped == null)
                                {

                                    Form3 warningBox = getQM().getWarningBox();

                                    warningBox.setLabel("Warning: Can't compare with AGE when smoking was stopped as that was not entered");
                                    warningBox.ShowDialog();

                                    dataOK = true;


                                }
                                else
                                {


                                    decimal ageSmokingStarted = Convert.ToDecimal(userData);
                                    decimal ageSmokingStopped = Convert.ToDecimal(ageStopped);

                                    if (ageSmokingStarted > ageSmokingStopped)
                                    {

                                        dataOK = false;
                                        errorMessage = "The stop-smoking age must be greater than the start-smoking age";

                                    }
                                    else
                                    {
                                        dataOK = true;

                                    }



                                }




                            }





                        }


                    }
                    catch
                    {

                        dataOK = false;
                        errorMessage = "Error: please enter a number";



                    }



                }



            }


            else if (validationCodeLabel == "TestHeight")
            {
                //check height in range

                //check number
                decimal height;

                try
                {
                    height = Convert.ToDecimal(userData);
                    dataOK = true;

                    if ((height <= 80) || (height >= 210))
                    {
                        Form1 mainForm = getQM().getMainForm();
                        ConfirmForm confirmBox = getQM().getConfirmBox();
                        string confLabel = "The Height is unusal (" + userData + "). Is this correct ?";
                        confirmBox.setFormLabel(confLabel, mainForm);
                        confirmBox.ShowDialog();

                        //the confirmbox calls back to the mainForm which button was pressed
                        string buttonResult = mainForm.confirmResult;


                        //if (MessageBox.Show("The height is unusal (" + userData + "). Is this correct ?", "Please Check:", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        if (buttonResult == "yes")
                        {
                            // a 'DialogResult.Yes' value was returned from the MessageBox

                            //alert data manager.

                            getSD().Add("DATA_MANAGER_REVIEW_HEIGHT", userData);



                        }
                        else
                        {
                            heightBad = true;

                        }

                    }

                }
                catch (FormatException e)
                {

                    //not a decimal
                    dataOK = false;
                    errorMessage = "This is not a number";

                }


            }

            else if (validationCodeLabel == "TestWeight")
            {
                //check height in range

                //check number
                decimal weight;

                try
                {
                    weight = Convert.ToDecimal(userData);
                    dataOK = true;

                    if ((weight <= 20) || (weight >= 180))
                    {

                        Form1 mainForm = getQM().getMainForm();
                        ConfirmForm confirmBox = getQM().getConfirmBox();
                        string confLabel = "The Weight is unusal (" + userData + "). Is this correct ?";
                        confirmBox.setFormLabel(confLabel, mainForm);
                        confirmBox.ShowDialog();

                        //the confirmbox calls back to the mainForm which button was pressed
                        string buttonResult = mainForm.confirmResult;



                        //if (MessageBox.Show("The weight is unusal (" + userData + "). Is this correct ?", "Please Check:", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        if (buttonResult == "yes")
                        {
                            // a 'DialogResult.Yes' value was returned from the MessageBox

                            //alert data manager.

                            getSD().Add("DATA_MANAGER_REVIEW_WEIGHT", userData);



                        }
                        else
                        {
                            weightBad = true;

                        }

                    }

                }
                catch (FormatException e)
                {

                    //not a decimal
                    dataOK = false;
                    errorMessage = "This is not a number";

                }


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

            else if (validationCodeLabel == "TestLessThan7")
            {
                dataOK = testLessThan(userData, 7);
                errorMessage = "You must enter a number that is 7 or less";

            }

            else if (validationCodeLabel == "TestLessThan20")
            {
                dataOK = testLessThan(userData, 20);
                errorMessage = "You must enter a number that is 20 or less";

            }

            else if (validationCodeLabel == "TestLessThan10")
            {
                dataOK = testLessThan(userData, 10);
                errorMessage = "You must enter a number that is 10 or less";

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

                if (heightBad == true || weightBad == true || armCircBad == true)
                {

                    nextCode = Code;

                }
                
                
                //process the data
                else if (processCodeLabel == "NoModify")
                {
                    //make no changes
                    processedData = userData;
                    //advance to the next question
                    nextCode = ToCode;

                }
                else if (processCodeLabel == "CalcBMI")
                {

                    calcBMI(userData);
                    
                    //make no changes
                    processedData = userData;
                    //advance to the next question
                    nextCode = ToCode;


                }
                else if (processCodeLabel == "SaveBMI")
                {
                    //user has entered their own BMI reading, so we need to override our 
                    //calculated one
                    //save the BMI in the special store
                    getSD().Add("BMI", userData);


                    //make no changes
                    processedData = userData;
                    //advance to the next question
                    nextCode = ToCode;


                }

                else if (processCodeLabel == "CalcCRWD")
                {
                    //calculate the crowding: CRWD
                    processedData = userData;


                    string resStr = getGS().Get("RES");
                    if (resStr == null)
                    {

                        //warning:
                        Form3 warningBox = getQM().getWarningBox();

                        warningBox.setLabel("Warning: Can't Calculate CRWD as Question 14 was not entered");
                        warningBox.ShowDialog();




                    }
                    else
                    {

                        decimal res = Convert.ToDecimal(resStr);
                        decimal rms = Convert.ToDecimal(userData);


                        decimal crwd = Math.Round(res / rms, 2);

                        getSD().Add("CRWD", crwd.ToString());


                    }


                    //advance to the next question
                    nextCode = ToCode;




                }
                else if (processCodeLabel == "CheckForTOBCOM")
                {
                    processedData = userData;

                    //if this field contains 0 or is blank then goto ToCode
                    //if not then goto TOBCOM

                    if (string.IsNullOrWhiteSpace(userData) || userData == "0")
                    {
                        nextCode = ToCode;

                    }
                    else
                    {

                        nextCode = "TOBCOM";
                    }



                }
                else if (processCodeLabel == "CorrectCuffSize")
                {
                    processedData = userData;

                    //direct to the correct label
                    decimal armCirc = Convert.ToDecimal(userData);

                    if (armCirc < 24)
                    {
                        nextCode = "AC-CUFF-1";


                    }
                    else if (armCirc >= 24 && armCirc <= 32)
                    {
                        nextCode = "AC-CUFF-2";

                    }
                    else
                    {
                        nextCode = "AC-CUFF-3";

                    }


                }

                else if (processCodeLabel == "If0GotoDIET3")
                {
                    processedData = userData;

                    if (userData == "0")
                    {
                        nextCode = "DIET3";

                    }
                    else
                    {
                        nextCode = ToCode;

                    }


                }

                else if (processCodeLabel == "If0GotoDIET5")
                {
                    processedData = userData;

                    if (userData == "0")
                    {
                        nextCode = "DIET5";

                    }
                    else
                    {
                        nextCode = ToCode;

                    }


                }

                else
                {
                    //add calls to specific processing methods here
                    processedData = userData;

                    //advance to the next question
                    nextCode = ToCode;

                }

                //save in global store if set.
                string globalKey = SetKey;
                if (globalKey != null)
                {
                    getGS().Add(globalKey, processedData);

                }



            }
            else
            {
                //validation has failed.



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


        private bool testDiagnosisAge(string userData)
        {

            //get DOB:
            string dobAsStr = getGS().Get("DOB");

            //get diagnosis year
            string diagYearAsStr = getGS().Get("DIAGYEAR");

            if (dobAsStr == null || diagYearAsStr == null)
            {
                //user skipped previous Q: can't do this test

                //show a warning

                Form3 warningBox = getQM().getWarningBox();

                warningBox.setLabel("Warning: Can't preform check as DOB or Diagnosis year was not entered");
                warningBox.ShowDialog();

                return true;


            }


            //get the year from the DOB
            //extract the day, months, years.
            Match match = Regex.Match(dobAsStr, @"(\d+)/(\d+)/(\d+)");

            int years;

            if (match.Success)
            {
                
                years = Convert.ToInt32(match.Groups[3].Value);

                //if years or months or days ==0, these are unknown so we can't do this test
                if (years == 0 )
                {

                    //show a warning

                    Form3 warningBox = getQM().getWarningBox();

                    warningBox.setLabel("Warning: Can't perform check as DOB years is not known");
                    warningBox.ShowDialog();


                    return true;
                }


            }
            else
            {
                throw new Exception("date parsing failed");

            }


            int diagYear = Convert.ToInt32(diagYearAsStr);

            if (diagYear == 0)
            {

                //show a warning

                Form3 warningBox = getQM().getWarningBox();

                warningBox.setLabel("Warning: Can't perform check as diagnosis years is not known");
                warningBox.ShowDialog();


                return true;

            }


            //calculate the elapsed years from birth to diagnosis
            int elapsedYears = diagYear - years;

            //if the diagnosis was before the person's birthday the elapsed years will be less by 1
            //we don't know the precise date of diagnosis, only the year

            if (elapsedYears == Convert.ToInt32(userData))
            {
                return true;

            }
            else if ((elapsedYears - 1) == Convert.ToInt32(userData))
            {
                return true;

            }
            else
            {
                return false;


            }





        }








        private bool testDOB(string userData)
        {
            //is this age consistent with the previously entered Date of Birth
            string dobAsStr = getGS().Get("DOB");

            if (dobAsStr == null)
            {
                //user skipped previous Q: can't do this test

                //show a warning

                Form3 warningBox = getQM().getWarningBox();

                warningBox.setLabel("Warning: Can't check that DOB matches age as DOB was not entered");
                warningBox.ShowDialog();

                return true;


            }

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

                
               
                //if years or months or days ==0, these are unknown so we can't do this test
                if (years == 0 || months ==0 || days == 0 )

                {

                    //show a warning

                    Form3 warningBox = getQM().getWarningBox();

                    warningBox.setLabel("Warning: Can't check that DOB matches age as some parts of age (day, month or year) are not known");
                    warningBox.ShowDialog();
                    
                    
                    return true;
                }



                dob = new DateTime(years, months, days);



            }
            else
            {
                throw new Exception("date parsing failed");

            }


            //subtract the year part of the current date from the year part of the dob

            int age = today.Year - years;

            //this will give the maximum possible age in years:
            //if today is before the birthday then we subtract 1

            //what date is the birthday this year ?

            DateTime dobThisYear = new DateTime(today.Year, months, days);

            if (today < dobThisYear)
            {
                
                //today is before birthday
                age -= 1;


            }

            //compare age based on todays date and the previously entered dob with the age the user entered

            if (age == Convert.ToInt32(userData))
            {
                return true;

            }
            else
            {
                return false;

            }





            /*



            //calc the elapsed time
            TimeSpan elapsedTime = today - dob;

            //double elapsedYears = elapsedTime.TotalDays / 365.25;

           


            //get number of whole years (ignore any parts therafter)
            int elapsedYears = (int)(elapsedTime.TotalDays / 365.25);

            if (elapsedYears == Convert.ToInt32(userData))
            {
                return true;

            }
            else
            {
                return false;

            }
             * 
             * */



        }


        private bool testLessThan(string userData, int testVal)
        {

            

            //is this a number ?

            int num;

            try
            {
                num = Convert.ToInt32(userData);


            }
            catch (FormatException e)
            {

                //was not a number
                return false;


            }

            if (num <= testVal)
            {
                //OK
                return true;

            }
            else
            {
                return false;



            }




        }

        private bool TestNoNumbers(string userData)
        {
            //does this contain any numbers ?
             Match match = Regex.Match(userData, @"\d");


             if (match.Success)
             {
                 //contains at least 1 number
                 return false;

             }
             else
             {
                 return true;


             }
            



        }

        private bool TestOnlyNumbers(string userData)
        {
            //does this contain ONLY numbers ?
            Match match = Regex.Match(userData, @"^\d+$");


            if (match.Success)
            {
                //contains only numbers
                return true;

            }
            else
            {
                return false;


            }




        }

        private void calcBMI(String userData)
        {

            //calculate the BMI

            string weight = userData;
            string height = getGS().Get("Height");

            //check that these are not null: which might happen if previous questions were skipped

            if ((weight != null) && (height != null) && (Convert.ToDecimal(height) != 0M))
            {
                decimal weightAsDec = Convert.ToDecimal(weight);
                decimal heightAsDec = Convert.ToDecimal(height) / 100;      //convert from cm to m

                decimal bmi = Math.Round(weightAsDec / (heightAsDec * heightAsDec), 2);

                //save the BMI in the special store
                getSD().Add("BMI", bmi.ToString());

                

            }
            else
            {
                //some data not present

                

                Form3 warningBox = getQM().getWarningBox();

                warningBox.setLabel("Warning: Can't calculate BMI due to missing data (height/weight)");
                warningBox.ShowDialog();



            }




        }

        

        private bool testCheckAgeSameAsOrLessThanAGE(string userData)
        {


            //is this age the same as or less than the  value entered for AGE ?

            int smokingAge;
            int currentAge;

            try
            {
                smokingAge = Convert.ToInt32(userData);
                currentAge = Convert.ToInt32(getGS().Get("AGE"));



            }
            catch (FormatException e)
            {

                return false;

            }


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
