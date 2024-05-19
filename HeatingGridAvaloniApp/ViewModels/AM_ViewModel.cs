using Avalonia.Collections;
using ReactiveUI;
using System.Linq;
using HeatingGridAvaloniaApp.Models;
using System.Collections.ObjectModel;


namespace HeatingGridAvaloniApp.ViewModels
{
    public class AM_ViewModel : ViewModelBase
    {
        public ObservableCollection<ProductionUnitViewModel> ProductionUnits {get;}

        public AM_ViewModel()
        {
            ProductionUnits = new ObservableCollection<ProductionUnitViewModel>();

            foreach(var assetManagerUnit in AssetManager.productionUnits)
            {
                ProductionUnitViewModel productionUnitViewModel = new ProductionUnitViewModel(assetManagerUnit);
                ProductionUnits.Add(productionUnitViewModel);
            }
        }
    }

    public class ProductionUnitViewModel : ViewModelBase
    {
        private ProductionUnit _productionUnit;

        public ProductionUnitViewModel(ProductionUnit productionUnit)
        {
            _productionUnit = productionUnit;
        }

        public string Name
        {
            get => _productionUnit.Name;
            set => _productionUnit.Name = value;
        }

        public decimal MaxHeat
        {
            get => _productionUnit.MaxHeat;
            set => _productionUnit.MaxHeat = value;
        }

        public int ProductionCosts
        {
            get => _productionUnit.ProductionCosts;
            set => _productionUnit.ProductionCosts = value;
        }

        public int Co2Emissions
        {
            get => _productionUnit.Co2Emissions;
            set => _productionUnit.Co2Emissions = value;
        }

        public decimal GasConsumption
        {
            get => _productionUnit.GasConsumption;
            set => _productionUnit.GasConsumption = value;
        }

        public decimal MaxElectricity
        {
            get => _productionUnit.MaxElectricity;
            set => _productionUnit.MaxElectricity = value;
        }


        public string ImagePath
        {
            get => "/Assets/"+_productionUnit.Name+".png";
        }
    }
}