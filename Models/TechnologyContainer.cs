using System.Collections.Generic;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows;

namespace HeadHunter_Parser.Models
{
    public class TechnologyContainer
    {

        private const string _technologiesFileName = "technologies.txt";

        private ObservableCollection<TechnologyRow> _technologies;

        public ReadOnlyObservableCollection<TechnologyRow> Technologies;

        public TechnologyContainer()
        {
            _technologies = GetTechnologiesInformation();
            Technologies = new ReadOnlyObservableCollection<TechnologyRow>(_technologies);
        }

        private ObservableCollection<TechnologyRow> GetTechnologiesInformation()
        {
            StreamReader technologiesFile = new StreamReader(_technologiesFileName);
            string tempLine;
            var tempTechs = new ObservableCollection<TechnologyRow>();
            while ((tempLine = technologiesFile.ReadLine()) != null)
                tempTechs.Add(new TechnologyRow(tempLine));
            technologiesFile.Close();
            return tempTechs;
        }

        public ObservableCollection<TechnologyRow> GetAllTechnologies() =>
            _technologies;

        public ObservableCollection<TechnologyRow> GetSearchedTechnologyByName(string techName)
        {
            if (techName == "")
                return GetAllTechnologies();
            techName = techName.ToLower();
            var requestedCities = new ObservableCollection<TechnologyRow>();
            for (int i = 0; i < _technologies.Count; i++)
            {
                if (_technologies[i].TechnologyName.ToLower().IndexOf(techName) != -1)
                    requestedCities.Add(_technologies[i]);
            }
            return requestedCities;
        }

        public TechnologyRow GetTechnologyByName(string techName) =>
            ContainsTechName(techName) ? new TechnologyRow(techName) : null;

        public void RefreshTechnologyCheckOption(ObservableCollection<ViewModels.TechnologyViewModel> technologyRows)
        {
            int cityIndex = -1;
            foreach (var techRow in technologyRows)
            {
                for (int i = 0; i < _technologies.Count; i++)
                    if (_technologies[i].TechnologyName == techRow.TechnologyName)
                    {
                        cityIndex = i;
                        break;
                    }
                if (_technologies[cityIndex].IsChecked != techRow.IsSelected)
                    _technologies[cityIndex].IsChecked = techRow.IsSelected;
            }
        }

        public bool CanAddNewTechnology(string technologyName) =>
            string.IsNullOrWhiteSpace(technologyName) || ContainsTechName(technologyName) ? false : true;

        public void ShowAddingFailureReasons(string technologyName)
        {
            if (string.IsNullOrWhiteSpace(technologyName))
                MessageBox.Show("Введите непустую строку");
            else if (ContainsTechName(technologyName))
                MessageBox.Show("Технология уже присутствует в списке");
        }

        public void AddTechnology(string technologyName)
        {
            _technologies.Add(new TechnologyRow(technologyName));
            AddTechnologyInDatabase(technologyName);
        }

        private void AddTechnologyInDatabase(string technologyName)
        {
            bool isAppend;
            if (File.Exists(_technologiesFileName))
                isAppend = true;
            else
                isAppend = false;
            StreamWriter techDatabase = new StreamWriter(_technologiesFileName, isAppend);
            techDatabase.WriteLine(technologyName);
            techDatabase.Close();
        }

        public void RemoveCheckedTechnologies()
        {
            var uncheckedTechnologies = new ObservableCollection<TechnologyRow>();
            foreach (var technology in _technologies)
            {
                if (!technology.IsChecked)
                    uncheckedTechnologies.Add(technology);
            }
            _technologies.Clear();
            _technologies = uncheckedTechnologies;
            WriteInDatabaseUncheckedTechs(); //removing checked technologies
        }

        private void WriteInDatabaseUncheckedTechs()
        {
            StreamWriter techDatabase = new StreamWriter(_technologiesFileName);
            foreach (var technology in _technologies)
            {
                techDatabase.WriteLine(technology.TechnologyName);
            }
            techDatabase.Close();
        }

        private bool ContainsTechName(string technologyName)
        {
            foreach (var technology in _technologies)
            {
                if (technology.TechnologyName.ToLower() == technologyName.ToLower())
                    return true;
            }
            return false;
        }

        public void CheckAllTechnologies()
        {
            foreach (var technology in _technologies)
            {
                if (!technology.IsChecked)
                    technology.IsChecked = true;
            }
        }

        public void UncheckAllTechnologies()
        {
            foreach (var technology in _technologies)
            {
                if (technology.IsChecked)
                    technology.IsChecked = false;
            }
        }

        public string[] GetCheckedTechsNames()
        {
            var names = new List<string>();
            foreach (var tech in _technologies)
            {
                if (tech.IsChecked)
                    names.Add(tech.TechnologyName);
            }
            return names.ToArray();
        }

    }
}
