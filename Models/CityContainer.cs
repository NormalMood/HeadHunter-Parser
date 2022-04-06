using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HeadHunter_Parser.Models
{
    public class CityContainer
    {

        private const string _citiesFileName = "cities.txt";

        private const char _infoSeparator = ':';

        private ObservableCollection<CityRow> _cities;

        public ReadOnlyObservableCollection<CityRow> Cities;

        public CityContainer()
        {
            _cities = GetCitiesInformation();
            Cities = new ReadOnlyObservableCollection<CityRow>(_cities);
        }

        private ObservableCollection<CityRow> GetCitiesInformation()
        {
            StreamReader citiesFile = new StreamReader(_citiesFileName);
            var citiesInfo = new ObservableCollection<CityRow>();
            string tempLine;
            string[] tempLines;
            while ((tempLine = citiesFile.ReadLine()) != null)
            {
                tempLines = tempLine.Split(_infoSeparator);
                citiesInfo.Add(new CityRow(tempLines[0], tempLines[1]));
            }
            citiesFile.Close();
            return citiesInfo;
        }

        public void RefreshCityCheckOption(ObservableCollection<ViewModels.CityViewModel> cityRows)
        {
            int cityIndex = -1;
            foreach (var cityRow in cityRows)
            {
                for (int i = 0; i < _cities.Count; i++)
                    if (_cities[i].CityName == cityRow.CityName)
                    {
                        cityIndex = i;
                        break;
                    }
                if (_cities[cityIndex].IsChecked != cityRow.IsSelected)
                    _cities[cityIndex].IsChecked = cityRow.IsSelected;
            }
        }
       
        public ObservableCollection<CityRow> GetSearchedCityByName(string cityName)
        {
            if (cityName == "")
                return _cities;
            cityName = cityName.ToLower();
            var requestedCities = new ObservableCollection<CityRow>();
            for (int i = 0; i < _cities.Count; i++)
            {
                if (_cities[i].CityName.ToLower().IndexOf(cityName) != -1)
                    requestedCities.Add(_cities[i]);
            }
            return requestedCities;
        }

        public string[] GetCheckedCitiesId()
        {
            List<string> id = new List<string>();
            for (int i = 0; i < _cities.Count; i++)
            {
                if (_cities[i].IsChecked)
                    id.Add(_cities[i].ID);
            }
            return id.ToArray();
        }

        public string[] GetCheckedCitiesName()
        {
            List<string> names = new List<string>();
            for (int i = 0; i < _cities.Count; i++)
            {
                if (_cities[i].IsChecked)
                    names.Add(_cities[i].CityName);
            }
            return names.ToArray();
        }

        public void CheckAllCities()
        {
            foreach (var city in _cities)
            {
                if (!city.IsChecked)
                    city.IsChecked = true;
            }
        }

        public void UncheckAllCities()
        {
            foreach (var city in _cities)
            {
                if (city.IsChecked)
                    city.IsChecked = false;
            }
        }

    }
}
