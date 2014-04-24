
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
using System.Xml;
using System.Xml.XPath;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Resources;





namespace BHS_questionnaire_demo
{
    class QuestionManager
    {
        //class that contains a hash of questions

        //fields

        //hash of Question objects, where the key is the code
        private Dictionary<string, Question> questionHash;

        //the question that we are currently showing
        private Question currentQuestion;

        //the code for the next question to process
        private string nextCode;

        private Form bigMessageBox;

        //stack to keep track of which questions have been visited
        //the stack contains the question code
        private Stack<string> qStack;


        //global-store: a data repository that all questions can access
        private GlobalStore gs;

        //special questions: for questions that generate more than 1 answer: that can be stored here
        private GlobalStore specialDataStore;

        private Panel mainPanel;

        private Label sectionLabel;

        private Form3 warningMessageBox;
        private AdviceForm adviceBox;

        private ConfirmForm confirmBox;

        private Form1 mainForm;

        private CompletenessForm cf;



        private string fileNameProcessedData = null;
        private string fileNameUserData = null;
        private string fileNameStack = null;
        private string fileNameGlobalStore = null;
        private string fileNameSpecialStore = null;
        private string fileNamePageSeen = null;

        //final data output
        private string fileNameFinalData = null;

        private Label latitudeLabel;
        private Label longitudeLabel;

        //if this is set to a specific country, limit possible longitude/latitude to that country, i.e. anything else is an error.
        private string gpsCountry;

        //used to check allowed limits of gps readings
        private GPSCountryManager countryManager= null;

        //map of section name to list of question-codes
        private Dictionary<string, List<string>> sectionToCodeList = null;

        //map of question code to section name
        private Dictionary<string, string> questionCodeToSection = null;



        //userID

        private string userID;


        //userDir: where to store files
        private string userDir;


        //can we skip this question ?
        public bool SkipThisQuestion { get; set; }


        //constructor
        public QuestionManager(Form bigMessageBox, string userDir, Panel mainPanel, Label sectionLabel, string userID, Label latitudeLabel, Label longitudeLabel, AdviceForm adviceBox, ConfirmForm confirmBox, Form1 mainForm, string gpsCountry)
        {

            this.adviceBox = adviceBox;
            this.confirmBox = confirmBox;
            this.mainPanel = mainPanel;
            this.sectionLabel = sectionLabel;
            this.userID = userID;
            this.mainForm = mainForm;
            this.gpsCountry = gpsCountry;

            if (gpsCountry != null)
            {
                countryManager = new GPSCountryManager(gpsCountry);

            }
            

            
            questionHash = new Dictionary<string, Question>();

            this.bigMessageBox = bigMessageBox;

            qStack = new Stack<string>();

            gs = new GlobalStore();

            specialDataStore = new GlobalStore();


            //userID = "testUser12345";
            

            this.userDir = userDir;


            //set up the filenames for data saving

            this.fileNameProcessedData = userDir + @"\state_data_processed" + ".txt";
            this.fileNameUserData = userDir + @"\state_data_userdata" + ".txt";
            this.fileNameStack = userDir + @"\state_data_stack" + ".txt";
            this.fileNameGlobalStore = userDir + @"\state_data_global" + ".txt";
            this.fileNameSpecialStore = userDir + @"\state_data_special" + ".txt";
            this.fileNamePageSeen = userDir + @"\state_data_page_seen" + ".txt";

            this.fileNameFinalData = userDir + @"\final_data_" + userID + ".txt";

            //labels which contain GPS data
            this.latitudeLabel = latitudeLabel;
            this.longitudeLabel = longitudeLabel;







        }

        //constructor used by CompletenessForm
        public QuestionManager(string userDir, string userID, CompletenessForm cf)
        {

            
            this.userID = userID;

            this.cf = cf;


            questionHash = new Dictionary<string, Question>();

            

            qStack = new Stack<string>();

            gs = new GlobalStore();

            specialDataStore = new GlobalStore();


            this.userDir = userDir;


            //set up the filenames for data saving

            this.fileNameProcessedData = userDir + @"\state_data_processed" + ".txt";
            this.fileNameUserData = userDir + @"\state_data_userdata" + ".txt";
            this.fileNameStack = userDir + @"\state_data_stack" + ".txt";
            this.fileNameGlobalStore = userDir + @"\state_data_global" + ".txt";
            this.fileNameSpecialStore = userDir + @"\state_data_special" + ".txt";
            this.fileNamePageSeen = userDir + @"\state_data_page_seen" + ".txt";

            this.fileNameFinalData = userDir + @"\final_data_" + userID + ".txt";


        }

        //methods

        public void showDebug()
        {

            String all = "";
            
            
            //get each object in the hash
            foreach (KeyValuePair<string, Question> kv in questionHash)
            {

                string key = kv.Key;
                Question val = kv.Value;

                all += (key + "\n");

                /*

                if (val is QuestionText)
                {
                    string show = key + ":" + ((QuestionText)val).LabelToBoxGap + "\n";

                    all += show;




                }
                 */



            }

            MessageBox.Show(all);




        }

        public void mapSectionToQuestion()
        {

            //map each section-name to a list of question-codes
            sectionToCodeList = new Dictionary<string, List<string>>();

            questionCodeToSection = new Dictionary<string, string>();


            string sectionName;
            string questionCode;
            Question thisQuestion;
            List<string> qList;

            //get each Question from the hash
            foreach (KeyValuePair<string, Question> kv in questionHash){

                //get the section-name
                questionCode = kv.Key;
                thisQuestion = kv.Value;

                sectionName = thisQuestion.Section;

                //map question to section
                questionCodeToSection[questionCode] = sectionName;

                //get the list of questions-codes for that section
                //qList = sectionToCodeList[sectionName];

                if (sectionToCodeList.TryGetValue(sectionName, out qList))
                {
                    //there is a list already -> add the code
                    qList.Add(questionCode);

                }
                else
                {
                    //not a list already: make one.
                    qList = new List<string>();
                    qList.Add(questionCode);
                    sectionToCodeList[sectionName] = qList;



                }





            }





        }


        private void showInPanel(bool sectionComplete, string sectionName, string message)
        {

            //show message in a panel on the completenessform
            //get a new panel to use

            Panel panel = cf.getNewPanel();

            Label label = new Label();

            if (message != null)
            {
                label.Text = sectionName + " (" + message + ")";

            }
            else
            {
                label.Text = sectionName;

            }


            

            label.Location = new Point(0, 10);
            label.Size = new Size(700, 50);
            setFontSize(label);


            //draw an icon to show a tick or cross, i.e. if sectionComplete is true draw a tick otherwise draw a cross
            //capture the paint event of the panel.
            PictureBox pb = new PictureBox();
            pb.Location = new Point(900, 0);
            pb.Size = new Size(48, 48);

            if (sectionComplete)
            {
                pb.Image = BHS_questionnaire_demo.Properties.Resources.onebit_34;

            }
            else
            {
                pb.Image = BHS_questionnaire_demo.Properties.Resources.onebit_33;

            }
            
            


            panel.Controls.Add(label);
            panel.Controls.Add(pb);





        }

        private void setFontSize(params Control[] controls)
        {

            //varargs function, takes any number of Controls

            float fontSize = 18;

            foreach (Control control in controls)
            {
                control.Font = new Font(control.Font.Name, fontSize, control.Font.Style, control.Font.Unit);

            }





        }





