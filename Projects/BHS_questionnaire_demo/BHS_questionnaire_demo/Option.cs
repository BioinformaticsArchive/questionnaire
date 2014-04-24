
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
    class Option
    {
        //An option in a set of radio buttons or select list

        //fields

        private string opValue;
        private string opText;

        private int widgetWidth;
        private int widgetHeight;
        private int widgetXpos;
        private int widgetYpos;


        //properties
        public bool Default { get; set; }

        //this is optional, i.e. only used if this option causes a branch in the processing
        public string ToCode { get; set; }

        //optional: same as ToCode, but also shows an error message
        public string ToCodeErr { get; set; }

        

        //constructor

        public Option(string opValue, string opText)
        {
            this.opValue = opValue;
            this.opText = opText;

        }


        //methods
        public override string ToString()
        {
            return opText;
        }



        public string getValue()
        {
            return opValue;
        }

        public string getText()
        {

            return opText;
        }

        public int getWidgetWidth()
        {

            return widgetWidth;

        }

        public void setWidgetWidth(string width)
        {

            widgetWidth = Convert.ToInt32(width);

        }

        public int getWidgetHeight()
        {

            return widgetHeight;


        }

        public void setWidgetHeight(string height)
        {

            widgetHeight = Convert.ToInt32(height);
        }

        public int getWidgetXpos()
        {

            return widgetXpos;
        }

        public void setWidgetXpos(string xpos)
        {

            widgetXpos = Convert.ToInt32(xpos);


        }

        public void setWidgetYpos(string ypos)
        {

            widgetYpos = Convert.ToInt32(ypos);

        }

        public int getWidgetYpos()
        {

            return widgetYpos;
        }




    }
}
