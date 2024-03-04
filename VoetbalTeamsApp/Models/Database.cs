using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Windows.Storage;

namespace VoetbalTeamsApp.Models
{
    public class DataBase
    {
        public Coach coach = new Coach("Timon", "Venninckx", 16, 2000f);
        public ObservableCollection<Position> Positions = new ObservableCollection<Position> { Position.Keeper, Position.Defender, Position.Midfielder, Position.Attacker };
        public ObservableCollection<Player> Players { get; set; } = new ObservableCollection<Player>();
        public static Club ClubLess { get; set; } = new Club("None", null);
        public ObservableCollection<Club> Clubs { get; set; } = new ObservableCollection<Club>() { ClubLess };
        public ObservableCollection<Club> ActiveClubs { get; set; } = new ObservableCollection<Club>();
        public ObservableCollection<Match> Matches { get; set; } = new ObservableCollection<Match>();
        private ObservableCollection<Goal> Goals { get; set; } = new ObservableCollection<Goal>();
        public void AddGoal(Goal goal) { Goals.Add(goal); goal.Scorer.Goals++; }
        public void RemoveGoal(Goal goal) { Goals.Remove(goal); goal.Scorer.Goals--; }

        public static SqliteConnection Conn;
        public DataBase()
        {
            Clubs.CollectionChanged += Clubs_CollectionChanged;


            Conn = CreateConnection();
            if (PlayerTableExists())
            {
                ReadData();
            }
            else
            {
                CreateTable();
                RegeneratePlayers();
            }
        }

        private void Clubs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (Club club in e.NewItems)
                    {
                        ActiveClubs.Add(club);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (Club club in e.OldItems)
                    {
                        ActiveClubs.Remove(club);
                    }
                    break;
            }
        }

        private static bool PlayerTableExists()
        {
            SqliteCommand sqlite_cmd;
            sqlite_cmd = Conn.CreateCommand();
            sqlite_cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Players'";
            SqliteDataReader r = sqlite_cmd.ExecuteReader();
            if (r.HasRows)
            {
                r.Close();
                return true;
            }
            return false;
        }


        static SqliteConnection CreateConnection()
        {
            SqliteConnection sqlite_conn;

            sqlite_conn = new SqliteConnection(@"data source=database.db");

            sqlite_conn.Open();
            return sqlite_conn;
        }

        public void CreateTable()
        {
            SqliteCommand sqlite_cmd;
            sqlite_cmd = Conn.CreateCommand();
            sqlite_cmd.CommandText = "DROP TABLE IF EXISTS Players";
            sqlite_cmd.ExecuteNonQuery();

            sqlite_cmd.CommandText = "CREATE TABLE IF NOT EXISTS Players (id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, firstname VARCHAR(20) NOT NULL, lastname VARCHAR(20) NOT NULL,age INT NOT NULL,salary BLOB NOT NULL,position INT NOT NULL,clubid INT NOT NULL,goals INT NOT NULL)";
            sqlite_cmd.ExecuteNonQuery();
            sqlite_cmd.CommandText = "CREATE TABLE IF NOT EXISTS Clubs(id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, name VARCHAR(20) NOT NULL)";
            sqlite_cmd.ExecuteNonQuery();
            sqlite_cmd.CommandText = "CREATE TABLE IF NOT EXISTS Matches(matchid INTEGER NOT NULL,homeclubid INTEGER NOT NULL,awayclubid INTEGER NOT NULL)";
            sqlite_cmd.ExecuteNonQuery();
            sqlite_cmd.CommandText = "CREATE TABLE IF NOT EXISTS Goals(matchid INTEGER NOT NULL,ishomegoal INTEGER NOT NULL,minute INTEGER NOT NULL,goalscorerid INTEGER NOT NULL)";
            sqlite_cmd.ExecuteNonQuery();
            sqlite_cmd.CommandText = "CREATE TABLE IF NOT EXISTS Coaches (clubid INTEGER NOT NULL PRIMARY KEY, firstname VARCHAR(20) NOT NULL, lastname VARCHAR(20) NOT NULL,age INT NOT NULL,salary BLOB NOT NULL)";
            sqlite_cmd.ExecuteNonQuery();
        }

