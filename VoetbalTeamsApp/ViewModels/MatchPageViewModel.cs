using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VoetbalTeamsApp.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VoetbalTeamsApp.ViewModels
{
    public class MatchPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        Match _currentmatch;
        public Match CurrentMatch { get { return _currentmatch; } set { _currentmatch = value; OnPropertyChanged(); }}

        Visibility _isshowingmatch = Visibility.Collapsed;
        public Visibility IsShowingMatch {get {return _isshowingmatch; } set
            {
                _isshowingmatch = value;
                OnPropertyChanged();
                IsShowingList = _isshowingmatch == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                OnPropertyChanged(nameof(IsShowingList));
            }
        }
        public Visibility IsShowingList { get; set; }

        Goal _newhomegoal = new Goal(0, 1, 0, null);
        public Goal NewHomeGoal { get { return _newhomegoal; } set { _newhomegoal = value;OnPropertyChanged(); } }
        Goal _newawaygoal= new Goal(0, 1, 0, null);
        public Goal NewAwayGoal { get { return _newawaygoal; } set { _newawaygoal = value; OnPropertyChanged(); } }

        public Goal SelectedHomeGoal { get; set; }
        public Goal SelectedAwayGoal { get; set; }
        public DataBase Db
        {
            get => ((App)Application.Current).Db;
            set
            {
                ((App)Application.Current).Db = value;
            }
        }

        //===========================================
        // Initiator
        //===========================================
        public MatchPageViewModel()
        {
        }

        public async Task CreateNewMatch()
        {
            Club homeclub = Db.Clubs[0];
            Club awayclub = Db.Clubs[0];
            IEnumerable<Club> query = Db.Clubs.Where(club => club.Id == 1);
            foreach (Club club in query)
            {
                homeclub = club;
            }
            query = Db.Clubs.Where(club => club.Id == 2);
            foreach (Club club in query)
            {
                awayclub = club;
            }
            CurrentMatch = new Match(homeclub, awayclub);
            Db.Matches.Add(CurrentMatch);
            IsShowingMatch = Visibility.Visible;
        }

        public async Task DeleteHomeGoal()
        {
            if (SelectedHomeGoal != null)
            {
                CurrentMatch.HomeGoals.Remove(SelectedHomeGoal);
                Db.RemoveGoal(SelectedHomeGoal);
            }
        }
        public async Task DeleteAwayGoal()
        {
            if (SelectedAwayGoal != null)
            {
                CurrentMatch.AwayGoals.Remove(SelectedAwayGoal);
                Db.RemoveGoal(SelectedAwayGoal);
            }
        }
        public async Task AddHomeGoal() 
        {
            if(NewHomeGoal.Scorer != null)
            {
                NewHomeGoal.MatchId = CurrentMatch.Id;
                CurrentMatch.HomeGoals.Add(NewHomeGoal);
                Db.AddGoal(NewHomeGoal);
                NewHomeGoal = new Goal(CurrentMatch.Id,1, 0, null);
            }
        }
        public async Task AddAwayGoal()
        {
            if(NewAwayGoal.Scorer != null)
            {
                NewAwayGoal.MatchId = CurrentMatch.Id;
                CurrentMatch.AwayGoals.Add(NewAwayGoal);
                Db.AddGoal(NewAwayGoal);
                NewAwayGoal = new Goal(CurrentMatch.Id,0, 0, null);
            }
        }
        public async Task ShowList()
        {
            IsShowingMatch = Visibility.Collapsed;
        }

        public void ShowMatch(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            CurrentMatch = (Match)button.DataContext;
            IsShowingMatch = Visibility.Visible;
        }

        public void DeleteMatch(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            CurrentMatch = (Match)button.DataContext;
            //remove goal by setting club to null;
            CurrentMatch.HomeClub = null;
            CurrentMatch.AwayClub = null;
            Db.Matches.Remove(CurrentMatch);
        }
    }
}
