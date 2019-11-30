using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TestVideoPlay.Class;
using TMDbLib.Client;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Search;

namespace TestVideoPlay.ViewModels
{
    public class HomePageViewModel : INotifyPropertyChanged
    {
        private static TMDbClient client = new TMDbClient("35eadc5a50894652fbf75ac8333b1991");
        public static List<Genre> MovieGenres = new List<Genre>();
        public static ObservableCollection<Movie> PopularMovies = new ObservableCollection<Movie>();
        //public static ObservableCollection<Movie> GreatestMovies = new ObservableCollection<Movie>();
        //public static ObservableCollection<Movie> RecentMovies = new ObservableCollection<Movie>();
        public static async Task GetPopularMoviesAsync(string language, int page, ListBox MoviesDisplay)
        {
            client.GetConfig();
            if (MovieGenres.Count == 0)
            {
                MovieGenres = InitialMovieGenres(language);
            }
            var genre = MovieGenres;
            var popularMovies = await client.GetMoviePopularListAsync(language, page);

            PopularMovies.Clear();

            foreach (var movie in popularMovies.Results)
            {
                Movie mov = new Movie()
                {
                    Id = movie.Id,
                    Name = movie.OriginalTitle,
                    ReleaseDate = movie.ReleaseDate.Value.ToShortDateString(),
                    Poster = client.GetImageUrl("w500", movie.PosterPath).AbsoluteUri,
                    Rating = movie.VoteAverage,
                    Genre = GetMovieGenres(movie, genre)
                };

                PopularMovies.Add(mov);

            }
            MoviesDisplay.ItemsSource = PopularMovies;
        }
        public static ObservableCollection<Class.GenreModel> Genres = new ObservableCollection<Class.GenreModel>();
        public static async void GetGenres(string language, ListBox GenresDisplay)
        {

            client.GetConfig();
            var genreList = await client.GetMovieGenresAsync(language);

            Genres.Clear();

            foreach (var genre in genreList)
            {
                Class.GenreModel gen = new Class.GenreModel()
                {
                    ID = genre.Id,
                    GenreName = genre.Name
                };

                Genres.Add(gen);

            }
            GenresDisplay.ItemsSource = Genres;
        }
        private static string GetMovieGenres(SearchMovie movie, List<Genre> genre)
        {
            string finalGenre = "";
            foreach (var VARIABLE in movie.GenreIds.Take(3))
            {
                foreach (var genre1 in genre)
                {
                    if (genre1.Id == VARIABLE)
                    {
                        finalGenre += genre1.Name + ", ";
                    }
                }
            }
            if (finalGenre.Length >= 3)
            {
                finalGenre = finalGenre.Remove(finalGenre.Length - 2);
            }
            return finalGenre;
        }
        public static List<Genre> InitialMovieGenres(string language)
        {
            return client.GetMovieGenresAsync(language).Result;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
