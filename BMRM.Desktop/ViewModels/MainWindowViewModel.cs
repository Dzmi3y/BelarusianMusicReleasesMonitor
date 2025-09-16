using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using BMRM.Core.Models;
using BMRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BMRM.Desktop.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private AppDbContext _appDbContext;
        private string _title = "BMRM";
        public ICommand AddTrackCommand { get; }
        public ICommand InitCommand { get; }

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private int _count = 0;

        public int Count
        {
            get { return _count; }
            set { SetProperty(ref _count, value); }
        }

        public ObservableCollection<Release> Tracks { get; } = new();

        public MainWindowViewModel(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
            AddTrackCommand = new DelegateCommand(OnAddTrack);
            InitCommand = new AsyncDelegateCommand(InitAsync);
        }

        public async Task InitAsync()
        {
            var trackList = await _appDbContext.Releases.AsNoTracking().ToListAsync();
            foreach (var track in trackList)
                Tracks.Add(track);
        }

        private void OnAddTrack()
        {
        }
    }
}