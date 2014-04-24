
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
    class QuestionDatePickerSelect : Question
    {


        //fields

        private Label label;    //main label


        private Label labelDays;
        private Label labelMonths;
        private Label labelYears;

        //private ComboBox selectDays;
        private ComboBox selectDays;
        private ComboBox selectMonths;
        private ComboBox selectYears;

        //reference to a subroutine that will do the validation
        private string validationCodeLabel;



        //the data the user entered, which may be different to the processed data
        //private string userData;

        private string[] months= { "Don't Know", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

        private Dictionary<string, int> monthMap;


        //properties
        public string OnError { get; set; }

        //constructor
        public QuestionDatePickerSelect(Form form, Form bigMessageBox, GlobalStore gs, GlobalStore specialDataStore, QuestionManager qm)
            : base(form, bigMessageBox, gs, specialDataStore, qm)
        {

           
            
            //mapping from month as int to month as string
            monthMap = new Dictionary<string, int>();

            for (int i = 0; i <= 12; i++)
            {

                monthMap.Add(months[i], i);

            }






        }



        //methods
        public void setValidation(string validation)
        {
            validationCodeLabel = validation;

        }






        public override void save(TextWriter dhProcessedData, TextWriter dhUserData)
        {

            //call the base method save to save the processed data
            save(dhProcessedData);

            



        }
        public override void load(Dictionary<string, string> pDataDict, Dictionary<string, string> uDataDict)
        {
            //load the processed data via the base class
            load(pDataDict);

            


        }



        public override void removeControls()
        {
            getQM().getPanel().Controls.Remove(label);
            getQM().getPanel().Controls.Remove(labelDays);
            getQM().getPanel().Controls.Remove(labelMonths);
            getQM().getPanel().Controls.Remove(labelYears);
            getQM().getPanel().Controls.Remove(selectDays);
            getQM().getPanel().Controls.Remove(selectMonths);
            getQM().getPanel().Controls.Remove(selectYears);
            


            label.Dispose();
            labelDays.Dispose();
            labelMonths.Dispose();
            labelYears.Dispose();
            selectDays.Dispose();
            selectMonths.Dispose();
            selectYears.Dispose();

            




        }

        public override void configureControls(UserDirection direction)
        {

            //direction is either 'forward' or 'reverse'
            //turn the skip controls on again
            getQM().getMainForm().setSkipControlsVisible();


            //create a label 
            label = new Label();

            //the question Text shown to the user
            label.Text = Val;
            label.ForeColor = GlobalColours.mainTextColour;

            int labelXpos = getWidgetXpos();
            int labelYpos = getWidgetYpos();



            //position of the Label
            label.Location = new Point(labelXpos, labelYpos);
            label.AutoSize = true;
            //label.Size = new Size(getWidgetWidth(), getWidgetHeight());

            int yPosDateLabel= labelYpos + 50;
            

            //labels for days, months, years
            labelDays = new Label();
            labelDays.Text = "Days";
            labelDays.Location = new Point(labelXpos + 10, yPosDateLabel);
            labelDays.AutoSize = true;
            labelDays.ForeColor = GlobalColours.mainTextColour;


            labelMonths = new Label();
            labelMonths.Text = "Months";
            labelMonths.Location = new Point(labelXpos + 180, yPosDateLabel);
            labelMonths.AutoSize = true;
            labelMonths.ForeColor = GlobalColours.mainTextColour;

            labelYears = new Label();
            labelYears.Text = "Years";
            labelYears.Location = new Point(labelXpos + 350, yPosDateLabel);
            labelYears.AutoSize = true;
            labelYears.ForeColor = GlobalColours.mainTextColour;

            int yPosSelect = yPosDateLabel + 50;
            //int yPosSelect = yPosDateLabel;

            //select boxes for day, month, year
            //selectDays = new ComboBox();
            selectDays = new ComboBox();
            //selectDays.ItemHeight = 5;

            selectMonths = new ComboBox();


            selectYears = new ComboBox();

            //set font size
            setFontSize(label, labelDays, labelMonths, labelYears, selectDays, selectMonths, selectYears);

            //event handlers
            //trap any keypress to deselect the skip-controls
            selectDays.Click += new EventHandler(button_click);
            selectMonths.Click += new EventHandler(button_click);
            selectYears.Click += new EventHandler(button_click);


            //stop user being able to type in the combobox
            selectDays.DropDownStyle = ComboBoxStyle.DropDownList;
            selectMonths.DropDownStyle = ComboBoxStyle.DropDownList;
            selectYears.DropDownStyle = ComboBoxStyle.DropDownList;



            selectDays.BackColor = GlobalColours.controlBackColour;
            selectMonths.BackColor = GlobalColours.controlBackColour;
            selectYears.BackColor = GlobalColours.controlBackColour;

            selectDays.Location = new Point(labelXpos, yPosSelect);
            //selectDays.Location = new Point(labelXpos + 90, yPosSelect);
            selectDays.Size = new Size(160, 20);

            

            selectMonths.Location = new Point(labelXpos + 170, yPosSelect);
            selectMonths.Size = new Size(160, 20);

            selectYears.Location = new Point(labelXpos + 340, yPosSelect);
            selectYears.Size = new Size(160, 20);
            //selectYears.Size = new Size(160, 150);



            //show only 5 items at a time in drop-down list
            selectMonths.IntegralHeight = false; //won't work unless this is set to false
            selectMonths.MaxDropDownItems = 5;

            selectDays.IntegralHeight = false; //won't work unless this is set to false
            selectDays.MaxDropDownItems = 5;

            selectYears.IntegralHeight = false; //won't work unless this is set to false
            selectYears.MaxDropDownItems = 5;





            //array of days
            string[] days = new string[32];

            days[0] = "Don't Know";

            
            for (int i = 1; i <= 31; i++)
            {
                days[i] = i.ToString();

            }
            selectDays.Items.AddRange(days);


            //array of months
           

            selectMonths.Items.AddRange(months);
            selectMonths.IntegralHeight = false;
            selectMonths.MaxDropDownItems = 5;

            //get the current year.
            int currentYear= DateTime.Now.Year;

            //first year should also be don't know
            selectYears.Items.Add("Don't Know");

            for (int i = currentYear; i >= 1900; i--)
            {
                selectYears.Items.Add(i);


            }




            //if userdirection is reverse, populate the control with the previously entered text
            if (PageSeen)
            {

                //assume the date is stored as dd/mm/yyyy

                //extract the day, months, years.

                if (processedData != null)
                {

                    Match match = Regex.Match(processedData, @"(.+)/(.+)/(.+)");

                    string pDays, pMonths, pYears;

                    if (match.Success)
                    {
                        pDays = match.Groups[1].Value;
                        pMonths = match.Groups[2].Value;
                        pYears = match.Groups[3].Value;

                        if (pDays == "0")
                        {
                            pDays = "Don't Know";
                        }

                        selectDays.SelectedItem = pDays;

                        //convert month from integer to string form.
                        pMonths = months[Convert.ToInt32(pMonths)];


                        selectMonths.SelectedItem = pMonths;

                        if (pYears == "0")
                        {
                            
                            selectYears.SelectedItem = "Don't Know";
                        }
                        else
                        {
                            //years must be numeric rather than string, otherwise won't be recognised by selectbox
                            selectYears.SelectedItem = Convert.ToInt32(pYears);

                        }

                        

                        

                        


                        //set the selected items in the combo boxes



                    }
                    





                }
               

                

            }

            //add to the form
            getQM().getPanel().Controls.Add(label);
            getQM().getPanel().Controls.Add(labelDays);
            getQM().getPanel().Controls.Add(labelMonths);
            getQM().getPanel().Controls.Add(labelYears);
            getQM().getPanel().Controls.Add(selectDays);
            getQM().getPanel().Controls.Add(selectMonths);
            getQM().getPanel().Controls.Add(selectYears);

            setSkipSetting();



        }



        public override string processUserData()
        {

            //we have seen this page
            PageSeen = true;

            //did the user skip this question ?
            string skipSetting = getSkipSetting();
            if (skipSetting != null)
            {
                //yes

                //do not overwrite data that may be there already
                //if (processedData == null)
                //{

                    processedData = skipSetting;

               // }

                return ToCode;




            }



            //get the selected date


            object sDays = selectDays.SelectedItem;
            object sMonths = selectMonths.SelectedItem;
            object sYears = selectYears.SelectedItem;

            //are any of these null
            if (sDays == null || sMonths == null || sYears == null)
            {
                ((Form2)getBigMessageBox()).setLabel("You must choose a day, month and year");
                getBigMessageBox().ShowDialog();

                return Code;
            }

            //convert to string

            string selectedDays = sDays.ToString();
            string selectedMonths = sMonths.ToString();
            string selectedYears = sYears.ToString();

            //convert the month from a string to a number
            int selectedMonthsInt = monthMap[selectedMonths];

            //convert the day to a number, where 'don't know' = 0.
            int selectedDayInt;

            if (selectedDays == "Don't Know")
            {

                selectedDayInt = 0;
            }
            else
            {
                selectedDayInt = Convert.ToInt32(selectedDays);
            }


            int selectedYearInt;

            if (selectedYears == "Don't Know")
            {

                selectedYearInt = 0;
            }
            else
            {
                selectedYearInt = Convert.ToInt32(selectedYears);
            }



            //check if this date is valid
            if ((selectedDays != "Don't Know") && (selectedMonths != "Don't Know") && (selectedYears != "Don't Know"))
            {
                
                try
                {

                    DateTime dt = new DateTime(selectedYearInt, selectedMonthsInt, selectedDayInt);

                }
                catch(ArgumentOutOfRangeException e){

                    //an invalid date was entered
                    ((Form2)getBigMessageBox()).setLabel("You must choose a valid date");
                    getBigMessageBox().ShowDialog();

                    return Code;


                }
                
                

            }

            if (validationCodeLabel == "CheckOver18")
            {
                //this person must be at least 18, otherwise error
                if (selectedYears != "Don't Know")
                {

                    int useDays;
                    int useMonths;

                    if (selectedDays == "Don't Know")
                    {
                        useDays = 1;
                    }
                    else
                    {
                        useDays = selectedDayInt;
                    }

                    if (selectedMonths == "Don't Know")
                    {
                        useMonths = 1;


                    }
                    else
                    {
                        useMonths = selectedMonthsInt;

                    }
                    
                    
                    
                    DateTime dob = new DateTime(selectedYearInt, useMonths, useDays);
                    DateTime now = DateTime.Now;
                    TimeSpan age = now - dob;

                    if (age < TimeSpan.FromDays(18 * 365.25))
                    {
                        //under 18: error
                        ((Form2)getBigMessageBox()).setLabel("You are under 18");
                        getBigMessageBox().ShowDialog();

                        return Code;



                    }


                }




            }



            //convert to std date format
            processedData = selectedDayInt + "/" + selectedMonthsInt + "/" + selectedYearInt;



            //if we have a global setting: save to the global object
            string globalKey = SetKey;
            if (globalKey != null)
            {
                getGS().Add(globalKey, processedData);

            }

            //the control does all the validation

            return ToCode;






        }














    }
}

