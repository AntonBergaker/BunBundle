using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace BunBundle {
    public class RelayCommand : ICommand {
        private readonly Predicate<object?>? canExecute;
        private readonly Action<object?> execute;

        public RelayCommand(Predicate<object?>? canExecute, Action<object?> execute) {
            this.canExecute = canExecute;
            this.execute = execute;
        }

        public event EventHandler? CanExecuteChanged {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter) {
            return this.canExecute?.Invoke(parameter) ?? true;
        }

        public void Execute(object? parameter) {
            this.execute(parameter);
        }
    }
}
