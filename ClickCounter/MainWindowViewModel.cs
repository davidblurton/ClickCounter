using System.Windows.Input;
using ClickCounter.Observables;
using SourceControlForOracle.Observables;
using SourceControlForOracle.Utils;

namespace ClickCounter
{
    public class MainWindowViewModel
    {
        public MainWindowViewModel()
        {
            NumberOfClicks = new ObservableValue<int>(0);
            HasClickedTooManyTimes = new ComputedValue<bool>(() => NumberOfClicks.Value >= 3);
        }

        public ObservableValue<int> NumberOfClicks { get; private set; }
        public ComputedValue<bool> HasClickedTooManyTimes { get; private set; }

        public ICommand RegisterClickCommand
        {
            get { return new RelayCommand(RegisterClick, () => !HasClickedTooManyTimes.Value); }
        }

        public ICommand ResetClicks
        {
            get { return new RelayCommand( () => NumberOfClicks.Value = 0); }
        }

        private void RegisterClick()
        {
            NumberOfClicks.Value++;
        }
    }
}