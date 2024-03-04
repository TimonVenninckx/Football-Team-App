using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace VoetbalTeamsApp.Models
{
    public enum Position { Keeper = 0, Defender = 1, Midfielder = 2, Attacker = 3}

    public class Player : Person
    {
        public Position Position { get; set; }
        private Club _club;
        public Club Club
        {
            get { return _club; }
            set {
                if (Club != value)
                {
                    if (value == null)
                    {
                        _club = DataBase.ClubLess;
                        _club.AddPlayer(this);
                    }
                    else
                    {
                        if (_club != null)
                        {
                            _club.RemovePlayer(this);
                        }
                        value.AddPlayer(this);
                        _club = value;
                        OnPropertyChanged();
                    }
                }
            } 
        }
        private static int _number;
        public int Id { get; set; }
        public int _goals = 0;
        public int Goals { get => _goals; set { _goals = value; OnPropertyChanged(); } }

        public Player(string FirstName, string LastName, int Age, float Salary, Position position, Club club) : base(FirstName, LastName, Age, Salary)
        {
            this.Position = position;
            this.Id = _number;
            _number++;
            this.Club = club;
        }
        public Player(int id, string FirstName, string LastName, int Age, float Salary, Position position,Club club,int goals) : base(FirstName, LastName, Age, Salary)
        {
            this.Id = id;
            _number = ++id;

            this.Position = position;
            this.Club = club;
            this.Goals = goals;
        }
    }
}
