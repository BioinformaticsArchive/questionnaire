
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BHS_questionnaire_demo
{
    public partial class ConfirmForm : Form
    {

        private Form parentForm = null;

        
        
        
        public ConfirmForm()
        {
            InitializeComponent();
        }


        public void setFormLabel(string text, Form parentForm)
        {

            label1.Text = text;
            this.parentForm = parentForm;



        }

        private void button1_Click(object sender, EventArgs e)
        {
            //user has clicked the Yes button
            //send a message to the parent form.

            if (parentForm is BaseForm)
            {
                ((BaseForm)parentForm).confirmResult= "yes";



            }
            else if (parentForm is Form1)
            {

                ((Form1)parentForm).confirmResult = "yes";

            }
            else
            {

                throw new Exception();

            }


            this.Close();





        }

        private void button2_Click(object sender, EventArgs e)
        {
            //user has clicked the No button

            if (parentForm is BaseForm)
            {
                ((BaseForm)parentForm).confirmResult = "no";



            }
            else if (parentForm is Form1)
            {

                ((Form1)parentForm).confirmResult = "no";

            }
            else
            {

                throw new Exception();

            }


            this.Close();





        }

        private void ConfirmForm_Load(object sender, EventArgs e)
        {
            //center the form
            Rectangle rect = Screen.PrimaryScreen.WorkingArea;
            //Divide the screen in half, and find the center of the form to center it
            this.Top = (rect.Height / 3) - (this.Height / 2);
            this.Left = (rect.Width / 2) - (this.Width / 2);




        }





    }
}
