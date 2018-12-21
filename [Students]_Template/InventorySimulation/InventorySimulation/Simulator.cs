using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InventoryModels;

/// <summary>
/// A completely useless class
/// </summary>
static class Simulator
{
    /// <summary>
    /// Guess what??? A Random Generator!
    /// </summary>
    static Random rnd = new Random((int)DateTime.Now.Ticks);
    /// <summary>
    /// This is the fourth time i have used this function, you should know what it does by now..
    /// </summary>
    static private int CalculateRandomValue(List<Distribution> Distribution, int RandomVariable)
    {
        for (int i = 0; i < Distribution.Count; i++)
        {
            if (!Distribution[i].IsCalculated)
            {
                if (i == 0)
                {
                    Distribution[i].CummProbability = Distribution[i].Probability;
                    Distribution[i].MinRange = 1;
                }
                else
                {
                    Distribution[i].CummProbability = Distribution[i].Probability + Distribution[i - 1].CummProbability;
                    Distribution[i].MinRange = Distribution[i - 1].MaxRange + 1;
                }
                Distribution[i].MaxRange = (int)(Distribution[i].CummProbability * 100);
                Distribution[i].IsCalculated = true;
            }
            if (RandomVariable <= Distribution[i].MaxRange && RandomVariable >= Distribution[i].MinRange)
            {
                return Distribution[i].Value;
            }
        }
        if (RandomVariable < 1 || RandomVariable > 100)
            throw new ArgumentOutOfRangeException("RandomValue should be between 1 and 100");
        else
            throw new Exception("Debug meeeeeeeee");
    }
    /// <summary>
    /// Clean code i guess...
    /// </summary>
    static private SimulationCase GenerateCase(SimulationSystem system, Inventory inventory, int Day)
    {
        int R1 = rnd.Next(1, 100);
        int V1 = CalculateRandomValue(system.DemandDistribution, R1);
        if(inventory.CurrentOrder != null && inventory.CurrentOrder.DueDate == Day)
        {
            inventory.Quantity += inventory.CurrentOrder.Quantity;
            inventory.CurrentOrder = null;
        }
        SimulationCase Case = new SimulationCase()
        {
            Day = Day + 1,
            Cycle = (Day / system.ReviewPeriod) + 1,
            BeginningInventory = Math.Max(0, inventory.Quantity),
            DayWithinCycle = (Day % system.ReviewPeriod) + 1,
            RandomDemand = R1,
            Demand = V1,
            EndingInventory = Math.Max(0, inventory.Quantity - V1),
            ShortageQuantity = Math.Max(0, V1 - inventory.Quantity)
        };
        inventory.Quantity -= V1;
        return Case;
    }
    /// <summary>
    /// That seemed important
    /// </summary>
    static private void CheckFullCycle(SimulationSystem system, SimulationCase Case, Inventory inventory)
    {
        if(Case.DayWithinCycle == system.ReviewPeriod)
        {
            Case.OrderQuantity = Math.Max(0, system.OrderUpTo - inventory.Quantity);
            Case.RandomLeadDays = rnd.Next(1, 100);
            Case.LeadDays = CalculateRandomValue(system.LeadDaysDistribution, Case.RandomLeadDays);
            inventory.CurrentOrder = new Order()
            {
                Quantity = Case.OrderQuantity,
                DueDate = Case.LeadDays + Case.Day
            };
        }
    }
    /// <summary>
    /// A very discriptive name
    /// </summary>
    static public void StartSimulation(SimulationSystem system)
    {
        Inventory inventory = new Inventory()
        {
            Quantity = system.StartInventoryQuantity,
            CurrentOrder = new Order()
            {
                Quantity = system.StartOrderQuantity,
                DueDate = system.StartLeadDays
            }
        };
        for(int i = 0; i < system.NumberOfDays; i++)
        {
            SimulationCase Case = GenerateCase(system, inventory, i);
            CheckFullCycle(system, Case, inventory);
            system.PerformanceMeasures.EndingInventoryAverage += Case.EndingInventory;
            system.PerformanceMeasures.ShortageQuantityAverage += Case.ShortageQuantity;
        }
        system.PerformanceMeasures.EndingInventoryAverage /= system.NumberOfDays;
        system.PerformanceMeasures.ShortageQuantityAverage /= system.NumberOfDays;
    }
}
