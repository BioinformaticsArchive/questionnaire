
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
    class GPSCountry
    {

        //properties
        public decimal limitNorth { get; set; }
        public decimal limitSouth { get; set; }
        public decimal limitEast { get; set; }
        public decimal limitWest { get; set; }

        public string countryName { get; set; }


        public GPSCountry(string countryName, decimal limitNorth, decimal limitSouth, decimal limitEast, decimal limitWest)
        {
            this.countryName = countryName;
            this.limitNorth = limitNorth;
            this.limitSouth = limitSouth;
            this.limitEast = limitEast;
            this.limitWest = limitWest;


        }


        public bool checkLimits(decimal latitude, decimal longitude){

            //is this latitude within the allowed limits for this country ?

            if ((latitude > limitNorth) || (latitude < limitSouth))
            {
                //error
                return false;

            }



            //is this longitude within the allowed limits for this country ?
            if ((longitude > limitEast) || (longitude < limitWest))
            {
                //error
                return false;

            }


            return true;



        }














    }
}
