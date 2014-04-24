
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
using System.Text.RegularExpressions;


namespace BHS_questionnaire_demo
{
    class Utils
    {

        //static utility functions

        public static bool GPSenabled { get; set; }
        


        public static List<string> getSectionList(){

            List<string> sections= new List<string>();

            //Malawi
            /*
            sections.Add("Consents");
            sections.Add("Demographic Information");
            sections.Add("Education, Occupation and Livelihood");
            sections.Add("Health: Tobacco Use");
            sections.Add("Health: Alcohol Consumption");
            sections.Add("Health: Diet");
            sections.Add("Physical Activity: Work");
            sections.Add("Physical Activity: Travel to and from places");
            sections.Add("Physical Activity: Recreational Activities");
            sections.Add("Physical Activity: Sedentary Behaviour");
            sections.Add("History of Raised Blood Pressure");
            sections.Add("History of Diabetes");
            sections.Add("History of High Cholesterol");
            sections.Add("History of Immunisation");
            sections.Add("Physical Measurements: Blood Pressure");
            sections.Add("Physical Measurements: Anthropometry");
            sections.Add("Blood Sample");
            sections.Add("Advice");
            sections.Add("Final Comments");
            */


            //SA
            sections.Add("Consents");
            sections.Add("Personal History");
            sections.Add("Zulu");
            sections.Add("Family History");
            sections.Add("Diabetes");
            sections.Add("Clinical Information");
            sections.Add("Clinical Measurements");



            return sections;

            



        }



        public static string getPosition(string nmeaNumber, string hemisphere, bool pureDecimal)
        {

            //convert the NMEA string data into a valid position
            //nmea number is Latitude or longitude
            //nmea hemisphere is N,E,S or W
            //pureDecimal: return with hemisphere as + for North, - for South, + for East, - for West


            /*
             * 
             * NMEA Format:
             * 
            $GPGGA,123519,4807.038,N,01131.000,E,1,08,0.9,545.4,M,46.9,M,,*47

            Where:
                 GGA          Global Positioning System Fix Data
                 123519       Fix taken at 12:35:19 UTC
                 4807.038,N   Latitude 48 deg 07.038' N
                 01131.000,E  Longitude 11 deg 31.000' E
                 1            Fix quality: 0 = invalid
                                           1 = GPS fix (SPS)
                                           2 = DGPS fix
                                           3 = PPS fix
			                   4 = Real Time Kinematic
			                   5 = Float RTK
                                           6 = estimated (dead reckoning) (2.3 feature)
			                   7 = Manual input mode
			                   8 = Simulation mode
                 08           Number of satellites being tracked
                 0.9          Horizontal dilution of position
                 545.4,M      Altitude, Meters, above mean sea level
                 46.9,M       Height of geoid (mean sea level) above WGS84
                                  ellipsoid
                 (empty field) time in seconds since last DGPS update
                 (empty field) DGPS station ID number
                 *47          the checksum data, always begins with *


            */


            //e.g. GPGGA,124053.000,5204.6890,N,00011.1347,E,1,05,3.2,38.0,M,47.0,M,,0000*64
            // latitude component:5204.6890
            //longitude component:00011.1347



            string degs;
            string minutes;


            //extract degs,minutes from nmean string
            //minutes are always the 2 digits left of the decimal point and all of the digits right of it
            //degrees are all the digits left of the minutes, if any.


            Match match = Regex.Match(nmeaNumber, @"^(\d+)(\d\d\.\d+)$");

            if(match.Success){

                degs = match.Groups[1].Value;
                minutes= match.Groups[2].Value;


            }
            else{

                //serious error
                throw new Exception();


            }

            //convert minutes to decimal form by / 60

            decimal converted = Convert.ToDecimal(degs) + (Convert.ToDecimal(minutes) / 60M);

            //round to 12 decimal places
            converted = Math.Round(converted, 12);

            

            //final formatting by adding the hemisphere component as either a sign or symbol
            string finalValue;

            if (pureDecimal)
            {
                //sign
                if (hemisphere == "S" || hemisphere == "W")
                {
                    //- (Note: don't need to do anything for N, E which are +, which is implied)

                    finalValue = "-" + converted.ToString();


                }
                else
                {

                    finalValue = converted.ToString();

                }
                



            }
            else
            {
                //symbol: Add N,S,E,W letter at start
                finalValue = hemisphere + converted.ToString();





            }

            return finalValue;






        }















    }
}
