
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
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing;


namespace BHS_questionnaire_demo
{
    abstract class Question
    {

        //base class of Question types

        //fields

        private int widgetWidth;
        private int widgetHeight;
        private int widgetXpos;
        private int widgetYpos;

        //big message box
        private Form bigMessageBox;

        //reference to the form that the main controls are placed on
        private Form form;

        //global storage which is visible to all questions
        private GlobalStore gs;

        private GlobalStore specialDataStore;

        private QuestionManager qm;

        
        
        

        //properties
        public string Code {get; set;}
        public string Val {get; set;}
        public string ToCode {get; set;}
        public string FromCode { get; set; }
        public string Section {get; set;}
        public string IfSettingVal {get; set;}
        public string IfSettingKey {get; set;}
        public string OnErrorQuestionCompare { get; set; }

        //properties
        public string SetKey { get; set; }

        //has the user entered data for this question ?
        public bool PageSeen { get; set; }

        public string processedData { get; set; }

        //constructor

        public Question(Form form, Form bigMessageBox, GlobalStore gs, GlobalStore specialDataStore, QuestionManager qm){

            this.form = form;
            this.bigMessageBox = bigMessageBox;
            this.PageSeen = false;
            this.gs= gs;
            this.specialDataStore = specialDataStore;
            this.qm = qm;
            processedData = null;



        }

        


        //methods
        public bool IfSettingsOK(){

            //check IfSettingKey and IfSettingVal to make sure we can go ahead with this question

            if ((IfSettingKey != null) && (IfSettingVal != null))
            {

                //get the value from the global store

                string globalSettingForKey = gs.Get(IfSettingKey);

                if (globalSettingForKey == null)
                {
                    //global key was not set: skip test
                    return true;

                }
                else
                {
                    //does the global value == the required value ?

                    if (globalSettingForKey == IfSettingVal)
                    {
                        //yes
                        // OK, we can continue
                        return true;



                    }
                    else
                    {
                        //no we can't continue
                        return false;



                    }


                }






            }
            else
            {
                //null settings, i.e. skip the test
                return true;


            }




        }





        public GlobalStore getGS(){

            return gs;


        }

        public QuestionManager getQM()
        {
            return qm;

        }


        public GlobalStore getSD()
        {
            return specialDataStore;


        }



        protected Form getForm()
        {
            return form;
        }

        protected Form getBigMessageBox()
        {
            return bigMessageBox;

        }


        public int getWidgetWidth() { 
            
           return widgetWidth;

        }

        public void setWidgetWidth(string width){

            widgetWidth= Convert.ToInt32(width);

        }

        public int getWidgetHeight(){

            return widgetHeight;


        }

        public void setWidgetHeight(string height){

            widgetHeight= Convert.ToInt32(height);
        }

        public int getWidgetXpos(){

            return widgetXpos;
        }

        public void setWidgetXpos(string xpos){

            widgetXpos= Convert.ToInt32(xpos);


        }

        public void setWidgetYpos(string ypos){

            widgetYpos= Convert.ToInt32(ypos);

        }

        public int getWidgetYpos(){

            return widgetYpos;
        }



        //methods
        //convert a string to an int
        protected int strToInt(string str){

            int numVal;

            numVal = Convert.ToInt32(str);
            
            return numVal;



        }

        public void showSection()
        {
            //display section on the form
            qm.getSectionLabel().Text = Section;




        }

        protected bool testNullEntry(string userData)
        {

            //returns true if not null, empty or white-space


            if(string.IsNullOrWhiteSpace(userData)){

                return false;
            }
            else{

                return true;
            }


        }




        //called only in child classes
        //public abstract bool isSectionExit(Dictionary<string, string> questionToSectionMap);


        public abstract void configureControls(UserDirection direction);

        public abstract string processUserData();

        public abstract void removeControls();

        public abstract void save(TextWriter dhProcessedData, TextWriter dhUserData);

        public void saveFinalData(TextWriter dataOut)
        {
            //same as save except we filter out Questions which are duplicates etc



            //save the data stored in this object
            if (processedData != null)
            {

                //is this a duplicate code e.g. PLSAVG has a duplicate PLSAVG-2 ?
                Match match = Regex.Match(Code, @".+-\d+$");

                if (!match.Success)
                {

                    //did not match the duplicate pattern, so save

                    dataOut.WriteLine(Code + "\t" + processedData);


                }

            }




        }
        