        public bool testEachSectionForCompletion()
        {
            //is each section complete or not?

            //whole form complete ?
            bool isComplete = true;

            //did the user select an abort (i.e. a consent which effectively terminates the form)?
            bool abort = false;

            string abortMessage = "consent denied";


            //each section:
            List<string> sectionList = Utils.getSectionList();

            foreach (string section in sectionList)
            {
                //test section for completeness
                


                if (abort && (section != "Final Comments"))
                {

                    //mark all sections are complete with abort message
                    showInPanel(true, section, abortMessage);



                }


                //final section is treated differently
                else if (section == "Final Comments")
                {
                    //finished if the question 'SPINE' has been answered
                    //Question spineQuestion = questionHash["SPINE"];

                    Question iCommentsQuestion = questionHash["INTERVIEWERCOMMENTS"];

                   // if (spineQuestion.processedData != null)

                    if(iCommentsQuestion.PageSeen)

                    {

                        //MessageBox.Show("section " + section + " is complete ");
                        showInPanel(true, section, null);
                        


                    }
                    else
                    {
                        showInPanel(false, section, null);

                        isComplete = false;


                    }
                    


                }


                    //advice treated differently
                else if (section == "Advice")
                {

                    //complete if ADVICE_BMI  was answered
                    Question adviceBMIQuestion = questionHash["ADVICE_BMI"];

                    if (adviceBMIQuestion.processedData != null)
                    {

                        //MessageBox.Show("section " + section + " is complete ");
                        showInPanel(true, section, null);
                        


                    }
                    else
                    {
                        showInPanel(false, section, null);

                        isComplete = false;


                    }





                }

                else if (section == "Consents")
                {
                    //are any of these fatal, i.e. does the participant want to abort the survey ?

                    List<string> qCodesThisSection = sectionToCodeList[section];

                    bool sectionComplete = false;

                    foreach (string qCode in qCodesThisSection)
                    {

                        //is this question a valid exit question ?
                        Question thisQuestion = questionHash[qCode];

                        if (thisQuestion.isFormExit())
                        {
                            //form is complete: user has aborted
                            abort = true;

                            sectionComplete = true;

                            showInPanel(true, section, abortMessage);

                            break;

                           

                        }
                        else if (thisQuestion.isSectionExit(questionCodeToSection))
                        {
                            //section is complete
                            //MessageBox.Show("section " + section + " is complete with exit at " + qCode);
                            showInPanel(true, section, null);

                            sectionComplete = true;

                            //skip any remaining questions in this section
                            break;




                        }


                    }

                    if (!sectionComplete)
                    {
                        
                        //there is a problem with declining blood samples, i.e. it will skip the last few questions,
                        //the last question in the section in this case is CON13
                        

                        string bloodConsent = gs.Get("CON5");

                        if ((bloodConsent == null) || (bloodConsent == "1"))
                        {
                            showInPanel(false, section, null);

                            isComplete = false;


                        }
                        else
                        {
                            //blood consent declined
                            //are we at the end of the section given that blood consent was declined ?
                            Question con13 = questionHash["CON13"];

                            if (con13.processedData == null)
                            {

                                //not at the end
                                showInPanel(false, section, null);

                                isComplete = false;


                            }
                            else
                            {
                                //yes, at the end

                                showInPanel(true, section, null);

                                


                            }



                        }
                        
                        
                        
                        
                        
                        

                    }





                }

                else if (section == "Blood Sample")
                {
                    List<string> qCodesThisSection = sectionToCodeList[section];

                    bool sectionComplete = false;

                    foreach (string qCode in qCodesThisSection)
                    {

                        //is this question a valid exit question ?
                        Question thisQuestion = questionHash[qCode];

                        if (thisQuestion.isSectionExit(questionCodeToSection))
                        {
                            //section is complete
                            //MessageBox.Show("section " + section + " is complete with exit at " + qCode);
                            showInPanel(true, section, null);

                            sectionComplete = true;


                            //skip any remaining questions in this section
                            break;




                        }


                    }
                    if (!sectionComplete)
                    {



                        //need to check if blood samples were consented to.
                        string bloodConsent = gs.Get("CON5");

                        if (bloodConsent == null)
                        {
                            //consent 5 has not been set
                            //MessageBox.Show("section " + section + " is NOT complete");
                            showInPanel(false, section, null);
                            isComplete = false;

                        }
                        else if (bloodConsent == "1")
                        {
                            //they DID consent to bloods
                            //MessageBox.Show("section " + section + " is NOT complete");
                            showInPanel(false, section, null);
                            isComplete = false;

                        }
                        else if (bloodConsent == "2")
                        {
                            //they did NOT consent to bloods
                            //MessageBox.Show("section " + section + " is complete (No Consent for Blood samples)");
                            showInPanel(true, section, "No Consent for Blood samples");

                        }
                        else
                        {
                            throw new Exception();

                        }



                    }
                }

                    //all other sections:
                else
                {

                    List<string> qCodesThisSection = sectionToCodeList[section];

                    bool sectionComplete = false;

                    foreach (string qCode in qCodesThisSection)
                    {

                        //is this question a valid exit question ?
                        Question thisQuestion = questionHash[qCode];

                        if (thisQuestion.isSectionExit(questionCodeToSection))
                        {
                            
                            //a special case: in Demographics, its possible to skip to GPS, ie. bypass the first few Qs, then complete
                            //the section, so we need to test that HSA (Q before GPS) was also anwered
                            if ((section == "Demographic Information") && (questionHash["HSA"].processedData == null))
                            {
                                sectionComplete = false;
                                break;


                            }
                            else
                            {

                                //section is complete
                                //MessageBox.Show("section " + section + " is complete with exit at " + qCode);
                                showInPanel(true, section, null);

                                sectionComplete = true;


                                //skip any remaining questions in this section
                                break;



                            }
                            
                            

                        }


                    }

                    if (! sectionComplete)
                    {
                        showInPanel(false, section, null);

                        isComplete = false;

                    }


                }



            }

            //return true if the form is complete else false
            return isComplete;


        }




        public GPSCountryManager getGPSCountryManager(){

            return countryManager;

        }


        public string getLatitude()
        {
            return latitudeLabel.Text;



        }

        public string getLongitude()
        {

            return longitudeLabel.Text;

        }


        public AdviceForm getAdviceBox()
        {

            return adviceBox;

        }

        public ConfirmForm getConfirmBox()
        {
            return confirmBox;

        }

        public Form1 getMainForm()
        {
            return mainForm;

        }


        public string getUserID()
        {
            return userID;


        }

        public void setWarningBox(Form3 warningMessageBox)
        {
            this.warningMessageBox = warningMessageBox;




        }

        public Form3 getWarningBox()
        {
            return warningMessageBox;


        }

        public Label getSectionLabel()
        {

            return sectionLabel;

        }


        public Panel getPanel()
        {

            return mainPanel;


        }

        public void save()
        {
            //save all the current data to a file
            //open a file:

            //open data files
            System.IO.TextWriter dhProcessedData = null;
            System.IO.TextWriter dhUserData = null;
            System.IO.TextWriter dhStack = null;
            System.IO.TextWriter dhGlobalStore = null;
            System.IO.TextWriter dhSpecialStore = null;

            System.IO.TextWriter dhPageSeen = null;

            System.IO.TextWriter dhFinalData = null;


            try
            {

                dhProcessedData = new System.IO.StreamWriter(fileNameProcessedData);
                dhUserData = new System.IO.StreamWriter(fileNameUserData);
                dhStack = new System.IO.StreamWriter(fileNameStack);
                dhGlobalStore = new System.IO.StreamWriter(fileNameGlobalStore);
                dhSpecialStore = new System.IO.StreamWriter(fileNameSpecialStore);

                dhPageSeen = new System.IO.StreamWriter(fileNamePageSeen);

                dhFinalData = new System.IO.StreamWriter(fileNameFinalData);



                //ask each question object to write its data to the files for userdata and processed-data

                foreach (KeyValuePair<string, Question> kvp in questionHash)
                {


                    Question thisQuestion = kvp.Value;
                    thisQuestion.save(dhProcessedData, dhUserData);

                    //page seen
                    thisQuestion.savePageSeen(dhPageSeen);

                    //final data
                    thisQuestion.saveFinalData(dhFinalData);






                }

                //write the items in the special data store
                specialDataStore.save(dhSpecialStore);
                //also save to final data
                specialDataStore.save(dhFinalData);


                //write the items in the global store
                gs.save(dhGlobalStore);

                //write the items in the Question Stack
                List<string> stackData = qStack.ToList();

                foreach (string item in stackData)
                {
                    dhStack.WriteLine(item);


                }


                


            }
            finally
            {

                if (dhProcessedData != null)
                {

                    dhProcessedData.Close();

                }

                if (dhUserData != null)
                {

                    dhUserData.Close();
                }

                if (dhStack != null)
                {
                    dhStack.Close();

                }

                if (dhGlobalStore != null)
                {
                    dhGlobalStore.Close();

                }

                if (dhSpecialStore != null)
                {

                    dhSpecialStore.Close();

                }

                if (dhPageSeen != null)
                {

                    dhPageSeen.Close();

                }

                if (dhFinalData != null)
                {
                    dhFinalData.Close();

                }



            }

            





        }

        public void load()
        {
            //load in previous data from files
            //open a file:

            //open data files

            StreamReader dhProcessedData = null;
            StreamReader dhUserData = null;
            StreamReader dhStack = null;
            StreamReader dhGlobalStore = null;
            StreamReader dhSpecialStore = null;
            StreamReader dhPageSeen = null;

            try
            {

                dhProcessedData = new StreamReader(fileNameProcessedData);
                dhUserData = new StreamReader(fileNameUserData);
                dhStack = new StreamReader(fileNameStack);
                dhGlobalStore = new StreamReader(fileNameGlobalStore);
                dhSpecialStore = new StreamReader(fileNameSpecialStore);
                dhPageSeen = new StreamReader(fileNamePageSeen);


                //read the processed data into a Dictionary
                Dictionary<string, string> pDataDict = new Dictionary<string, string>();
                Char[] delim = new Char[] { '\t' };

                while (dhProcessedData.EndOfStream == false)
                {
                    string line = dhProcessedData.ReadLine();

                    //using tab as delim
                    string[] parts = line.Split(delim);

                    pDataDict[parts[0]] = parts[1];



                }




                //read the user data into a dictionary

                Dictionary<string, string> uDataDict = new Dictionary<string, string>();


                while (dhUserData.EndOfStream == false)
                {
                    string line = dhUserData.ReadLine();

                    //using tab as delim
                    string[] parts = line.Split(delim);

                    int arrLength = parts.Length;

                    //the data itself may be in several parts.
                    if (arrLength == 2)
                    {
                        //normal case
                        uDataDict[parts[0]] = parts[1];


                    }
                    else
                    {
                        //when the data is split into multiple parts

                        //create an array without the first element of parts
                        string[] subList = new string[arrLength - 1];

                        for (int i = 1; i < arrLength; i++)
                        {

                            subList[i - 1] = parts[i];

                        }

                        string partsJoined = string.Join("\t", subList);

                        uDataDict[parts[0]] = partsJoined;


                    }


                }


                //load the seen pages
                HashSet<string> seenPages = new HashSet<string>();

                while (dhPageSeen.EndOfStream == false)
                {
                    string line = dhPageSeen.ReadLine();


                    //add to the set of seen pages
                    seenPages.Add(line);




                }



                //read the userdata and processed data back 

                foreach (KeyValuePair<string, Question> kvp in questionHash)
                {


                    Question thisQuestion = kvp.Value;
                    thisQuestion.load(pDataDict, uDataDict);
                    thisQuestion.loadPageSeen(seenPages);


                }

                //load the items in the special data store
                specialDataStore.load(dhSpecialStore);



                //load the items in the global store
                gs.load(dhGlobalStore);



                //read the stack data from the file and push onto the stack

                //create a temp list to get the contents of the file
                List<string> fromFileToStack = new List<string>();

                while (dhStack.EndOfStream == false)
                {
                    string line = dhStack.ReadLine();
                    fromFileToStack.Add(line);



                }

                //push list onto the stack in reverse order
                fromFileToStack.Reverse();

                foreach (string code in fromFileToStack)
                {

                    qStack.Push(code);
                }



            }
            finally
            {

                //close the files
                if (dhProcessedData != null)
                {

                    dhProcessedData.Close();

                }

                if (dhUserData != null)
                {

                    dhUserData.Close();
                }

                if (dhStack != null)
                {
                    dhStack.Close();

                }

                if (dhGlobalStore != null)
                {
                    dhGlobalStore.Close();

                }

                if (dhSpecialStore != null)
                {

                    dhSpecialStore.Close();

                }

                if (dhPageSeen != null)
                {

                    dhPageSeen.Close();

                }
                
                

            }

            



        }



