
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
using System.IO;



namespace BHS_questionnaire_demo
{
    class GlobalStore
    {

        //a key-value store that all questions can access

        private Dictionary<string, string> store;

        //constructor
        public GlobalStore()
        {
            store = new Dictionary<string, string>();




        }

        //add a key/value to the store
        public void Add(string key, string val)
        {

            //store.Add(key, val);
            store[key] = val;


        }

        //get a value from the store, given a key
        public string Get(string key)
        {
            try
            {
                return store[key];


            }
            catch (KeyNotFoundException e)
            {

                return null;

            }



        }



        public void save(System.IO.TextWriter dataOut)
        {

            //save the data stored in this object to a file

            foreach (KeyValuePair<string, string> kv in store)
            {

                dataOut.WriteLine(kv.Key + "\t" + kv.Value);

            }
            
            
           
        }

        public void load(StreamReader reader)
        {
            //read data out of this file

            Char[] delim = new Char[] { '\t' };

            while (reader.EndOfStream == false)
            {
                string line = reader.ReadLine();

                //using tab as delim
                string[] parts = line.Split(delim);

                //add to the store

                store[parts[0]] = parts[1];



            }



        }





    }
}
