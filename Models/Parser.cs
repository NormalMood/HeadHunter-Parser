using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.IO;
using Newtonsoft.Json;
using HtmlAgilityPack;

namespace HeadHunter_Parser.Models
{
    public class Parser
    {

        private List<string> _vacancyUrl;

        private List<string> _resumeUrl;

        private const int _roundAccuracy = 2;

        private const string _searchedVacancyFirstPartTemplate = "https://api.hh.ru/vacancies?";

        private const string _searchedVacancySecondPartTemplate = "&per_page=100";

        private const string _searchedResumeFirstPartTemplate = "https://hh.ru/search/resume?clusters=True";

        private const string _searchedResumeSecondPartTemplate = "&ored_clusters=True&order_by=relevance&logic=normal&pos=full_text&exp_period=all_time";

        private bool _isURLBuildingPossible;

        private bool _isCommonInfoParsingStopped = true;

        private bool _isVacancyInfoParsingStopped = true;

        private bool _isResumeInfoParsingStopped = true;

        public bool IsParsingStopped 
        {
            get
            {
                return _isCommonInfoParsingStopped && _isVacancyInfoParsingStopped && _isResumeInfoParsingStopped;
            }
        }

        public void ParseHeadHunter(
            CityContainer cityContainer,
            TechnologyContainer techContainer,
            ref ViewModels.CommonInfoViewModel commonInfoDataContext,
            ref ViewModels.VacancyInfoViewModel vacancyInfoDataContext,
            ref ViewModels.ResumeInfoViewModel resumeInfoDataContext
            )
        {
            BuildURL(cityContainer.GetCheckedCitiesId(), techContainer.GetCheckedTechsNames());
            if (!_isURLBuildingPossible)
                return;
               ParseCommonInfo(cityContainer.GetCheckedCitiesName(), techContainer.GetCheckedTechsNames(), ref commonInfoDataContext);
              ParseVacancyInfo(cityContainer.GetCheckedCitiesName(), techContainer.GetCheckedTechsNames(), ref vacancyInfoDataContext);
            ParseResumeInfo(cityContainer.GetCheckedCitiesName(), techContainer.GetCheckedTechsNames(), ref resumeInfoDataContext);
        }

        private void CheckURLBuildingPossibility(string[] citiesID, string[] techsNames)
        {
            _isURLBuildingPossible = true;

            if (citiesID.Length == 0)
            {
                MessageBox.Show("Выберите регион/город");
                _isURLBuildingPossible = false;
            }
            if (techsNames.Length == 0)
            {
                MessageBox.Show("Выберите технологию");
                _isURLBuildingPossible = false;
            }
        }

        private string[] GetCorrectTechnologyName(string[] techsNames) //correcting some symbols for headhunter's url before parsing
        {
            string[] correctedTechsNames = new string[techsNames.Length];
            for (int i = 0; i < techsNames.Length; i++)
                correctedTechsNames[i] = techsNames[i];
            List<string> symbols;
            
            for (int i = 0; i < correctedTechsNames.Length; i++)
            {
                symbols = new List<string>();
                foreach (var letter in correctedTechsNames[i])
                {
                    symbols.Add(letter.ToString());
                }
                for (int j = 0; j < symbols.Count; j++)
                {
                    for (int k = 0; k < CorrectURLSymbol.CorrectURLSymbols.Count; k++)
                    {
                        if (symbols[j].Contains(CorrectURLSymbol.CorrectURLSymbols[k].InitialSymbol))
                        {
                            symbols[j] = CorrectURLSymbol.CorrectURLSymbols[k].CorrectedSymbol;
                            break;
                        }
                    }
                }
                correctedTechsNames[i] = string.Join("", symbols);
            }
            return correctedTechsNames;
        }

        public void BuildURL(string[] citiesID, string[] techsNames)
        {
            _vacancyUrl = new List<string>();
            _resumeUrl = new List<string>();

            CheckURLBuildingPossibility(citiesID, techsNames);

            if (!_isURLBuildingPossible)
                return;

            techsNames = GetCorrectTechnologyName(techsNames);
            for (int i = 0; i < citiesID.Length; i++)
            {
                for (int j = 0; j < techsNames.Length; j++)
                {
                    _vacancyUrl.Add($"{_searchedVacancyFirstPartTemplate}area={citiesID[i]}{_searchedVacancySecondPartTemplate}&text={techsNames[j]}");
                    _resumeUrl.Add($"{_searchedResumeFirstPartTemplate}&area={citiesID[i]}{_searchedResumeSecondPartTemplate}&text={techsNames[j]}");
                }
            }
        }

