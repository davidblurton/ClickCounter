using System.Collections.Generic;
using System.ComponentModel;

namespace ClickCounter.Observables
{
    public class ObservableValue<T> : INotifyPropertyChanged
    {
        private T m_ValueField;

        public T Value 
        {
            get { return m_ValueField; }
            set
            {
                if (!EqualityComparer<T>.Default.Equals(m_ValueField, value))
                {
                    m_ValueField = value;
                    OnPropertyChanged("Value");
                }
            }
        }

        public ObservableValue(T value = default(T))
        {
            Value = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}