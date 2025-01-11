using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneticAgent : MonoBehaviour
{
    [SerializeField]
    private List<Gene> genes;
    [SerializeField]
    private int numGenes = 5;
    [SerializeField]
    private GameObject wolfPrefab;

    [field: SerializeField]
    public float Fitness { get; private set; } = 0;
    private void Start()
    {
        Color randomColor = new Color(Random.value, Random.value, Random.value);
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
            gene.CurrentValue = Random.Range(gene.getMin(), gene.getMax() + 1);
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
    public void MutateGenes(float mutationChance)
    {
        System.Random rand = new System.Random();
        foreach(Gene gene in genes)
        {
            if(Random.Range(0f, 1f) < mutationChance)
            {
                gene.CurrentValue = rand.Next(gene.getMin(), gene.getMax());
            }
        }
    }
}
