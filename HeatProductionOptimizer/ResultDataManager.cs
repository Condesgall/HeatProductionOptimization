namespace ResultDataManager_
{
    public class OptimizationResults
    {
        private double producedHeat;
        private double producedElectricity;
        private double consumedElectricity;
        private double expenses;
        private double profit;
        private double primaryEnergyConsumption;
        private double co2Emissions;

        //constructor
        public OptimizationResults(double producedHeat_, double producedElectricity_, double consumedElectricity_, double expenses_, double profit_, double primaryEnergyConsumption_, double co2Emissions_) 
        {
            producedHeat = producedHeat_;
            producedElectricity = producedElectricity_;
            consumedElectricity = consumedElectricity_;
            expenses = expenses_;
            profit = profit_;
            primaryEnergyConsumption = primaryEnergyConsumption_;
            co2Emissions = co2Emissions_;
        }

        // properties
        public double ProducedHeat
        {
            get { return producedHeat; }
            set { producedHeat = value; }
        }

        public double ProducedElectricity
        {
            get { return producedElectricity; }
            set { producedElectricity = value; }
        }

        public double ConsumedElectricity
        {
            get { return consumedElectricity; }
            set { consumedElectricity = value; }
        }

        public double Expenses
        {
            get { return expenses; }
            set { expenses = value; }
        }

        public double Profit
        {
            get { return profit; }
            set { producedHeat = value; }
        }

        public double PrimaryEnergyConsumption
        {
            get { return primaryEnergyConsumption; }
            set { producedHeat = value; }
        }

        public double Co2Emissions
        {
            get { return co2Emissions; }
            set { co2Emissions = value; }
        }
    }

    public class ResultDataManager()
    {
        // result data gets stored in this dictionary (allows to separate it by production unit).
        public Dictionary<string, OptimizationResults> resultData = new Dictionary<string, OptimizationResults>();

        // adds result data.
        public void AddResultData(string productionUnitName, OptimizationResults result)
        {
            //adds result data (separated by production unit) to dictionary
            resultData.Add(productionUnitName, result);
        }
    }
}