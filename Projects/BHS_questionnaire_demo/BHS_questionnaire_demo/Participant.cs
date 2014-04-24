
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

namespace BHS_questionnaire_demo
{
    class Participant
    {

        private string dirPath;
        private string partID;
        private string language;
        
        //integer version, when ids are always ints
        private int partIDAsInt;

        public bool Locked
        {
            set;
            get;

        }

        public int getPartIDint()
        {
            return partIDAsInt;


        }

        public void setPartIDint(int id)
        {

            partIDAsInt = id;

        }


        public Participant(string dirPath, string partID, string language){

            this.dirPath = dirPath;
            this.partID = partID;
            this.language = language;
            Locked = false;



        }

        public override string ToString()
        {
            if (Locked)
            {
                return partID + " (locked)";

            }
            else
            {
                return partID;

            }
            


        }

        public string getPath()
        {
            return dirPath;

        }

        public string getID()
        {

            return partID;

        }

        public string getLanguage()
        {

            return language;

        }

       











    }
}
