using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using App1.Client.Helpers;
using App1.Client.Models;
using App1.Client.Services;
using App1.Shared;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace App1.Client.Views
{
    [Restricted]
    public sealed partial class FetchDataPage : Page, INotifyPropertyChanged
    {
        public ObservableCollection<WeatherForecast> Source { get; } = new ObservableCollection<WeatherForecast>();

        public FetchDataPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Source.Clear();

            // TODO WTS: Replace this with your actual data
            //var data = await SampleDataService.GetGridDataAsync();

            //foreach (var item in data)
            //{
            //    Source.Add(item);
            //}
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
