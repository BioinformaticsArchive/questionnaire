
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
using MySql.Data.MySqlClient;


namespace EQdbUploader
{
    class DB
    {

        private string connectionStr;
        private MySqlConnection conn= null;

        public DB(string connectionStr)
        {
            this.connectionStr = connectionStr;
            
            
            //connectionStr = "SERVER=localhost;DATABASE=malawi_survey;UID=survey;PASSWORD=goose_stepper98;";

        }

        public void connect()
        {

            //try and connect
            conn = new MySqlConnection(connectionStr);
            conn.Open();



        }

        public void close()
        {

            try
            {
                conn.Close();

            }
            catch
            {

            }
            
            

        }

        public void addParticipantData(string partID, string qCode, string qData)
        {
            //string query = "insert into participant_data(participant_id, question_code, question_response) values('" + partID + "', '" + qCode + "', '" + qData + "')";
            string query = "insert into participant_data(participant_id, question_code, question_response) values(@partID, @qCode, @qData)";

            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@partID", partID);
            cmd.Parameters.AddWithValue("@qCode", qCode);
            cmd.Parameters.AddWithValue("@qData", qData);


            cmd.ExecuteNonQuery();



        }


        public List<string> showCurrentParticipants()
        {

            //return a list of all participants and the number of rows for each in the db
            //string query = "select participant_id, count(*) from participant_data group by participant_id";

            string query = "select distinct participant_id from participant_data order by participant_id";

            List<string> parts = new List<string>();

            MySqlCommand cmd = new MySqlCommand(query, conn);

            MySqlDataReader dataReader = cmd.ExecuteReader();

            while (dataReader.Read())
            {

                //parts.Add(dataReader[0] + "(" + dataReader[1] + ")");
                parts.Add(dataReader[0].ToString());




            }

            dataReader.Close();
            return parts;




        }



        public void deleteParticipant(string partID)
        {

            //string query = "delete from participant_data where participant_id= '" + partID + "'";

            string query = "delete from participant_data where participant_id= @partID";

            MySqlCommand cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@partID", partID);

            cmd.ExecuteNonQuery();



        }

        public bool isPartLoaded(string partID)
        {
            //is this participant loaded in the db

            string query = "select count(*) from participant_data where participant_id= @partID";

            MySqlCommand cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@partID", partID);

            object result= cmd.ExecuteScalar();

            if (result == null)
            {

                throw new Exception();

            }
            else
            {
                int num = Convert.ToInt32(result);

                if (num == 0)
                {
                    //not loaded
                    return false;

                }
                else
                {
                    //loaded
                    return true;

                }


            }



        }











    }
}
