using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VoetbalTeamsApp.Models;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace VoetbalTeamsApp.ViewModels
{
    public class PlayerListPageViewModel : INotifyPropertyChanged
    {
        /*
         * =====================
         *  ON PROPERTY CHANGED
         * =====================
         */
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        

        public DataBase Db
        {
            get
            {
                return ((App)Application.Current).Db;
            }
            set
            {
                ((App)Application.Current).Db = value;
                OnPropertyChanged();
            }
        }

        private Player _selectedPlayer;
        public Player SelectedPlayer
        {
            get => _selectedPlayer;
            set
            {
                if (_selectedPlayer != value)
                {
                    _selectedPlayer = value;
                    OnPropertyChanged();
                }
            }
        }
        private Player _newPlayer;
        public Player NewPlayer
        {
            get => _newPlayer;
            set
            {
                if (_newPlayer != value)
                {
                    _newPlayer = value;
                    OnPropertyChanged();
                }
            }
        }
        private bool _addingNewPlayer = false;
        public bool AddingNewPlayer
        {
            get => _addingNewPlayer;
            set
            {
                if (_addingNewPlayer != value)
                {
                    _addingNewPlayer = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(EnableCommandBar));
                    //mainCommandBar.IsEnabled = !value;
                    //newCustomerPanel.Visibility = (Visibility)Convert.ToInt32(!value);
                }
            }
        }
        public bool EnableCommandBar => !AddingNewPlayer;

        /*
         * ========================================================================================
         * =====================================  Initiating  ====================================
         * ========================================================================================
         */

        public PlayerListPageViewModel()
        {

            NewPlayer = new Player("", "", 0, 0, Position.Keeper,DataBase.ClubLess);
            //this.grid.ScrollIndexIntoView(100);
        }

        public async Task DeletePlayerAsync()
        {
            if (SelectedPlayer != null)
            {
                // Create the message dialog and set its content
                var messageDialog = new MessageDialog("Are you sure you want to delete " + SelectedPlayer.FirstName);

                // Add commands and set their callbacks; both buttons use the same callback function instead of inline event handlers
                messageDialog.Commands.Add(new UICommand("Yes, Delete", new UICommandInvokedHandler(this.DeletePlayer)));
                messageDialog.Commands.Add(new UICommand("No"));

                // Set the command that will be invoked by default
                messageDialog.DefaultCommandIndex = 0;

                // Set the command to be invoked when escape is pressed
                messageDialog.CancelCommandIndex = 1;

                // Show the message dialog
                await messageDialog.ShowAsync();
            }
        }
        void DeletePlayer(IUICommand command)
        {
            Db.Players.Remove(SelectedPlayer);
        }
        
        public async Task CreateNewPlayerAsync()
        {
            AddingNewPlayer = true;
        }

        public async Task CancelNewPlayer()
        {
            AddingNewPlayer = false;
        }
        public async Task SaveNewPlayer()
        {
            try
            {
                if (NewPlayer.FirstName.Length == 0) { throw new Exception("FirstName Cant be Empty"); }
                if (NewPlayer.LastName.Length == 0) { throw new Exception("LastName Cant be Empty"); }
                if (NewPlayer.Age == 0) { throw new Exception("Age Cant be 0"); }
                if (NewPlayer.Salary == 0) { throw new Exception("Salary Cant be 0"); }

                Db.Players.Add(NewPlayer);

                AddingNewPlayer = false;
                NewPlayer = new Player("", "", 0, 0, Position.Keeper, DataBase.ClubLess);
            }
            catch (Exception exc)
            {
                var message = new MessageDialog(exc.Message);
                //message.Title = "Error";
                message.Commands.Add(new UICommand("OK"));
                await message.ShowAsync();
            }
        }
    }
}