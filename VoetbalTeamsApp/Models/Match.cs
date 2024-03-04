using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace VoetbalTeamsApp.Models
{
    public class Goal : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public Goal(int matchid ,int ishomegoal,int minute,Player player) {
            MatchId = matchid;
            IsHomeGoal = ishomegoal;
            Minute = minute;
            Scorer = player;
            ScorerId = player != null ? player.Id : 9999;
        }

        /// <summary>
        /// Is int for easier database 0 or 1
        /// </summary>
        public int IsHomeGoal{ get; set; }
        public int MatchId { get; set; }
        Player _scorer;
        public Player Scorer{get => _scorer;
            set
            {
                if(value != null)
                {
                    ScorerName = value.FirstName + " " + value.LastName;
                    ScorerId = value.Id;
                }
                else
                {
                    ScorerName = "Deleted";
                }
                _scorer = value;
            }
        }
        public int ScorerId { get; set; }
        public string ScorerName { get; set; }

        int _minute;
        public int Minute { get => _minute; set { try { _minute = Math.Clamp(value, 0, 90); } catch { _minute = 0; } } }
    }






    public class Match : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public DataBase Db
        {
            get => ((App)Windows.UI.Xaml.Application.Current).Db;
            set
            {
                ((App)Windows.UI.Xaml.Application.Current).Db = value;
            }
        }
        public Match(Club homeclub, Club awayclub)
        {
            Id = _count;
            HomeClub = homeclub;
            AwayClub = awayclub;
            UpdatePoints();


            HomeGoals.CollectionChanged += HomeGoals_CollectionChanged;
            AwayGoals.CollectionChanged += AwayGoals_CollectionChanged;
        }
        public Match(int id,Club homeclub,Club awayclub)
        {
            Id = id;
            _count = ++id;
            HomeClub = homeclub;
            AwayClub = awayclub;
            UpdatePoints();


            HomeGoals.CollectionChanged += HomeGoals_CollectionChanged;
            AwayGoals.CollectionChanged += AwayGoals_CollectionChanged;
        }

        private void HomeGoals_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    HomeClub.GF++;
                    AwayClub.GA++;
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    HomeClub.GF--;
                    AwayClub.GA--;
                    break;
                    //.clear in HomeClub Property to remove the old clubs goals
            }
            UpdatePoints();
        }
        private void AwayGoals_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    HomeClub.GA++;
                    AwayClub.GF++;
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    HomeClub.GA--;
                    AwayClub.GF--;
                    break;
                    //.clear in AwayClub Property to remove the old clubs goals
            }
            UpdatePoints();
        }

        static int _count;
        public int Id { get; set; }

        Club _homeclub;
        public Club HomeClub
        {
            get => _homeclub; set
            {
                Club newvalue = value == null ? DataBase.ClubLess : value;
                if (_homeclub != newvalue)
                {
                    //first time initialising
                    if (_homeclub != null)
                    {
                        _homeclub.GF -= HomeGoals.Count;
                        _homeclub.GA -= AwayGoals.Count;

                        _awayclub.GA -= HomeGoals.Count;


                        foreach (Goal goal in HomeGoals)
                        {
                            Db.RemoveGoal(goal);
                        }
                        HomeGoals.Clear();
                        if (CurrentScore == "home")
                        {
                            _homeclub.Points -= 3;
                            CurrentScore = "none";
                        }
                        else if (CurrentScore == "draw")
                        {
                            _homeclub.Points -= 1;
                            _awayclub.Points -= 1;
                            CurrentScore = "none";
                        }
                    }
                    _homeclub = newvalue;
                    newvalue.GA += AwayGoals.Count;
                    OnPropertyChanged();
                    UpdatePoints();
                }
            }
        }
        Club _awayclub;
        public Club AwayClub
        {
            get => _awayclub; set
            {
                // for when club gets removed
                Club newvalue = value ?? DataBase.ClubLess;
                if (_awayclub != newvalue)
                {
                    if (_awayclub != null)
                    {
                        _awayclub.GF -= AwayGoals.Count;
                        _awayclub.GA -= HomeGoals.Count;

                        _homeclub.GA -= AwayGoals.Count;
                        foreach (Goal goal in AwayGoals)
                        {
                            Db.RemoveGoal(goal);
                        }
                        AwayGoals.Clear();
                        if (CurrentScore == "away")
                        {
                            _awayclub.Points -= 3;
                            CurrentScore = "none";
                        }
                        else if (CurrentScore == "draw")
                        {
                            _awayclub.Points -= 1;
                            _homeclub.Points -= 1;
                            CurrentScore = "none";
                        }
                    }
                    _awayclub = newvalue;
                    newvalue.GA += HomeGoals.Count;
                    OnPropertyChanged();
                    UpdatePoints();
                }
            }
        }

        public ObservableCollection<Goal> HomeGoals { get; set; } = new ObservableCollection<Goal>() { };
        public ObservableCollection<Goal> AwayGoals { get; set; } = new ObservableCollection<Goal>() { };

        


        /// <summary>
        /// 'home' 'draw' 'away' and 'none'
        /// </summary>
        private string CurrentScore = "none";
        public void UpdatePoints()
        {
            if (HomeClub != null && AwayClub != null)
            {
                switch (CurrentScore)
                {
                    case "none":
                        if (HomeGoals.Count > AwayGoals.Count)
                        {
                            HomeClub.Points += 3;
                            CurrentScore = "home";
                        }
                        else if (HomeGoals.Count == AwayGoals.Count)
                        {
                            HomeClub.Points++;
                            AwayClub.Points++;
                            CurrentScore = "draw";
                        }
                        else if (HomeGoals.Count < AwayGoals.Count)
                        {
                            AwayClub.Points += 3;
                            CurrentScore = "away";
                        }
                        break;
                    case "home":
                        if (HomeGoals.Count == AwayGoals.Count)
                        {
                            HomeClub.Points -= 2;
                            AwayClub.Points++;
                            CurrentScore = "draw";
                        }
                        else if (HomeGoals.Count < AwayGoals.Count)
                        {
                            HomeClub.Points -= 3;
                            AwayClub.Points += 3;
                            CurrentScore = "away";
                        }
                        break;
                    case "draw":
                        if (HomeGoals.Count > AwayGoals.Count)
                        {
                            HomeClub.Points += 2;
                            AwayClub.Points--;
                            CurrentScore = "home";
                        }
                        else if (HomeGoals.Count < AwayGoals.Count)
                        {
                            HomeClub.Points--;
                            AwayClub.Points += 2;
                            CurrentScore = "away";
                        }
                        break;
                    case "away":
                        if (HomeGoals.Count > AwayGoals.Count)
                        {
                            HomeClub.Points += 3;
                            AwayClub.Points -= 3;
                            CurrentScore = "home";
                        }
                        else if (HomeGoals.Count == AwayGoals.Count)
                        {
                            HomeClub.Points++;
                            AwayClub.Points -= 2;
                            CurrentScore = "draw";
                        }
                        break;
                }
            }
        }
    }
}
