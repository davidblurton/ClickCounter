using System;
using System.Diagnostics;
using System.Windows.Input;

namespace SourceControlForOracle.Utils
{
    /// <summary>
    /// A command whose sole purpose is to 
    /// relay its functionality to other
    /// objects by invoking delegates. The
    /// default return value for the CanExecute
    /// method is 'true'.
    /// </summary>
    internal class RelayCommand<T> : ICommand
    {
        private readonly Action<T> m_Execute;
        private readonly Predicate<T> m_CanExecute;

        public RelayCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            m_Execute = execute;
            m_CanExecute = canExecute;
        }

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return m_CanExecute == null || m_CanExecute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (m_CanExecute != null)
                {
                    CommandManager.RequerySuggested += value;
                }
            }
            remove
            {
                if (m_CanExecute != null)
                {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }

        public void Execute(object parameter)
        {
            m_Execute((T)parameter);
        }
    }

    /// <summary>
    /// A command whose sole purpose is to 
    /// relay its functionality to other
    /// objects by invoking delegates. The
    /// default return value for the CanExecute
    /// method is 'true'.
    /// </summary>
    internal class RelayCommand : ICommand
    {
        #region Fields

        private readonly Action m_Execute;
        private readonly Func<bool> m_CanExecute;

        #endregion // Fields

        #region Constructors

        /// <summary>
        /// Creates a new command that can always execute.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        public RelayCommand(Action execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            m_Execute = execute;
            m_CanExecute = canExecute;
        }

        #endregion // Constructors

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return m_CanExecute == null || m_CanExecute();
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (m_CanExecute != null)
                {
                    CommandManager.RequerySuggested += value;
                }
            }
            remove
            {
                if (m_CanExecute != null)
                {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }

        public void Execute(object parameter)
        {
            m_Execute();
        }
    }
}