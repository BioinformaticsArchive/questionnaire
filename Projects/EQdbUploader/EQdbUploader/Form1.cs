
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
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.Security.Cryptography;




namespace EQdbUploader
{
    public partial class Form1 : Form
    {
        private DB db = null;

        //db settings
        private string mysqlHost = null;
        private string mysqlUser = null;
        private string mysqlPassword = null;
        private string mysqlDBName = null;

        private bool dbIsConnected = false;

        //Registry Key
        private string keyName = @"Software\EQdbUploader\Settings";

        //password entry box
        private PasswordForm passBox = null;


        
        
        
        public Form1()
        {
            InitializeComponent();

            passBox = new PasswordForm(this);




        }


        public void setPassword(string pw)
        {

            mysqlPassword = pw;



        }




        private void button1_Click(object sender, EventArgs e)
        {

            //load the data from a single file, chosen by the user

            //are we connected to the db ?
            if (! dbIsConnected)
            {

                MessageBox.Show("Not Connected to DataBase. Please connect via Settings first.");
                return;



            }

            

            string dataFileName;

            char[] splitOn= {'\t'};


            //get the file to open
            OpenFileDialog ofd = new OpenFileDialog();


            ofd.Title = "Please select the Data file to upload";



            if (ofd.ShowDialog() == DialogResult.OK)
            {

                dataFileName = ofd.FileName;


            }
            else
            {

                return;



            }


            loadFile(dataFileName);


            //show the current participants in the db
            updateParticipantList();


        }

        private void loadFile(string dataFileName)
        {

            //load the contents of a single file into the db
            
           

            char[] splitOn = { '\t' };

            StreamReader reader = null;

            try
            {



                //is this file of the correct type
                string partID;
                string qCode;
                string qData;

                string pattern = @"final_data_([^_]+)\.txt";

                Match match = Regex.Match(dataFileName, pattern);

                if (match.Success)
                {

                    partID = match.Groups[1].Value;



                }
                else
                {

                    //error: wrong file type

                    MessageBox.Show("This file is not of the correct type");
                    return;

                }



                //is this participant already present in the db?
                if (db.isPartLoaded(partID))
                {

                    //yes: abort load

                    MessageBox.Show("Aborting load for Participant:" + partID + " who is already loaded", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;


                }



                //read each line from the file


                reader = new StreamReader(dataFileName);

                while (reader.EndOfStream == false)
                {

                    string line = reader.ReadLine();

                    string[] items = line.Split(splitOn);

                    //first item is the qCode
                    //second item is the data for that qCode
                    qCode = items[0];
                    qData = items[1];

                    db.addParticipantData(partID, qCode, qData);


                }

               // MessageBox.Show("Data was loaded successfully");



            }
            catch (Exception ex1)
            {
                MessageBox.Show("Error:" + ex1.Message + " " + ex1.StackTrace);


            }

            finally
            {

                if (reader != null)
                {

                    reader.Close();

                }

            }





        }


       




        private void doConnect()
        {
            //try and connect to the db

            //check the db settings

            if (mysqlHost == null || mysqlUser == null || mysqlDBName == null)
            {

                //some connection params not set
                MessageBox.Show("Database is NOT connected: Please set the MySQL connection parameters");
                return;



            }

            //is the password set (won't be if the user has saved the data)
            if (mysqlPassword == null)
            {

                //ask the user to enter password

                passBox.ShowDialog();


            }

            if (mysqlPassword == null)
            {

                //user has not entered a password
                MessageBox.Show("No Password entered: cannot connect.");

            }


            else
            {

                //try and connect to the db
                string connStr = "SERVER=" + mysqlHost + ";DATABASE=" + mysqlDBName + ";UID=" + mysqlUser + ";PASSWORD=" + mysqlPassword + ";";


                db = new DB(connStr);

                //open connection

                try
                {
                    db.connect();

                    dbIsConnected = true;

                    //disable the connect button
                    button4.Enabled = false;

                    MessageBox.Show("Database Connection was successfull");




                }
                catch (Exception ex)
                {
                    //connection error

                    MessageBox.Show("There was a problem connecting to the database: " + ex.Message);
                    return;



                }






            }




        }





        private void Form1_Load(object sender, EventArgs e)
        {

            //form is loading

            //center the form
            Rectangle rect = Screen.PrimaryScreen.WorkingArea;
            //Divide the screen in half, and find the center of the form to center it
            this.Top = (rect.Height / 2) - (this.Height / 2);
            this.Left = (rect.Width / 2) - (this.Width / 2);


            //read settings from registry



            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(keyName);
            if (regKey != null)
            {

                //get values
                //hostname
                mysqlHost= (string)regKey.GetValue("HOST_NAME");
                textBox1.Text = mysqlHost;
                
                //dbname
                mysqlDBName= (string)regKey.GetValue("DB_NAME");
                textBox2.Text = mysqlDBName;
                
                //user-id
                mysqlUser= (string)regKey.GetValue("USER_NAME");
                textBox3.Text = mysqlUser;
                
                regKey.Close();


            }



            //connect to db with current settings
            doConnect();

            //show the current participants in the db
            updateParticipantList();

            

            
           

        }


        private void updateParticipantList()
        {

            //if we have a valid connection, show the current participants
            if (dbIsConnected)
            {

                listBox1.Items.Clear();
                
                List<string> dataList = db.showCurrentParticipants();

                foreach (string item in dataList)
                {

                    listBox1.Items.Add(item);

                }


            }
            else
            {
                MessageBox.Show("Error: database is not connected");



            }





        }






        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //form closing


            //disconnect from the db

            if (dbIsConnected)
            {
                db.close();


            }








        }

