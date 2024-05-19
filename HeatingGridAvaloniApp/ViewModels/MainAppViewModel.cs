using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using HeatingGridAvaloniaApp.Models;

namespace HeatingGridAvaloniApp.ViewModels
{
    public class MainAppViewModel : ViewModelBase
    {
        public ObservableCollection<SdmParameters> SourceData { get; }
        public MainAppViewModel()
        {
            SourceData = new ObservableCollection<SdmParameters>(GenerateMockSourceDataTable());
        }

        private IEnumerable<SdmParameters> GenerateMockSourceDataTable()
        {
            var defaultSourceData = new List<SdmParameters>()
            {
                new SdmParameters()
                {
                    TimeFrom = "summer",
                    TimeTo = "winter",
                    HeatDemand = 10,
                    ElPrice = 20
                },
                new SdmParameters()
                {
                    TimeFrom = "winter",
                    TimeTo = "summer",
                    HeatDemand = 15,
                    ElPrice = 25
                }

            };

            return defaultSourceData;
        }
    }
}