using HeadHunter_Parser.Base;
using HeadHunter_Parser.Models;

namespace HeadHunter_Parser.ViewModels
{
    public class TechnologyViewModel : BasePropertyChanged
    {

        private TechnologyRow _technologyRow;

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

        public string TechnologyName => _technologyRow.TechnologyName;

        public TechnologyViewModel(TechnologyRow technology)
        {
            _technologyRow = technology;
            _isSelected = technology.IsChecked;
        }

    }
}
