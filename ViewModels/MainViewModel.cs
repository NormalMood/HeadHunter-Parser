using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel; 
using HeadHunter_Parser.Commands;
using HeadHunter_Parser.Models;
using System.Windows.Controls;
using HeadHunter_Parser.Views;

namespace HeadHunter_Parser.ViewModels
{
    public class MainViewModel : Base.BasePropertyChanged
    {

        private bool _isParsingStarted = false;
        private bool _isSettingsWindowOpened = false;

        private Page _commonInfo;
        private CommonInfoViewModel _commonInfoDataContext;

        private Page _resumeInfo;
        private ResumeInfoViewModel _resumeInfoDataContext;

        private Page _vacancyInfo;
        private VacancyInfoViewModel _vacancyInfoDataContext;

        private Page _currentPage;

        private string _searchedCity;
        private RelayCommand _checkAllCities;
        private RelayCommand _uncheckAllCities;

        private string _searchedTechnology;
        private RelayCommand _addTechnology;
        private RelayCommand _checkAllTechnologies;
        private RelayCommand _uncheckAllTechnologies;
        private RelayCommand _removeTechnology;

        private RelayCommand _makeLog;

        private RelayCommand _parse;

        private RelayCommand _openSettings;

        private CityContainer _cityContainer;
        private TechnologyContainer _technologyContainer;
        private Parser _parser;