        public void ReadData()
        {
            // load clubs from database
            Dictionary<int, Club> clublist = new Dictionary<int, Club>();
            SqliteCommand sqlite_cmd;
            sqlite_cmd = Conn.CreateCommand();
            sqlite_cmd.CommandText = "Select * FROM Clubs";
            SqliteDataReader r = sqlite_cmd.ExecuteReader();
            while (r.Read())
            {
                Club club = new Club(Convert.ToInt32(r["id"]), Convert.ToString(r["name"]), coach);
                Clubs.Add(club);
                clublist.Add(club.Id, club);
            }
            r.Close();
            // load players from database
            // Player map for loading match goals without searching through observablecollection everytime
            // cant be used instead of observablecollection because i cant update UI on map.add()
            Dictionary<int, Player> playerlist = new Dictionary<int, Player>();

            sqlite_cmd.CommandText = "SELECT * FROM Players";
            r = sqlite_cmd.ExecuteReader();
            while (r.Read())
            {
                Position positie = Position.Keeper;
                Enum.TryParse<Position>(Convert.ToString(r["position"]), out positie);
                int clubid = Convert.ToInt32(r["clubid"]);
                Club club = DataBase.ClubLess;
                if(clubid != 0)
                {
                    foreach (Club _club in Clubs)
                    {
                        if (_club.Id == clubid)
                        {
                            club = _club;
                            break;
                        }
                    }
                }
                Player player = new Player(Convert.ToInt32(r["id"]), Convert.ToString(r["firstname"]), Convert.ToString(r["lastname"]), Convert.ToInt32(r["age"]), (float)Convert.ToDouble(r["salary"]), positie, club,Convert.ToInt32(r["goals"]));
                Players.Add(player);
                playerlist.Add(player.Id, player);
            }
            r.Close();
            // load coaches from database
            sqlite_cmd.CommandText = "SELECT * FROM Coaches";
            r = sqlite_cmd.ExecuteReader();
            while (r.Read())
            {
                int clubid = Convert.ToInt32(r["clubid"]);
                if (playerlist.ContainsKey(clubid))
                {
                    clublist[clubid].Coach = new Coach(Convert.ToString(r["firstname"]), Convert.ToString(r["lastname"]), Convert.ToInt32(r["age"]), (float)Convert.ToDouble(r["salary"]));
                }

            }

            r.Close();
            // load matches from database
            Dictionary<int, Match> matchlist = new Dictionary<int, Match>();
            sqlite_cmd.CommandText = "SELECT * FROM Matches";
            r = sqlite_cmd.ExecuteReader();
            while (r.Read())
            {
                Club homeclub = ClubLess;
                Club awayclub = ClubLess;
                clublist.TryGetValue(Convert.ToInt32(r["homeclubid"]), out homeclub);
                clublist.TryGetValue(Convert.ToInt32(r["awayclubid"]), out awayclub);

                Match match = new Match(Convert.ToInt32(r["matchid"]), homeclub, awayclub);
                Matches.Add(match);
                matchlist.Add(match.Id,match);
            }
            r.Close();
            // load goals from database
            sqlite_cmd.CommandText = "SELECT * FROM Goals";
            r = sqlite_cmd.ExecuteReader();
            while (r.Read())
            {
                Match match;
                Player scorer;
                if(matchlist.TryGetValue(Convert.ToInt32(r["matchid"]), out match) && playerlist.TryGetValue(Convert.ToInt32(r["goalscorerid"]),out scorer)){

                    Goal goal = new Goal(match.Id, Convert.ToInt32(r["ishomegoal"]), Convert.ToInt32(r["minute"]),scorer);
                    Goals.Add(goal);
                    if(Convert.ToInt32(r["ishomegoal"]) == 1)
                    {
                        match.HomeGoals.Add(goal);
                    }
                    else
                    {
                        match.AwayGoals.Add(goal);
                    }
                }
            }

        }

