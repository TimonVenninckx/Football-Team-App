using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace VoetbalTeamsApp.Models
{
    public class Club : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private static int _idcount;
        public int Id { get; set; }
        private string _name;
        public string Name { get { return _name; } set { if (value != "") { _name = value; OnPropertyChanged(); } } }
        public ObservableCollection<Player> Players { get; set; } = new ObservableCollection<Player>();
        public Coach Coach { get; set; }
        int _won = 0;
        public int Won { get => _won; set { _won = value; OnPropertyChanged(); } }
        int _drawn;
        public int Drawn { get => _drawn; set { _drawn = value; OnPropertyChanged(); } }
        int _lost;
        public int Lost { get => _lost; set { _lost = value; OnPropertyChanged(); } }
        int _points;
        public int Points { get => _points; set { _points = value; OnPropertyChanged(); } }
        int _gf;
        ///<summary>
        ///Goals for the club
        ///</summary>
        public int GF { get => _gf; set { _gf = value; OnPropertyChanged(); } }

        int _ga;
        ///<summary>
        ///Goals scored against the club
        ///</summary>
        public int GA { get => _ga; set { _ga = value; OnPropertyChanged(); } }

        ///<summary>
        ///Goal difference GF - GA
        ///</summary>
        public int GD { get { return GF - GA; } }


        public Club(string name, Coach coach)
        {
            this.Name = name;
            this.Coach = coach;
            this.Id = _idcount;
            _idcount++;
        }
        public Club(int id,string name, Coach coach)
        {
            this.Name = name;
            this.Coach = coach;
            this.Id = id;
            _idcount = ++id;
        }

        public void AddPlayer(ObservableCollection<Player> players)
        {
            foreach (var player in players)
            {
                if (!this.Players.Contains(player))
                {
                    this.Players.Add(player);
                    player.Club = this;
                }
            }
        }
        public void AddPlayer(Player player)
        {
            if (!this.Players.Contains(player))
            {
                this.Players.Add(player);
                player.Club = this;
            }
        }
        public void RemovePlayer(Player player)
        {
            if (this.Players.Contains(player))
            {
                this.Players.Remove(player);
            }
        }
        public void RemoveAllPlayers()
        {
            foreach (Player player in Players)
            {
                player.Club = null;
            }
        }
    }
}