        public void save(TextWriter dataOut)
        {

            //save the data stored in this object
            if (processedData != null)
            {
                dataOut.WriteLine(Code + "\t" + processedData);


            }
            



        }

        public abstract void load(Dictionary<string, string> pDataDict, Dictionary<string, string> uDataDict);

        public void load(Dictionary<string, string> pDataDict)
        {

            //can I find this code in the dictionary
            if (pDataDict.ContainsKey(Code))
            {
                processedData = pDataDict[Code];

            }
            else
            {
                processedData = null;

            }

            

        }


        public void savePageSeen(TextWriter dataOut)
        {

            //save the data stored in this object
            if (PageSeen)
            {
                dataOut.WriteLine(Code);


            }
            

        }

        public void loadPageSeen(HashSet<string> seenPages)
        {

            //load data from file
            if (seenPages.Contains(Code))
            {

                PageSeen = true;


            }



        }


        protected string getPreviousData(){

            //return the processedData for the previous question
            if (OnErrorQuestionCompare == null)
            {
                throw new Exception();

            }


            string previousQuestionCode = OnErrorQuestionCompare;

            Question previousQuestion = qm.getQuestion(previousQuestionCode);

            string previousUserData = previousQuestion.processedData;

            return previousUserData;




        }
        





        protected virtual bool testCheckSameAsPrevious(string userData)
        {
            //is this the same as the previous userdata ?

            //get the previous question code
            string previousQuestionCode = FromCode;

            //get the previous question object

            Question previousQuestion = qm.getQuestion(previousQuestionCode);

            string previousUserData = previousQuestion.processedData;

            //are these the same ?

            if (userData == previousUserData)
            {
                return true;

            }
            else
            {
                
                //check the special case where the previous value was null

                if (string.IsNullOrEmpty(userData) && (previousUserData == "No User Entry"))
                {
                    return true;


                }
                else
                {

                    return false;

                }
                
                
                

            }


        }


        public bool TestNumeric(string data)
        {

            try
            {
                Convert.ToDecimal(data);
                return true;



            }
            catch
            {
                return false;

            }



        }

        public bool TestNumericNotZero(string data)
        {

            try
            {
                decimal num= Convert.ToDecimal(data);

                if (num > 0)
                {
                    return true;

                }
                else
                {
                    return false;


                }

               

            }
            catch
            {
                return false;

            }



        }


        public string getSkipSetting()
        {
            return getQM().getMainForm().getSkipSetting();


        }

        public void clearControlButtons()
        {
            //deselect all control buttons (no answer, etc)
            getQM().getMainForm().clearControlButtons();




        }

        public void button_click(object sender, EventArgs e)
        {
            //a radiobutton has been clicked: deselect any of the control buttons (no answer, etc) that might have been selected
            clearControlButtons();



        }


        public void setSkipSetting()
        {
            if (processedData != null)
            {
                if (processedData == "No Answer" || processedData == "Don't Know" || processedData == "Not Applicable")
                {

                    //set the radiobutton on the main form

                    getQM().getMainForm().setSkipSetting(processedData);





                }
                else
                {
                    //normal answer
                    //deselect all the skip controls
                    getQM().getMainForm().clearControlButtons();

                }


            }


            else
            {

                //de-select skip buttons
                getQM().getMainForm().clearControlButtons();


            }



        }


        public void setFontSize(params Control[] controls){

            //varargs function, takes any number of Controls

            float fontSize=18;

            foreach (Control control in controls)
            {
                control.Font = new Font(control.Font.Name, fontSize, control.Font.Style, control.Font.Unit);

            }





        }

        public decimal CalcAverageOf2(String val1, String val2)
        {

            decimal reading1AsDec = Convert.ToDecimal(val1);
            decimal reading2AsDec = Convert.ToDecimal(val2);


            decimal average = (reading1AsDec + reading2AsDec) / 2;

            return average;




        }


        public virtual bool isFormExit()
        {
            //will this exit the form
            return false;




        }




        public virtual bool isSectionExit(Dictionary<string, string> questionToSectionMap)
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
                //this question has been answered, but does the ToCode point to a different section
                string toCodeSection = questionToSectionMap[ToCode];

                if (toCodeSection == Section)
                {
                    //next question is in this section
                    return false;

                }
                else
                {
                    //next question is in another section -> exit question
                    return true;


                }


            }




        }



    }
}
