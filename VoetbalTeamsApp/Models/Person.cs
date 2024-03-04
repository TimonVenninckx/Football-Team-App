using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace VoetbalTeamsApp.Models
{
    public class Person : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        string _firstname;
        public string FirstName { get => _firstname;  set { _firstname = value; OnPropertyChanged(); } }
        string _lastname;
        public string LastName { get => _lastname; set { _lastname = value; OnPropertyChanged(); } }
        int _age;
        public int Age { get => _age; set { _age = value; OnPropertyChanged(); } }
        float _salary;
        public float Salary { get => _salary; set { _salary = value; OnPropertyChanged(); } }

        public Person(string firstname, string lastname, int age, float salary)
        {
            this.FirstName = firstname;
            this.LastName = lastname;
            this.Age = age;
            this.Salary = salary;
        }
    }
}
