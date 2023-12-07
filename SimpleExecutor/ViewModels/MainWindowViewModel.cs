using System.Collections.Generic;
using System.Windows.Input;
using Avalonia.Collections;
using ReactiveUI;

namespace SimpleExecutor.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly AvaloniaList<TabBase> _tabs = new();
    private ICommand? _addNewRobotCommand;
    private ICommand? _addNewTabCommand;
    private ICommand? _removeTabCommand;

    public IEnumerable<TabBase> Tabs => _tabs;

    public ICommand AddNewTurtleCommand =>
        _addNewTabCommand ??= ReactiveCommand.Create(() => _tabs.Add(new TurtleTabViewModel()));

    public ICommand AddNewRobotCommand =>
        _addNewRobotCommand ??= ReactiveCommand.Create(() => _tabs.Add(new RobotTabViewModel()));

    public ICommand RemoveTabCommand =>
        _removeTabCommand ??= ReactiveCommand.Create<TabBase>(o =>
        {
            _tabs.Remove(o);
            o.Dispose();
        });

    public TurtleTabViewModel AddTab(string name)
    {
        var tab = new TurtleTabViewModel
        {
            Name = name
        };

        _tabs.Add(tab);

        return tab;
    }
}