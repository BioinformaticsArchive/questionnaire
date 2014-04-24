
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



namespace BHS_questionnaire_demo
{
    class QuestionTextRadioButton : QuestionTextRadio
    {

        private string validationButtonCodeLabel;

        private Button button;



        //this is the same as a QuestionTextRadio, except we have a button to perform an extra check
        //constructor
        public QuestionTextRadioButton(Form form, Form bigMessageBox, GlobalStore gs, GlobalStore specialDataStore, QuestionManager qm)
            : base(form, bigMessageBox, gs, specialDataStore, qm)
        {



        }


        public void setValidationButton(string validation)
        {
            validationButtonCodeLabel = validation;

        }

        public override void removeControls()
        {

            //call base
            base.removeControls();

            //remove button
            getQM().getPanel().Controls.Remove(button);
            button.Dispose();




        }

        public override void configureControls(UserDirection direction)
        {
            //call the base
            base.configureControls(direction);

            //add the button
            button = new Button();
            button.Text = "Press to check barcode is correct";

            //set font size
            setFontSize(button);

            int labelXpos = getWidgetXpos();
            int labelYpos = getWidgetYpos();

            button.Location = new Point(labelXpos + 620, labelYpos + 80);
            button.Size = new Size(250, 50);
            button.BackColor = GlobalColours.mainButtonColour;
            button.Click += new EventHandler(button_click);


            getQM().getPanel().Controls.Add(button);





        }

        public void button_click(object sender, EventArgs e)
        {
            //called when the user clicks the button to check if a barcode is correct.
            if (validationButtonCodeLabel == "TestBloodSerum" || validationButtonCodeLabel == "TestBloodEDTA" || validationButtonCodeLabel == "TestBloodNAF")
            {
                //this is the barcode from the serum tube: we need to check that it matches the same group as the master lab barcode
                string typeSuffix;

                //get barcode from textbox
                string barcode = textbox.Text;

                bool barcodeOK= false;
                string errorMessage= null;



                if (validationButtonCodeLabel == "TestBloodSerum")
                {
                    typeSuffix = "S1";

                }
                else if (validationButtonCodeLabel == "TestBloodEDTA")
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

                   

                    errorMessage = "Warning: Can't compare with the master barcode, which was not entered";
                    barcodeOK = false;



                }
                else
                {

                    //e.g. master BGZ100
                    //the serum tube should be <master>S1

                    if (string.IsNullOrWhiteSpace(barcode))
                    {
                        //no entry

                        errorMessage = "Please scan a barcode";
                        barcodeOK = false;


                    }
                    else
                    {
                        //compare with master
                        string expectedBarcode = masterBarCode + typeSuffix;

                        if (expectedBarcode == barcode)
                        {
                            barcodeOK = true;

                        }
                        else
                        {
                            barcodeOK = false;
                            errorMessage = "You have entered the wrong barcode";


                        }



                    }

                }

                if (barcodeOK)
                {

                    Form3 warningBox = getQM().getWarningBox();

                    warningBox.setLabel("Barcode is Correct");
                    warningBox.ShowDialog();

                }
                else
                {

                    Form3 warningBox = getQM().getWarningBox();

                    warningBox.setLabel(errorMessage);
                    warningBox.ShowDialog();

                    //wipe the textbox contents
                    textbox.Text = "";
                    textbox.Focus();

                }


            }


        }
    }



}
