using HeadHunter_Parser.Commands;
using HeadHunter_Parser.Models;
using System.Windows;

namespace HeadHunter_Parser.ViewModels
{
    public class SettingsViewModel : Base.BasePropertyChanged
    {

        private RelayCommand _saveSettings;

        private string _maxResumeAmount;

        public string MaxResumeAmount
        {
            get
            {
                return _maxResumeAmount;
            }
            set
            {
                _maxResumeAmount = value;
                OnPropertyChanged();
            }
        }

        private string _maxVacancyAmount;

        public string MaxVacancyAmount
        {
            get
            {
                return _maxVacancyAmount;
            }
            set
            {
                if (value == "")
                    _maxVacancyAmount = "2000";
                else
                    _maxVacancyAmount = value;
                OnPropertyChanged();
            }
        }

        private bool _isCommonInfoParsingOn;

        public bool IsCommonInfoParsingOn
        {
            get
            {
                return _isCommonInfoParsingOn;
            }
            set
            {
                _isCommonInfoParsingOn = value;
                OnPropertyChanged();
            }
        }

        private bool _isVacancyParsingOn;

        public bool IsVacancyParsingOn
        {
            get
            {
                return _isVacancyParsingOn;
            }
            set
            {
                _isVacancyParsingOn = value;
                OnPropertyChanged();
            }
        }

        private bool _isResumeParsingOn;

        public bool IsResumeParsingOn
        {
            get
            {
                return _isResumeParsingOn;
            }
            set
            {
                _isResumeParsingOn = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand SaveSettings
        {
            get
            {
                return _saveSettings;
            }
        }

        public SettingsViewModel()
        {
            _maxResumeAmount = SettingsContainer.settings.MaxResumeAmount.ToString();
            _maxVacancyAmount = SettingsContainer.settings.MaxVacancyAmount.ToString();
            _isCommonInfoParsingOn = SettingsContainer.settings.IsCommonInfoParsingOn;
            _isVacancyParsingOn = SettingsContainer.settings.IsVacancyParsingOn;
            _isResumeParsingOn = SettingsContainer.settings.IsResumeParsingOn;
            _saveSettings = new RelayCommand(SaveNewSettings);
        }

        private void SaveNewSettings(object obj)
        {
            if (HasCorrectTextFields(MaxResumeAmount, MaxVacancyAmount))
                SettingsContainer.settings.SaveSettings
                    (
                        MaxResumeAmount, 
                        MaxVacancyAmount, 
                        IsCommonInfoParsingOn, 
                        IsVacancyParsingOn, 
                        IsResumeParsingOn
                    );
        }

        private bool HasCorrectTextFields(string maxResumeAmount, string maxVacancyAmount)
        {
            if (!int.TryParse(maxResumeAmount, out int maxResumeResult))
            {
                MessageBox.Show("Введите число в поле для количества резюме");
                return false;
            }
            if (!int.TryParse(maxVacancyAmount, out int maxVacancyResult))
            {
                MessageBox.Show("Введите число в поле для количества вакансий");
                return false;
            }
            if ((int.Parse(maxResumeAmount) < 1) || 
                (int.Parse(maxResumeAmount) > SettingsContainer.settings.MaxAvailableResumeAmount))
            {
                MessageBox.Show($"Можно ввести число от 1 до {SettingsContainer.settings.MaxAvailableResumeAmount}");
                return false;
            }
            if ((int.Parse(maxVacancyAmount) < 1) ||
                (int.Parse(maxVacancyAmount) > SettingsContainer.settings.MaxAvailableVacancyAmount))
            {
                MessageBox.Show($"Можно ввести число от 1 до {SettingsContainer.settings.MaxAvailableVacancyAmount}");
                return false;
            }
            return true;
        }

    }
}
