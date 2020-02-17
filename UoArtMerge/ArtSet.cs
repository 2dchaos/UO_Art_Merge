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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

using Ultima;

namespace UOArtMerge
{
    public class ArtAsset : INotifyPropertyChanged, ICloneable
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

        #region Static Helper Methods

        public static BitmapImage GetBitmapImage(Bitmap bmp)
        {
            if (bmp == null)
            {
                return null;
            }

            byte[] imageBytes = null;
            using (MemoryStream stream = new MemoryStream())
            {
                bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                imageBytes = stream.ToArray();
            }

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(imageBytes);
            bitmapImage.EndInit();
            return bitmapImage;
        }

        static bool ArraysEqual<T>(T[] a1, T[] a2)
        {
            if (ReferenceEquals(a1, a2))
                return true;

            if (a1 == null || a2 == null)
                return false;

            if (a1.Length != a2.Length)
                return false;

            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < a1.Length; i++)
            {
                if (!comparer.Equals(a1[i], a2[i])) return false;
            }
            return true;
        }
        #endregion

        public void Save()
        {
            if (ArtInstance == null)
            {
                return;
            }

            if (m_type == ArtType.Item)
            {
                ArtInstance.ReplaceStatic(Index, m_bmp);
            }
            else
            {
                ArtInstance.ReplaceLand(Index, m_bmp);
            }

            if (TileDataInstance != null)
            {
                if (m_type == ArtType.Item)
                {
                    if (m_itemDatum.HasValue)
                    {
                        TileDataInstance.ItemTable[Index] = m_itemDatum.Value;
                    }
                    else
                    {
                        TileDataInstance.ItemTable[Index] = new ItemData();
                    }
                }
                else if (m_type == ArtType.Land)
                {
                    if (m_landDatum.HasValue)
                    {
                        TileDataInstance.LandTable[Index] = m_landDatum.Value;
                    }
                    else
                    {
                        TileDataInstance.LandTable[Index] = new LandData();
                    }
                }
            }
        }

        public void DeleteArt()
        {
            if (ArtInstance == null)
            {
                return;
            }

            if (m_type == ArtType.Item)
            {
                ArtInstance.RemoveStatic(Index);
            }
            else
            {
                ArtInstance.RemoveLand(Index);
            }
        }

        public void DeleteTileData()
        {
            if (TileDataInstance != null)
            {
                if (m_type == ArtType.Land)
                {
                    TileDataInstance.LandTable[m_index] = new LandData();
                }
                else if (m_type == ArtType.Item)
                {
                    TileDataInstance.ItemTable[m_index] = new ItemData();
                }
            }
        }

        private BitmapImage m_bmpImage;
        public BitmapImage BmpImage
        {
            get
            {
                if (m_bmpImage == null)
                {
                    m_bmpImage = GetBitmapImage(Bmp);
                }

                return m_bmpImage;
            }

            set
            {
                m_bmpImage = value;
                OnPropertyChanged("BmpImage");
            }
        }

        private Bitmap m_bmp;
        public Bitmap Bmp
        {
            get
            {
                if (m_bmp == null)
                {
                    if (m_type == ArtType.Item && m_art != null)
                    {
                        m_bmp = m_art.GetStatic(m_index);
                    }
                    else if (m_type == ArtType.Land && m_art != null)
                    {
                        m_bmp = m_art.GetLand(m_index);
                    }
                }

                return m_bmp;
            }

            set
            {
                m_bmp = value;
                OnPropertyChanged("Bmp");
            }
        }

        private TileData m_tileDataInstance;
        public TileData TileDataInstance
        {
            get
            {
                return m_tileDataInstance;
            }

            set
            {
                m_tileDataInstance = value;
                OnPropertyChanged("TileData");
            }
        }


        private LandData? m_landDatum;
        public LandData? LandDatum
        {
            get
            {
                return m_landDatum;
            }

            set
            {
                m_landDatum = value;
                OnPropertyChanged("LandDatum");
            }
        }

        private ItemData? m_itemDatum;
        public ItemData? ItemDatum
        {
            get
            {
                return m_itemDatum;
            }

            set
            {
                m_itemDatum = value;
                OnPropertyChanged("ItemDatum");
            }
        }

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

        private Art m_art;
        public Art ArtInstance
        {
            get
            {
                return m_art;
            }

            set
            {
                m_art = value;
                OnPropertyChanged("ArtInstance");
            }
        }

        private ArtType m_type;

        public enum ArtType
        {
            Land,
            Item
        }

        public ArtAsset(int idx, Art art, ArtType type, LandData? landDatum, ItemData? itemDatum, TileData tileData)
        {
            Index = idx;
            m_art = art;
            m_type = type;
            m_landDatum = landDatum;
            m_itemDatum = itemDatum;
            m_tileDataInstance = tileData;
        }

        public object Clone()
        {
            ArtAsset newAsset = new ArtAsset(Index, m_art, m_type, null, null, m_tileDataInstance);
            newAsset.m_bmp = this.m_bmp;
            newAsset.m_bmpImage = this.m_bmpImage;
            return newAsset;
        }
    }

    public class ArtSet : INotifyPropertyChanged
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

        public string Path { get; set; }
        public Art ArtInstance { get; set; }
        public TileData TileDataInstance { get; set; }
        public BindingList<ArtAsset> Items { get; set; }
        public BindingList<ArtAsset> Land { get; set; }

        public ArtSet(string path)
        {
            Path = path;
            Items = new BindingList<ArtAsset>();
            Land = new BindingList<ArtAsset>();
        }

        public void SetItem(int idx, ArtAsset asset)
        {
            Items[idx] = asset;
            OnPropertyChanged("Items");
        }

        public void Save()
        {
            ArtInstance.Save(Path);
            TileDataInstance.SaveTileData(Path);
        }

        public void Load()
        {
            Items.AllowEdit = true;
            Items.AllowNew = true;
            Items.AllowRemove = true;
            Items.RaiseListChangedEvents = false;

            Land.AllowEdit = true;
            Land.AllowNew = true;
            Land.AllowRemove = true;
            Land.RaiseListChangedEvents = false;

            ArtInstance = new Art(System.IO.Path.Combine(Path, "Art.mul"), System.IO.Path.Combine(Path, "Artidx.mul"));
            TileDataInstance = new TileData(ArtInstance);
            TileDataInstance.Initialize(System.IO.Path.Combine(Path, "TileData.mul"));

            int maxId = ArtInstance.GetMaxItemID();
            for (int i = 0; i < maxId; ++i)
            {
                LandData? landData = null;
                if (i < TileDataInstance.LandTable.Length)
                {
                    landData = TileDataInstance.LandTable[i];
                }

                ItemData? itemData = null;
                if (i < TileDataInstance.ItemTable.Length)
                {
                    itemData = TileDataInstance.ItemTable[i];
                }

                Items.Add(new ArtAsset(i, this.ArtInstance, ArtAsset.ArtType.Item, landData, itemData, TileDataInstance));
            }

            for (int i = 0; i < 0x3FFF; ++i)
            {
                LandData? landData = null;
                if (i < TileDataInstance.LandTable.Length)
                {
                    landData = TileDataInstance.LandTable[i];
                }

                ItemData? itemData = null;
                if (i < TileDataInstance.ItemTable.Length)
                {
                    itemData = TileDataInstance.ItemTable[i];
                }

                Land.Add(new ArtAsset(i, this.ArtInstance, ArtAsset.ArtType.Land, landData, itemData, TileDataInstance));
            }

            Items.RaiseListChangedEvents = true;
            Items.ResetBindings();

            Land.RaiseListChangedEvents = true;
            Land.ResetBindings();
        }
    }
}
