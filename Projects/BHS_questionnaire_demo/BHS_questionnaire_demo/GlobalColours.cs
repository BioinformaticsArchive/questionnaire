
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
using System.Drawing;

namespace BHS_questionnaire_demo
{
    class GlobalColours

    {



        //colours for the Questionnaire



        public static Color mainFormColour
        {

            
            //white fonts version
            get { return Color.FromArgb(0x0, 0x0, 0x0); }

            
            //black fonts version
            //get { return Color.FromArgb(0xFF, 0xFF, 0xFF); }

            


        }

        public static Color mainPanelColour
        {

            //black fonts version
            //get { return Color.FromArgb(0xF0, 0xF0, 0xF0); }
            
            
            //white fonts version
            get { return Color.FromArgb(0x40, 0x40, 0x40); }

          

        }

        public static Color mainButtonColour
        {
            //get { return Color.FromArgb(0x92, 0xA6, 0x8A); }

            get { return Color.FromArgb(0xC8, 0xC8, 0xC8); }

        }


        public static Color altButtonColour
        {

           // get { return Color.FromArgb(0xBC, 0xC4, 0x99); }

            get { return Color.FromArgb(0x88, 0x88, 0x88); }

        }

        public static Color controlBackColour
        {

            get { return Color.FromArgb(0xF5, 0xDD, 0x9D); }
        }


        public static Color mainTextColour
        {
            
            //black fonts version
            //get { return Color.FromArgb(0x0, 0x0, 0x0); }
            
            
            
            //white fonts version
            get { return Color.FromArgb(0xFF, 0xFF, 0xFF); }
            

        }




    }
}
