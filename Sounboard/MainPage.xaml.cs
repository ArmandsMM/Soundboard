using Sounboard.model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Sounboard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ObservableCollection<Sound> Sounds;
        private List<String> Suggestions;
        private List<MenuItem> MenuItems;

        public MainPage()
        {
            this.InitializeComponent();
            Sounds = new ObservableCollection<Sound>();
            SoundManager.GetAllSounds(Sounds);

            MenuItems = new List<MenuItem>();
            MenuItems.Add(new MenuItem { IconFile = "/Assets/Icons/animals.png", Category = SoundCategory.Animals});
            MenuItems.Add(new MenuItem { IconFile = "/Assets/Icons/cartoon.png", Category = SoundCategory.Cartoons });
            MenuItems.Add(new MenuItem { IconFile = "/Assets/Icons/taunt.png", Category = SoundCategory.Animals });
            MenuItems.Add(new MenuItem { IconFile = "/Assets/Icons/warning.png", Category = SoundCategory.Warnings });

            backButton.Visibility = Visibility.Collapsed;
        }

        private void hamburgerbutton_Click(object sender, RoutedEventArgs e)
        {
            splitViewName.IsPaneOpen = !splitViewName.IsPaneOpen;
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            SoundManager.GetAllSounds(Sounds);
            menuItemsListView.SelectedItem = null;
            CategoryTextBlock.Text = "All sounds";
            backButton.Visibility = Visibility.Collapsed;
        }

        private void searchAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            SoundManager.GetAllSounds(Sounds);            
            Suggestions = Sounds.Where(p => p.Name.Contains(sender.Text)).Select(p => p.Name).ToList();
            sender.ItemsSource = Suggestions;

           
            //CategoryTextBlock.Text = Suggestions.First();
        }

        private void searchAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            SoundManager.getSoundsByName(Sounds, sender.Text);
            menuItemsListView.SelectedItem = null;
            CategoryTextBlock.Text = sender.Text;
            backButton.Visibility = Visibility.Visible;            
        }

        private void menuItemsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var menuItem = (MenuItem)e.ClickedItem;
            //filter on category
            CategoryTextBlock.Text = menuItem.Category.ToString();

            SoundManager.getSoundsByCategory(Sounds, menuItem.Category);
            backButton.Visibility = Visibility.Visible;
        }

        private void soundGridview_ItemClick(object sender, ItemClickEventArgs e)
        {
            var sound = (Sound)e.ClickedItem;
            myMediaElement.Source = new Uri(this.BaseUri, sound.AudioFile);
        }

        private async void soundGridview_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems)) 
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Any())
                {
                    var storageFile = items.First() as StorageFile;
                    var contentType = storageFile.ContentType;
                    StorageFolder folder = ApplicationData.Current.LocalFolder;

                    if (contentType == "audio/wav" || contentType == "audio/mpeg")
                    {
                        StorageFile newFile = await storageFile.CopyAsync(folder, storageFile.Name, NameCollisionOption.ReplaceExisting);
                        myMediaElement.SetSource(await storageFile.OpenAsync(FileAccessMode.Read), contentType);
                        myMediaElement.Play();
                    }
                }
            }
        }

        private void soundGridview_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.Caption = "drop to play this item";
            e.DragUIOverride.IsCaptionVisible = true;
            e.DragUIOverride.IsContentVisible = true;
            e.DragUIOverride.IsGlyphVisible = true;
        }
    }
}
