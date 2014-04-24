
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
    class QuestionAutomatic : Question
    {
        //text box

        //fields
       

        public string UnderWeightAdvice
        {
            get;
            set;
        }

        public string OverWeightAdvice
        {
            get;
            set;
        }

        public string HypertensiveAdvice
        {
            get;
            set;

        }


        //reference to a subroutine that will do any needed processing
        private string processCodeLabel;

        

        //constructor
        public QuestionAutomatic(Form form, Form bigMessageBox, GlobalStore gs, GlobalStore specialDataStore, QuestionManager qm)
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

            //Note: no userdata for this widget



        }
        

        public void setProcess(string process)
        {
            processCodeLabel = process;

        }

       

        public override string processUserData()
        {

            //code for the next question
            string nextCode;
            

            //we have seen this page
            PageSeen = true;

            if (processCodeLabel == "SetParticipantID")
            {
                //get this from the form
                processedData = getQM().getUserID();



            }

            else if (processCodeLabel == "SetCurrentTimeBlood")
            {

                //get the current time.
                DateTime now = DateTime.Now;


                string day = now.Day.ToString();
                string month = now.Month.ToString();
                string year = now.Year.ToString();

                getSD().Add("DEXAM2", day);
                getSD().Add("MEXAM2", month);
                getSD().Add("YEXAM2", year);


            }

            else if (processCodeLabel == "SetCurrentTimeInterview")
            {

                //get the current time.
                DateTime now = DateTime.Now;


                string day = now.Day.ToString();
                string month = now.Month.ToString();
                string year = now.Year.ToString();

                getSD().Add("DEXAM", day);
                getSD().Add("MEXAM", month);
                getSD().Add("YEXAM", year);


            }

            else if (processCodeLabel == "CalcBMI")
            {
                //calculate the BMI

                //get weight and height that were previously entered

                string weight= getGS().Get("Weight");
                string height = getGS().Get("Height");

                

                //check that these are not null: which might happen if previous questions were skipped

                if ((weight != null) && (height != null) && (Convert.ToDecimal(height) != 0M))
                {
                    decimal weightAsDec = Convert.ToDecimal(weight);
                    decimal heightAsDec = Convert.ToDecimal(height) / 100;      //convert from cm to m

                    decimal bmi = Math.Round(weightAsDec / (heightAsDec * heightAsDec), 2);

                    //check if this is outside the normal range:

                    processedData = bmi.ToString();



                    if (bmi < 18.5M || bmi >= 25M)
                    {
                        //outside normal range

                        string message;

                        if (bmi < 18.5M)
                        {
                            //underweight
                            message = "You have a BMI of " + bmi.ToString() + ", indicating underweight\n";
                            message += UnderWeightAdvice;

                        }
                        else if (bmi >= 30)
                        {
                            //obese
                            message = "You have a BMI of " + bmi.ToString() + ", indicating obese";
                            message += OverWeightAdvice;



                        }
                        else
                        {
                            //overweight
                            message = "You have a BMI of " + bmi.ToString() + ", indicating overweight";
                            message += OverWeightAdvice;



                        }

                        //show advice screen
                        AdviceForm af = getQM().getAdviceBox();
                        af.setAdviceText("BMI Advice", message);
                        af.ShowDialog();







                    }



                }
                else
                {
                    //some data not present

                    //set a value for processedData to prevent problems with locking the Questionnaire
                    processedData = "BMI_NOT_AVAILABLE";
                    
                    Form3 warningBox = getQM().getWarningBox();

                    warningBox.setLabel("Warning: Can't show BMI advice due to missing data");
                    warningBox.ShowDialog();



                }


            }

            else if(processCodeLabel=="CalcWHR"){

                string waist = getGS().Get("WCAVG");
                string hip = getGS().Get("HIPCAVG");
                string sex = getGS().Get("Sex");        //1= male, 2= female

                string message;

                //check these are not null

                if ((waist != null) && (hip != null) && (sex != null) && (Convert.ToDecimal(hip) != 0M))
                {
                    decimal waistAsDec = Convert.ToDecimal(waist);
                    decimal hipAsDec = Convert.ToDecimal(hip);

                    decimal ratio = Math.Round(waistAsDec / hipAsDec, 2);

                    processedData = ratio.ToString();


                    if (sex == "1")
                    {
                        //male
                        if (ratio > 0.95M)
                        {
                            //abnormal
                            message = "You have a waist/hip ratio of " + ratio + " indicating Overweight:";
                            message += OverWeightAdvice;

                            //show advice screen
                            AdviceForm af = getQM().getAdviceBox();
                            af.setAdviceText("Waist-hip ratio Advice", message);
                            af.ShowDialog();


                        }



                    }
                    else
                    {
                        //female
                        if (ratio > 0.8M)
                        {
                            //abnormal
                            message = "You have a waist/hip ratio of " + ratio + " indicating Overweight:";
                            message += OverWeightAdvice;

                            //show advice screen
                            AdviceForm af = getQM().getAdviceBox();
                            af.setAdviceText("Waist-hip ratio Advice", message);
                            af.ShowDialog();



                        }


                    }



                }
                else
                {
                    //some data not present

                    Form3 warningBox = getQM().getWarningBox();

                    warningBox.setLabel("Warning: Can't show HWR advice due to missing data");
                    warningBox.ShowDialog();



                }



            }
            else if (processCodeLabel == "CheckSYST")
            {
                string message;
                
                //check systolic blood pressure
                string systAVG = getGS().Get("SYSTAVG");

                if (systAVG == null)
                {
                    //missing data

                    Form3 warningBox = getQM().getWarningBox();

                    warningBox.setLabel("Warning: Can't show SYST advice due to missing data");
                    warningBox.ShowDialog();



                }
                else
                {
                    decimal syst = Convert.ToDecimal(systAVG);

                    if (syst > 120)
                    {
                        if (syst >= 140)
                        {

                            //abnormal

                            message = "You have a systolic blood pressure of " + systAVG + " indicating Abnormal";
                            message += HypertensiveAdvice;


                            //show advice screen
                            AdviceForm af = getQM().getAdviceBox();
                            af.setAdviceText("Systolic Blood Pressure", message);
                            af.ShowDialog();


                        }
                        else
                        {
                            //pre-hypertensive
                            message = "You have a systolic blood pressure of " + systAVG + " indicating Pre-Hypertensive";
                            message += HypertensiveAdvice;


                            //show advice screen
                            AdviceForm af = getQM().getAdviceBox();
                            af.setAdviceText("Systolic Blood Pressure", message);
                            af.ShowDialog();


                        }



                    }




                }



            }

            else if (processCodeLabel == "CheckDIAST")
            {
                string message;

                //check diastolic blood pressure
                string diastAVG = getGS().Get("DIASTAVG");

                if (diastAVG == null)
                {
                    //missing data

                    Form3 warningBox = getQM().getWarningBox();

                    warningBox.setLabel("Warning: Can't show DIAST advice due to missing data");
                    warningBox.ShowDialog();



                }
                else
                {
                    decimal diast = Convert.ToDecimal(diastAVG);

                    if (diast > 80)
                    {
                        if (diast >= 90)
                        {

                            //abnormal

                            message = "You have a diastolic blood pressure of " + diastAVG + " indicating Abnormal";
                            message += HypertensiveAdvice;


                            //show advice screen
                            AdviceForm af = getQM().getAdviceBox();
                            af.setAdviceText("Diastolic Blood Pressure", message);
                            af.ShowDialog();


                        }
                        else
                        {
                            //pre-hypertensive
                            message = "You have a diastolic blood pressure of " + diastAVG + " indicating Pre-Hypertensive";
                            message += HypertensiveAdvice;

                            //show advice screen
                            AdviceForm af = getQM().getAdviceBox();
                            af.setAdviceText("Diastolic Blood Pressure", message);
                            af.ShowDialog();
                            


                        }



                    }




                }
            }



            else
            {
                throw new Exception();

            }




            //advance to the next question
            nextCode = ToCode;

            
            return nextCode;



        }

        public override void removeControls()
        {
            


        }

        public override void configureControls(UserDirection direction)
        {

            

        }





    }
}

