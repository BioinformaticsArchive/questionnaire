
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
using System.IO;



namespace BHS_questionnaire_demo
{
    class QuestionRadio : Question
    {
        //a Radio Button set

        //fields
        protected List<Option> optionList;

        protected GroupBox radioGroup;

        public string LabelToGroupBoxGap { get; set; }


        //constructor
        public QuestionRadio(Form form, Form bigMessageBox, GlobalStore gs, GlobalStore specialDataStore, QuestionManager qm)
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


        }

        public override void save(TextWriter dhProcessedData, TextWriter dhUserData)
        {

            //call the base method save to save the processed data
            save(dhProcessedData);

            //Note: no userdata for this widget



        }





        public void addOption(Option op)
        {
            optionList.Add(op);



        }

        public override void removeControls()
        {
            
            //getForm().Controls.Remove(radioGroup);
            getQM().getPanel().Controls.Remove(radioGroup);

            
            radioGroup.Dispose();





        }

        public override void configureControls(UserDirection direction)
        {


            //turn the skip controls on again
            getQM().getMainForm().setSkipControlsVisible();
            
            radioGroup = new GroupBox();
            radioGroup.ForeColor = GlobalColours.mainTextColour;

            setFontSize(radioGroup);
           

            //position the group box under the label, i.e. at the same xpos but an increased ypos
            int groupBoxXpos = getWidgetXpos();
            int groupBoxYpos = getWidgetYpos();



            //position of the groupbox
            radioGroup.Location = new Point(groupBoxXpos, groupBoxYpos);
            radioGroup.Size = new Size(getWidgetWidth(), getWidgetHeight());
            radioGroup.Text = Val;
            
           
            
            //create the RadioButton objects that we need, i.e. one for each option.

            RadioButton rb;
           
            //create a radiobutton for each option
            foreach (Option op in optionList)
            {
                //create a radiobutton
                rb = new RadioButton();

                //trap the click
                rb.Click += new EventHandler(button_click);


                rb.ForeColor = GlobalColours.mainTextColour;

                rb.Text = op.getText();
                //rb.Tag = op.getValue();
                rb.Tag = op;
                rb.Location = new Point(op.getWidgetXpos(), op.getWidgetYpos());
                rb.Size = new Size(op.getWidgetWidth(), op.getWidgetHeight());

                if (! PageSeen)
                {
                    if (op.Default)
                    {
                        rb.Checked = true;
                    }

                }
                else
                {
                    //page has been seen, set the previous data
                    if (op.getValue() == processedData)
                    {
                        rb.Checked = true;

                    }

                }

                
                

                radioGroup.Controls.Add(rb);

                



            }

            //add controls to the form
            

            //getForm().Controls.Add(radioGroup);
            getQM().getPanel().Controls.Add(radioGroup);

            setSkipSetting();



        }

        

        public override string processUserData()
        {

            PageSeen = true;

            //did the user skip this question ?
            //if (getQM().SkipThisQuestion)
            string skipSetting = getSkipSetting();  //returns null if none of those skip-controls were selected

            if(skipSetting != null)
            {
                //yes

                //do not overwrite data that may be there already
               // if (processedData == null)
               // {

                    //processedData = "ENTRY_REFUSED";
                    processedData = skipSetting;

              //  }

                return ToCode;




            }
            
            //find the selected button
            RadioButton selectedButton= null;
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
            processedData = selectedOption.getValue();
            string optionToCode = selectedOption.ToCode;

            //in some cases we need to show an error box
            string optionToCodeErr = selectedOption.ToCodeErr;


            //processedData = (string)(selectedButton.Tag);


            //if we have a global setting: save to the global object
            string globalKey = SetKey;
            if (globalKey != null)
            {
                getGS().Add(globalKey, processedData);

            }

            //get the next question code

            if (optionToCodeErr != null)
            {

                //show error message
                ((Form2)getBigMessageBox()).setLabel("Please try again");
                getBigMessageBox().ShowDialog();

                return optionToCodeErr;

            }


            //special check for stop-smoking question
            if (Code == "TOB8")
            {
                //get the person's current age
                string currentAge = getGS().Get("AGE");

                //get age they stopped smoking
                string ageStoppedSmoking = getGS().Get("AGE_STOPPED_SMOKING");


                //are these both non-null
                if ((currentAge == null) || (ageStoppedSmoking == null))
                {
                    Form3 warningBox = getQM().getWarningBox();

                    warningBox.setLabel("Warning: Can't cross-check against age or age when stopped smoking as one or both has not been entered");
                    warningBox.ShowDialog();

                    return ToCode;



                }
                else
                {
                    //work out the difference between the 2 ages 

                    decimal yearsSinceStoppedSmoking = Convert.ToDecimal(currentAge) - Convert.ToDecimal(ageStoppedSmoking);
                    decimal daysSinceStoppedSmoking = yearsSinceStoppedSmoking * 365.25M;


                    //is this consistent with what the user has chossen
                    bool hasError = false;

                    if (processedData == "1")
                    {
                        //user selected Today or Yesterday
                        if (daysSinceStoppedSmoking > 1)
                        {

                            hasError = true;

                        }


                    }
                    else if (processedData == "2")
                    {
                        //user selected 2-6 days
                        if (daysSinceStoppedSmoking > 6)
                        {

                            hasError = true;
                        }


                    }
                    else if (processedData == "3")
                    {
                        //user selected 1 week- < 1 month
                        if (daysSinceStoppedSmoking > 31)
                        {

                            hasError = true;
                        }



                    }
                    else if (processedData == "4")
                    {
                        //user selected 1 month - < 1 year
                        if (yearsSinceStoppedSmoking > 1)
                        {

                            hasError = true;
                        }

                    }
                    else if (processedData == "5")
                    {
                        //user selected 1-5 years
                        if ((yearsSinceStoppedSmoking < 1) || (yearsSinceStoppedSmoking > 5))
                        {

                            hasError = true;
                        }


                    }
                    else if (processedData == "6")
                    {
                        //user selected > 5 years
                        if (yearsSinceStoppedSmoking <= 5)
                        {

                            hasError = true;
                        }

                    }
                    else
                    {
                        //error
                        throw new Exception();


                    }

                    if (hasError)
                    {
                        Form3 warningBox = getQM().getWarningBox();

                        warningBox.setLabel("Warning: Your selection is not consistent with your current age (" + currentAge + ") and the age when you stopped smoking (" + ageStoppedSmoking + ")");
                        warningBox.ShowDialog();



                    }

                    return ToCode;




                }


            }


            else if (optionToCode == null)
            {
                //no tocode for this option, i.e. use the std tocode
                return ToCode;
            }
            else
            {
                //this option has a toCode
                return optionToCode;

            }
            





        }