        public void ParseCommonInfo(string[] citiesNames, string[] techsNames, ref ViewModels.CommonInfoViewModel commonInfoDataContext)
        {
            if (!_isURLBuildingPossible)
                return;
            _isCommonInfoParsingStopped = false;
            commonInfoDataContext.ParsedText = "";
            int urlCollectionIndex = 0;
            string parsedText;
            int vacancyAmount = 0;
            int resumeAmount = 0;
            int applicants = 0;
            double applicantsPerPlace;
            double technologyProspects;
            for (int i = 0; i < citiesNames.Length; i++)
            {
                commonInfoDataContext.ParsedText += $"Регион/Город: {citiesNames[i]}\n";
                for (int j = 0; j < techsNames.Length; j++)
                {
                    commonInfoDataContext.ParsedText += $"\nТехнология: {techsNames[j]}\n";

                    HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(_vacancyUrl[urlCollectionIndex]);
                    httpWebRequest.UserAgent = "MyParser";
                    HttpWebResponse webResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (StreamReader stream = new StreamReader(webResponse.GetResponseStream(), Encoding.UTF8))
                    {
                        parsedText = stream.ReadToEnd();
                        dynamic headHunterJSON = JsonConvert.DeserializeObject(parsedText);
                        vacancyAmount = headHunterJSON.found;
                    }

                    commonInfoDataContext.ParsedText += $"Вакансий: {vacancyAmount}\n";

                    using (HttpClientHandler clientHandler = new HttpClientHandler 
                    {
                        AllowAutoRedirect = true,
                        AutomaticDecompression = DecompressionMethods.GZip | 
                        DecompressionMethods.Deflate | 
                        DecompressionMethods.None
                    })
                    {
                        using (HttpClient client = new HttpClient(clientHandler))
                        {
                            using (HttpResponseMessage response = client.GetAsync(_resumeUrl[urlCollectionIndex]).Result)
                            {
                                if (response.IsSuccessStatusCode)
                                {
                                    var html = response.Content.ReadAsStringAsync().Result;
                                    if (!string.IsNullOrEmpty(html))
                                    {
                                        HtmlDocument document = new HtmlDocument();
                                        document.LoadHtml(html);
                                        var vacancyHeader = document.DocumentNode.SelectNodes(".//h1[@class='bloko-header-1']");
                                        parsedText = vacancyHeader.Select(text => text.InnerText).FirstOrDefault();
                                        if (!string.IsNullOrEmpty(parsedText))
                                        {
                                            if (parsedText[8] == '0') //Найдено 0... <- amount of resumes
                                                resumeAmount = 0;
                                            else
                                                resumeAmount = int.Parse(parsedText
                                                    .Substring(8, parsedText.IndexOf('р') - 1 - 8)
                                                    .Replace(" ", "")); //deleting nbspace, not just space
                                            if (!parsedText.Contains("соискател"))
                                                applicants = 0;
                                            else if (parsedText[parsedText.IndexOf("соискател") - 2] == '0' &&
                                                     parsedText[parsedText.IndexOf("соискател") - 3] == ' ') //0 <- amount of applicants
                                                applicants = 0;
                                            else
                                                applicants = int.Parse(parsedText
                                                    .Substring(parsedText.IndexOf("резюме") + 9, parsedText.IndexOf('с') - 1 - 9 - parsedText.IndexOf("резюме"))
                                                    .Replace(" ", "")); //deleting nbspace, not just space
                                        }
                                    }
                                }
                            }
                        }
                    }

                    commonInfoDataContext.ParsedText += $"Резюме: {resumeAmount}\nСоискателей: {applicants}\n";
                    if (vacancyAmount != 0)
                        applicantsPerPlace = Math.Round((double)applicants / vacancyAmount, _roundAccuracy);
                    else
                        applicantsPerPlace = 0;
                    commonInfoDataContext.ParsedText += $"Чел/место: {applicantsPerPlace}\n";
                    technologyProspects = Math.Round(applicantsPerPlace * vacancyAmount, _roundAccuracy);
                    commonInfoDataContext.ParsedText += $"Перспективность технологии (Чел/место * вакансии): {technologyProspects}\n";

                    urlCollectionIndex++;
                }

                commonInfoDataContext.ParsedText += "\n--------------------------------------------\n\n";

            }
            
            _isCommonInfoParsingStopped = true;
        }

