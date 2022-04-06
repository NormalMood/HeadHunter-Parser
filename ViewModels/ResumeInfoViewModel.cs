

namespace HeadHunter_Parser.ViewModels
{
    public class ResumeInfoViewModel : Base.BasePropertyChanged
    {

        private string _parsedText;

        public string ParsedText
        {
            get
            {
                return _parsedText;
            }
            set
            {
                _parsedText = value;
                OnPropertyChanged();
            }
        }

    }
}
