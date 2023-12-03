using System;
using System.IO;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using SimpleExecutor.ViewModels;

namespace SimpleExecutor.Views;

public partial class MainView : UserControl
{
    private static readonly FilePickerFileType CodeFileType = new("Code")
    {
        Patterns = new[] {"*.llang"},
        MimeTypes = new[] {"text/*"}
    };
    
    public MainView()
    {
        InitializeComponent();
    }

    public async Task Save()
    {
        if (TabControl.SelectedItem is not ExecutorTabViewModel tabViewModel)
            return;
        
        var file = await TopLevel.GetTopLevel(this)!.StorageProvider.SaveFilePickerAsync(new()
        {
            SuggestedFileName = tabViewModel.Name,
            DefaultExtension = ".llang",
            FileTypeChoices = new[] {CodeFileType}
        });
        
        if (file is null)
            return;

        await using var stream = await file.OpenWriteAsync();

        var bytes = Encoding.Default.GetBytes(tabViewModel.Code);

        await stream.WriteAsync(bytes);

        tabViewModel.Name = file.Name;
    }

    public async Task OpenScript()
    {
        if (DataContext is not MainWindowViewModel viewModel)
            return;
        
        var files = await TopLevel.GetTopLevel(this)!.StorageProvider.OpenFilePickerAsync(new()
        {
            AllowMultiple = true,
            FileTypeFilter = new[] {CodeFileType}
        });

        foreach (var file in files)
        {
            var tab = viewModel.AddTab(file.Name);
            
            await using var stream = await file.OpenReadAsync();

            using var reader = new StreamReader(stream);

            tab.Code = await reader.ReadToEndAsync();
        }
    }
}