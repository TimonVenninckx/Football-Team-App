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
    public class LeagueTablePageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public DataBase Db
        {
            get => ((App)Application.Current).Db;
            set
            {
                ((App)Application.Current).Db = value;
                OnPropertyChanged();
            }
        }
        private Club _selectedClub;
        public Club SelectedClub { 
            get =>_selectedClub;
            set {
                _selectedClub = value;
                OnPropertyChanged();
            } 
        }
        //let them change manager in club view menu
        private Club _newClub = new Club("", new Coach("Timon", "Venninckx", 20, 2000f));
        public Club NewClub
        {
            get => _newClub;
            set
            {
                _newClub = value;
                OnPropertyChanged();
            }
        }
        private bool _addingNewClub = false;
        public bool AddingNewClub
        {
            get => _addingNewClub;
            set
            {
                if (_addingNewClub != value)
                {
                    _addingNewClub = value;
                    OnPropertyChanged();
                }
            }
        }
        private Visibility _gridVisibility = Visibility.Visible;
        public Visibility GridVisibility
        {
            get => _gridVisibility;
            set
            {
                _gridVisibility = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ClubViewVisibility));
            }
        }
        public Visibility ClubViewVisibility { get 
            {
                if (GridVisibility == Visibility.Visible)
                {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            } 
        }
        public LeagueTablePageViewModel()
        {

        }
        public async Task DeleteClubAsync() {
            if (SelectedClub != null)
            {
                // Create the message dialog and set its content
                var messageDialog = new MessageDialog("Are you sure you want to delete " + SelectedClub.Name);

                // Add commands and set their callbacks; both buttons use the same callback function instead of inline event handlers
                messageDialog.Commands.Add(new UICommand("Yes, Delete", new UICommandInvokedHandler(this.DeleteClub)));
                messageDialog.Commands.Add(new UICommand("No"));

                // Set the command that will be invoked by default
                messageDialog.DefaultCommandIndex = 0;

                // Set the command to be invoked when escape is pressed
                messageDialog.CancelCommandIndex = 1;

                // Show the message dialog
                await messageDialog.ShowAsync();
            }
        }
        private void DeleteClub(IUICommand command)
        {
            SelectedClub.RemoveAllPlayers();
            Db.Clubs.Remove(SelectedClub);
            SelectedClub = null;
        }

        public async Task CreateNewClubAsync()
        {
            AddingNewClub = true;
        }

        public async Task CancelNewClubAsync()
        {
            AddingNewClub = false;
        }
        ///<summary>
        ///Try to create new Club or Show error message
        ///</summary>
        public async Task SaveNewClubAsync()
        {
            try
            {
                if (NewClub.Name == null) { throw new Exception("ClubName Cant be Empty"); }
                Db.Clubs.Add(NewClub);
                NewClub = new Club("", new Coach("test", "tester", 20, 2000f));
                AddingNewClub = false;
            }
            catch (Exception exc)
            {
                var message = new MessageDialog(exc.Message);
                //message.Title = "Error";
                message.Commands.Add(new UICommand("OK"));
                await message.ShowAsync();
            }
        }

        ///<summary>
        ///show club details in window
        ///</summary>
        public async Task ShowClubAsync()
        {
            if(SelectedClub != null)
            {
                GridVisibility = Visibility.Collapsed;
            }
        }
        public async Task GoBackAsync()
        {
            GridVisibility = Visibility.Visible;
        }

    }
}
