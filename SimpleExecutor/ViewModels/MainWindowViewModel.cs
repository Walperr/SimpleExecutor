using System.Collections.Generic;
using System.Reactive;
using System.Windows.Input;
using Avalonia.Collections;
using Avalonia.Controls;
using AvaloniaEdit;
using ReactiveUI;

namespace SimpleExecutor.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly AvaloniaList<ExecutorTabViewModel> _tabs = new();
    private ICommand? _addNewTabCommand;
    private ICommand? _removeTabCommand;

    public IEnumerable<ExecutorTabViewModel> Tabs => _tabs;

    public ExecutorTabViewModel AddTab(string name)
    {
        var tab = new ExecutorTabViewModel
        {
            Name = name
        };

        _tabs.Add(tab);

        return tab;
    }

    public ICommand AddNewTabCommand =>
        _addNewTabCommand ??= ReactiveCommand.Create(() => _tabs.Add(new ExecutorTabViewModel()));

    public ICommand RemoveTabCommand =>
        _removeTabCommand ??= ReactiveCommand.Create<ExecutorTabViewModel>(o =>
        {
            _tabs.Remove(o);
            o.Dispose();
        });
}