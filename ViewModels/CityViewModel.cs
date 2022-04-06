using HeadHunter_Parser.Base;
using HeadHunter_Parser.Models;

namespace HeadHunter_Parser.ViewModels
{
    public class CityViewModel : BasePropertyChanged
    {

        private CityRow _cityRow;

        private bool _isSelected;

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public string CityName => _cityRow.CityName;

        public CityViewModel(CityRow city)
        {
            _cityRow = city;
            _isSelected = city.IsChecked;
        }

    }
}
