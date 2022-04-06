using System.IO;

namespace HeadHunter_Parser.Models
{
    public class Settings
    {

        private const string _settingsFileName = "settings.set";

        /*DEFAULT VALUES FOR DATABASE IF IT DOESN'T EXIST IN FOLDER WITH APP*/

        private const int _defaultMaxResumeAmount = _maxAvailableResumeAmount;

        private const int _defaultMaxVacancyAmount = _maxAvailableVacancyAmount;

        private const bool _defaultCheckOption = true;

        /**/

        private const int _maxAvailableResumeAmount = 100000000;

        private const int _maxAvailableVacancyAmount = 2000;

        public int MaxAvailableResumeAmount {
            get
            {
                return _maxAvailableResumeAmount;
            }
        }

        public int MaxAvailableVacancyAmount
        {
            get
            {
                return _maxAvailableVacancyAmount;
            }
        }

        public int MaxResumeAmount { get; private set; }

        public int MaxVacancyAmount { get; private set; }

        public bool IsCommonInfoParsingOn { get; private set; }

        public bool IsVacancyParsingOn { get; private set; }

        public bool IsResumeParsingOn { get; private set; }

        public Settings()
        {
            ReadSettingsDatabase();
        }

        public void SaveSettings
            (string maxResumeAmount,
             string maxVacancyAmount,
             bool isCommonInfoParsingOn,
             bool isVacancyParsingOn,
             bool isResumeParsingOn
            )
        {
            MaxResumeAmount = int.Parse(maxResumeAmount);
            MaxVacancyAmount = int.Parse(maxVacancyAmount);
            IsCommonInfoParsingOn = isCommonInfoParsingOn;
            IsVacancyParsingOn = isVacancyParsingOn;
            IsResumeParsingOn = isResumeParsingOn;
            SaveSettingsInDatabase();
        }

        private void SaveSettingsInDatabase()
        {
            using (StreamWriter database = new StreamWriter(_settingsFileName))
            {
                database.WriteLine(MaxResumeAmount);
                database.WriteLine(MaxVacancyAmount);
                database.WriteLine(IsCommonInfoParsingOn);
                database.WriteLine(IsVacancyParsingOn);
                database.WriteLine(IsResumeParsingOn);
            }
        }

        private void ReadSettingsDatabase()
        {
            if (!File.Exists(_settingsFileName))
                CreateDefaultDatabase();
            using (StreamReader database = new StreamReader(_settingsFileName))
            {
                MaxResumeAmount = int.Parse(database.ReadLine());
                MaxVacancyAmount = int.Parse(database.ReadLine());
                IsCommonInfoParsingOn = bool.Parse(database.ReadLine());
                IsVacancyParsingOn = bool.Parse(database.ReadLine());
                IsResumeParsingOn = bool.Parse(database.ReadLine());
            }
        }

        private void CreateDefaultDatabase()
        {
            using (StreamWriter database = new StreamWriter(_settingsFileName))
            {
                database.WriteLine(_defaultMaxResumeAmount);
                database.WriteLine(_defaultMaxVacancyAmount);
                database.WriteLine(_defaultCheckOption);
                database.WriteLine(_defaultCheckOption);
                database.WriteLine(_defaultCheckOption);
            }
        }

    }
}
