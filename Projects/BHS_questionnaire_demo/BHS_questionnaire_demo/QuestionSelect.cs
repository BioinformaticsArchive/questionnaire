
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
    class QuestionSelect : Question
    {

        
        private Label label; 

        private ComboBox selectBox;

        private List<Option> optionList;

        //length of the comboBox 
        public int SelectLength { get; set; }



        //constructor
        public QuestionSelect(Form form, Form bigMessageBox, GlobalStore gs, GlobalStore specialDataStore, QuestionManager qm)
            : base(form, bigMessageBox, gs, specialDataStore, qm)
        {
            //init the optionslist
            optionList = new List<Option>();


        }

        //methods
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
            getQM().getPanel().Controls.Remove(selectBox);

            label.Dispose();
            selectBox.Dispose();



        }

        public void addOption(Option op)
        {
            optionList.Add(op);



        }

        public override void configureControls(UserDirection direction)
        {

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
            //label.AutoSize = true;
            label.Size = new Size(getWidgetWidth(), getWidgetHeight());

             
            selectBox = new ComboBox();

            //set font size
            setFontSize(label, selectBox);
            

            //event handlers
            //trap any keypress to deselect the skip-controls
            selectBox.Click += new EventHandler(button_click);
            
            //stop user being able to type in the combobox
            selectBox.DropDownStyle = ComboBoxStyle.DropDownList;
            

            selectBox.BackColor = GlobalColours.controlBackColour;
            
            selectBox.Location = new Point(labelXpos, labelYpos + getWidgetHeight());
            selectBox.Size = new Size(SelectLength, 20);


            Option previouslySelectedOption = null;
            
            
            //add items to combobox
            foreach (Option op in optionList)
            {

                //selectBox.Items.Add(op.getText());

                selectBox.Items.Add(op);

                //is this option the one that was selected previously ?
                if (processedData != null)
                {
                    if (processedData == op.getValue())
                    {
                        previouslySelectedOption = op;

                    }



                }




                
               
            }



            if (PageSeen && (processedData != null))
            {

                if (previouslySelectedOption != null)
                {

                    selectBox.SelectedItem = previouslySelectedOption;

                }
                



            }

            //add to the form
            getQM().getPanel().Controls.Add(label);
            
            getQM().getPanel().Controls.Add(selectBox);
            
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
               

                processedData = skipSetting;

                return ToCode;




            }



            //get the selected date


            //object selectedData = selectBox.SelectedItem;

            Option selectedOption = (Option)(selectBox.SelectedItem);
            

            //are any of these null
            if (selectedOption == null)
            {
                ((Form2)getBigMessageBox()).setLabel("You must choose something");
                getBigMessageBox().ShowDialog();

                return Code;
            }

            //convert to string

            //processedData = selectedData.ToString();

            //get the value not the text for the option
            processedData = selectedOption.getValue();


            

            return ToCode;






        }







    }
}
