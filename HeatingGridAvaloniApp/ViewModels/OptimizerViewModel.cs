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

namespace HeatingGridAvaloniApp.ViewModels
{
    public class OptimizerViewModel : ReactiveObject
    {
        public Optimizer VMOptimizer{get;}
        public ParameterLoader VMParameterLoader{get;}
        private List<SdmParameters> filteredSourceData;
        
        // ReactiveCommand to make Optimization accessible from UI
        public ReactiveCommand<Unit, Unit> ReactiveOptimize { get; }
        public OptimizerViewModel()
        {
            VMOptimizer = new Optimizer();
            VMParameterLoader = new ParameterLoader("Assets/SourceData.csv");
            VMParameterLoader.Load();

            // Constructing that ReactiveCommand (basically converting the normal command to it)
            ReactiveOptimize = ReactiveCommand.Create(OptimizeApplyFilters);
        }

        // Optimization settings (entered by buttons in the UI)
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
            set => this.RaiseAndSetIfChanged(ref _isCostsChosen, value);
        }

        private bool _isCo2Chosen;
        public bool IsCo2Chosen
        {
            get => _isCo2Chosen;
            set => this.RaiseAndSetIfChanged(ref _isCo2Chosen, value);
        }


        
        public void OptimizeApplyFilters()
        {
            // Declares a list to hold filtered data.
            filteredSourceData = new List<SdmParameters>();
            
            // Filters the chosen season.
            string chosenSeason="x";
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
            else chosenSeason = "x";

            // Filters the chosen optimization way
            string chosenOptimizeBy = "x";
            if(_isCostsChosen && !_isCo2Chosen)
            {
                chosenOptimizeBy = "1";
            }
            else if(!_isCostsChosen && _isCo2Chosen)
            {
                chosenOptimizeBy = "2";
            }
            else if(_isCostsChosen && _isCo2Chosen)
            {
                chosenOptimizeBy = "3";
            }
            else chosenOptimizeBy = "x";

            Console.WriteLine($"Chosen Season: {chosenSeason}, Chosen OptimizeBy: {chosenOptimizeBy}");
            
            //Finally, the optimization.)
            if(chosenOptimizeBy != "x" && chosenSeason != "x")
            {
                VMOptimizer.OptimizeProduction(filteredSourceData, int.Parse(chosenOptimizeBy));
                System.Console.WriteLine("Optimizing happens.");
            }
            else
            {
                System.Console.WriteLine("Optimizing don't happen.");
            }
        }
    }
}