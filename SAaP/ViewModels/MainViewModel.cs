using CommunityToolkit.Mvvm.ComponentModel;
using SAaP.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAaP.ViewModels
{
    public class MainViewModel : ObservableRecipient
    {
        public ObservableCollection<AnalysisResult> Source { get; } = new ObservableCollection<AnalysisResult>();

    }
}
