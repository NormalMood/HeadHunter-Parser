using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HeadHunter_Parser.Models
{
    public static class Logger
    {

        public static bool IsLoggingOver = true;

        private const string _commonInfoFileNamePart = "-common_info";

        private const string _commonInfoFileLogExtension = ".log";

        private const string _vacancyInfoFileNamePart = "-vacancy_info.log";

        private const string _resumeInfoFileNamePart = "-resume_info.log";

        private const string _defaultCultureName = "ru-RU";

        private static CultureInfo _cultureInfo = new CultureInfo(_defaultCultureName);

        private const string _rootLoggingFolder = @"Logs\";

        private const string _cityColumnHeader = "Регион/Город";

        private const string _techColumnHeader = "Технология";

        private const string _vacancyColumnHeader = "Вакансий";

        private const string _resumeColumnHeader = "Резюме";

        private const string _applicantsColumnHeader = "Соискателей";

        private const string _peoplePerPositionColumnHeader = "Чел/место";

        private const string _techPerspectiveColumnHeader = "Перспективность";

        private const int _csvFileColumnsAmount = 7;

        private const string _csvFileLineSeparator = ";";

        private const char _csvFileInappropriateSymbol = '-';

        public static void CreateLog(string commonInfo, string vacancyInfo, string resumeInfo)
        {
            try
            {
                if ((!string.IsNullOrEmpty(commonInfo)) ||
                    (!string.IsNullOrEmpty(vacancyInfo)) ||
                    (!string.IsNullOrEmpty(resumeInfo)))
                {
                    IsLoggingOver = false;
                    if (!Directory.Exists(_rootLoggingFolder))
                        Directory.CreateDirectory(_rootLoggingFolder);
                    DateTime dateTime = DateTime.Now;
                    string dateTimeRussian = dateTime.ToString(_cultureInfo).Replace(':', '-');
                    Directory.CreateDirectory(_rootLoggingFolder + dateTimeRussian);
                    CreateCommonInfoLog(commonInfo, _rootLoggingFolder + dateTimeRussian + @"\" + dateTimeRussian + _commonInfoFileNamePart);
                    CreateVacancyInfoLog(in vacancyInfo, _rootLoggingFolder + dateTimeRussian + @"\" + dateTimeRussian + _vacancyInfoFileNamePart);
                    CreateResumeInfoLog(in resumeInfo, _rootLoggingFolder + dateTimeRussian + @"\" + dateTimeRussian + _resumeInfoFileNamePart);
                    Application.Current.Dispatcher?.Invoke(() =>
                        MessageBox.Show("Создание логов завершено")
                    );
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Во время создания логов произошла ошибка: \n{ex.Message}");
            }
            IsLoggingOver = true;
        }

        private static void CreateCommonInfoLog(string commonInfo, string loggingFileName)
        {
            if (!string.IsNullOrEmpty(commonInfo))
            {
                File.WriteAllText(loggingFileName + _commonInfoFileLogExtension, commonInfo);
            }
        }

        private static void CreateVacancyInfoLog(in string vacancyInfo, string loggingFileName)
        {
            if (!string.IsNullOrEmpty(vacancyInfo))
            {
                File.WriteAllText(loggingFileName, vacancyInfo);
            }
        }

        private static void CreateResumeInfoLog(in string resumeInfo, string loggingFileName)
        {
            if (!string.IsNullOrEmpty(resumeInfo))
            {
                File.WriteAllText(loggingFileName, resumeInfo);
            }
        }

    }
}
