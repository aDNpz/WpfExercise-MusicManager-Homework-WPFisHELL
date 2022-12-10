using Repository.Logic.Models;
using Repository.Logic.Repos;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MusicManager.WpfApp.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private ICommand? addAlbumCommand = null;
        private ICommand? editAlbumCommand = null;
        private ICommand? deleteAlbumCommand = null;

        private ICommand? addTrackCommand = null;
        private ICommand? editTrackCommand = null;
        private ICommand? deleteTrackCommand = null;

        public ObservableCollection<Album> Albums
        {
            get
            {
                var albumRep = new AlbumRepository();
                var albums = string.IsNullOrWhiteSpace(searchAlbum) ? albumRep.GetAllAsync().Result.OrderBy(a => a.Title) : albumRep.GetAllAsync().Result.Where(x => x.Interpreter.ToUpper().Contains(searchAlbum.ToUpper()) || x.Title.Contains(searchAlbum.ToUpper())).OrderBy(x => x.Title.ToUpper());
                return new ObservableCollection<Album>(albums);
            }
        }

        public ObservableCollection<Track> Tracks
        {
            get
            {
                var trackRep = new TrackRepository();
                var tracks = selectedAlbum != null ? trackRep.GetAllAsync().Result.Where(x => x.AlbumId == selectedAlbum.Id) : Array.Empty<Track>();
                return new ObservableCollection<Track>(tracks);
            }
        }

        public ICommand AddAlbumCommand
        {
            get
            {
                return RelayCommand.CreateCommand(ref addAlbumCommand, (p) => AddAlbum());
            }
        }
        private void AddAlbum()
        {
            var albumEditWindow = new AlbumEditWindow();
            albumEditWindow.ShowDialog();
            OnPropertyChanged(nameof(Albums));
        }

        public ICommand EditAlbumCommand => RelayCommand.CreateCommand(ref editAlbumCommand, (p) => EditAlbum(), (p) => SelectedAlbum != null);

        private void EditAlbum()
        {
            var albumEditWindow = new AlbumEditWindow();
            if (albumEditWindow.ViewModel is AlbumEditViewModel aevm && SelectedAlbum != null)
            {
                aevm.Album = SelectedAlbum!;
                albumEditWindow.ShowDialog();
                OnPropertyChanged(nameof(Albums));
            }

        }

        public ICommand DeleteAlbumCommand => RelayCommand.CreateCommand(ref deleteAlbumCommand, (p) => DeleteAlbum(), (p) => SelectedAlbum != null);

        private void DeleteAlbum()
        {
            if (SelectedAlbum != null)
            {
                var response = MessageBox.Show($"Wirklich {SelectedAlbum.Interpreter} {SelectedAlbum.Title} löschen?", "Album löschen", MessageBoxButton.YesNo, MessageBoxImage.Stop);

                if (response == MessageBoxResult.Yes)
                {
                    var repo = new AlbumRepository();
                    repo.DeleteAsync(SelectedAlbum.Id).Wait();
                    repo.SaveChangesAsync().Wait();
                    OnPropertyChanged(nameof(Albums));
                }
            }
        }

        public ICommand AddTrackCommand => RelayCommand.CreateCommand(ref addAlbumCommand, (p) => AddTrack(), (p) => SelectedAlbum != null);

        private void AddTrack()
        {
            if (SelectedAlbum != null)
            {
                var trackEditWindow = new TrackEditWindow();

                if (trackEditWindow.ViewModel is TrackEditViewModel tevm)
                {
                    tevm.Track = new Track() { AlbumId = SelectedAlbum.Id };

                }
            }
        }

        //edit del track fehlt, kein bock mehr nach 5h. viel zu umfangreich dieser müll hier. und VS spinnt auch rum aufeinmal hat man 7000 fehler.

        private Album? selectedAlbum = null;

        public Album? SelectedAlbum
        {
            get => selectedAlbum;
            set
            {
                selectedAlbum = value;
                OnPropertyChanged(nameof(Tracks));
            }
        } = null;

        public Track? SelectedTrack { get; set; } = null;

        private string searchAlbum;
        public string SearchAlbum
        {
            get => searchAlbum;
            set
            {
                searchAlbum = value;
                OnPropertyChanged(nameof(Albums));
            }
        }


    }
}
