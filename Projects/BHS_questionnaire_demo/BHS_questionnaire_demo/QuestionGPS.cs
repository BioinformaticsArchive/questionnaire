
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
    class QuestionGPS : Question
    {
        //text box

        //fields
        private TextBox textboxLat;     //latitude
        private TextBox textboxLong;    //longitude

        private TextBox textboxLat2;     //latitude 2nd entry
        private TextBox textboxLong2;    //longitude 2nd entry

        private Label labelFirstEntry;
        private Label labelSecondEntry;



        private Button setLocationButton;   //user clicks to set the location
        private Label label;
        private Label longLabel;
        private Label latLabel;
        private Label helpLabel;

        //reference to a subroutine that will do the validation
        private string validationCodeLabel;

        //reference to a subroutine that will do any needed processing
        private string processCodeLabel;


        //the data the user entered, which may be different to the processed data
        private string latitude = null;
        private string longitude = null;


        //properties
        public string OnError { get; set; }

        //constructor
        public QuestionGPS(Form form, Form bigMessageBox, GlobalStore gs, GlobalStore specialDataStore, QuestionManager qm)
            : base(form, bigMessageBox, gs, specialDataStore, qm)
        {



        }



        //methods
        public override void save(TextWriter dhProcessedData, TextWriter dhUserData)
        {

            //call the base method save to save the processed data
            save(dhProcessedData);

            //save the user data

            if ((longitude != null) && (latitude != null))
            {

                //save the data stored in this object
                dhUserData.WriteLine(Code + "\t" + longitude + "\t" + latitude);

            }


        }

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

                longitude = parts[0];
                latitude = parts[1];
                



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

            if (getQM().getMainForm().isGPSconnected())
            {

                //GPS dongle
                getQM().getPanel().Controls.Remove(label);
                getQM().getPanel().Controls.Remove(textboxLat);
                getQM().getPanel().Controls.Remove(textboxLong);
                getQM().getPanel().Controls.Remove(setLocationButton);
                getQM().getPanel().Controls.Remove(longLabel);
                getQM().getPanel().Controls.Remove(latLabel);
                getQM().getPanel().Controls.Remove(helpLabel);

                label.Dispose();
                textboxLat.Dispose();
                textboxLong.Dispose();
                setLocationButton.Dispose();
                longLabel.Dispose();
                latLabel.Dispose();
                helpLabel.Dispose();



            }
            else
            {
                //unconnected GPS device

                getQM().getPanel().Controls.Remove(label);
                getQM().getPanel().Controls.Remove(textboxLat);
                getQM().getPanel().Controls.Remove(textboxLong);
                getQM().getPanel().Controls.Remove(textboxLat2);
                getQM().getPanel().Controls.Remove(textboxLong2);
                getQM().getPanel().Controls.Remove(labelFirstEntry);
                getQM().getPanel().Controls.Remove(labelSecondEntry);

                
              

                label.Dispose();
                textboxLat.Dispose();
                textboxLong.Dispose();

                textboxLat2.Dispose();
                textboxLong2.Dispose();

                labelFirstEntry.Dispose();
                labelSecondEntry.Dispose();


            }

            

        }

        public override void configureControls(UserDirection direction)
        {

            //direction is either 'forward' or 'reverse'
            //turn the skip controls on again
            getQM().getMainForm().setSkipControlsVisible();



            //are we using a connected GPS unit or a stand-alone unit ?
            if (getQM().getMainForm().isGPSconnected())
            {
                //connected GPS dongle

                //create a label and textbox control
                label = new Label();
                label.ForeColor = GlobalColours.mainTextColour;

                textboxLat = new TextBox();
                textboxLat.BackColor = GlobalColours.controlBackColour;

                textboxLong = new TextBox();
                textboxLong.BackColor = GlobalColours.controlBackColour;

                //trap any keypress to deselect the skip-controls
                textboxLat.KeyPress += new KeyPressEventHandler(button_click);
                textboxLong.KeyPress += new KeyPressEventHandler(button_click);



                setLocationButton = new Button();
                setLocationButton.BackColor = GlobalColours.mainButtonColour;

                longLabel = new Label();
                longLabel.ForeColor = GlobalColours.mainTextColour;

                latLabel = new Label();
                latLabel.ForeColor = GlobalColours.mainTextColour;


                helpLabel = new Label();
                helpLabel.ForeColor = GlobalColours.mainTextColour;

                //set font size
                setFontSize(label, textboxLat, textboxLong, setLocationButton, longLabel, latLabel, helpLabel);




                //the question Text shown to the user
                label.Text = Val;

                int labelXpos = getWidgetXpos();
                int labelYpos = getWidgetYpos();

                //position the text box under the label, i.e. at the same xpos but an increased ypos




                //position of the Label
                label.Location = new Point(labelXpos, labelYpos);
                label.Size = new Size(getWidgetWidth(), getWidgetHeight());

                //position of the longitude/latitude labels
                longLabel.Location = new Point(labelXpos, labelYpos + 40);
                longLabel.Size = new Size(250, 50);
                longLabel.Text = "Longitude";

                latLabel.Location = new Point(labelXpos + 300, labelYpos + 40);
                latLabel.Size = new Size(250, 50);
                latLabel.Text = "Latitude";


                //help label
                helpLabel.Location = new Point(labelXpos, labelYpos + 150);
                helpLabel.Size = new Size(800, 80);
                helpLabel.Text = "Note: Click the button even if the GPS is disconnected or unavailable. You might need to click 'GPS connect' first.";





                //position of the textboxes
                textboxLong.Location = new Point(labelXpos, labelYpos + 80);
                textboxLong.Size = new Size(250, 50);


                textboxLat.Location = new Point(labelXpos + 300, labelYpos + 80);
                textboxLat.Size = new Size(250, 50);

                setLocationButton.Text = "click here when you want to record the GPS position";
                //setLocationButton.Location = new Point(labelXpos, labelYpos + 120);
                setLocationButton.Location = new Point(labelXpos + 600, labelYpos - 10);
                setLocationButton.Size = new Size(200, 150);
                setLocationButton.Click += new EventHandler(button_click2);




                //if page seen before, populate the control with the previously entered text
                if (PageSeen)
                {
                    textboxLat.Text = latitude;
                    textboxLong.Text = longitude;



                }



                //add controls to the panel
                getQM().getPanel().Controls.Add(label);
                getQM().getPanel().Controls.Add(textboxLong);
                getQM().getPanel().Controls.Add(textboxLat);
                getQM().getPanel().Controls.Add(setLocationButton);
                getQM().getPanel().Controls.Add(latLabel);
                getQM().getPanel().Controls.Add(longLabel);
                getQM().getPanel().Controls.Add(helpLabel);





            }
            else
            {

                //separate GPS unit

                //create a label and textbox control
                label = new Label();
                label.ForeColor = GlobalColours.mainTextColour;

                textboxLat = new TextBox();
                textboxLat.BackColor = GlobalColours.controlBackColour;

                textboxLong = new TextBox();
                textboxLong.BackColor = GlobalColours.controlBackColour;

                //extra boxes for second entry:

                textboxLat2 = new TextBox();
                textboxLat2.BackColor = GlobalColours.controlBackColour;

                textboxLong2 = new TextBox();
                textboxLong2.BackColor = GlobalColours.controlBackColour;



                //trap any keypress to deselect the skip-controls
                textboxLat.KeyPress += new KeyPressEventHandler(button_click);
                textboxLong.KeyPress += new KeyPressEventHandler(button_click);

                textboxLat2.KeyPress += new KeyPressEventHandler(button_click);
                textboxLong2.KeyPress += new KeyPressEventHandler(button_click);



                labelFirstEntry = new Label();
                labelFirstEntry.ForeColor = GlobalColours.mainTextColour;

                labelSecondEntry = new Label();
                labelSecondEntry.ForeColor = GlobalColours.mainTextColour;


               

                //set font size
                setFontSize(label, textboxLat, textboxLong, textboxLat2, textboxLong2 , labelFirstEntry, labelSecondEntry);




                //the question Text shown to the user
                label.Text = Val;

                int labelXpos = getWidgetXpos();
                int labelYpos = getWidgetYpos();

                //position the text box under the label, i.e. at the same xpos but an increased ypos




                //position of the Label
                label.Location = new Point(labelXpos, labelYpos);
                label.Size = new Size(getWidgetWidth(), getWidgetHeight());

                //position of the first entry label
                labelFirstEntry.Location = new Point(labelXpos, labelYpos + 70);
                labelFirstEntry.Size = new Size(600, 50);
                labelFirstEntry.Text = "First Entry:       Longitude                            Latitude";

                
                //position of second entry label
                labelSecondEntry.Location = new Point(labelXpos, labelYpos + 130);
                labelSecondEntry.Size = new Size(600, 50);
                labelSecondEntry.Text = "Second Entry: Longitude                            Latitude";

                


                //position of the textboxes
                textboxLong.Location = new Point(labelXpos + 290, labelYpos + 70);
                textboxLong.Size = new Size(120, 50);


                textboxLat.Location = new Point(labelXpos + 540, labelYpos + 70);
                textboxLat.Size = new Size(120, 50);


                textboxLong2.Location = new Point(labelXpos + 290, labelYpos + 130);
                textboxLong2.Size = new Size(120, 50);


                textboxLat2.Location = new Point(labelXpos + 540, labelYpos + 130);
                textboxLat2.Size = new Size(120, 50);






                //if page seen before, populate the control with the previously entered text
                if (PageSeen)
                {
                    textboxLat.Text = latitude;
                    textboxLong.Text = longitude;

                    textboxLat2.Text = latitude;
                    textboxLong2.Text = longitude;



                }



                //add controls to the panel
                getQM().getPanel().Controls.Add(label);
                getQM().getPanel().Controls.Add(textboxLong);
                getQM().getPanel().Controls.Add(textboxLat);

                getQM().getPanel().Controls.Add(textboxLong2);
                getQM().getPanel().Controls.Add(textboxLat2);

               
                getQM().getPanel().Controls.Add(labelFirstEntry);
                getQM().getPanel().Controls.Add(labelSecondEntry);
                




            }






            

            setSkipSetting();




        }



        public override string processUserData()
        {

            //code for the next question
            string nextCode;


            string errorMessage = null;


            //get the raw data from the user
            latitude = textboxLat.Text;
            longitude = textboxLong.Text;


            //we have seen this page
            PageSeen = true;

            //did the user skip this question ?
            string skipSetting = getSkipSetting();
            if (skipSetting != null)
            {
                //yes

                //do not overwrite data that may be there already
                //if (processedData == null)
               // {

                    processedData = skipSetting;

               // }

                return ToCode;




            }



            bool dataOK = false;


            if (! getQM().getMainForm().isGPSconnected())
            {

                //Garmin mode, i.e. we have 2 entries for each: check they are both the same
                if((longitude != textboxLong2.Text) || (latitude != textboxLat2.Text)){

                    
                    errorMessage = "Error: The longitude or Latitude is not the same between the 2 entries";

                    //validation has failed.

                    ((Form2)getBigMessageBox()).setLabel(errorMessage);
                    getBigMessageBox().ShowDialog();

                    //reset fields

                    latitude = "";
                    longitude = "";


                    return Code;


                }



            }



            //validate the data
            if (string.IsNullOrWhiteSpace(latitude) || string.IsNullOrWhiteSpace(longitude))
            {
                dataOK = false;
                errorMessage = "Error: please try again";


            }
            else
            {

                //are these the special values:
                //latitude: unavailable
                //longitude: unavailable
                //latitude: GPS disconnected
                //longitude: GPS disconnected

                if (((latitude == "GPS unavailable") && (longitude == "GPS unavailable")) || ((latitude == "GPS disconnected") && (longitude == "GPS disconnected")))
                {
                    dataOK = true;

                }
                else
                {

                    //check that values are decimal
                    try
                    {

                        decimal latitudeAsDec = Convert.ToDecimal(latitude);
                        decimal longitudeAsDec = Convert.ToDecimal(longitude);

                        //are these within the known limits of the selected country ?
                        GPSCountryManager countryManager= getQM().getGPSCountryManager();


                        if(countryManager.checkLimits(latitudeAsDec, longitudeAsDec)){

                            //OK
                            dataOK= true;

                        }
                        else{

                            //failed
                            dataOK= false;
                            string countryName= countryManager.getSelectedCountryName();
                            errorMessage="latitude or longitude are outside the known limits of the selected country (" + countryName + ")";


                        }
                        
                      

                    }
                    catch(FormatException e){

                        dataOK = false;
                        errorMessage = "latitude or longitude in unsupported format";

                    }


                }
                
                

            }

            

            //if data is OK, process the data if needed
            if (dataOK)
            {

                //process the data

                processedData = "latitude:" + latitude + ", longitude:" + longitude;

                nextCode = ToCode;

               

            }
            else
            {
                //validation has failed.

                ((Form2)getBigMessageBox()).setLabel(errorMessage);
                getBigMessageBox().ShowDialog();

                nextCode = Code;



            }

            return nextCode;



        }

        public void button_click2(object sender, EventArgs e)
        {
            //called when the user clicks the button to record the GPS

            //copy the values from the main form labels to the textBoxes
            textboxLat.Text = getQM().getLatitude();
            textboxLong.Text = getQM().getLongitude();


            //clear the skip control buttons
            clearControlButtons();


           



        }

        





        private bool testNullEntry(string userData)
        {
            //does this field contain an empty string
            if (userData == null)
            {
                return false;

            }
            else if (userData == "")
            {
                return false;

            }
            else
            {
                return true;

            }





        }

       

    }
}
