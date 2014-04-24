
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
    class QuestionLabel : Question
    {

        //fields
        private Label label;
        
        
        
        //constructor: pass the form to the base constructor
        public QuestionLabel(Form form, Form bigMessageBox, GlobalStore gs, GlobalStore specialDataStore, QuestionManager qm)
            : base(form, bigMessageBox, gs, specialDataStore, qm)
        {

        }

        public override void removeControls()
        {
            //getForm().Controls.Remove(label);
            getQM().getPanel().Controls.Remove(label);
            label.Dispose();



        }



        public override void configureControls(UserDirection direction)
        {
            
            //turn off the skip controls
            getQM().getMainForm().setSkipControlsInvisible();
            
            
            
            
            label = new Label();

            //set font size
            setFontSize(label);
            
            //the question Text shown to the user
            label.Text = Val;

            int labelXpos = getWidgetXpos();
            int labelYpos = getWidgetYpos();

           

            //position of the Label
            label.Location = new Point(labelXpos, labelYpos);
            label.Size = new Size(getWidgetWidth(), getWidgetHeight());
            label.ForeColor = GlobalColours.mainTextColour;

            //getForm().Controls.Add(label);

            getQM().getPanel().Controls.Add(label);


          


        }

        public override string processUserData()
        {

            
            
            
            //there is no user data for this control
            return ToCode;



        }

        public override void load(Dictionary<string, string> pDataDict, Dictionary<string, string> uDataDict)
        {
            //do nothing

        }



        public override void save(TextWriter dhProcessedData, TextWriter dhUserData)
        {

           //do nothing

        }






    }
}
