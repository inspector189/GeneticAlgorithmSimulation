using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GeneticAgent : MonoBehaviour
{
    [SerializeField]
    private List<Gene> genes;
    [SerializeField]
    private GameObject wolfPrefab;

    [field: SerializeField]
    public float Fitness { get; private set; } = 0;
    private void Start()
    {
        Color randomColor = new(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
        Renderer wolfRenderer = this.GetComponent<Renderer>();
        wolfRenderer.material.color = randomColor;
    }
    public void EvaluateFitness()
    {
        Renderer wolfRenderer = this.GetComponent<Renderer>();
        Fitness = wolfRenderer.material.color.r + wolfRenderer.material.color.g + wolfRenderer.material.color.b;
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
        foreach(Gene gene in genes)
        {
            if(gene.Name == "Movement Range")
            {
                return random.Next(gene.Min, gene.CurrentValue + 1);
            }
        }
        return 1;
    }
}