        public void ParseResumeInfo(string[] citiesNames, string[] techsNames, ref ViewModels.ResumeInfoViewModel resumeInfoDataContext)
        {
            if (!_isURLBuildingPossible)
                return;
            _isResumeInfoParsingStopped = false;
            resumeInfoDataContext.ParsedText = ""; 
            int urlCollectionIndex = 0;
            int currentResumeAmountIndex = 0;
            bool hasPage = true;
            int page = 0;
            for (int i = 0; i < citiesNames.Length; i++)
            {
                resumeInfoDataContext.ParsedText += $"Регион/Город: {citiesNames[i]}\n";
                for (int j = 0; j < techsNames.Length; j++)
                {

                    currentResumeAmountIndex = 0;

                    resumeInfoDataContext.ParsedText += $"\nТехнология: {techsNames[j]}\n\n";
                    using (HttpClientHandler clientHandler = new HttpClientHandler
                    {
                        AllowAutoRedirect = true,
                        AutomaticDecompression = DecompressionMethods.GZip | 
                        DecompressionMethods.Deflate | 
                        DecompressionMethods.None
                    })
                    {
                        using (HttpClient client = new HttpClient(clientHandler))
                        {
                            while (hasPage)
                            {
                                using (HttpResponseMessage response = client.GetAsync(_resumeUrl[urlCollectionIndex] + $"&page={page}").Result)
                                {
                                    if (response.IsSuccessStatusCode)
                                    {
                                        var html = response.Content.ReadAsStringAsync().Result;
                                        if (!string.IsNullOrEmpty(html))
                                        {
                                            HtmlDocument document = new HtmlDocument();
                                            document.LoadHtml(html);
                                            var resumes = document.DocumentNode.SelectNodes(".//div[@class='resume-search-item__content']");

                                            if (resumes != null)
                                            {
                                                foreach (var resume in resumes)
                                                {
                                                    if (currentResumeAmountIndex < SettingsContainer.settings.MaxResumeAmount)
                                                    {
                                                        resumeInfoDataContext.ParsedText += GetResumeContent(resume) + '\n';
                                                        currentResumeAmountIndex++;
                                                    }
                                                    else
                                                    {
                                                        hasPage = false;
                                                        break;
                                                    }
                                                }
                                                page++;
                                            }
                                            else if (resumes == null)
                                                hasPage = false;
                                        }
                                    } 
                                }
                            }
                        }
                    }

                    resumeInfoDataContext.ParsedText += "\n--------------------------------------------\n\n";

                    urlCollectionIndex++;
                    hasPage = true;
                    page = 0;
                }
            }
            _isResumeInfoParsingStopped = true;
        }

