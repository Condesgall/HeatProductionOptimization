using Avalonia.Metadata;
using HeatingGridAvaloniaApp.Models;
using ReactiveUI;
using System.Reactive;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.ComponentModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Runtime.CompilerServices;

namespace HeatingGridAvaloniApp.ViewModels
{
    public class OptimizerViewModel : ReactiveObject
    {
        
        public Optimizer VMOptimizer{get;}
        public ParameterLoader VMParameterLoader{get;}
        private List<SdmParameters> filteredSourceData;
        
        // ReactiveCommand to make Optimization accessible from UI
        public ReactiveCommand<Unit, Unit> ReactiveOptimize { get; }
        public ReactiveCommand<Unit, Unit> ReactiveOptimizeSc2 { get; }
        public ReactiveCommand<Unit, Unit> ReactiveSaveCSV { get; }
        public ReactiveCommand<Unit, Unit> ReactiveSaveWeights { get; }

        public OptimizerViewModel()
        {
            VMOptimizer = new Optimizer();
            VMParameterLoader = new ParameterLoader("Assets/SourceData.csv");
            VMParameterLoader.Load();

            // Constructing that ReactiveCommand (basically converting the normal command to it)
            ReactiveOptimize = ReactiveCommand.Create(OptimizeApplyFilters);
            ReactiveSaveCSV = ReactiveCommand.Create(SaveToCSV);
            ReactiveSaveWeights = ReactiveCommand.Create(SaveWeights);
            OptimizationSuccessful=false;
        }

        // Optimization settings (entered by buttons in the UI)
        private bool isScenario1Chosen;
        public bool IsScenario1Chosen
        {
            get => isScenario1Chosen;
            set => this.RaiseAndSetIfChanged(ref isScenario1Chosen, value);
        }

        private bool isScenario2Chosen;
        public bool IsScenario2Chosen
        {
            get => isScenario2Chosen;
            set => this.RaiseAndSetIfChanged(ref isScenario2Chosen, value);
        }
        

        private bool _isSummerChosen;
        public bool IsSummerChosen
        {
            get => _isSummerChosen;
            set => this.RaiseAndSetIfChanged(ref _isSummerChosen, value);
        }

        private bool _isWinterChosen;
        public bool IsWinterChosen
        {
            get => _isWinterChosen;
            set => this.RaiseAndSetIfChanged(ref _isWinterChosen, value);
        }

        private bool _isCostsChosen;
        public bool IsCostsChosen
        {
            get => _isCostsChosen;
            set 
            {
                this.RaiseAndSetIfChanged(ref _isCostsChosen, value);
                UpdateOptimizationChoice3();
            }
        }

        private bool _isCo2Chosen;
        public bool IsCo2Chosen
        {
            get => _isCo2Chosen;
            set 
            {
                this.RaiseAndSetIfChanged(ref _isCo2Chosen, value);
                UpdateOptimizationChoice3();
            }
        }

        private bool isOptimizationChoice3;
        public bool IsOptimizationChoice3
        {
            get => isOptimizationChoice3;
            set => this.RaiseAndSetIfChanged(ref isOptimizationChoice3, value);
        }

        private bool _optimizationSuccessful;
        public bool OptimizationSuccessful
        {
            get => _optimizationSuccessful;
            set => this.RaiseAndSetIfChanged(ref _optimizationSuccessful, value);
        }

        private decimal netWeight;
        public decimal NetWeight
        {
            get => netWeight;
            set => this.RaiseAndSetIfChanged(ref netWeight, value);           
        }

        private decimal co2Weight;
        public decimal Co2Weight
        {
            
            get => co2Weight;
            set => this.RaiseAndSetIfChanged(ref co2Weight, value); 
        }

        public void OptimizeApplyFilters()
        {
            try
            {
                // Declares a list to hold filtered data.
                filteredSourceData = new List<SdmParameters>();
                string chosenSeason = GetSeason();
                // Filters the chosen optimization way
                string chosenOptimizeBy = GetOptimizationOption();
                string chosenScenario;

                if (isScenario1Chosen && isScenario2Chosen)
                {
                    OptimizationSuccessful = false;
                }
                else if (isScenario1Chosen && !isScenario2Chosen)
                {
                    Scenario1Filters(chosenSeason, chosenOptimizeBy);
                }
                else if (isScenario2Chosen && !isScenario1Chosen)
                {
                    Scenario2Filters(chosenSeason, chosenOptimizeBy);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                OptimizationSuccessful = false;
            }
        }

        public void Scenario1Filters(string chosenSeason, string chosenOptimizeBy)
        {
            Console.WriteLine($"Chosen Season: {chosenSeason}, Chosen OptimizeBy: {chosenOptimizeBy}");
            
            if(chosenOptimizeBy != "x" && chosenSeason != "x")
            {
                VMOptimizer.OptimizeProduction(filteredSourceData, int.Parse(chosenOptimizeBy));
                Console.WriteLine("Optimization successful.");
                OptimizationSuccessful=true;
            }
            else
            {
                Console.WriteLine("Failed to optimize.");
            }
        }

        public void Scenario2Filters(string chosenSeason, string chosenOptimizeBy)
        {
            if(chosenOptimizeBy != "x" && chosenSeason != "x")
            {
                VMOptimizer.OptimizeResultsSc2(filteredSourceData, int.Parse(chosenOptimizeBy));
                Console.WriteLine("Optimization successful.");
                OptimizationSuccessful=true;
            }
            else
            {
                Console.WriteLine("Failed to optimize.");
            } 
        }

        public string GetOptimizationOption()
        {
            string optimizationChoice;
            if (_isCostsChosen && !_isCo2Chosen)
            {
                optimizationChoice = "1";
            }
            else if (!_isCostsChosen && _isCo2Chosen)
            {
                optimizationChoice = "2";
            }
            else if (_isCostsChosen && _isCo2Chosen)
            {
                optimizationChoice = "3";
            }
            else 
            {
                optimizationChoice = "x";
                OptimizationSuccessful=false;
            }
            return optimizationChoice;
        }

        public string GetSeason()
        {
            string chosenSeason;
            if(_isWinterChosen && _isSummerChosen)
            {
                chosenSeason="Both";
                foreach(var parameters in VMParameterLoader.SDMParameters)
                {
                    filteredSourceData.Add(parameters);
                }
            }
            else if(!_isWinterChosen && _isSummerChosen)
            {
                chosenSeason="Summer";
                foreach(var parameters in VMParameterLoader.SDMParameters)
                {
                    if(parameters.HeatDemand<4)
                        filteredSourceData.Add(parameters);
                }
            }
            else if(_isWinterChosen && !_isSummerChosen)
            {
                chosenSeason="Winter";
                foreach(var parameters in VMParameterLoader.SDMParameters)
                {
                    if(parameters.HeatDemand>4)
                        filteredSourceData.Add(parameters);
                }
            }
            else 
            {
                chosenSeason = "x";
                OptimizationSuccessful=false;
            }
            return chosenSeason;
        }

        public void SaveToCSV()
        {
            ResultDataCSV resultDataCSV = new ResultDataCSV("Assets/ResultData.csv");
            resultDataCSV.Save(ResultDataManager.ResultData);
        }

        public void SaveWeights()
        {
            VMOptimizer.Co2Weight = Co2Weight;
            VMOptimizer.NetWeight = NetWeight;
        }

        public void UpdateOptimizationChoice3()
        {
            IsOptimizationChoice3 = IsCo2Chosen && IsCostsChosen;
        }
    }
}