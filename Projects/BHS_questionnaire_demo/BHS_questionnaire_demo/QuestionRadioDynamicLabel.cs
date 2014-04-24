
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
    
    //same as a radio button, but with an extra function that will be used to generate the 
    //label, i.e. based on some other thing in the system

    class QuestionRadioDynamicLabel : QuestionRadio
    {

        public string LabelGeneratorFunc { get; set; }

         //constructor
        public QuestionRadioDynamicLabel(Form form, Form bigMessageBox, GlobalStore gs, GlobalStore specialDataStore, QuestionManager qm)
            : base(form, bigMessageBox, gs, specialDataStore, qm)
        {
           



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


            //label: this needs to be calcuated

            if (LabelGeneratorFunc == "DisplayBMI")
            {

                string label;

                //get the precalculated BMI
                string BMI = getSD().Get("BMI");

                if (BMI == null)
                {
                    label = "BMI is unavailable. Please select 'No', below";

                }
                else
                {
                    label = "BMI calculated as: " + BMI + ". Is this correct?";


                }

                radioGroup.Text = Val + " " + label;



            }
            else
            {
                throw new Exception();


            }



            



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





    }
}
