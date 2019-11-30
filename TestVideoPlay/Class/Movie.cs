using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestVideoPlay.Class
{
    public class Movie : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
                PropertyChanged(this, new PropertyChangedEventArgs("DisplayMember"));
            }
        }
        private double rating;
        public string Genre { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string ReleaseDate { get; set; }
        public string Poster { get; set; }
        public double Rating
        {
            get
            {
                return rating;
            }
            set
            {
                rating = value;
                rating = rating / 2;
            }
        }
    }
}