        private void button2_Click(object sender, EventArgs e)
        {

            //save settings in the system registry

            string hostname = textBox1.Text;
            string dbName = textBox2.Text;
            string userName = textBox3.Text;
            //string password = textBox4.Text;

           

            if ((hostname == null) || (dbName == null) || (userName == null) )
            {
                MessageBox.Show("Error: some of the connection parameters have not been set.");


            }
            else
            {
                RegistryKey regKey = Registry.CurrentUser.OpenSubKey(keyName, RegistryKeyPermissionCheck.ReadWriteSubTree);
                if (regKey == null)
                {
                    //not present: create the key
                    regKey = Registry.CurrentUser;
                    regKey = regKey.CreateSubKey(keyName);

                    

                    //save data in this key

                    regKey.SetValue("HOST_NAME", hostname);
                    regKey.SetValue("USER_NAME", userName);
                    regKey.SetValue("DB_NAME", dbName);
                    

                    regKey.Close();


                }
                else
                {
                    //save data in this key
                    regKey.SetValue("HOST_NAME", hostname);
                    regKey.SetValue("USER_NAME", userName);
                    regKey.SetValue("DB_NAME", dbName);
                    

                    regKey.Close();

                }




            }



        }

        private void button4_Click(object sender, EventArgs e)
        {

            //manual db connect

            mysqlHost = textBox1.Text;
            mysqlDBName = textBox2.Text;
            mysqlUser = textBox3.Text;
            mysqlPassword = textBox4.Text;

            doConnect();

            //show the current participants in the db
            updateParticipantList();






        }

        private void button3_Click(object sender, EventArgs e)
        {

            //load all the files in the folder selected by the user
            
            
            //are we connected to the db ?
            if (!dbIsConnected)
            {

                MessageBox.Show("Not Connected to DataBase. Please connect via Settings first.");
                return;



            }
            
            
            
            //dir where the data files are stored
            string dataDir = null;

            //load from directory.
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Select the folder which contains the data files to load";

            // Show the FolderBrowserDialog.
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                dataDir = folderBrowserDialog.SelectedPath;

                //get all the datafiles in this dir, that have the expected pattern

                string[] dataFiles = Directory.GetFiles(dataDir, @"*final_data_*.txt");

                //are there no files here ?

                if (dataFiles.Length == 0)
                {

                    MessageBox.Show("No files of the correct format were found");
                    return;



                }

                progressBar1.Minimum = 1;
                progressBar1.Maximum = dataFiles.Length;
                progressBar1.Value = 1;
                progressBar1.Step = 1;
                progressBar1.Visible = true;



                foreach (string fileName in dataFiles)
                {
                    loadFile(fileName);

                    // Perform the increment on the ProgressBar.
                    progressBar1.PerformStep();

                }

                //show the current participants in the db
                updateParticipantList();

                progressBar1.Visible = false;

                MessageBox.Show("Data was loaded successfully");




            }






        }

        private void button5_Click(object sender, EventArgs e)
        {

            //delete the selected participant
            string selectedPart = (string)listBox1.SelectedItem;

            //make sure someone was selected
            if (selectedPart == null)
            {

                MessageBox.Show("Please select a participant");
                return;


            }





            //make sure the user realy wants this
            
            var result = MessageBox.Show("Are you sure you want to delete this participant ?", "Delete ?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
               
                //delete

                try
                {
                    db.deleteParticipant(selectedPart);

                    updateParticipantList();



                }
                catch(Exception ex)
                {
                    MessageBox.Show("Delete failed:" + ex.Message);


                }
               


            }







        }
    }
}
