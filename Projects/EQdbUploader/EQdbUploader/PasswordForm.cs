
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

namespace EQdbUploader
{
    public partial class PasswordForm : Form
    {

        //form that calls this, so we can pass back data
        private Form1 callBack = null;
        
        
        public PasswordForm(Form1 callBack)
        {
            InitializeComponent();

            this.callBack = callBack;



        }

        private void button1_Click(object sender, EventArgs e)
        {

            processEntry();




        }

        private void processEntry()
        {
            string password = textBox1.Text;

            callBack.setPassword(password);

            Close();


        }

        private void PasswordForm_Load(object sender, EventArgs e)
        {

            //center the form
            Rectangle rect = Screen.PrimaryScreen.WorkingArea;
            //Divide the screen in half, and find the center of the form to center it
            this.Top = (rect.Height / 2) - (this.Height / 2);
            this.Left = (rect.Width / 2) - (this.Width / 2);




        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {

            //check for the return key-press, i.e. as a shortcut to pressing the OK button
            if (e.KeyCode == Keys.Return)
            {
                processEntry();


            }






        }
    }
}
