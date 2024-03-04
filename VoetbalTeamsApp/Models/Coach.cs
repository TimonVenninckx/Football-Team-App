using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoetbalTeamsApp.Models
{
    public class Coach : Person
    {
        public Coach(string FirstName, string LastName, int Age, float Salary) : base(FirstName, LastName, Age, Salary)
        {
        }
    }
}
