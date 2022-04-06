

namespace HeadHunter_Parser.Models
{
    public class CityRow
    {

        public string ID { get; private set; }

        public bool IsChecked { get; set; }
        
        public string CityName { get; private set; }

        public CityRow(string id, string cityName)
        {
            ID = id;
            CityName = cityName;
        }
        
    }
}