        public void SaveData()
        {
            SqliteCommand sqlite_cmd;
            sqlite_cmd = Conn.CreateCommand();
            sqlite_cmd.CommandText = "DELETE FROM Players";
            sqlite_cmd.ExecuteNonQuery();
            if (Players != null)
            {
                sqlite_cmd.CommandText = "INSERT INTO Players ('id','firstname','lastname','age','salary','position','clubid','goals') VALUES (@id,@firstname,@lastname,@age,@salary,@position,@clubid,@goals)";
                sqlite_cmd.Parameters.Add("@id", SqliteType.Integer);
                sqlite_cmd.Parameters.Add("@firstname", SqliteType.Text);
                sqlite_cmd.Parameters.Add("@lastname", SqliteType.Text);
                sqlite_cmd.Parameters.Add("@age", SqliteType.Integer);
                sqlite_cmd.Parameters.Add("@salary", SqliteType.Real);
                sqlite_cmd.Parameters.Add("@position", SqliteType.Integer);
                sqlite_cmd.Parameters.Add("@clubid", SqliteType.Integer);
                sqlite_cmd.Parameters.Add("@goals", SqliteType.Integer);
                foreach (Player player in Players)
                {
                    sqlite_cmd.Parameters[0].Value = player.Id;
                    sqlite_cmd.Parameters[1].Value = player.FirstName;
                    sqlite_cmd.Parameters[2].Value = player.LastName;
                    sqlite_cmd.Parameters[3].Value = player.Age;
                    sqlite_cmd.Parameters[4].Value = player.Salary;
                    sqlite_cmd.Parameters[5].Value = player.Position;
                    sqlite_cmd.Parameters[6].Value = player.Club.Id;
                    sqlite_cmd.Parameters[7].Value = player.Goals;

                    sqlite_cmd.ExecuteNonQuery();
                }
            }
            sqlite_cmd.CommandText = "DELETE FROM Clubs";
            sqlite_cmd.ExecuteNonQuery();
            if (Clubs != null)
            {
                sqlite_cmd = Conn.CreateCommand();
                sqlite_cmd.CommandText = "INSERT INTO Clubs ('id','name') VALUES (@id,@name)";
                sqlite_cmd.Parameters.Add("@id", SqliteType.Integer);
                sqlite_cmd.Parameters.Add("@name", SqliteType.Text);
                foreach (Club club in ActiveClubs)
                {
                    sqlite_cmd.Parameters[0].Value = club.Id;
                    sqlite_cmd.Parameters[1].Value = club.Name;

                    sqlite_cmd.ExecuteNonQuery();
                }
            }
            sqlite_cmd.CommandText = "DELETE FROM Coaches";
            sqlite_cmd.ExecuteNonQuery();

            if (Clubs != null)
            {
                sqlite_cmd = Conn.CreateCommand();
                sqlite_cmd.CommandText = "INSERT INTO Coaches ('clubid','firstname','lastname','age','salary') VALUES (@clubid,@firstname,@lastname,@age,@salary)";
                sqlite_cmd.Parameters.Add("@clubid", SqliteType.Integer);
                sqlite_cmd.Parameters.Add("@firstname", SqliteType.Text);
                sqlite_cmd.Parameters.Add("@lastname", SqliteType.Text);
                sqlite_cmd.Parameters.Add("@age", SqliteType.Integer);
                sqlite_cmd.Parameters.Add("@salary", SqliteType.Real);
                foreach (Club club in ActiveClubs)
                {
                    sqlite_cmd.Parameters[0].Value = club.Id; 
                    sqlite_cmd.Parameters[1].Value = club.Coach.FirstName;
                    sqlite_cmd.Parameters[2].Value = club.Coach.LastName;
                    sqlite_cmd.Parameters[3].Value = club.Coach.Age;
                    sqlite_cmd.Parameters[4].Value = club.Coach.Salary;

                    sqlite_cmd.ExecuteNonQuery();
                }
            }


            sqlite_cmd.CommandText = "DELETE FROM Matches";
            sqlite_cmd.ExecuteNonQuery();
            if (Matches != null)
            {
                sqlite_cmd = Conn.CreateCommand();
                sqlite_cmd.CommandText = "INSERT INTO Matches ('matchid','homeclubid','awayclubid') VALUES (@matchid,@homeclubid,@awayclubid)";
                sqlite_cmd.Parameters.Add("@matchid", SqliteType.Integer);
                sqlite_cmd.Parameters.Add("@homeclubid", SqliteType.Integer);
                sqlite_cmd.Parameters.Add("@awayclubid", SqliteType.Integer);
                foreach (Match match in Matches)
                {
                    sqlite_cmd.Parameters[0].Value = match.Id;
                    sqlite_cmd.Parameters[1].Value = match.HomeClub.Id;
                    sqlite_cmd.Parameters[2].Value = match.AwayClub.Id;

                    sqlite_cmd.ExecuteNonQuery();
                }
            }
            sqlite_cmd.CommandText = "DELETE FROM Goals";
            sqlite_cmd.ExecuteNonQuery();
            if (Goals != null)
            {
                sqlite_cmd = Conn.CreateCommand();
                sqlite_cmd.CommandText = "INSERT INTO Goals ('matchid','ishomegoal','minute','goalscorerid') VALUES (@matchid,@ishomegoal,@minute,@goalscorerid)";
                sqlite_cmd.Parameters.Add("@matchid", SqliteType.Integer);
                sqlite_cmd.Parameters.Add("@ishomegoal", SqliteType.Integer);
                sqlite_cmd.Parameters.Add("@minute", SqliteType.Integer);
                sqlite_cmd.Parameters.Add("@goalscorerid", SqliteType.Integer);
                foreach (Goal goal in Goals)
                {
                    sqlite_cmd.Parameters[0].Value = goal.MatchId;
                    sqlite_cmd.Parameters[1].Value = goal.IsHomeGoal;
                    sqlite_cmd.Parameters[2].Value = goal.Minute;
                    sqlite_cmd.Parameters[3].Value = goal.ScorerId;

                    sqlite_cmd.ExecuteNonQuery();
                }
            }

        }


