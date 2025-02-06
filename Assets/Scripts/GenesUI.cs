using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GenesUI : MonoBehaviour
{
    [SerializeField]
    private GameObject genesPanelPrefab;
    [SerializeField]
    private Canvas uiCanvas;
    [SerializeField]
    private GameObject wolfPrefab;
    private Dictionary<Wolf, GameObject> wolfPanels = new Dictionary<Wolf, GameObject>();

    private void Start()
    {
        InvokeRepeating(nameof(UpdateWolvesList), 0f, 1f);
    }

    private void Update()
    {
        UpdatePanelPositions();
    }
    private void UpdateWolvesList()
    {
        Wolf[] wolves = FindObjectsOfType<Wolf>();
        foreach(Wolf wolf in wolves)
        {
            if(!wolfPanels.ContainsKey(wolf) && wolf != wolfPrefab.GetComponent<Wolf>())
            {
                GameObject panelInstance = Instantiate(genesPanelPrefab, uiCanvas.transform);
                genesPanelPrefab.SetActive(false);
                panelInstance.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                panelInstance.SetActive(true);
                TextMeshProUGUI panelTxt = panelInstance.GetComponentInChildren<TextMeshProUGUI>();
                panelTxt.fontSize = 12;
                panelTxt.transform.localScale = new Vector3(12f, 12f, 12f);
                wolfPanels[wolf] = panelInstance;
            }
        }
        List<Wolf> toRemove = new List<Wolf>();
        foreach (var entry in wolfPanels)
        {
            if (entry.Key == null) 
            {
                Destroy(entry.Value); 
                toRemove.Add(entry.Key);
            }
        }
        foreach (Wolf deadWolf in toRemove)
        {
            wolfPanels.Remove(deadWolf);
        }
    }
    private void UpdatePanelPositions()
    {
        foreach(KeyValuePair<Wolf, GameObject> entry in wolfPanels)
        {
            Wolf wolf = entry.Key;
            GameObject panel = entry.Value;
            if (wolf == null || panel == null || wolf == wolfPrefab.GetComponent<Wolf>()) continue;
            TextMeshProUGUI panelTxt = panel.GetComponentInChildren<TextMeshProUGUI>();
            if (panelTxt != null)
            {
                panelTxt.text = GetWolfGenesInfo(wolf);
            }
            Vector3 worldPosition = wolf.transform.position + new Vector3(0, 3f, 0);
            panel.transform.position = worldPosition;
        }
    }
    private string GetWolfFitness(Wolf wolf)
    {
        return (wolf.GetFoodNum()).ToString();
    }
    private static string ToHexString(Color color)
    {
        int r = Mathf.RoundToInt(color.r * 255f);
        int g = Mathf.RoundToInt(color.g * 255f);
        int b = Mathf.RoundToInt(color.b * 255f);

        return $"#{r:X2}{g:X2}{b:X2}";
    }
    private string GetWolfColor(Wolf wolf)
    {
        string wolfColor = ToHexString(wolf.GetWolfColor());
        return wolfColor;
    }
    private string GetWolfGenesInfo(Wolf wolf)
    {
        GeneticAgent geneticAgent = wolf.GetComponent<GeneticAgent>();
        if (geneticAgent == null) return "Brak danych";

        string genesInfo = "";
        foreach (Gene gene in geneticAgent.GetGenes())
        {
            genesInfo += $"{gene.Name}: {gene.CurrentValue}\n";
        }
        genesInfo += "Fitness: " + GetWolfFitness(wolf);
        genesInfo += "Color: " + GetWolfColor(wolf);
        return genesInfo;
    }
}
