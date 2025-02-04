using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GeneticAgent : MonoBehaviour
{
    [SerializeField]
    private List<Gene> genes;
    private Wolf wolf;

    [field: SerializeField]
    public int Fitness { get ; private set; } = 100;     //get with body in SerializeField variable: https://developercommunity.visualstudio.com/t/CS8652-The-feature-field-keyword-is-cu/10802850?space=62&sort=newest
    private void Awake()
    {
        wolf = GetComponent<Wolf>();
    }
    public void EvaluateFitness()
    {
        Fitness = wolf.GetFoodNum();
    }
    public void RandomizeGenes()
    {
        foreach(Gene gene in genes)
        {
            gene.CurrentValue = UnityEngine.Random.Range(gene.getMin(), gene.getMax() + 1);
        }
    }
    public void InheritGenes(GeneticAgent parent1, GeneticAgent parent2)
    {
        genes = new List<Gene>();
        int halfSize = parent1.genes.Count / 2;
        for(int i = 0; i < halfSize; i++)
        {
            genes.Add(parent1.genes[i]);
        }
        for(int i = halfSize; i < parent2.genes.Count; i++)
        {
            genes.Add(parent2.genes[i]);
        }
    }
    public void MutateGenes()
    {
        System.Random rand = new();
        int maxNumRandomGenes = 3;
        List<int> indexesArray = Enumerable.Range(0, genes.Count).ToList();
        int numRandomGenes = UnityEngine.Random.Range(0, maxNumRandomGenes);
        for(int i = 0; i < numRandomGenes; i++)
        {
            int randomIndex = rand.Next(0, indexesArray.Count);
            int geneIndex = indexesArray[randomIndex];
            int currentValue = genes[geneIndex].CurrentValue;
            double tenPercent = genes[geneIndex].getMax() * 0.1;
            int randomChange = rand.Next((int)(-tenPercent), (int)(tenPercent));
            currentValue = Math.Max(genes[geneIndex].getMin(), Math.Min(currentValue + randomChange, genes[geneIndex].getMax()));
            genes[geneIndex].CurrentValue = currentValue;
            indexesArray.RemoveAt(randomIndex);
        }
    }
    public int GetMovementRangeValue()
    {
        System.Random random = new();
        Gene movementRangeGene = GetGene("Movement Range");
        return random.Next(movementRangeGene.Min, movementRangeGene.CurrentValue + 1);    
    }
    public int GetViewRangeValue()
    {
        return GetGene("View Range").CurrentValue;
    }

    public int GetEnergyEfficiencyValue()
    {
        return GetGene("Energy Efficiency").CurrentValue;
    }
    public Gene GetGene(string name)
    {
        return genes.Find(gene => gene.Name == name);
    }
    public bool IsWolfFitnessPositive()
    {
        return Fitness > 0;
    }
}