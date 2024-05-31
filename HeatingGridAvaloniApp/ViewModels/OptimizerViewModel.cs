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
        public Optimizer VMOptimizer = new Optimizer();
        public ParameterLoader VMParameterLoader{get;}
        private List<SdmParameters>? filteredSourceData;
        
        // ReactiveCommand to make Optimization accessible from UI
        public ReactiveCommand<Unit, Unit> ReactiveOptimize { get; }
        public ReactiveCommand<Unit, Unit> ReactiveResetCheckBoxes { get; }

        public OptimizerViewModel()
        {
            VMParameterLoader = new ParameterLoader("Assets/SourceData.csv");
            VMParameterLoader.Load();

            // Constructing that ReactiveCommand (basically converting the normal command to it)
            ReactiveOptimize = ReactiveCommand.Create(OptimizeApplyFilters);
            ReactiveResetCheckBoxes = ReactiveCommand.Create(ResetAllCheckBoxes);
            OptimizationSuccessful=false;
        }

        // Optimization settings (entered by buttons in the UI)
        private bool isScenario1Chosen;
        public bool IsScenario1Chosen
        {
            get => isScenario1Chosen;
            set 
            {
                if (value)
                {
                    // Scenario 1 is chosen, so uncheck Scenario 2
                    IsScenario2Chosen = false;
                }
                this.RaiseAndSetIfChanged(ref isScenario1Chosen, value);
            }
        }

        private bool isScenario2Chosen;
        public bool IsScenario2Chosen
        {
            get => isScenario2Chosen;
            set 
            {
                if (value)
                {
                    // Scenario 2 is chosen, so uncheck Scenario 1
                    IsScenario1Chosen = false;
                }
                this.RaiseAndSetIfChanged(ref isScenario2Chosen, value);
            }
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
            set
            {
                this.RaiseAndSetIfChanged(ref netWeight, value);
                AdjustWeights();
                SaveWeights();
            }   
        }

        private decimal co2Weight = 0.1m;
        public decimal Co2Weight
        {
            
            get => co2Weight;
            set
            {
                this.RaiseAndSetIfChanged(ref co2Weight, value);
                AdjustWeights();
                SaveWeights();
            } 
        }

        public void ResetAllCheckBoxes()
        {
            IsSummerChosen = false;
            IsWinterChosen = false;
            IsCostsChosen = false;
            IsCo2Chosen = false;
            IsScenario1Chosen = false;
            IsScenario2Chosen = false;
            ResultDataManager.ResultData.Clear();
            OptimizeApplyFilters();
        }
        
        public void OptimizeApplyFilters()
        {
            // Instances a new Optimizer so that it doesn't collide with previous optimizations
            VMOptimizer = new Optimizer();
            // Saves the weights to it
            SaveWeights();

            ResultDataManager.ResultData.Clear();
            try
            {
                // Declares a list to hold filtered data.
                filteredSourceData = new List<SdmParameters>();
                string chosenSeason = GetSeason();
                // Filters the chosen optimization way
                string chosenOptimizeBy = GetOptimizationOption();

                if (IsScenario1Chosen && IsScenario2Chosen)
                {
                    OptimizationSuccessful = false;
                }
                else if (IsScenario1Chosen && !IsScenario2Chosen)
                {
                    Scenario1Filters(chosenSeason, chosenOptimizeBy);
                }
                else if (IsScenario2Chosen && !IsScenario1Chosen)
                {
                    Scenario2Filters(chosenSeason, chosenOptimizeBy);
                }
                SaveToCSV();
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
            
            if(chosenOptimizeBy != "x" && chosenSeason != "x" && filteredSourceData != null)
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
            if(chosenOptimizeBy != "x" && chosenSeason != "x" && filteredSourceData != null)
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
            if (IsCostsChosen && !IsCo2Chosen)
            {
                optimizationChoice = "1";
            }
            else if (!IsCostsChosen && IsCo2Chosen)
            {
                optimizationChoice = "2";
            }
            else if (IsCostsChosen && IsCo2Chosen)
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
            if(IsWinterChosen && IsSummerChosen)
            {
                chosenSeason="Both";
                foreach(var parameters in VMParameterLoader.SDMParameters)
                {
                    if (filteredSourceData != null)
                    {
                        filteredSourceData.Add(parameters);
                    }
                }
            }
            else if(!IsWinterChosen && IsSummerChosen)
            {
                chosenSeason="Summer";
                foreach(var parameters in VMParameterLoader.SDMParameters)
                {
                    if(parameters.HeatDemand < 4 && filteredSourceData != null)
                        filteredSourceData.Add(parameters);
                }
            }
            else if(IsWinterChosen && !IsSummerChosen)
            {
                chosenSeason="Winter";
                foreach(var parameters in VMParameterLoader.SDMParameters)
                {
                    if(parameters.HeatDemand > 4 && filteredSourceData != null)
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
            VMOptimizer.Co2Weight = Convert.ToDecimal(co2Weight);
            VMOptimizer.NetWeight = netWeight;
        }

        public void UpdateOptimizationChoice3()
        {
            IsOptimizationChoice3 = IsCo2Chosen && IsCostsChosen;
        }

        private void AdjustWeights()
        {
            if (netWeight + co2Weight > 1)
            {
                if (netWeight > co2Weight)
                {
                    Co2Weight = 1 - netWeight;
                }
                else
                {
                    NetWeight = 1 - co2Weight;
                }
            }
        }
    }
}