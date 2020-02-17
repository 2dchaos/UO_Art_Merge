/*
* Copyright (C) 2013 Ian Karlinsey
* 
* 
* UOArtMerge is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
* 
* UOArtMerge is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
* 
* You should have received a copy of the GNU General Public License
* along with UltimaLive.  If not, see <http://www.gnu.org/licenses/>. 
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UoArtMerge
{
    public class BitmapPair : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        // Create the OnPropertyChanged method to raise the event 
        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

        private int m_index;
        public int Index
        {
            get
            {
                return m_index;
            }

            set
            {
                m_index = value;
                OnPropertyChanged("Index");
            }
        }

        private BitmapImage m_image1;
        public BitmapImage Image1
        {
            get
            {
                return m_image1;
            }

            set
            {
                m_image1 = value;
                OnPropertyChanged("Image1");
            }
        }

        private BitmapImage m_image2;
        public BitmapImage Image2
        {
            get
            {
                return m_image2;
            }

            set
            {
                m_image2 = value;
                OnPropertyChanged("Image2");
            }
        }
    }
}