        public void RegeneratePlayers()
        {
            Club psv = new Club("Psv", new Coach("Peter", "Bosz", 60, 4000));
            Club ajax = new Club("Ajax", new Coach("John", "Van 't Schip", 60, 4000));
            Club feyenoord = new Club("Feyenoord", new Coach("Arne", "Slot", 45, 4000));
            //remove all clubs except Emptryclub from clubs (automaticly removes from activeclubs)
            int clublength = ActiveClubs.Count;
            for(int i = 0; i < clublength; i++)
            {
                Clubs.Remove(ActiveClubs[0]);
            }
            Clubs.Add(psv);
            Clubs.Add(ajax);
            Clubs.Add(feyenoord);

            //first ID == 0 for when you reset the database otherwise you get high ids
            var newplayers = new ObservableCollection<Player>
            {
                new Player(0,"timon","Venninckx",20,1500, Position.Attacker,ClubLess,0),
                new Player("Juul","Wanten",20,2000, Position.Defender,ClubLess),
                new Player("Jens","Vullings",21,2300.4F, Position.Midfielder,ClubLess),
                new Player("Jurn","Leijsten",21,2100.4F, Position.Keeper,ClubLess),
                new Player("Walter","Benitez",23,1500f,Position.Keeper,psv),
                new Player("André","Ramalho",18,1300f,Position.Defender,psv),
                new Player("Jordan","Teze",26,1200f,Position.Defender,psv),
                new Player("Olivier","Boscagli",28,1600f,Position.Defender,psv),
                new Player("Patrick","van Aanholt",23,2100f,Position.Defender,psv),
                new Player("Joey","Veerman",26,1000f,Position.Midfielder,psv),
                new Player("Malik","Tillman",25,600f,Position.Midfielder,psv),
                new Player("Jerdy","Schouten",27,4300f,Position.Midfielder,psv),
                new Player("Ismael","Saibari",28,4200f,Position.Attacker,psv),
                new Player("Luuk","de Jong",19,700f,Position.Attacker,psv),
                new Player("Noa","Lang",31,4000f,Position.Attacker,psv),
                new Player("Remko","Pasveer",23,1500f,Position.Keeper,ajax),
                new Player("Jorrel","Hato",18,1300f,Position.Defender,ajax),
                new Player("Owen","Wijndal",26,1200f,Position.Defender,ajax),
                new Player("Devyne","Fabian Jairo Rensch",28,1600f,Position.Defender,ajax),
                new Player("Anass","Salah-Eddine",23,2100f,Position.Defender,ajax),
                new Player("Benjamin","Tahirovic",26,1000f,Position.Midfielder,ajax),
                new Player("Branco","van den Boomen",25,600f,Position.Midfielder,ajax),
                new Player("Kenneth","Taylor",27,4300f,Position.Midfielder,ajax),
                new Player("Brian","Brobbey",28,4200f,Position.Attacker,ajax),
                new Player("Steven","Bergwijn",19,700f,Position.Attacker,ajax),
                new Player("Mika","Godts",31,4000f,Position.Attacker,ajax),
                new Player("Justin","Bijlow",23,1500f,Position.Keeper,feyenoord),
                new Player("Gernot","Trauner",18,1300f,Position.Defender,feyenoord),
                new Player("Marcos","Lopez",26,1200f,Position.Defender,feyenoord),
                new Player("Quilindschy","Hartman",28,1600f,Position.Defender,feyenoord),
                new Player("Lutsharel","Geertruida",23,2100f,Position.Defender,feyenoord),
                new Player("Mats","Wieffer",26,1000f,Position.Midfielder,feyenoord),
                new Player("Quinten","Timber",25,600f,Position.Midfielder,feyenoord),
                new Player("Ramiz","Zerrouki",27,4300f,Position.Midfielder,feyenoord),
                new Player("Calvin","Stengs",28,4200f,Position.Attacker,feyenoord),
                new Player("Ayase","Ueda",19,700f,Position.Attacker,feyenoord),
                new Player("Alireza","Jahanbakhsh",31,4000f,Position.Attacker,feyenoord)
            };
            Players.Clear();
            foreach (Player player in newplayers)
            {
                Players.Add(player);
            }

            // third match is 0-0
            Matches.Clear();
            Matches.Add(new Match(0, psv, ajax));
            Matches.Add(new Match(1, feyenoord, ajax));
            Matches.Add(new Match(2, psv, feyenoord));


            var newgoals = new ObservableCollection<Goal>
            {
                new Goal(0,1, 45, psv.Players[3]),
                new Goal(0,1, 62, psv.Players[9]),
                new Goal(0,0, 88, ajax.Players[9]),
                new Goal(1,0, 22, ajax.Players[9]),
                new Goal(1,0, 34, ajax.Players[9]),
                new Goal(1,0, 37, ajax.Players[9])
            };
            Goals.Clear();
            foreach(Goal goal in newgoals)
            {
                AddGoal(goal);
            }

            psv.Players[3].Goals = 1;
            psv.Players[9].Goals = 1;
            ajax.Players[9].Goals = 4;
            Matches[0].HomeGoals.Add(Goals[0]);
            Matches[0].HomeGoals.Add(Goals[1]);
            Matches[0].AwayGoals.Add(Goals[2]);
            Matches[1].AwayGoals.Add(Goals[3]);
            Matches[1].AwayGoals.Add(Goals[4]);
            Matches[1].AwayGoals.Add(Goals[5]);
        }
    }
}
