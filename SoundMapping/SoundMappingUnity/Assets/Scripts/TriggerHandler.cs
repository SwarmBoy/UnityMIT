using UnityEngine;
using UnityEngine.Events; // For UnityEvent
using UnityEditor;
using System; // For PropertyDrawer

public class TriggerHandlerWithCallback : MonoBehaviour
{
    [TagSelector] // Custom attribute for tag selection
    public string targetTag; // Tag to filter the objects

    public bool allCollectiblesCollected
    {
        get
        {
            try
            {
                return GameObject.FindGameObjectsWithTag("Collectibles").Length == 0;
            }
            catch (Exception e)
            {
                return true;
            }
            //return GameObject.FindGameObjectsWithTag("Collectibles").Length == 0;
        }
    }

    [SerializeField] bool useUnityEvent = true;
    [SerializeField] bool isStart = true;

    public UnityEvent onTriggerEnter; // Callback to assign in the Inspector

    private static GameObject gm;


    public static void setGM(GameObject gameManager)
    {
        gm = gameManager;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            if (useUnityEvent)
            {
                if (isStart)
                {
                    print(gm.name);
                    gm.GetComponent<Timer>().StartTimer();
                }
                else
                {
                    if (allCollectiblesCollected)
                    {
                        ExperimentSetupS.levelFinished();
                        gm.GetComponent<Timer>().StopTimer();
                    }
                }
            }
            else
            {
                onTriggerEnter?.Invoke(); // Call the assigned callback
            }
            
        }
    }
}

public class TagSelectorAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(TagSelectorAttribute))]
public class TagSelectorPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.String)
        {
            EditorGUI.BeginProperty(position, label, property);
            property.stringValue = EditorGUI.TagField(position, label, property.stringValue);
            EditorGUI.EndProperty();
        }
        else
        {
            EditorGUI.PropertyField(position, property, label);
            Debug.LogWarning("TagSelector can only be used with string properties.");
        }
    }
}
#endif