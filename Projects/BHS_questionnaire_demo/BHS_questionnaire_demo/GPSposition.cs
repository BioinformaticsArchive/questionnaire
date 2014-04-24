
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
    class GPSposition
    {

        private string latitude;
        private string longitude;

        public GPSposition(string latitude, string longitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;




        }


        public string getXMLposition(string icon)
        {

            StringBuilder sb = new StringBuilder();
            sb.Append("<Placemark>");
            sb.Append("<styleUrl>");
            sb.Append(icon);
            sb.Append("</styleUrl>");
            sb.Append("<Point>");
            sb.Append("<coordinates>");
            sb.Append(longitude);
            sb.Append(",");
            sb.Append(latitude);
            sb.Append("</coordinates>");
            sb.Append("</Point>");
            sb.Append("</Placemark>");

            return sb.ToString();



        }












    }
}