        public Page CurrentPage
        {
            get
            {
                return _currentPage;
            }
            set
            {
                _currentPage = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<CityViewModel> AvailableCities { get; set; } 

        public string SearchedCity
        {
            get
            {
                return _searchedCity;
            }
            set
            { 
                _searchedCity = value;
                RefreshViewModelCityCollection(_cityContainer.GetSearchedCityByName(_searchedCity));
            }
        }

        public RelayCommand CheckAllCities
        {
            get
            {
                return _checkAllCities;
            }
        }

        public RelayCommand UncheckAllCities
        {
            get
            {
                return _uncheckAllCities;
            }
        }

        public ObservableCollection<TechnologyViewModel> Technologies { get; set; }

        public string SearchedTechnology
        {
            get
            {
                return _searchedTechnology;
            }
            set
            {
                _searchedTechnology = value;
                RefreshViewModelTechnologyCollection(_technologyContainer.GetSearchedTechnologyByName(_searchedTechnology));
            }
        }

        public RelayCommand AddTechnology
        {
            get
            {
                return _addTechnology;
            }
        }

        public RelayCommand CheckAllTechnologies
        {
            get
            {
                return _checkAllTechnologies;
            }
        }

        public RelayCommand UncheckAllTechnologies
        {
            get
            {
                return _uncheckAllTechnologies;
            }
        }

        public RelayCommand RemoveTechnology
        {
            get
            {
                return _removeTechnology;
            }
        }

        public RelayCommand MakeLog
        {
            get
            {
                return _makeLog;
            }
        }

        public RelayCommand Parse
        {
            get
            {
                return _parse;
            }
        }

        private readonly RelayCommand _openCommonInfo;
        public RelayCommand OpenCommonInfo
        {
            get
            {
                return _openCommonInfo;
            }
        }

        private readonly RelayCommand _openVacancyInfo;
        public RelayCommand OpenVacancyInfo
        {
            get
            {
                return _openVacancyInfo;
            }
        }

        private readonly RelayCommand _openResumeInfo;
        public RelayCommand OpenResumeInfo
        {
            get
            {
                return _openResumeInfo;
            }
        }

        public RelayCommand OpenSettings
        {
            get
            {
                return _openSettings;
            }
        }

        public MainViewModel()
        {
            _commonInfoDataContext = new CommonInfoViewModel();
            _commonInfo = new CommonInfoView();
            _commonInfo.DataContext = _commonInfoDataContext;

            _resumeInfoDataContext = new ResumeInfoViewModel();
            _resumeInfo = new ResumeInfoView();
            _resumeInfo.DataContext = _resumeInfoDataContext;

            _vacancyInfoDataContext = new VacancyInfoViewModel();
            _vacancyInfo = new VacancyInfoView();
            _vacancyInfo.DataContext = _vacancyInfoDataContext;

            _currentPage = _commonInfo;

            _openCommonInfo = new RelayCommand(SetCommonInfoPage);
            _openVacancyInfo = new RelayCommand(SetVacancyInfoPage);
            _openResumeInfo = new RelayCommand(SetResumeInfoPage);

            _checkAllCities = new RelayCommand(CheckAllVMCityCollectionItems);
            _uncheckAllCities = new RelayCommand(UncheckAllVMCityCollectionItems);

            _checkAllTechnologies = new RelayCommand(CheckAllVMTechCollectionItems);
            _uncheckAllTechnologies = new RelayCommand(UncheckAllVMTechCollectionItems);
            _addTechnology = new RelayCommand(AddVMTechCollectionNewTech);
            _removeTechnology = new RelayCommand(RemoveVMTechCollectionItem);

            _openSettings = new RelayCommand((obj) =>
            {
                OpenSettingsWindow(obj);
            }, (obj) => !_isParsingStarted);

            _cityContainer = new CityContainer();
            _technologyContainer = new TechnologyContainer();
            AvailableCities = new ObservableCollection<CityViewModel>(
                _cityContainer.Cities.Select(city => new CityViewModel(city)));
            SubscribeVMCityCollectionItems_PropertyChanged();
            Technologies = new ObservableCollection<TechnologyViewModel>(
                _technologyContainer.Technologies.Select(tech => new TechnologyViewModel(tech)));
            SubscribeVMTechCollectionItems_PropertyChanged();

            _parser = new Parser();
            _parse = new RelayCommand((obj) =>
            {
                Task.Run(() =>
                {
                    _parser.BuildURL(_cityContainer.GetCheckedCitiesId(), _technologyContainer.GetCheckedTechsNames());
                    Parallel.Invoke(
                        () => 
                        {
                            if (SettingsContainer.settings.IsCommonInfoParsingOn)
                            {
                                _isParsingStarted = true;
                                _parser.ParseCommonInfo(_cityContainer.GetCheckedCitiesName(),
                                        _technologyContainer.GetCheckedTechsNames(),
                                        ref _commonInfoDataContext);
                            }
                        },
                        () =>
                        {
                            if (SettingsContainer.settings.IsVacancyParsingOn)
                            {
                                _isParsingStarted = true;
                                _parser.ParseVacancyInfo(_cityContainer.GetCheckedCitiesName(),
                                        _technologyContainer.GetCheckedTechsNames(),
                                        ref _vacancyInfoDataContext);
                            }
                        },
                        () =>
                        {
                            if (SettingsContainer.settings.IsResumeParsingOn)
                            {
                                _isParsingStarted = true; 
                                _parser.ParseResumeInfo(_cityContainer.GetCheckedCitiesName(),
                                        _technologyContainer.GetCheckedTechsNames(),
                                        ref _resumeInfoDataContext);
                            }
                        }
                        );
                    _isParsingStarted = false;
                    App.Current.Dispatcher?.Invoke(() =>
                        System.Windows.MessageBox.Show("Парсинг завершен")
                    );
                });
            }, (obj) => !_isSettingsWindowOpened && Logger.IsLoggingOver);

            _makeLog = new RelayCommand((obj) =>
            {
                Task.Run(() =>
                {
                    Logger.CreateLog(
                        _commonInfoDataContext.ParsedText,
                        _vacancyInfoDataContext.ParsedText,
                        _resumeInfoDataContext.ParsedText);
                });
            }, (obj) => !_isParsingStarted && Logger.IsLoggingOver);
        }
        
        private void SetCommonInfoPage(object obj)
        {
            CurrentPage = _commonInfo;
        }

        private void SetVacancyInfoPage(object obj)
        {
            CurrentPage = _vacancyInfo;
        }

        private void SetResumeInfoPage(object obj)
        {
            CurrentPage = _resumeInfo;
        }

        private void RefreshViewModelCityCollection(ObservableCollection<CityRow> cityRows)
        {
            UnsubscribeVMCityCollectionItems_PropertyChanged();
            AvailableCities.Clear();
            for (int i = 0; i < cityRows.Count - 1; i++)
            {
                AvailableCities.Add(new CityViewModel(cityRows[i]));
            }
            if ((cityRows.Count - 1) >= 0)
                AvailableCities.Add(new CityViewModel(cityRows[cityRows.Count - 1]));
            SubscribeVMCityCollectionItems_PropertyChanged();
        }

        private void SubscribeVMCityCollectionItems_PropertyChanged()
        {
            foreach (var city in AvailableCities)
            {
                city.PropertyChanged += RefreshCityContainer;
            }
        }

        private void UnsubscribeVMCityCollectionItems_PropertyChanged()
        {
            foreach (var city in AvailableCities)
            {
                city.PropertyChanged -= RefreshCityContainer;
            }
        }

        private void RefreshCityContainer(object sender, PropertyChangedEventArgs e)
        {
            _cityContainer.RefreshCityCheckOption(AvailableCities);
        }

        private void CheckAllVMCityCollectionItems(object obj)
        {
            UnsubscribeVMCityCollectionItems_PropertyChanged();
            foreach (var city in AvailableCities)
            {
                if (!city.IsSelected)
                    city.IsSelected = true;
            }
            _cityContainer.CheckAllCities();
            SubscribeVMCityCollectionItems_PropertyChanged();
        }

        private void UncheckAllVMCityCollectionItems(object obj)
        {
            UnsubscribeVMCityCollectionItems_PropertyChanged();
            foreach (var city in AvailableCities)
            {
                if (city.IsSelected)
                    city.IsSelected = false;
            }
            _cityContainer.UncheckAllCities();
            SubscribeVMCityCollectionItems_PropertyChanged();
        }

        private void RefreshViewModelTechnologyCollection(ObservableCollection<TechnologyRow> techRows)
        {
            UnsubscribeVMTechCollectionItems_PropertyChanged();
            Technologies.Clear();
            for (int i = 0; i < techRows.Count - 1; i++)
            {
                Technologies.Add(new TechnologyViewModel(techRows[i]));
            }
            if ((techRows.Count - 1) >= 0)
                Technologies.Add(new TechnologyViewModel(techRows[techRows.Count - 1]));
            SubscribeVMTechCollectionItems_PropertyChanged();
        }

        private void SubscribeVMTechCollectionItems_PropertyChanged()
        {
            foreach (var technology in Technologies)
            {
                technology.PropertyChanged += RefreshTechnologyContainer;
            }
        }

        private void UnsubscribeVMTechCollectionItems_PropertyChanged()
        {
            foreach (var technology in Technologies)
            {
                technology.PropertyChanged -= RefreshTechnologyContainer;
            }
        }

        private void RefreshTechnologyContainer(object sender, PropertyChangedEventArgs e)
        {
            _technologyContainer.RefreshTechnologyCheckOption(Technologies);
        }

        private void CheckAllVMTechCollectionItems(object obj)
        {
            UnsubscribeVMTechCollectionItems_PropertyChanged();
            foreach (var technology in Technologies)
            {
                if (!technology.IsSelected)
                    technology.IsSelected = true;
            }
            _technologyContainer.CheckAllTechnologies();
            SubscribeVMTechCollectionItems_PropertyChanged();
        }

        private void UncheckAllVMTechCollectionItems(object obj)
        {
            UnsubscribeVMTechCollectionItems_PropertyChanged();
            foreach (var technology in Technologies)
            {
                if (technology.IsSelected)
                    technology.IsSelected = false;
            }
            _technologyContainer.UncheckAllTechnologies();
            SubscribeVMTechCollectionItems_PropertyChanged();
        }

        private void AddVMTechCollectionNewTech(object obj)
        {
            if (_technologyContainer.CanAddNewTechnology(_searchedTechnology))
            {
                Technologies.Add(new TechnologyViewModel(new TechnologyRow(_searchedTechnology)));
                _technologyContainer.AddTechnology(_searchedTechnology);
            }
            else
                _technologyContainer.ShowAddingFailureReasons(_searchedTechnology);
        }

        private void RemoveVMTechCollectionItem(object obj)
        {
            var removedTechs = new ObservableCollection<TechnologyViewModel>();
            int removedTechIndex = -1;
            foreach (var technology in Technologies)
            {
                if (technology.IsSelected)
                    removedTechs.Add(technology);
            }
            foreach (var removedTech in removedTechs)
            {
                for (int i = 0; i < Technologies.Count; i++)
                    if (removedTech.TechnologyName == Technologies[i].TechnologyName)
                    {
                        removedTechIndex = i;
                        break;
                    }
                Technologies.RemoveAt(removedTechIndex);
            }
            _technologyContainer.RemoveCheckedTechnologies();
        }

        private void OpenSettingsWindow(object obj)
        {
            SettingsView settingsWindow = new SettingsView();
            settingsWindow.Show();
            _isSettingsWindowOpened = true;
            settingsWindow.Closed += SettingsWindow_Closed;
        }

        private void SettingsWindow_Closed(object sender, EventArgs e)
        {
            _isSettingsWindowOpened = false;
        }

    }

}
