
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
    class GPSCountryManager
    {

        
        //maps country name to object that contains the limits for gps coordinates
        private GPSCountry thisCountry;

        private string countryName;

        public GPSCountryManager(string countryName)
        {

            this.countryName = countryName;

            if (countryName == "Any")
            {

                thisCountry = new GPSCountry("Any", 90M, -90M, 180M, -180M);

            }
            else if (countryName == "United Kingdom")
            {

                thisCountry= new GPSCountry("United Kingdom", 60.865M, 49.865M, 1.7676M, -8.1828M);



            }
            else if (countryName == "Malawi")
            {
                thisCountry= new GPSCountry("Malawi", -9.3636M, -17.1396M, 35.9244M, 32.6772M);


            }
            else
            {
                throw new Exception();

            }




        }


        public bool checkLimits(decimal latitude, decimal longitude)
        {

            return thisCountry.checkLimits(latitude, longitude);


        }

        public string getSelectedCountryName()
        {
            return countryName;



        }
















    }
}