        public override bool isFormExit()
        {
            //will this exit the form
            if (processedData == null)
            {

                //question was not answered:
                return false;



            }
            else
            {

                //get the appropriate ToCode.
                //find the option for this data
                string useToCode = null;

                string fromCode = null;


                foreach (Option option in optionList)
                {


                    if (option.ToCodeErr != null)
                    {

                        fromCode = option.ToCodeErr;
                    }
                    
                    if (processedData == option.getValue())
                    {
                        //does this option have a tocode ?


                        if (option.ToCode == null)
                        {
                            useToCode = ToCode;

                        }
                        else
                        {

                            useToCode = option.ToCode;

                        }

                        break;



                    }





                }

                // if useToCode is still null, there was no matching option, so must have been a skip button
                if (useToCode == null)
                {

                    useToCode = ToCode;
                }


                //does this tocode point to the final section ?
                if (useToCode == "THANKYOU")
                {
                    
                    //certain questions in the consents section are second-answer types, where sometimes the 
                    //user has changed their mind for the first answer from no to yes. In these cases, we should not count them 
                    //as true

                    if (Section == "Consents")
                    {
                        //get the linking question which brought us here

                        if (fromCode != null)
                        {

                            //get the question object and get the processedData for the fromCode
                            string fromQdata= getQM().getQuestion(fromCode).processedData;

                            if (fromQdata == "1")
                            {
                                //ignore this as the user has changed their mind from unconsented to consented
                                return false;

                            }
                            else
                            {
                                return true;

                            }


                        }
                        else
                        {

                            return true;

                        }




                    }
                    else
                    {
                        return true;

                    }
                    
                    
                    
                    
                    
                    
                    
                    
                    
                }
                else
                {
                    return false;

                }



            }
           




        }



        public override bool isSectionExit(Dictionary<string, string> questionToSectionMap)
        {

            //is this question a valid section exit, i.e. 
            //we must have a non-null processedData AND must point to a question outside of this section

            if (processedData == null)
            {

                //question was not answered:
                return false;



            }
            else
            {
                
                //get the appropriate ToCode.
                //find the option for this data
                string useToCode = null;
                string fromCode = null;

                foreach (Option option in optionList)
                {

                    if (option.ToCodeErr != null)
                    {

                        fromCode = option.ToCodeErr;
                    }
                    
                    
                    
                    if (processedData == option.getValue())
                    {
                        //does this option have a tocode ?
                        

                        if (option.ToCode == null)
                        {
                            useToCode = ToCode;

                        }
                        else
                        {

                            useToCode = option.ToCode;

                        }

                        break;



                    }

                    



                }

                // if useToCode is still null, there was no matching option, so must have been a skip button
                if (useToCode == null)
                {

                    useToCode = ToCode;
                }
                

                //is this code outside the section?
                //this question has been answered, but does the ToCode point to a different section
                string toCodeSection = questionToSectionMap[useToCode];

                if (toCodeSection == Section)
                {
                    //next question is in this section
                    return false;

                }
                else
                {
                    //next question is in another section -> exit question
                    //Note: there are some exceptions to this in the Consents section, if this is a second entry where the user has changed their mind on the first entry

                    if (Section == "Consents")
                    {
                        //get the linking question which brought us here

                        if (fromCode != null)
                        {

                            //get the question object and get the processedData for the fromCode
                            string fromQdata = getQM().getQuestion(fromCode).processedData;

                            if (fromQdata == "1")
                            {
                                //ignore this as the user has changed their mind from unconsented to consented
                                return false;

                            }
                            else
                            {
                                return true;

                            }


                        }
                        else
                        {

                            return true;

                        }




                    }
                    else
                    {
                        return true;

                    }





                    


                }
                
                

            }




        }





    }
}