        public bool isLastPage()
        {

            //is this the last page of the questionnaire ?

            if (currentQuestion.Code == "EXIT")
            {
                return true;

            }
            else
            {
                return false;

            }



        }

        public bool isFirstPage()
        {

            //is this the first page of the questionnaire ?

            if (currentQuestion.Code == "START")
            {
                return true;

            }
            else
            {
                return false;

            }



        }






        public void processUserData()
        {

            //process the data entered by the user and fetch the next code to process
            nextCode= currentQuestion.processUserData();


        }

        public void removeControls()
        {
            //delete form controls for the current question
            currentQuestion.removeControls();

        }


        public void configureControls(UserDirection direction)
        {
            
             
            
            //is this an automatic question, i.e. no UI ?
            if (currentQuestion is QuestionAutomatic)
            {
                //automatic Question: no controls to configure

                

                //fetch the next Question or the previous question
                if (direction == UserDirection.forward)
                {
                    //process
                    processUserData();

                    save();

                    setNextQuestion();

                }
                else
                {
                    setPreviousQuestion();

                }

                

                //recursive call
                configureControls(direction);





            }
            else if(! currentQuestion.IfSettingsOK()){

                //skip this question as permission was not granted to show it, e.g. the user denied permission
                //fetch the next Question or the previous question
                if (direction == UserDirection.forward)
                {

                    //get the default next code from the current question
                    nextCode = currentQuestion.ToCode;
                    
                    setNextQuestion();

                }
                else
                {
                    setPreviousQuestion();

                }



                //recursive call
                configureControls(direction);



            }

            else
            {
                //normal Question

                //configure the controls for the current Question
                currentQuestion.showSection();


                currentQuestion.configureControls(direction);



            }
            
            
            
            
            

        }

        public string getCodeAtTopOfStack()
        {

            //return the top question on the stack
            return qStack.Pop();



        }




        public void setCurrentQuestion(string code)
        {
            //set the question pointer to the question object for this code
            //it is possible that the code is not valid, so check.
            currentQuestion = questionHash[code];

            //add the code to the question stack
            qStack.Push(code);




        }

        public void setNextQuestion()
        {
            //save a ref to the current question
            Question tempQuestion = currentQuestion;
            
            
            //advance the question pointer to the next question in the survey
            try
            {
                currentQuestion = questionHash[nextCode];

            }
            catch{

                MessageBox.Show("error fetching qCode:" + nextCode);
                showDebug();



            }
            
            

            //push the new Question onto the stack ONLY if it is not the same as the previous question
            if (currentQuestion != tempQuestion)
            {
                //add the code to the question stack
                qStack.Push(nextCode);


            }

            

        }

        public void setNextQuestion(string qCode)
        {
            //used to jump to a specific section

            //advance the question pointer to the next question in the survey
            currentQuestion = questionHash[qCode];

            //add the code to the question stack
            qStack.Push(qCode);



        }

        public void setPreviousQuestion()
        {
            //pop the current question off the stack
            qStack.Pop();

            //peek at the new top of the stack to see what the previous code is
            currentQuestion = questionHash[qStack.Peek()];


        }


        

        public Question getQuestion(string qCode)
        {
            //get the Question object with this code.

            return questionHash[qCode];



        }

