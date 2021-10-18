using UnityEngine;
using UnityEditor;
using System.Collections;

public class ExampleClass : MonoBehaviour
{

/*
    [MenuItem("Example/Example2", false, 100)]
    public static void Example2()
    {
        print("Example/Example2");
    }
*/
    [MenuItem("GameObject/Create Other/LarCamera")]
    static void CreateLarCamera()
    {
        Object prefab = AssetDatabase.LoadAssetAtPath("Assets/LAR/Prefabs/LARCamera.prefab", typeof(GameObject));
        GameObject clone = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
        clone.gameObject.name = "LarCamera";
        // Modify the clone to your heart's content
        //clone.transform.position = Vector3.one;
    }
    [MenuItem("GameObject/Create Other/LarTarget")]
    static void CreateLarTarget()
    {
        Object prefab = AssetDatabase.LoadAssetAtPath("Assets/LAR/Prefabs/LarTarget.prefab", typeof(GameObject));
        GameObject clone = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
        clone.gameObject.name = "LarTarget";
    }
    [MenuItem("GameObject/Create Other/LarWorldSpaceLabel")]
    static void CreateLarLabel()
    {
        Object prefab = AssetDatabase.LoadAssetAtPath("Assets/LAR/Prefabs/LarWorldSpaceLabel.prefab", typeof(GameObject));
        GameObject clone = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
        clone.gameObject.name = "LarWorldSpaceLabel";
    }
}


