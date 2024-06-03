// File: HeatingGridAvaloniApp/ViewModels/AM_ViewModel.cs

using Avalonia.Collections;
using ReactiveUI;
using System.Linq;
using HeatingGridAvaloniaApp.Models;
using System.Collections.ObjectModel;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Interactivity;

namespace HeatingGridAvaloniApp.ViewModels
{
    public class AM_ViewModel : ViewModelBase
    {
        public ObservableCollection<ProductionUnitViewModel> ProductionUnits { get; }

        public ReactiveCommand<Unit, Unit> SaveCommand { get; }

        public AM_ViewModel()
        {
            ProductionUnits = new ObservableCollection<ProductionUnitViewModel>();

            foreach (var assetManagerUnit in AssetManager.productionUnits)
            {
                ProductionUnitViewModel productionUnitViewModel = new ProductionUnitViewModel(assetManagerUnit);
                ProductionUnits.Add(productionUnitViewModel);
            }

            SaveCommand = ReactiveCommand.Create(SaveData);
        }

        private void SaveData()
        {
            var assetManagerStorage = new AssetManagerStorage();
            assetManagerStorage.SaveAMData();
        }
    }

    public class ProductionUnitViewModel : ViewModelBase
    {
        private ProductionUnit _productionUnit;
        private decimal _maxHeat;

        public ProductionUnitViewModel(ProductionUnit productionUnit)
        {
            _productionUnit = productionUnit;
            _maxHeat = productionUnit.MaxHeat;
        }

        public string Name
        {
            get => _productionUnit.Name;
            set
            {
                _productionUnit.Name = value;
                this.RaisePropertyChanged();
            }
        }

        public decimal MaxHeat
        {
            get => _maxHeat;
            set
            {
                this.RaiseAndSetIfChanged(ref _maxHeat, value);
                _productionUnit.MaxHeat = value;
            }
        }

        public decimal ProductionCosts
        {
            get => _productionUnit.ProductionCosts;
            set
            {
                _productionUnit.ProductionCosts = value;
                this.RaisePropertyChanged();
            }
        }

        public decimal Co2Emissions
        {
            get => _productionUnit.Co2Emissions;
            set
            {
                _productionUnit.Co2Emissions = value;
                this.RaisePropertyChanged();
            }
        }

        public decimal GasConsumption
        {
            get => _productionUnit.GasConsumption;
            set
            {
                _productionUnit.GasConsumption = value;
                this.RaisePropertyChanged();
            }
        }

        public decimal MaxElectricity
        {
            get => _productionUnit.MaxElectricity;
            set
            {
                _productionUnit.MaxElectricity = value;
                this.RaisePropertyChanged();
            }
        }

        public string ImagePath
        {
            get => "/Assets/" + _productionUnit.Name + ".png";
        }
    }
}
