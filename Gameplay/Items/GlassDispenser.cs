using UnityEngine;

public class GlassDispenser : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Le prefab de verre qu'on a créé")]
    public GameObject glassPrefab;

    /// <summary>
    /// Crée un verre et le renvoie
    /// </summary>
    public GameObject GetNewGlass()
    {
        if (glassPrefab == null)
        {
            Debug.LogError("❌ glassPrefab est null dans GlassDispenser !");
            return null;
        }

        // On crée une copie du prefab
        GameObject newGlassObj = Instantiate(glassPrefab);
        
        if (newGlassObj == null)
        {
            Debug.LogError("❌ Échec de l'instantiation du verre !");
            return null;
        }

        // On s'assure qu'il est bien activé
        newGlassObj.SetActive(true);

        Debug.Log("✨ Nouveau verre créé");
        return newGlassObj;
    }
}