        public void ParseVacancyInfo(string[] citiesNames, string[] techsNames, ref ViewModels.VacancyInfoViewModel vacancyInfoDataContext)
        {
            if (!_isURLBuildingPossible)
                return;
            _isVacancyInfoParsingStopped = false;
            vacancyInfoDataContext.ParsedText = "";
            int urlCollectionIndex = 0;
            string parsedText;
            dynamic vacancies;
            string vacancyName;
            string url;
            dynamic salary;
            string salaryFrom, salaryTo;
            string salaryCurrency;
            string employerName;
            string schedule;
            string requirements, responsibilities;
            string address = "";
            string creationDate = "";
            int pages;
            int vacancyAmount;
            int currentVacancyAmountIndex = 0;
            dynamic headHunterJson;
            HttpWebRequest httpWebRequest;
            HttpWebResponse httpWebResponse;
            for (int i = 0; i < citiesNames.Length; i++)
            {
                vacancyInfoDataContext.ParsedText += $"Регион/Город: {citiesNames[i]}\n";
                for (int j = 0; j < techsNames.Length; j++)
                {

                    currentVacancyAmountIndex = 0;

                    vacancyInfoDataContext.ParsedText += $"\nТехнология: {techsNames[j]}\n";
                    httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(_vacancyUrl[urlCollectionIndex]);
                    httpWebRequest.UserAgent = "MyParser";
                    httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (StreamReader stream = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8))
                    {
                        parsedText = stream.ReadToEnd();
                        headHunterJson = JsonConvert.DeserializeObject(parsedText);
                        pages = headHunterJson.pages;
                        vacancyAmount = headHunterJson.found;
                        if (vacancyAmount > 0)
                        {
                            for (int page = 0; 
                                page < pages && currentVacancyAmountIndex < SettingsContainer.settings.MaxVacancyAmount; 
                                page++)
                            {
                                httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(_vacancyUrl[urlCollectionIndex] + $"&page={page}");
                                httpWebRequest.UserAgent = "MyParser";
                                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                                using (StreamReader vacancyStream = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8))
                                {
                                    parsedText = vacancyStream.ReadToEnd();
                                    headHunterJson = JsonConvert.DeserializeObject(parsedText);
                                    vacancies = headHunterJson.items;
                                    foreach (var vacancy in vacancies)
                                    {
                                        if (currentVacancyAmountIndex < SettingsContainer.settings.MaxVacancyAmount)
                                        {
                                            vacancyName = vacancy.name;
                                            url = vacancy.alternate_url;
                                            salary = vacancy.salary;
                                            employerName = vacancy.employer.name;
                                            if (vacancy.address != null)
                                                address = vacancy.address.raw + "\n";
                                            schedule = vacancy.schedule.name;
                                            requirements = vacancy.snippet.requirement;
                                            requirements = DeleteTextHighlightingTag(requirements);
                                            responsibilities = vacancy.snippet.responsibility;
                                            responsibilities = DeleteTextHighlightingTag(responsibilities);
                                            creationDate = GetCreationDate((string)vacancy.created_at);
                                            vacancyInfoDataContext.ParsedText += $"\n{vacancyName}\n";
                                            vacancyInfoDataContext.ParsedText += $"{url}\n";
                                            vacancyInfoDataContext.ParsedText += $"{schedule}\n";
                                            if (salary != null)
                                            {
                                                salaryFrom = salary.from;
                                                salaryTo = salary.to;
                                                salaryCurrency = salary.currency;
                                                if ((salaryFrom != null) && (salaryTo != null))
                                                    vacancyInfoDataContext.ParsedText += $"{salaryFrom} - {salaryTo} ";
                                                else if ((salaryFrom != null) && (salaryTo == null))
                                                    vacancyInfoDataContext.ParsedText += $"от {salaryFrom} ";
                                                if (salaryCurrency == "RUR")
                                                    vacancyInfoDataContext.ParsedText += "руб.\n";
                                                else
                                                    vacancyInfoDataContext.ParsedText += $"{salaryCurrency}\n";
                                            }
                                            vacancyInfoDataContext.ParsedText += $"{employerName}\n";
                                            vacancyInfoDataContext.ParsedText += $"{address}";
                                            vacancyInfoDataContext.ParsedText += $"{requirements}";
                                            vacancyInfoDataContext.ParsedText += $"{responsibilities}";
                                            vacancyInfoDataContext.ParsedText += $"{creationDate}\n";

                                            currentVacancyAmountIndex++;

                                        }
                                        else
                                            break;
                                    }
                                }
                            }
                        }
                        else
                            vacancyInfoDataContext.ParsedText += $"\nНайдено 0 вакансий\n";
                    }
                    urlCollectionIndex++;
                }
                vacancyInfoDataContext.ParsedText += "\n--------------------------------------------\n\n";
            }
            _isVacancyInfoParsingStopped = true;
        }

        private string DeleteTextHighlightingTag(string text) =>
            string.IsNullOrEmpty(text) ? text : text.Replace("<highlighttext>", "").Replace("</highlighttext>", "") + "\n";

        private string GetCreationDate(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            string[] date = text.Split(' ')[0].Split('/');
            string day, month, year;
            day = date[1];
            month = date[0];
            year = date[2];
            switch (month)
            {
                case "01":
                    month = "января";
                    break;
                case "02":
                    month = "февраля";
                    break;
                case "03":
                    month = "марта";
                    break;
                case "04":
                    month = "апреля";
                    break;
                case "05":
                    month = "мая";
                    break;
                case "06":
                    month = "июня";
                    break;
                case "07":
                    month = "июля";
                    break;
                case "08":
                    month = "августа";
                    break;
                case "09":
                    month = "сентября";
                    break;
                case "10":
                    month = "октября";
                    break;
                case "11":
                    month = "ноября";
                    break;
                case "12":
                    month = "декабря";
                    break;
                default:
                    break;
            }
            return $"Вакансия была опубликована {day.TrimStart('0')} {month} {year}";
        }

        private string GetResumeContent(HtmlNode nodes)
        {
            string resumeHtml = nodes.InnerHtml.Replace("<!-- -->", "").Replace("</span> <span>", " ");
            string url = resumeHtml?.Substring(resumeHtml.IndexOf("href=\"") + 6);
            url = url?.Substring(0, url.IndexOf("?query="));
            if (!string.IsNullOrEmpty(url))
                url = "https://hh.ru" + url;
            string resumeContent = "";
            int leftIndex = resumeHtml.IndexOf('<');
            int rightIndex;
            while (leftIndex != -1)
            {
                rightIndex = resumeHtml.IndexOf('>');
                resumeHtml = resumeHtml.Remove(leftIndex, rightIndex + 1);
                leftIndex = resumeHtml.IndexOf('<');
                if (leftIndex > 0)
                {
                    resumeContent += $"{resumeHtml.Substring(0, leftIndex)}\n";
                    resumeHtml = resumeHtml.Remove(0, leftIndex);
                    leftIndex = resumeHtml.IndexOf('<');
                }
            }
            resumeContent = $"{url}\n" + resumeContent;
            return resumeContent?.Replace("\n, \n", ", ").Replace("&quot;", "\"").Replace("&amp;", "&"); 
        }

    }
}