        //parse the XML config file
        public void ParseConfigXML(string configFileName, Form form)
        {
            XPathNavigator nav;
            XPathDocument docNav;
            XPathNodeIterator textBoxItr;
            XPathNodeIterator radioItr;
            XPathNodeIterator optionItr;
            XPathNodeIterator labelItr;
            XPathNodeIterator dateItr;
            XPathNodeIterator textRadioItr;
            XPathNodeIterator fromCodeNode;

            XPathNavigator gen;





            string thisCode;
            string thisQuestionText;
            string widgetPosWidth;
            string widgetPosHeight;
            string widgetPosX;
            string widgetPosY;
            string toCode;


            string validation;
            string process;
            string section;

            string setKey;
            string optionValue;
            string optionText;
            string optionToCode;
            string optionToCodeErr;
            string fromCode;

            QuestionText qt;
            QuestionRadio qr;
            QuestionLabel ql;
            QuestionDatePickerSelect qdp;
            QuestionTextRadio qtr;
            QuestionTextOneCheck qtc;
            QuestionTextDouble qtd;
            QuestionGetTime qgt;
            QuestionTextTriple qtt;
            QuestionAutomatic qauto;
            QuestionGPS qgps;
            QuestionTextDuplicate qtdup;
            QuestionTextRadioButton qtrb;
            QuestionSelect qs;
            QuestionYear qy;
            QuestionRadioDynamicLabel qrd;
            QuestionTextDate qtdt;





            Option op;






            // Open the XML.
            docNav = new XPathDocument(configFileName);




            // Create a navigator to query with XPath.
            nav = docNav.CreateNavigator();

            //select datepicker nodes
            dateItr = nav.Select("//QuestionDate");

            while (dateItr.MoveNext())
            {

                qdp = new QuestionDatePickerSelect(form, bigMessageBox, gs, specialDataStore, this);

                //get the code from the current node
                thisCode = dateItr.Current.SelectSingleNode("@code").Value;
                qdp.Code = thisCode;


                //get the text of the question
                thisQuestionText = dateItr.Current.SelectSingleNode("@val").Value;
                qdp.Val = thisQuestionText;

                //get the child node: WidgetPos
                widgetPosWidth = dateItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                widgetPosHeight = dateItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                widgetPosX = dateItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                widgetPosY = dateItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;

                qdp.setWidgetWidth(widgetPosWidth);
                qdp.setWidgetHeight(widgetPosHeight);
                qdp.setWidgetXpos(widgetPosX);
                qdp.setWidgetYpos(widgetPosY);

                //FromCode: optional
                gen = dateItr.Current.SelectSingleNode("child::FromCode");
                if (gen != null)
                {
                    fromCode = gen.Value;

                }
                else
                {
                    fromCode = null;
                }

                qdp.FromCode = fromCode;


                //ToCode
                toCode = dateItr.Current.SelectSingleNode("child::ToCode").Value;
                qdp.ToCode = toCode;

                setKey = dateItr.Current.SelectSingleNode("child::SetKey").Value;
                qdp.SetKey = setKey;

                //section
                section = dateItr.Current.SelectSingleNode("child::Section").Value;
                qdp.Section = section;

                XPathNavigator qCompare = dateItr.Current.SelectSingleNode("child::OnErrorQuestionCompare");
                if (qCompare != null)
                {
                    qdp.OnErrorQuestionCompare = qCompare.Value;

                }

                //validation
                XPathNavigator validationNode = dateItr.Current.SelectSingleNode("child::Validation");
                if (validationNode != null)
                {
                    qdp.setValidation(validationNode.Value);

                }


                questionHash.Add(thisCode, qdp);


            }


            dateItr = nav.Select("//QuestionYear");

            while (dateItr.MoveNext())
            {

                qy = new QuestionYear(form, bigMessageBox, gs, specialDataStore, this);

                //get the code from the current node
                thisCode = dateItr.Current.SelectSingleNode("@code").Value;
                qy.Code = thisCode;


                //get the text of the question
                thisQuestionText = dateItr.Current.SelectSingleNode("@val").Value;
                qy.Val = thisQuestionText;

                //get the child node: WidgetPos
                widgetPosWidth = dateItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                widgetPosHeight = dateItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                widgetPosX = dateItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                widgetPosY = dateItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;

                qy.setWidgetWidth(widgetPosWidth);
                qy.setWidgetHeight(widgetPosHeight);
                qy.setWidgetXpos(widgetPosX);
                qy.setWidgetYpos(widgetPosY);

                //FromCode: optional
                gen = dateItr.Current.SelectSingleNode("child::FromCode");
                if (gen != null)
                {
                    fromCode = gen.Value;

                }
                else
                {
                    fromCode = null;
                }

                qy.FromCode = fromCode;


                //ToCode
                toCode = dateItr.Current.SelectSingleNode("child::ToCode").Value;
                qy.ToCode = toCode;

                setKey = dateItr.Current.SelectSingleNode("child::SetKey").Value;
                qy.SetKey = setKey;

                //section
                section = dateItr.Current.SelectSingleNode("child::Section").Value;
                qy.Section = section;

                XPathNavigator qCompare = dateItr.Current.SelectSingleNode("child::OnErrorQuestionCompare");
                if (qCompare != null)
                {
                    qy.OnErrorQuestionCompare = qCompare.Value;

                }

                //validation
                XPathNavigator validationNode = dateItr.Current.SelectSingleNode("child::Validation");
                if (validationNode != null)
                {
                    qy.setValidation(validationNode.Value);

                }


                questionHash.Add(thisCode, qy);


            }




            // Select the TextBox nodes
            textBoxItr = nav.Select("//QuestionTextDuplicate");


            while (textBoxItr.MoveNext())
            {

                //a new QuestionText object
                qtdup = new QuestionTextDuplicate(form, bigMessageBox, gs, specialDataStore, this);



                //get the code from the current node
                thisCode = textBoxItr.Current.SelectSingleNode("@code").Value;
                qtdup.Code = thisCode;


                //get the text of the question
                thisQuestionText = textBoxItr.Current.SelectSingleNode("@val").Value;
                qtdup.Val = thisQuestionText;

                //get the child node: WidgetPos
                widgetPosWidth = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                widgetPosHeight = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                widgetPosX = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                widgetPosY = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;

                qtdup.setWidgetWidth(widgetPosWidth);
                qtdup.setWidgetHeight(widgetPosHeight);
                qtdup.setWidgetXpos(widgetPosX);
                qtdup.setWidgetYpos(widgetPosY);


                //ToCode
                toCode = textBoxItr.Current.SelectSingleNode("child::ToCode").Value;
                qtdup.ToCode = toCode;

                //FromCode: optional
                gen = textBoxItr.Current.SelectSingleNode("child::FromCode");
                if (gen != null)
                {
                    fromCode = gen.Value;

                }
                else
                {
                    fromCode = null;
                }

                qtdup.FromCode = fromCode;





                XPathNavigator qCompare = textBoxItr.Current.SelectSingleNode("child::OnErrorQuestionCompare");
                if (qCompare != null)
                {
                    qtdup.OnErrorQuestionCompare = qCompare.Value;

                }

                XPathNavigator qSetKey = textBoxItr.Current.SelectSingleNode("child::SetKey");
                if (qSetKey != null)
                {
                    qtdup.SetKey = qSetKey.Value;

                }




                //validation
                validation = textBoxItr.Current.SelectSingleNode("child::Validation").Value;
                qtdup.setValidation(validation);

                //process
                process = textBoxItr.Current.SelectSingleNode("child::Process").Value;
                qtdup.setProcess(process);

                //section
                section = textBoxItr.Current.SelectSingleNode("child::Section").Value;
                qtdup.Section = section;



                //ifSetting

                XPathNavigator ifSettingKeyNode = textBoxItr.Current.SelectSingleNode("child::IfSetting/@key");
                if (ifSettingKeyNode != null)
                {
                    qtdup.IfSettingKey = ifSettingKeyNode.Value;

                }
                else
                {
                    qtdup.IfSettingKey = null;

                }

                XPathNavigator ifSettingValNode = textBoxItr.Current.SelectSingleNode("child::IfSetting/@val");
                if (ifSettingValNode != null)
                {
                    qtdup.IfSettingVal = ifSettingValNode.Value;

                }
                else
                {
                    qtdup.IfSettingVal = null;

                }




                //save the object in the hash
                questionHash.Add(thisCode, qtdup);



            }



            // Select the TextBox nodes
            textBoxItr = nav.Select("//QuestionTextBox");


            while (textBoxItr.MoveNext())
            {

                //a new QuestionText object
                qt = new QuestionText(form, bigMessageBox, gs, specialDataStore, this);



                //get the code from the current node
                thisCode = textBoxItr.Current.SelectSingleNode("@code").Value;
                qt.Code = thisCode;


                //get the text of the question
                thisQuestionText = textBoxItr.Current.SelectSingleNode("@val").Value;
                qt.Val = thisQuestionText;

                //get the child node: WidgetPos
                widgetPosWidth = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                widgetPosHeight = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                widgetPosX = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                widgetPosY = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;

                qt.setWidgetWidth(widgetPosWidth);
                qt.setWidgetHeight(widgetPosHeight);
                qt.setWidgetXpos(widgetPosX);
                qt.setWidgetYpos(widgetPosY);


                //ToCode
                toCode = textBoxItr.Current.SelectSingleNode("child::ToCode").Value;
                qt.ToCode = toCode;

                //FromCode: optional
                gen = textBoxItr.Current.SelectSingleNode("child::FromCode");
                if (gen != null)
                {
                    fromCode = gen.Value;

                }
                else
                {
                    fromCode = null;
                }

                qt.FromCode = fromCode;



                //onError
                //onError = textBoxItr.Current.SelectSingleNode("child::OnError").Value;
                //qt.OnError = onError;



                XPathNavigator qCompare = textBoxItr.Current.SelectSingleNode("child::OnErrorQuestionCompare");
                if (qCompare != null)
                {
                    qt.OnErrorQuestionCompare = qCompare.Value;

                }

                XPathNavigator qSetKey = textBoxItr.Current.SelectSingleNode("child::SetKey");
                if (qSetKey != null)
                {
                    qt.SetKey = qSetKey.Value;

                }




                //validation
                validation = textBoxItr.Current.SelectSingleNode("child::Validation").Value;
                qt.setValidation(validation);

                //process
                process = textBoxItr.Current.SelectSingleNode("child::Process").Value;
                qt.setProcess(process);

                //section
                section = textBoxItr.Current.SelectSingleNode("child::Section").Value;
                qt.Section = section;


                //multiline (i.e. like a textarea)
                XPathNavigator multiLineNode = textBoxItr.Current.SelectSingleNode("child::MultiLine");
                if (multiLineNode != null)
                {
                    qt.HasTextArea = true;

                }
                else
                {
                    qt.HasTextArea = false;

                }





                //ifSetting

                XPathNavigator ifSettingKeyNode = textBoxItr.Current.SelectSingleNode("child::IfSetting/@key");
                if (ifSettingKeyNode != null)
                {
                    qt.IfSettingKey = ifSettingKeyNode.Value;

                }
                else
                {
                    qt.IfSettingKey = null;

                }

                XPathNavigator ifSettingValNode = textBoxItr.Current.SelectSingleNode("child::IfSetting/@val");
                if (ifSettingValNode != null)
                {
                    qt.IfSettingVal = ifSettingValNode.Value;

                }
                else
                {
                    qt.IfSettingVal = null;

                }

                //optional gap between the label and the textbox
                gen = textBoxItr.Current.SelectSingleNode("child::LabelToBoxGap");
                if (gen != null)
                {
                    qt.LabelToBoxGap = Convert.ToInt32(gen.Value);

                    //MessageBox.Show("value is" + qt.LabelToBoxGap + "for:" + thisCode);

                    

                }
                else
                {
                    qt.LabelToBoxGap=0;

                    //MessageBox.Show("value is" + qt.LabelToBoxGap + "for:" + thisCode);

                }



                //save the object in the hash
                questionHash.Add(thisCode, qt);







            }


            
            textBoxItr = nav.Select("//QuestionTextDate");


            while (textBoxItr.MoveNext())
            {

                //a new QuestionText object
                qtdt = new QuestionTextDate(form, bigMessageBox, gs, specialDataStore, this);



                //get the code from the current node
                thisCode = textBoxItr.Current.SelectSingleNode("@code").Value;
                qtdt.Code = thisCode;


                //get the text of the question
                thisQuestionText = textBoxItr.Current.SelectSingleNode("@val").Value;
                qtdt.Val = thisQuestionText;

                //get the child node: WidgetPos
                widgetPosWidth = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                widgetPosHeight = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                widgetPosX = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                widgetPosY = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;

                qtdt.setWidgetWidth(widgetPosWidth);
                qtdt.setWidgetHeight(widgetPosHeight);
                qtdt.setWidgetXpos(widgetPosX);
                qtdt.setWidgetYpos(widgetPosY);


                //ToCode
                toCode = textBoxItr.Current.SelectSingleNode("child::ToCode").Value;
                qtdt.ToCode = toCode;

                //FromCode: optional
                gen = textBoxItr.Current.SelectSingleNode("child::FromCode");
                if (gen != null)
                {
                    fromCode = gen.Value;

                }
                else
                {
                    fromCode = null;
                }

                qtdt.FromCode = fromCode;



                //onError
                //onError = textBoxItr.Current.SelectSingleNode("child::OnError").Value;
                //qt.OnError = onError;



                XPathNavigator qCompare = textBoxItr.Current.SelectSingleNode("child::OnErrorQuestionCompare");
                if (qCompare != null)
                {
                    qtdt.OnErrorQuestionCompare = qCompare.Value;

                }

                XPathNavigator qSetKey = textBoxItr.Current.SelectSingleNode("child::SetKey");
                if (qSetKey != null)
                {
                    qtdt.SetKey = qSetKey.Value;

                }




                //validation
                validation = textBoxItr.Current.SelectSingleNode("child::Validation").Value;
                qtdt.setValidation(validation);

                //process
                process = textBoxItr.Current.SelectSingleNode("child::Process").Value;
                qtdt.setProcess(process);

                //section
                section = textBoxItr.Current.SelectSingleNode("child::Section").Value;
                qtdt.Section = section;


                //ifSetting

                XPathNavigator ifSettingKeyNode = textBoxItr.Current.SelectSingleNode("child::IfSetting/@key");
                if (ifSettingKeyNode != null)
                {
                    qtdt.IfSettingKey = ifSettingKeyNode.Value;

                }
                else
                {
                    qtdt.IfSettingKey = null;

                }

                XPathNavigator ifSettingValNode = textBoxItr.Current.SelectSingleNode("child::IfSetting/@val");
                if (ifSettingValNode != null)
                {
                    qtdt.IfSettingVal = ifSettingValNode.Value;

                }
                else
                {
                    qtdt.IfSettingVal = null;

                }

                //optional gap between the label and the textbox
                gen = textBoxItr.Current.SelectSingleNode("child::LabelToBoxGap");
                if (gen != null)
                {
                    qtdt.LabelToBoxGap = Convert.ToInt32(gen.Value);

                    //MessageBox.Show("value is" + qt.LabelToBoxGap + "for:" + thisCode);



                }
                else
                {
                    qtdt.LabelToBoxGap = 0;

                    //MessageBox.Show("value is" + qt.LabelToBoxGap + "for:" + thisCode);

                }



                //save the object in the hash
                questionHash.Add(thisCode, qtdt);







            }




            textBoxItr = nav.Select("//QuestionGPS");


            while (textBoxItr.MoveNext())
            {

                //a new QuestionText object
                qgps = new QuestionGPS(form, bigMessageBox, gs, specialDataStore, this);



                //get the code from the current node
                thisCode = textBoxItr.Current.SelectSingleNode("@code").Value;
                qgps.Code = thisCode;


                //get the text of the question
                thisQuestionText = textBoxItr.Current.SelectSingleNode("@val").Value;
                qgps.Val = thisQuestionText;

                //get the child node: WidgetPos
                widgetPosWidth = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                widgetPosHeight = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                widgetPosX = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                widgetPosY = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;

                qgps.setWidgetWidth(widgetPosWidth);
                qgps.setWidgetHeight(widgetPosHeight);
                qgps.setWidgetXpos(widgetPosX);
                qgps.setWidgetYpos(widgetPosY);


                //ToCode
                toCode = textBoxItr.Current.SelectSingleNode("child::ToCode").Value;
                qgps.ToCode = toCode;



                XPathNavigator qSetKey = textBoxItr.Current.SelectSingleNode("child::SetKey");
                if (qSetKey != null)
                {
                    qgps.SetKey = qSetKey.Value;

                }



                //section
                section = textBoxItr.Current.SelectSingleNode("child::Section").Value;
                qgps.Section = section;

                //ifSetting

                XPathNavigator ifSettingKeyNode = textBoxItr.Current.SelectSingleNode("child::IfSetting/@key");
                if (ifSettingKeyNode != null)
                {
                    qgps.IfSettingKey = ifSettingKeyNode.Value;

                }
                else
                {
                    qgps.IfSettingKey = null;

                }

                XPathNavigator ifSettingValNode = textBoxItr.Current.SelectSingleNode("child::IfSetting/@val");
                if (ifSettingValNode != null)
                {
                    qgps.IfSettingVal = ifSettingValNode.Value;

                }
                else
                {
                    qgps.IfSettingVal = null;

                }



                //save the object in the hash
                questionHash.Add(thisCode, qgps);







            }

            // Select the Automatic nodes
            textBoxItr = nav.Select("//QuestionAutomatic");


            while (textBoxItr.MoveNext())
            {

                //a new QuestionText object
                qauto = new QuestionAutomatic(form, bigMessageBox, gs, specialDataStore, this);



                //get the code from the current node
                thisCode = textBoxItr.Current.SelectSingleNode("@code").Value;
                qauto.Code = thisCode;




                //ToCode
                toCode = textBoxItr.Current.SelectSingleNode("child::ToCode").Value;
                qauto.ToCode = toCode;



                //process
                process = textBoxItr.Current.SelectSingleNode("child::Process").Value;
                qauto.setProcess(process);

                //section
                section = textBoxItr.Current.SelectSingleNode("child::Section").Value;
                qauto.Section = section;

                //advice-underweight

                XPathNavigator uwNode = textBoxItr.Current.SelectSingleNode("child::AdviceText[@name='underweight']");
                if (uwNode != null)
                {
                    qauto.UnderWeightAdvice = uwNode.Value;

                }


                //advice overweight

                XPathNavigator owNode = textBoxItr.Current.SelectSingleNode("child::AdviceText[@name='overweight']");
                if (owNode != null)
                {
                    qauto.OverWeightAdvice = owNode.Value;

                }

                //advice hypertensive
                XPathNavigator htNode = textBoxItr.Current.SelectSingleNode("child::AdviceText[@name='hypertensive']");
                if (htNode != null)
                {
                    qauto.HypertensiveAdvice = htNode.Value;

                }


                //save the object in the hash
                questionHash.Add(thisCode, qauto);



            }

            // Select the TextBox nodes
            textBoxItr = nav.Select("//QuestionTextTriple");


            while (textBoxItr.MoveNext())
            {

                //a new QuestionText object
                qtt = new QuestionTextTriple(form, bigMessageBox, gs, specialDataStore, this);



                //get the code from the current node
                thisCode = textBoxItr.Current.SelectSingleNode("@code").Value;
                qtt.Code = thisCode;


                //get the text of the question
                thisQuestionText = textBoxItr.Current.SelectSingleNode("@val").Value;
                qtt.Val = thisQuestionText;

                //get the child node: WidgetPos
                widgetPosWidth = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                widgetPosHeight = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                widgetPosX = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                widgetPosY = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;

                qtt.setWidgetWidth(widgetPosWidth);
                qtt.setWidgetHeight(widgetPosHeight);
                qtt.setWidgetXpos(widgetPosX);
                qtt.setWidgetYpos(widgetPosY);


                //ToCode
                toCode = textBoxItr.Current.SelectSingleNode("child::ToCode").Value;
                qtt.ToCode = toCode;

                //FromCode: optional
                gen = textBoxItr.Current.SelectSingleNode("child::FromCode");
                if (gen != null)
                {
                    fromCode = gen.Value;

                }
                else
                {
                    fromCode = null;
                }

                qtt.FromCode = fromCode;



                //onError
                //onError = textBoxItr.Current.SelectSingleNode("child::OnError").Value;
                //qtt.OnError = onError;



                XPathNavigator qCompare = textBoxItr.Current.SelectSingleNode("child::OnErrorQuestionCompare");
                if (qCompare != null)
                {
                    qtt.OnErrorQuestionCompare = qCompare.Value;

                }

                XPathNavigator qSetKey = textBoxItr.Current.SelectSingleNode("child::SetKey");
                if (qSetKey != null)
                {
                    qtt.SetKey = qSetKey.Value;

                }




                //validation
                validation = textBoxItr.Current.SelectSingleNode("child::Validation").Value;
                qtt.setValidation(validation);

                //process
                process = textBoxItr.Current.SelectSingleNode("child::Process").Value;
                qtt.setProcess(process);

                //section
                section = textBoxItr.Current.SelectSingleNode("child::Section").Value;
                qtt.Section = section;

                //ifSetting
                XPathNavigator ifSettingKeyNode = textBoxItr.Current.SelectSingleNode("child::IfSetting/@key");
                if (ifSettingKeyNode != null)
                {
                    qtt.IfSettingKey = ifSettingKeyNode.Value;

                }
                else
                {
                    qtt.IfSettingKey = null;

                }

                XPathNavigator ifSettingValNode = textBoxItr.Current.SelectSingleNode("child::IfSetting/@val");
                if (ifSettingValNode != null)
                {
                    qtt.IfSettingVal = ifSettingValNode.Value;

                }
                else
                {
                    qtt.IfSettingVal = null;

                }





                //save the object in the hash
                questionHash.Add(thisCode, qtt);







            }

            // Select the TextBox nodes
            textBoxItr = nav.Select("//QuestionGetTime");


            while (textBoxItr.MoveNext())
            {

                //a new QuestionText object
                qgt = new QuestionGetTime(form, bigMessageBox, gs, specialDataStore, this);



                //get the code from the current node
                thisCode = textBoxItr.Current.SelectSingleNode("@code").Value;
                qgt.Code = thisCode;


                //get the text of the question
                thisQuestionText = textBoxItr.Current.SelectSingleNode("@val").Value;
                qgt.Val = thisQuestionText;

                //get the child node: WidgetPos
                widgetPosWidth = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                widgetPosHeight = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                widgetPosX = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                widgetPosY = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;

                qgt.setWidgetWidth(widgetPosWidth);
                qgt.setWidgetHeight(widgetPosHeight);
                qgt.setWidgetXpos(widgetPosX);
                qgt.setWidgetYpos(widgetPosY);


                //ToCode
                toCode = textBoxItr.Current.SelectSingleNode("child::ToCode").Value;
                qgt.ToCode = toCode;



                XPathNavigator qSetKey = textBoxItr.Current.SelectSingleNode("child::SetKey");
                if (qSetKey != null)
                {
                    qgt.SetKey = qSetKey.Value;

                }

                //process
                process = textBoxItr.Current.SelectSingleNode("child::Process").Value;
                qgt.setProcess(process);

                //section
                section = textBoxItr.Current.SelectSingleNode("child::Section").Value;
                qgt.Section = section;

                //ifSetting
                XPathNavigator ifSettingKeyNode = textBoxItr.Current.SelectSingleNode("child::IfSetting/@key");
                if (ifSettingKeyNode != null)
                {
                    qgt.IfSettingKey = ifSettingKeyNode.Value;

                }
                else
                {
                    qgt.IfSettingKey = null;

                }

                XPathNavigator ifSettingValNode = textBoxItr.Current.SelectSingleNode("child::IfSetting/@val");
                if (ifSettingValNode != null)
                {
                    qgt.IfSettingVal = ifSettingValNode.Value;

                }
                else
                {
                    qgt.IfSettingVal = null;

                }





                //save the object in the hash
                questionHash.Add(thisCode, qgt);







            }


            // Select the TextBoxDouble nodes
            textBoxItr = nav.Select("//QuestionTextDouble");


            while (textBoxItr.MoveNext())
            {

                //a new QuestionText object
                qtd = new QuestionTextDouble(form, bigMessageBox, gs, specialDataStore, this);



                //get the code from the current node
                thisCode = textBoxItr.Current.SelectSingleNode("@code").Value;
                qtd.Code = thisCode;


                //get the text of the question
                thisQuestionText = textBoxItr.Current.SelectSingleNode("@val").Value;
                qtd.Val = thisQuestionText;

                //get the child node: WidgetPos
                widgetPosWidth = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                widgetPosHeight = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                widgetPosX = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                widgetPosY = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;

                qtd.setWidgetWidth(widgetPosWidth);
                qtd.setWidgetHeight(widgetPosHeight);
                qtd.setWidgetXpos(widgetPosX);
                qtd.setWidgetYpos(widgetPosY);


                //ToCode
                toCode = textBoxItr.Current.SelectSingleNode("child::ToCode").Value;
                qtd.ToCode = toCode;

                //FromCode: optional
                gen = textBoxItr.Current.SelectSingleNode("child::FromCode");
                if (gen != null)
                {
                    fromCode = gen.Value;

                }
                else
                {
                    fromCode = null;
                }

                qtd.FromCode = fromCode;







                XPathNavigator qCompare = textBoxItr.Current.SelectSingleNode("child::OnErrorQuestionCompare");
                if (qCompare != null)
                {
                    qtd.OnErrorQuestionCompare = qCompare.Value;

                }

                XPathNavigator qSetKey = textBoxItr.Current.SelectSingleNode("child::SetKey");
                if (qSetKey != null)
                {
                    qtd.SetKey = qSetKey.Value;

                }




                //validation
                validation = textBoxItr.Current.SelectSingleNode("child::Validation").Value;
                qtd.setValidation(validation);

                //process
                process = textBoxItr.Current.SelectSingleNode("child::Process").Value;
                qtd.setProcess(process);

                //section
                section = textBoxItr.Current.SelectSingleNode("child::Section").Value;
                qtd.Section = section;

                //ifSetting
                XPathNavigator ifSettingKeyNode = textBoxItr.Current.SelectSingleNode("child::IfSetting/@key");
                if (ifSettingKeyNode != null)
                {
                    qtd.IfSettingKey = ifSettingKeyNode.Value;

                }
                else
                {
                    qtd.IfSettingKey = null;

                }

                XPathNavigator ifSettingValNode = textBoxItr.Current.SelectSingleNode("child::IfSetting/@val");
                if (ifSettingValNode != null)
                {
                    qtd.IfSettingVal = ifSettingValNode.Value;

                }
                else
                {
                    qtd.IfSettingVal = null;

                }





                //save the object in the hash
                questionHash.Add(thisCode, qtd);







            }

            // Select the TextBoxOneCheck nodes, i.e. widgets which have a textbox and a single checkbox
            textBoxItr = nav.Select("//QuestionTextOneCheck");


            while (textBoxItr.MoveNext())
            {

                //a new QuestionText object
                qtc = new QuestionTextOneCheck(form, bigMessageBox, gs, specialDataStore, this);



                //get the code from the current node
                thisCode = textBoxItr.Current.SelectSingleNode("@code").Value;
                qtc.Code = thisCode;


                //get the text of the question
                thisQuestionText = textBoxItr.Current.SelectSingleNode("@val").Value;
                qtc.Val = thisQuestionText;

                //get the child node: WidgetPos
                widgetPosWidth = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                widgetPosHeight = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                widgetPosX = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                widgetPosY = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;

                qtc.setWidgetWidth(widgetPosWidth);
                qtc.setWidgetHeight(widgetPosHeight);
                qtc.setWidgetXpos(widgetPosX);
                qtc.setWidgetYpos(widgetPosY);


                //ToCode
                toCode = textBoxItr.Current.SelectSingleNode("child::ToCode").Value;
                qtc.ToCode = toCode;

                //FromCode: optional
                gen = textBoxItr.Current.SelectSingleNode("child::FromCode");
                if (gen != null)
                {
                    fromCode = gen.Value;

                }
                else
                {
                    fromCode = null;
                }

                qtc.FromCode = fromCode;






                XPathNavigator qCompare = textBoxItr.Current.SelectSingleNode("child::OnErrorQuestionCompare");
                if (qCompare != null)
                {
                    qtc.OnErrorQuestionCompare = qCompare.Value;

                }

                XPathNavigator qSetKey = textBoxItr.Current.SelectSingleNode("child::SetKey");
                if (qSetKey != null)
                {
                    qtc.SetKey = qSetKey.Value;

                }




                //validation
                validation = textBoxItr.Current.SelectSingleNode("child::Validation").Value;
                qtc.setValidation(validation);

                //process
                process = textBoxItr.Current.SelectSingleNode("child::Process").Value;
                qtc.setProcess(process);

                //section
                section = textBoxItr.Current.SelectSingleNode("child::Section").Value;
                qtc.Section = section;

                //ifSetting
                XPathNavigator ifSettingKeyNode = textBoxItr.Current.SelectSingleNode("child::IfSetting/@key");
                if (ifSettingKeyNode != null)
                {
                    qtc.IfSettingKey = ifSettingKeyNode.Value;

                }
                else
                {
                    qtc.IfSettingKey = null;

                }

                XPathNavigator ifSettingValNode = textBoxItr.Current.SelectSingleNode("child::IfSetting/@val");
                if (ifSettingValNode != null)
                {
                    qtc.IfSettingVal = ifSettingValNode.Value;

                }
                else
                {
                    qtc.IfSettingVal = null;

                }




                //label for the checkbox

                qtc.CheckBoxLabel = textBoxItr.Current.SelectSingleNode("child::CheckBoxLabel").Value;

                //code to insert as the data if the checkbox is checked

                qtc.CheckBoxCheckCode = textBoxItr.Current.SelectSingleNode("child::CheckBoxCheckCode").Value;

                //save the object in the hash
                questionHash.Add(thisCode, qtc);







            }

            //process the Radio Questions

            radioItr = nav.Select("//QuestionRadio");

            while (radioItr.MoveNext())
            {


                qr = new QuestionRadio(form, bigMessageBox, gs, specialDataStore, this);

                //fields that appear once per radio

                //get the code from the current node
                thisCode = radioItr.Current.SelectSingleNode("@code").Value;
                qr.Code = thisCode;


                //get the text of the question
                thisQuestionText = radioItr.Current.SelectSingleNode("@val").Value;
                qr.Val = thisQuestionText;

                //get the child node: WidgetPos
                widgetPosWidth = radioItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                widgetPosHeight = radioItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                widgetPosX = radioItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                widgetPosY = radioItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;

                qr.setWidgetWidth(widgetPosWidth);
                qr.setWidgetHeight(widgetPosHeight);
                qr.setWidgetXpos(widgetPosX);
                qr.setWidgetYpos(widgetPosY);

                //ToCode
                toCode = radioItr.Current.SelectSingleNode("child::ToCode").Value;
                qr.ToCode = toCode;

                //LabelToGroupBoxGap : optional
                string labelToGroupBoxGap = null;
                XPathNavigator ltgbgNode = radioItr.Current.SelectSingleNode("child::LabelToGroupBoxGap");
                if (ltgbgNode != null)
                {

                    labelToGroupBoxGap = ltgbgNode.Value;


                }
                qr.LabelToGroupBoxGap = labelToGroupBoxGap;





                //FromCode: optional
                gen = radioItr.Current.SelectSingleNode("child::FromCode");
                if (gen != null)
                {
                    fromCode = gen.Value;

                }
                else
                {
                    fromCode = null;
                }

                qr.FromCode = fromCode;


                //setKey : optional

                XPathNavigator setKeyNode = radioItr.Current.SelectSingleNode("child::SetKey");
                if (setKeyNode == null)
                {
                    qr.SetKey = null;

                }
                else
                {
                    qr.SetKey = setKeyNode.Value;


                }

                //setKey = radioItr.Current.SelectSingleNode("child::SetKey").Value;
                //qr.SetKey = setKey;

                //section
                section = radioItr.Current.SelectSingleNode("child::Section").Value;
                qr.Section = section;

                //ifSetting
                XPathNavigator ifSettingKeyNode = radioItr.Current.SelectSingleNode("child::IfSetting/@key");
                if (ifSettingKeyNode != null)
                {
                    qr.IfSettingKey = ifSettingKeyNode.Value;

                }
                else
                {
                    qr.IfSettingKey = null;

                }

                XPathNavigator ifSettingValNode = radioItr.Current.SelectSingleNode("child::IfSetting/@val");
                if (ifSettingValNode != null)
                {
                    qr.IfSettingVal = ifSettingValNode.Value;

                }
                else
                {
                    qr.IfSettingVal = null;

                }




                //each option node
                optionItr = radioItr.Current.Select("OptionRadio");

                while (optionItr.MoveNext())
                {


                    //value
                    optionValue = optionItr.Current.SelectSingleNode("child::Value").Value;

                    //text
                    optionText = optionItr.Current.SelectSingleNode("child::Text").Value;

                    //toCode: optional
                    XPathNavigator toCodeNode = optionItr.Current.SelectSingleNode("child::ToCode");
                    if (toCodeNode != null)
                    {
                        optionToCode = toCodeNode.Value;

                    }
                    else
                    {
                        optionToCode = null;
                    }


                    //ToCodeAndError: optional

                    XPathNavigator toCodeErrNode = optionItr.Current.SelectSingleNode("child::ToCodeAndError");
                    if (toCodeErrNode != null)
                    {
                        optionToCodeErr = toCodeErrNode.Value;

                    }
                    else
                    {
                        optionToCodeErr = null;
                    }



                    //get the child node: WidgetPos
                    widgetPosWidth = optionItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                    widgetPosHeight = optionItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                    widgetPosX = optionItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                    widgetPosY = optionItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;



                    op = new Option(optionValue, optionText);

                    op.setWidgetWidth(widgetPosWidth);
                    op.setWidgetHeight(widgetPosHeight);
                    op.setWidgetXpos(widgetPosX);
                    op.setWidgetYpos(widgetPosY);

                    op.ToCode = optionToCode;
                    op.ToCodeErr = optionToCodeErr;

                    //is this option the default ?
                    var defaultOption = optionItr.Current.SelectSingleNode("child::Default");

                    if (defaultOption == null)
                    {
                        op.Default = false;
                    }
                    else
                    {
                        op.Default = true;

                    }


                    qr.addOption(op);


                }

                //save the object in the hash
                questionHash.Add(thisCode, qr);

            }


            radioItr = nav.Select("//QuestionRadioDynamicLabel");

            while (radioItr.MoveNext())
            {


                qrd = new QuestionRadioDynamicLabel(form, bigMessageBox, gs, specialDataStore, this);

                //fields that appear once per radio

                //get the code from the current node
                thisCode = radioItr.Current.SelectSingleNode("@code").Value;
                qrd.Code = thisCode;


                //get the text of the question
                thisQuestionText = radioItr.Current.SelectSingleNode("@val").Value;
                qrd.Val = thisQuestionText;

                //get the child node: WidgetPos
                widgetPosWidth = radioItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                widgetPosHeight = radioItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                widgetPosX = radioItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                widgetPosY = radioItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;

                qrd.setWidgetWidth(widgetPosWidth);
                qrd.setWidgetHeight(widgetPosHeight);
                qrd.setWidgetXpos(widgetPosX);
                qrd.setWidgetYpos(widgetPosY);

                //ToCode
                toCode = radioItr.Current.SelectSingleNode("child::ToCode").Value;
                qrd.ToCode = toCode;

                //label generator func
                string labelGenFunc = radioItr.Current.SelectSingleNode("child::LabelGeneratorFunc").Value;
                qrd.LabelGeneratorFunc = labelGenFunc;


                //LabelToGroupBoxGap : optional
                string labelToGroupBoxGap = null;
                XPathNavigator ltgbgNode = radioItr.Current.SelectSingleNode("child::LabelToGroupBoxGap");
                if (ltgbgNode != null)
                {

                    labelToGroupBoxGap = ltgbgNode.Value;


                }
                qrd.LabelToGroupBoxGap = labelToGroupBoxGap;





                //FromCode: optional
                gen = radioItr.Current.SelectSingleNode("child::FromCode");
                if (gen != null)
                {
                    fromCode = gen.Value;

                }
                else
                {
                    fromCode = null;
                }

                qrd.FromCode = fromCode;


                //setKey : optional

                XPathNavigator setKeyNode = radioItr.Current.SelectSingleNode("child::SetKey");
                if (setKeyNode == null)
                {
                    qrd.SetKey = null;

                }
                else
                {
                    qrd.SetKey = setKeyNode.Value;


                }

                //setKey = radioItr.Current.SelectSingleNode("child::SetKey").Value;
                //qr.SetKey = setKey;

                //section
                section = radioItr.Current.SelectSingleNode("child::Section").Value;
                qrd.Section = section;

                //ifSetting
                XPathNavigator ifSettingKeyNode = radioItr.Current.SelectSingleNode("child::IfSetting/@key");
                if (ifSettingKeyNode != null)
                {
                    qrd.IfSettingKey = ifSettingKeyNode.Value;

                }
                else
                {
                    qrd.IfSettingKey = null;

                }

                XPathNavigator ifSettingValNode = radioItr.Current.SelectSingleNode("child::IfSetting/@val");
                if (ifSettingValNode != null)
                {
                    qrd.IfSettingVal = ifSettingValNode.Value;

                }
                else
                {
                    qrd.IfSettingVal = null;

                }




                //each option node
                optionItr = radioItr.Current.Select("OptionRadio");

                while (optionItr.MoveNext())
                {


                    //value
                    optionValue = optionItr.Current.SelectSingleNode("child::Value").Value;

                    //text
                    optionText = optionItr.Current.SelectSingleNode("child::Text").Value;

                    //toCode: optional
                    XPathNavigator toCodeNode = optionItr.Current.SelectSingleNode("child::ToCode");
                    if (toCodeNode != null)
                    {
                        optionToCode = toCodeNode.Value;

                    }
                    else
                    {
                        optionToCode = null;
                    }


                    //ToCodeAndError: optional

                    XPathNavigator toCodeErrNode = optionItr.Current.SelectSingleNode("child::ToCodeAndError");
                    if (toCodeErrNode != null)
                    {
                        optionToCodeErr = toCodeErrNode.Value;

                    }
                    else
                    {
                        optionToCodeErr = null;
                    }



                    //get the child node: WidgetPos
                    widgetPosWidth = optionItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                    widgetPosHeight = optionItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                    widgetPosX = optionItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                    widgetPosY = optionItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;



                    op = new Option(optionValue, optionText);

                    op.setWidgetWidth(widgetPosWidth);
                    op.setWidgetHeight(widgetPosHeight);
                    op.setWidgetXpos(widgetPosX);
                    op.setWidgetYpos(widgetPosY);

                    op.ToCode = optionToCode;
                    op.ToCodeErr = optionToCodeErr;

                    //is this option the default ?
                    var defaultOption = optionItr.Current.SelectSingleNode("child::Default");

                    if (defaultOption == null)
                    {
                        op.Default = false;
                    }
                    else
                    {
                        op.Default = true;

                    }


                    qrd.addOption(op);


                }

                //save the object in the hash
                questionHash.Add(thisCode, qrd);

            }


            radioItr = nav.Select("//QuestionSelect");

            while (radioItr.MoveNext())
            {


                qs = new QuestionSelect(form, bigMessageBox, gs, specialDataStore, this);

                //fields that appear once per radio

                //get the code from the current node
                thisCode = radioItr.Current.SelectSingleNode("@code").Value;
                qs.Code = thisCode;


                //get the text of the question
                thisQuestionText = radioItr.Current.SelectSingleNode("@val").Value;
                qs.Val = thisQuestionText;

                //get the child node: WidgetPos
                widgetPosWidth = radioItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                widgetPosHeight = radioItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                widgetPosX = radioItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                widgetPosY = radioItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;

                qs.setWidgetWidth(widgetPosWidth);
                qs.setWidgetHeight(widgetPosHeight);
                qs.setWidgetXpos(widgetPosX);
                qs.setWidgetYpos(widgetPosY);

                //ToCode
                toCode = radioItr.Current.SelectSingleNode("child::ToCode").Value;
                qs.ToCode = toCode;

                //SelectLength
                qs.SelectLength = Convert.ToInt32(radioItr.Current.SelectSingleNode("child::SelectLength").Value);

                
                //FromCode: optional
                gen = radioItr.Current.SelectSingleNode("child::FromCode");
                if (gen != null)
                {
                    fromCode = gen.Value;

                }
                else
                {
                    fromCode = null;
                }

                qs.FromCode = fromCode;


                //setKey : optional

                XPathNavigator setKeyNode = radioItr.Current.SelectSingleNode("child::SetKey");
                if (setKeyNode == null)
                {
                    qs.SetKey = null;

                }
                else
                {
                    qs.SetKey = setKeyNode.Value;


                }

                

                //section
                section = radioItr.Current.SelectSingleNode("child::Section").Value;
                qs.Section = section;

                //ifSetting
                XPathNavigator ifSettingKeyNode = radioItr.Current.SelectSingleNode("child::IfSetting/@key");
                if (ifSettingKeyNode != null)
                {
                    qs.IfSettingKey = ifSettingKeyNode.Value;

                }
                else
                {
                    qs.IfSettingKey = null;

                }

                XPathNavigator ifSettingValNode = radioItr.Current.SelectSingleNode("child::IfSetting/@val");
                if (ifSettingValNode != null)
                {
                    qs.IfSettingVal = ifSettingValNode.Value;

                }
                else
                {
                    qs.IfSettingVal = null;

                }


                //each option node
                optionItr = radioItr.Current.Select("OptionSelect");

                while (optionItr.MoveNext())
                {


                    //value
                    optionValue = optionItr.Current.SelectSingleNode("child::Value").Value;

                    //text
                    optionText = optionItr.Current.SelectSingleNode("child::Text").Value;

                    //toCode: optional
                    XPathNavigator toCodeNode = optionItr.Current.SelectSingleNode("child::ToCode");
                    if (toCodeNode != null)
                    {
                        optionToCode = toCodeNode.Value;

                    }
                    else
                    {
                        optionToCode = null;
                    }


                    //ToCodeAndError: optional

                    XPathNavigator toCodeErrNode = optionItr.Current.SelectSingleNode("child::ToCodeAndError");
                    if (toCodeErrNode != null)
                    {
                        optionToCodeErr = toCodeErrNode.Value;

                    }
                    else
                    {
                        optionToCodeErr = null;
                    }



                    op = new Option(optionValue, optionText);

                    

                    op.ToCode = optionToCode;
                    op.ToCodeErr = optionToCodeErr;

                    //is this option the default ?
                    var defaultOption = optionItr.Current.SelectSingleNode("child::Default");

                    if (defaultOption == null)
                    {
                        op.Default = false;
                    }
                    else
                    {
                        op.Default = true;

                    }


                    qs.addOption(op);


                }

                //save the object in the hash
                questionHash.Add(thisCode, qs);

            }




            //process the labelQuestions:

            labelItr = nav.Select("//QuestionLabel");

            while (labelItr.MoveNext())
            {


                ql = new QuestionLabel(form, bigMessageBox, gs, specialDataStore, this);

                //fields that appear once per radio

                //get the code from the current node
                thisCode = labelItr.Current.SelectSingleNode("@code").Value;
                ql.Code = thisCode;


                //get the text of the question
                thisQuestionText = labelItr.Current.SelectSingleNode("@val").Value;
                ql.Val = thisQuestionText;

                //get the child node: WidgetPos
                widgetPosWidth = labelItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                widgetPosHeight = labelItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                widgetPosX = labelItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                widgetPosY = labelItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;

                ql.setWidgetWidth(widgetPosWidth);
                ql.setWidgetHeight(widgetPosHeight);
                ql.setWidgetXpos(widgetPosX);
                ql.setWidgetYpos(widgetPosY);

                //ToCode
                toCode = labelItr.Current.SelectSingleNode("child::ToCode").Value;
                ql.ToCode = toCode;

                //FromCode: optional
                gen = labelItr.Current.SelectSingleNode("child::FromCode");
                if (gen != null)
                {
                    fromCode = gen.Value;

                }
                else
                {
                    fromCode = null;
                }

                ql.FromCode = fromCode;

                //section
                section = labelItr.Current.SelectSingleNode("child::Section").Value;
                ql.Section = section;

                //ifSetting
                XPathNavigator ifSettingKeyNode = labelItr.Current.SelectSingleNode("child::IfSetting/@key");
                if (ifSettingKeyNode != null)
                {
                    ql.IfSettingKey = ifSettingKeyNode.Value;

                }
                else
                {
                    ql.IfSettingKey = null;

                }

                XPathNavigator ifSettingValNode = labelItr.Current.SelectSingleNode("child::IfSetting/@val");
                if (ifSettingValNode != null)
                {
                    ql.IfSettingVal = ifSettingValNode.Value;

                }
                else
                {
                    ql.IfSettingVal = null;

                }


                //ifSettingKey = labelItr.Current.SelectSingleNode("child::IfSetting/@key").Value;
                //ifSettingVal = labelItr.Current.SelectSingleNode("child::IfSetting/@val").Value;
                //ql.IfSettingKey = ifSettingKey;
                //ql.IfSettingVal = ifSettingVal;



                //save the object in the hash
                questionHash.Add(thisCode, ql);

            }

            //process TextRadio questions (have a textbox plus a set of radio buttons)

            textRadioItr = nav.Select("//QuestionTextRadio");


            while (textRadioItr.MoveNext())
            {

                //a new QuestionText object
                qtr = new QuestionTextRadio(form, bigMessageBox, gs, specialDataStore, this);



                //get the code from the current node
                thisCode = textRadioItr.Current.SelectSingleNode("@code").Value;
                qtr.Code = thisCode;


                //get the text of the question
                thisQuestionText = textRadioItr.Current.SelectSingleNode("@val").Value;
                qtr.Val = thisQuestionText;

                //get the child node: WidgetPos
                widgetPosWidth = textRadioItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                widgetPosHeight = textRadioItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                widgetPosX = textRadioItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                widgetPosY = textRadioItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;

                qtr.setWidgetWidth(widgetPosWidth);
                qtr.setWidgetHeight(widgetPosHeight);
                qtr.setWidgetXpos(widgetPosX);
                qtr.setWidgetYpos(widgetPosY);


                //ToCode
                toCode = textRadioItr.Current.SelectSingleNode("child::ToCode").Value;
                qtr.ToCode = toCode;

                //FromCode: optional
                gen = textRadioItr.Current.SelectSingleNode("child::FromCode");
                if (gen != null)
                {
                    fromCode = gen.Value;

                }
                else
                {
                    fromCode = null;
                }

                qtr.FromCode = fromCode;





                XPathNavigator qCompare = textRadioItr.Current.SelectSingleNode("child::OnErrorQuestionCompare");
                if (qCompare != null)
                {
                    qtr.OnErrorQuestionCompare = qCompare.Value;

                }


                XPathNavigator checkDontKnow = textRadioItr.Current.SelectSingleNode("child::CheckPreviousDontKnow");
                if (checkDontKnow != null)
                {
                    qtr.CheckPreviousDontKnow = true;


                }
                else
                {
                    qtr.CheckPreviousDontKnow = false;

                }



                //validation
                validation = textRadioItr.Current.SelectSingleNode("child::Validation").Value;
                qtr.setValidation(validation);

                //process
                process = textRadioItr.Current.SelectSingleNode("child::Process").Value;
                qtr.setProcess(process);

                //section
                section = textRadioItr.Current.SelectSingleNode("child::Section").Value;
                qtr.Section = section;

                //ifSetting
                XPathNavigator ifSettingKeyNode = textRadioItr.Current.SelectSingleNode("child::IfSetting/@key");
                if (ifSettingKeyNode != null)
                {
                    qtr.IfSettingKey = ifSettingKeyNode.Value;

                }
                else
                {
                    qtr.IfSettingKey = null;

                }

                XPathNavigator ifSettingValNode = textRadioItr.Current.SelectSingleNode("child::IfSetting/@val");
                if (ifSettingValNode != null)
                {
                    qtr.IfSettingVal = ifSettingValNode.Value;

                }
                else
                {
                    qtr.IfSettingVal = null;

                }


                //ifSettingKey = textRadioItr.Current.SelectSingleNode("child::IfSetting/@key").Value;
                //ifSettingVal = textRadioItr.Current.SelectSingleNode("child::IfSetting/@val").Value;
                //qtr.IfSettingKey = ifSettingKey;
                //qtr.IfSettingVal = ifSettingVal;


                //setKey

                setKey = textRadioItr.Current.SelectSingleNode("child::SetKey").Value;
                qtr.SetKey = setKey;

                //label for the radio-set
                string radioLabel = textRadioItr.Current.SelectSingleNode("child::RadioLabel").Value;
                qtr.RadioLabel = radioLabel;

                //each option node
                optionItr = textRadioItr.Current.Select("OptionRadio");

                while (optionItr.MoveNext())
                {


                    //value
                    optionValue = optionItr.Current.SelectSingleNode("child::Value").Value;

                    //text
                    optionText = optionItr.Current.SelectSingleNode("child::Text").Value;



                    //get the child node: WidgetPos
                    widgetPosWidth = optionItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                    widgetPosHeight = optionItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                    widgetPosX = optionItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                    widgetPosY = optionItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;



                    op = new Option(optionValue, optionText);

                    op.setWidgetWidth(widgetPosWidth);
                    op.setWidgetHeight(widgetPosHeight);
                    op.setWidgetXpos(widgetPosX);
                    op.setWidgetYpos(widgetPosY);



                    //is this option the default ?
                    var defaultOption = optionItr.Current.SelectSingleNode("child::Default");

                    if (defaultOption == null)
                    {
                        op.Default = false;
                    }
                    else
                    {
                        op.Default = true;

                    }


                    qtr.addOption(op);


                }



                //save the object in the hash
                questionHash.Add(thisCode, qtr);
            }


            textRadioItr = nav.Select("//QuestionTextRadioButton");


            while (textRadioItr.MoveNext())
            {

                //a new QuestionText object
                qtrb = new QuestionTextRadioButton(form, bigMessageBox, gs, specialDataStore, this);



                //get the code from the current node
                thisCode = textRadioItr.Current.SelectSingleNode("@code").Value;
                qtrb.Code = thisCode;


                //get the text of the question
                thisQuestionText = textRadioItr.Current.SelectSingleNode("@val").Value;
                qtrb.Val = thisQuestionText;

                //get the child node: WidgetPos
                widgetPosWidth = textRadioItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                widgetPosHeight = textRadioItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                widgetPosX = textRadioItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                widgetPosY = textRadioItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;

                qtrb.setWidgetWidth(widgetPosWidth);
                qtrb.setWidgetHeight(widgetPosHeight);
                qtrb.setWidgetXpos(widgetPosX);
                qtrb.setWidgetYpos(widgetPosY);


                //ToCode
                toCode = textRadioItr.Current.SelectSingleNode("child::ToCode").Value;
                qtrb.ToCode = toCode;

                //FromCode: optional
                gen = textRadioItr.Current.SelectSingleNode("child::FromCode");
                if (gen != null)
                {
                    fromCode = gen.Value;

                }
                else
                {
                    fromCode = null;
                }

                qtrb.FromCode = fromCode;





                gen = textRadioItr.Current.SelectSingleNode("child::OnErrorQuestionCompare");
                if (gen != null)
                {
                    qtrb.OnErrorQuestionCompare = gen.Value;

                }


                gen = textRadioItr.Current.SelectSingleNode("child::CheckPreviousDontKnow");
                if (gen != null)
                {
                    qtrb.CheckPreviousDontKnow = true;


                }
                else
                {
                    qtrb.CheckPreviousDontKnow = false;

                }



                //validation
                validation = textRadioItr.Current.SelectSingleNode("child::Validation").Value;
                qtrb.setValidation(validation);

                //process
                process = textRadioItr.Current.SelectSingleNode("child::Process").Value;
                qtrb.setProcess(process);

                //section
                section = textRadioItr.Current.SelectSingleNode("child::Section").Value;
                qtrb.Section = section;

                //ifSetting
                gen = textRadioItr.Current.SelectSingleNode("child::IfSetting/@key");
                if (gen != null)
                {
                    qtrb.IfSettingKey = gen.Value;

                }
                else
                {
                    qtrb.IfSettingKey = null;

                }

                gen = textRadioItr.Current.SelectSingleNode("child::IfSetting/@val");
                if (gen != null)
                {
                    qtrb.IfSettingVal = gen.Value;

                }
                else
                {
                    qtrb.IfSettingVal = null;

                }




                //setKey

                setKey = textRadioItr.Current.SelectSingleNode("child::SetKey").Value;
                qtrb.SetKey = setKey;

                //label for the radio-set
                //radioLabel = textRadioItr.Current.SelectSingleNode("child::RadioLabel").Value;
                qtrb.RadioLabel = textRadioItr.Current.SelectSingleNode("child::RadioLabel").Value;


                //validation that runs when the special button is pressed
                string buttonValidationCode = textRadioItr.Current.SelectSingleNode("child::ValidationButton").Value;
                qtrb.setValidationButton(buttonValidationCode);





                //each option node
                optionItr = textRadioItr.Current.Select("OptionRadio");

                while (optionItr.MoveNext())
                {


                    //value
                    optionValue = optionItr.Current.SelectSingleNode("child::Value").Value;

                    //text
                    optionText = optionItr.Current.SelectSingleNode("child::Text").Value;



                    //get the child node: WidgetPos
                    widgetPosWidth = optionItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                    widgetPosHeight = optionItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                    widgetPosX = optionItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                    widgetPosY = optionItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;



                    op = new Option(optionValue, optionText);

                    op.setWidgetWidth(widgetPosWidth);
                    op.setWidgetHeight(widgetPosHeight);
                    op.setWidgetXpos(widgetPosX);
                    op.setWidgetYpos(widgetPosY);



                    //is this option the default ?
                    var defaultOption = optionItr.Current.SelectSingleNode("child::Default");

                    if (defaultOption == null)
                    {
                        op.Default = false;
                    }
                    else
                    {
                        op.Default = true;

                    }


                    qtrb.addOption(op);


                }



                //save the object in the hash
                questionHash.Add(thisCode, qtrb);







            }


        }

    


    }
}
