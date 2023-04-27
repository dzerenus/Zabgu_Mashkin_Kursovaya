using System;
using System.Windows.Input;

namespace UDPFlood.Flooder.ViewModels;

public class RelayCommand : ICommand
{
    private readonly Action<object> execute;
    private readonly Predicate<object> canExecute;

    public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
    {
        if (execute == null) throw new ArgumentNullException("execute");

        this.execute = execute;
        this.canExecute = canExecute;
    }

    public bool CanExecute(object? param)
    {
        return canExecute == null || canExecute(param ?? "<N/A>");
    }

    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public void Execute(object? param)
    {
        execute(param ?? "<N/A>");
    }
}
