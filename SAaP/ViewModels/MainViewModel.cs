using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SAaP.Core.Helpers;
using SAaP.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.System;
using SAaP.Core.Services;

namespace SAaP.ViewModels
{
    public class MainViewModel : ObservableRecipient
    {
        private string _codeInput;

        public ObservableCollection<AnalysisResult> AnalyzedResults { get; } = new();

        public ICommand AnalysisPressedCommand { get; }

        public string CodeInput
        {
            get => _codeInput;
            set => SetProperty(ref _codeInput, value);
        }

        public MainViewModel()
        {
            AnalysisPressedCommand = new RelayCommand(OnAnalysisPressed);
        }

        private async void OnAnalysisPressed()
        {
            // format input
            var codes = StringHelper.FormattingWithComma(CodeInput);

            // check code accuracy
            var accuracyCodes = StockService.CheckStockCodeAccuracy(codes);
            // add comma
            var pyArg = StockService.FormatPyArgument(accuracyCodes);

            // formatted code resetting
            CodeInput = pyArg;

            // python script execution
            await PythonService.RunPythonScript(PythonService.TdxReader, "C:/devEnv/Tools/TDX", LocalService.PyDataPath, pyArg);

            // TODO remove this after release
            await Launcher.LaunchFolderAsync(await StorageFolder.GetFolderFromPathAsync(LocalService.PyDataPath));

            // TODO read pydata

            // TODO write to sqlite database

            // TODO analyze data

            // TODO show to UI data gram
        }
    }
}