using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading;
using Microsoft.Win32;
using Ultima;

namespace UOArtMerge
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property. 
        // The CallerMemberName attribute that is applied to the optional propertyName 
        // parameter causes the property name of the caller to be substituted as an argument. 
        public void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

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

        public ArtSet ArtSet1 { get; set; }
        public ArtSet ArtSet2 { get; set; }
        public BindingList<ArtAsset> ClipBoardItems { get; set; }
        public BindingList<ArtAsset> ClipBoardLand { get; set; }
        private bool m_linked = true;

        private bool m_displayItemData = true;
        public bool DisplayItemData
        {
            get
            {
                return m_displayItemData;
            }

            set
            {
                m_displayItemData = value;
                NotifyPropertyChanged("DisplayItemData");
            }
        }

        private bool m_modifyTileData = true;
        public bool ModifyTileData
        {
            get
            {
                return m_modifyTileData;
            }

            set
            {
                m_modifyTileData = value;
                NotifyPropertyChanged("ModifyTileData");
            }
        }

        #region scrollbar linking

        public Visual GetDescendantByType(Visual element, Type type)
        {
            if (element == null) return null;
            if (element.GetType() == type) return element;
            Visual foundElement = null;
            if (element is FrameworkElement)
            {
                (element as FrameworkElement).ApplyTemplate();
            }
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                Visual visual = VisualTreeHelper.GetChild(element, i) as Visual;
                foundElement = GetDescendantByType(visual, type);
                if (foundElement != null)
                    break;
            }
            return foundElement;
        }

        private void ArtSet1_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (!m_linked)
            {
                return;
            }

            ScrollViewer _listboxScrollViewer1 = GetDescendantByType(ArtList1, typeof(ScrollViewer)) as ScrollViewer;
            ScrollViewer _listboxScrollViewer2 = GetDescendantByType(ArtList2, typeof(ScrollViewer)) as ScrollViewer;
            _listboxScrollViewer2.ScrollToVerticalOffset(_listboxScrollViewer1.VerticalOffset);
        }

        private void ArtSet2_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (!m_linked)
            {
                return;
            }

            ScrollViewer _listboxScrollViewer1 = GetDescendantByType(ArtList1, typeof(ScrollViewer)) as ScrollViewer;
            ScrollViewer _listboxScrollViewer2 = GetDescendantByType(ArtList2, typeof(ScrollViewer)) as ScrollViewer;
            _listboxScrollViewer1.ScrollToVerticalOffset(_listboxScrollViewer2.VerticalOffset);
        }

        private void LandSet1_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (!m_linked)
            {
                return;
            }

            ScrollViewer _listboxScrollViewer1 = GetDescendantByType(LandList1, typeof(ScrollViewer)) as ScrollViewer;
            ScrollViewer _listboxScrollViewer2 = GetDescendantByType(LandList2, typeof(ScrollViewer)) as ScrollViewer;
            _listboxScrollViewer2.ScrollToVerticalOffset(_listboxScrollViewer1.VerticalOffset);
        }

        private void LandSet2_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (!m_linked)
            {
                return;
            }

            ScrollViewer _listboxScrollViewer1 = GetDescendantByType(LandList1, typeof(ScrollViewer)) as ScrollViewer;
            ScrollViewer _listboxScrollViewer2 = GetDescendantByType(LandList2, typeof(ScrollViewer)) as ScrollViewer;
            _listboxScrollViewer1.ScrollToVerticalOffset(_listboxScrollViewer2.VerticalOffset);
        }



        #endregion

        public MainWindow()
        {
            InitializeComponent();
            ClipBoardItems = new BindingList<ArtAsset>();
            ClipBoardLand = new BindingList<ArtAsset>();
            NotifyPropertyChanged("ClipBoardItems");
            NotifyPropertyChanged("ClipBoardLand");
        }

        private void Link_Click(object sender, RoutedEventArgs e)
        {
            m_linked = !m_linked;

            if (m_linked)
            {
                LinkedButtonpath.Style = Resources["LinkedIcon"] as System.Windows.Style;
            }
            else
            {
                LinkedButtonpath.Style = Resources["UnlinkedIcon"] as System.Windows.Style;
            }

            NotifyPropertyChanged("LinkedButtonText");
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
            if (button == null)
            {
                return;
            }

            if (m_displayItemData)
            {
                if (button.CommandParameter.ToString() == "1")
                {
                    DeleteSelectedItemsFromList(ArtList1.SelectedItems, ArtSet1.Items);
                }
                else if (button.CommandParameter.ToString() == "2")
                {
                    DeleteSelectedItemsFromList(ArtList2.SelectedItems, ArtSet2.Items);
                }
            }
            else
            {
                if (button.CommandParameter.ToString() == "1")
                {
                    DeleteSelectedItemsFromList(LandList1.SelectedItems, ArtSet1.Land);
                }
                else if (button.CommandParameter.ToString() == "2")
                {
                    DeleteSelectedItemsFromList(LandList2.SelectedItems, ArtSet2.Land);
                }
            }
        }

        private void Click_Load(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
            if (button == null)
            {
                return;
            }

            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string path = fbd.SelectedPath;

                if (button.CommandParameter.ToString() == "1")
                {
                    ArtSet1 = new ArtSet(path);
                    ArtSet1.Load();
                    NotifyPropertyChanged("ArtSet1");
                }
                else if (button.CommandParameter.ToString() == "2")
                {
                    ArtSet2 = new ArtSet(path);
                    ArtSet2.Load();
                    NotifyPropertyChanged("ArtSet2");
                }
            }
        }

        private void Click_Save(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
            if (button == null)
            {
                return;
            }

            if (button.CommandParameter.ToString() == "1")
            {
                SaveDialog dialog = new SaveDialog(1, this);
                dialog.ShowDialog();
            }
            else if (button.CommandParameter.ToString() == "2")
            {
                ArtSet2.Save();
            }
        }

        public void Save(int set)
        {
            if (set == 1)
            {
                ArtSet1.Save();
            }
            else if (set == 2)
            {
                ArtSet2.Save();
            }
        }

        private void ClearClipboard_Click(object sender, RoutedEventArgs e)
        {
            if (m_displayItemData)
            {
                ClipBoardItems.Clear();
            }
            else
            {
                ClipBoardLand.Clear();
            }
        }

        private void DeleteItemsFromClipboard_Click(object sender, RoutedEventArgs e)
        {
            if (m_displayItemData)
            {
                RemoveSelectedItemsFromList(ClipboardItemList.SelectedItems, ClipBoardItems);
            }
            else
            {
                RemoveSelectedItemsFromList(ClipboardLandList.SelectedItems, ClipBoardLand);
            }
        }

        private void RemoveSelectedItemsFromList(IList selectedItems, BindingList<ArtAsset> list)
        {
            List<ArtAsset> removables = new List<ArtAsset>();

            foreach (ArtAsset asset in selectedItems)
            {
                removables.Add(asset);
            }

            foreach (ArtAsset asset in removables)
            {
                list.Remove(asset);
            }
        }

        private void DeleteSelectedItemsFromList(IList selectedItems, BindingList<ArtAsset> list)
        {
            List<ArtAsset> removables = new List<ArtAsset>();

            foreach (ArtAsset asset in selectedItems)
            {
                removables.Add(asset);
            }

            foreach (ArtAsset asset in removables)
            {
                asset.DeleteArt();
                if (ModifyTileData)
                {
                    asset.DeleteTileData();
                }
                asset.Bmp = null;
                asset.BmpImage = null;
                list.Remove(asset);
                list.Insert(asset.Index, asset);
            }
        }

        private void CopyFromClipboard_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
            if (button == null)
            {
                return;
            }

            if (m_displayItemData)
            {
                if (button.CommandParameter.ToString() == "1" && ArtSet1 != null)
                {
                    MoveSelectedItems(ClipboardItemList.SelectedItems, ArtSet1, ArtList1.SelectedItems);
                }
                else if (button.CommandParameter.ToString() == "2" && ArtSet2 != null)
                {
                    MoveSelectedItems(ClipboardItemList.SelectedItems, ArtSet2, ArtList2.SelectedItems);
                }
            }
            else
            {
                if (button.CommandParameter.ToString() == "1" && ArtSet1 != null)
                {
                    MoveSelectedItems(ClipboardLandList.SelectedItems, ArtSet1, LandList1.SelectedItems);
                }
                else if (button.CommandParameter.ToString() == "2" && ArtSet2 != null)
                {
                    MoveSelectedItems(ClipboardLandList.SelectedItems, ArtSet2, LandList2.SelectedItems);
                }
            }
        }

        private void CopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
            if (button == null)
            {
                return;
            }

            IList selectedItems = null;
            BindingList<ArtAsset> targetList = null;

            if (m_displayItemData)
            {
                targetList = ClipBoardItems;
                if (button.CommandParameter.ToString() == "1")
                {
                    selectedItems = ArtList1.SelectedItems;
                }
                else
                {
                    selectedItems = ArtList2.SelectedItems;
                }
            }
            else
            {
                targetList = ClipBoardLand;
                if (button.CommandParameter.ToString() == "1")
                {
                    selectedItems = LandList1.SelectedItems;
                }
                else
                {
                    selectedItems = LandList2.SelectedItems;
                }
            }

            if (selectedItems != null && selectedItems.Count > 0)
            {
                foreach (ArtAsset asset in selectedItems)
                {
                    ArtAsset newAsset = asset.Clone() as ArtAsset;
                    if (newAsset != null)
                    {
                        if (ModifyTileData)
                        {
                            newAsset.ItemDatum = asset.ItemDatum;
                            newAsset.LandDatum = asset.LandDatum;
                        }
                        newAsset.Index = -1;
                        newAsset.ArtInstance = null;
                        newAsset.TileDataInstance = null;
                        targetList.Add(newAsset);
                    }
                }
            }
        }

        private void MoveRight_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
            if (button == null)
            {
                return;
            }

            if (m_displayItemData)
            {
                MoveSelectedItems(ArtList1.SelectedItems, ArtSet2, ArtList2.SelectedItems);
            }
            else
            {
                MoveSelectedItems(LandList1.SelectedItems, ArtSet2, LandList2.SelectedItems);
            }
        }

        private void MoveSelectedItems(IList sourceSelection, ArtSet destination, IList destinationSelection)
        {
            if (sourceSelection == null || destination == null || destinationSelection == null || destinationSelection.Count < 1)
            {
                return;
            }

            ArtAsset destAsset = destinationSelection[0] as ArtAsset;

            if (destAsset == null)
            {
                return;
            }

            int idx = destAsset.Index;

            if (idx < 0 || idx >= destination.Items.Count)
            {
                return;
            }

            foreach (ArtAsset asset in sourceSelection)
            {
                if (idx < destination.Items.Count)
                {
                    ArtAsset clone = asset.Clone() as ArtAsset;
                    if (clone != null)
                    {
                        clone.Index = idx;
                        clone.ArtInstance = destination.ArtInstance;
                        clone.TileDataInstance = destination.TileDataInstance;

                        if (ModifyTileData)
                        {
                            clone.ItemDatum = asset.ItemDatum;
                            clone.LandDatum = asset.LandDatum;
                        }
                        else
                        {



                            ArtAsset existingDestinationAsset = null;

                            if (m_displayItemData)
                            {
                                existingDestinationAsset = destination.Items[idx];
                            }
                            else
                            {
                                existingDestinationAsset = destination.Land[idx];
                            }

                            if (existingDestinationAsset != null)
                            {
                                clone.ItemDatum = existingDestinationAsset.ItemDatum;
                                clone.LandDatum = existingDestinationAsset.LandDatum;
                            }
                        }

                        if (m_displayItemData)
                        {
                            destination.Items.RemoveAt(idx);
                            destination.Items.Insert(idx, clone);
                        }
                        else
                        {
                            destination.Land.RemoveAt(idx);
                            destination.Land.Insert(idx, clone);
                        }
                        clone.Save();
                        idx++;
                    }
                }
            }
        }

        private void MoveLeft_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
            if (button == null)
            {
                return;
            }

            if (m_displayItemData)
            {
                MoveSelectedItems(ArtList2.SelectedItems, ArtSet1, ArtList1.SelectedItems);
            }
            else
            {
                MoveSelectedItems(LandList2.SelectedItems, ArtSet1, LandList1.SelectedItems);
            }
        }
    }
}
