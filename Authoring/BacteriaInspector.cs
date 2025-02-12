using UnityEngine;
using Unity.Entities;
using LakeBacteria.Components;
using UnityEngine.UI;
public class BacteriaInspector : MonoBehaviour
{

    // UI Elements for displaying gene values
    public Text shapeText;
    public Text speedText;
    public Text metabolicEfficiencyText;
    public Text sturdinessText;
    public Text sensorRadiusText;
    public Text radiationResistanceText;
    public Text reproductiveCostText;
    public Text aggressionBiasText;
    public Text clusterPreferenceText;
    public Text mutationRateText;
    public Text healthText;
    public Text energyText;
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Check if the clicked object has an EntityLinkWrapper
                EntityLinkWrapper wrapper = hit.collider.gameObject.GetComponent<EntityLinkWrapper>();
                if (wrapper != null)
                {
                    Entity entity = wrapper.Entity;
                    var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

                    // Check if the entity has the required components
                    if (entityManager.HasComponent<BacteriaData>(entity) &&
                        entityManager.HasComponent<Health>(entity) &&
                        entityManager.HasComponent<Energy>(entity))
                    {
                        // Retrieve the data
                        BacteriaData bacteriaData = entityManager.GetComponentData<BacteriaData>(entity);
                        Health health = entityManager.GetComponentData<Health>(entity);
                        Energy energy = entityManager.GetComponentData<Energy>(entity);

                        // Log the data (or display it in the UI)
                        Debug.Log($"Selected Bacterium: Shape={bacteriaData.shapeType}, " +
                                  $"Speed={bacteriaData.Speed}, MetabolicEfficiency={bacteriaData.MetabolicEfficiency}, " +
                                  $"Health={health.Value}, Energy={energy.Value}");

                        // TODO: Update the UI with this data
                        UpdateUIWithBacteriaData(bacteriaData, health, energy);
                    }
                }
            }
        }
    }

    private void UpdateUIWithBacteriaData(BacteriaData bacteriaData, Health health, Energy energy)
    {
        // Display Shape Type
        shapeText.text = bacteriaData.shapeType.ToString();

        // Display Gene Values
        speedText.text = bacteriaData.Speed.ToString("F2");
        metabolicEfficiencyText.text = bacteriaData.MetabolicEfficiency.ToString("F2");
        sturdinessText.text = bacteriaData.Sturdiness.ToString("F2");
        sensorRadiusText.text = bacteriaData.SensorRadius.ToString("F2");
        radiationResistanceText.text = bacteriaData.RadiationResistance.ToString("F2");
        reproductiveCostText.text = bacteriaData.ReproductiveCost.ToString("F2");
        aggressionBiasText.text = bacteriaData.AggressionBias.ToString("F2");
        clusterPreferenceText.text = bacteriaData.ClusterPreference.ToString("F2");
        mutationRateText.text = bacteriaData.MutationRate.ToString("F2");

        // Display Health and Energy
        healthText.text = health.Value.ToString("F2");
        energyText.text = energy.Value.ToString("F2");

        // Optional: Highlight critical values (e.g., low health or high aggression)
        if (health.Value < 20) healthText.color = Color.red;
        else healthText.color = Color.green;

        if (bacteriaData.AggressionBias > 0.8f) aggressionBiasText.color = Color.red;
        else aggressionBiasText.color = Color.white;
    }

}