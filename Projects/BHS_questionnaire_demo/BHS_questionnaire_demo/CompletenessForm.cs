
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
    public partial class CompletenessForm : Form
    {

        //reference to parent form
        private BaseForm baseForm = null;
        private QuestionManager qm;
        private Form2 bigMessageBox;

        //panel location

        private int currentPanelXpos = 10;
        private int currentPanelYpos = 10;
        private int panelWidth = 950;
        private int panelHeight = 50;

        //is the form complete ?
        private bool isComplete = true;



        
        
        public CompletenessForm(bool lockingEnabled)
        {
            InitializeComponent();


            bigMessageBox = new Form2();

            //show the lock button only if locking is enabled

            if (!lockingEnabled)
            {

                button1.Visible = false;

            }





        }

        private void CompletenessForm_Load(object sender, EventArgs e)
        {

        }

        public Panel getNewPanel()
        {
            //create a new Panel for a section and return it
            Panel panel = new Panel();
            panel.Location = new Point(currentPanelXpos, currentPanelYpos);
            panel.Size = new Size(panelWidth, panelHeight);
            panel.BackColor = Color.FromArgb(0xF5, 0xDD, 0x9D);

            //add to the main panel
            panel1.Controls.Add(panel);

            //change the position for the next panel

            currentPanelYpos += (panelHeight + 2);

            return panel;






        }


        public void startCheck(string xmlFileName, string userDir, string userID, BaseForm baseForm)
        {
            //called after loading the form: passes in needed vars from baseform
            //open a file dialog box

            //application title
            Text = "Sections Complete for: " + userID;

            //reference the the parent form
            this.baseForm = baseForm;



            qm = new QuestionManager(userDir, userID, this);
            

            try
            {

                qm.ParseConfigXML(xmlFileName, this);



            }
            catch (Exception e)
            {
                //parsing error
                isComplete = false;

                bigMessageBox.setLabel("Error: There is an error in the XML configuration file");
                bigMessageBox.ShowDialog();

                MessageBox.Show(e.StackTrace);

                //exit form

                Close();


            }



            //load in previous data 
            

                try
                {

                    qm.load();

                }
                catch
                {
                    //probably a Questionnaire that was terminated on the start page, i.e. counts as existing in the parent form
                    //but has no data files
                    //startQuestionCode = "START";

                    bigMessageBox.setLabel("Error: Cannot load data: please delete this Questionnaire and start a new one");
                    bigMessageBox.ShowDialog();



                    //exit form

                    Close();

                    return;


                }


            //construct a mapping of sectionnames to lists of question-codes

                qm.mapSectionToQuestion();


            //for each section, get each question and find out if it is an exit question.
                isComplete= qm.testEachSectionForCompletion();

            //enable the lock button if form is complete
                if (isComplete)
                {

                    button1.Enabled = true;


                }
                else
                {

                    button1.Enabled = false;


                }



        }

        private void CompletenessForm_FormClosing(object sender, FormClosingEventArgs e)
        {

            //fires when the form is closing, but not yet closed

            //tell the baseForm that we are closing so it can let other forms open
            //baseForm.questionFormClosing();





        }

        

        private void button1_Click_1(object sender, EventArgs e)
        {
            //user wants to lock the questionnaire
            //call the method in the baseform

            baseForm.lockQuestionnaire();


        }

        private void button2_Click(object sender, EventArgs e)
        {
            //user pressed OK: exit
            Close();




        }






    }
}
