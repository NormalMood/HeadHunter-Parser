

namespace HeadHunter_Parser.Models
{
    public class TechnologyRow
    {

        public bool IsChecked { get; set; }

        public string TechnologyName { get; private set; }

        public TechnologyRow(string techName)
        {
            TechnologyName = techName;
        }

    }
}
