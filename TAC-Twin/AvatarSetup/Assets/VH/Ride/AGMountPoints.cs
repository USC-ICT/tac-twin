/* This script parents the child object based on its mount point to the parent mount point object.
 *
 * Joe Yip
 * 2012-May-05
 * yip@ict.usc.edu
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AGMountPoints : MonoBehaviour {

    [Tooltip("Asset in project that will be instantiated.")]
    public Object childObject;
    [Tooltip("Name of object/point in the character that item will be attached to.")]
    public string parentMountPointName = "MountPointParent";
    [Tooltip("Name of object/point in the item that mounting will be based off of.")]
    public string childMountPointName = "MountPointChild";
    private GameObject parentMountPoint;
    private GameObject childMountPoint;
    private GameObject itemGameObject;
    [Tooltip("Whether or not the mounted item is active.")]
    public bool childObjectActive = true;
    List<Transform> itemGameObjectTransforms = new List<Transform>();

    void Start(){
        //Instantiate child item
        itemGameObject = Instantiate(childObject as UnityEngine.Object, Vector3.zero, Quaternion.identity) as GameObject;
        if (itemGameObject == null){
            Debug.Log("Skipped: Child object not assigned.");
        }
        else{
            //Get parent and child mount point gameObjects
            foreach (Transform obj in this.gameObject.GetComponentsInChildren<Transform>(includeInactive: true)){
                if (obj.name == parentMountPointName){
                    parentMountPoint = obj.gameObject;
                    break;
                }
            }
            foreach (Transform obj in itemGameObject.GetComponentsInChildren<Transform>(includeInactive: true)){
                itemGameObjectTransforms.Add(obj);
                if (obj.name == childMountPointName){
                    childMountPoint = obj.gameObject;
                }
            }

            if (parentMountPoint == null){
                Debug.Log("Could not find parent mount point: '" + parentMountPointName + "'");
                DestroyImmediate(itemGameObject);
            }
            else if (childMountPoint == null){
                Debug.Log("Could not find child mount point: '" + childMountPointName + "'");
                DestroyImmediate(itemGameObject);
            }
            else{
                //Debug.Log("Attaching mount points: " + itemGameObject.name + "." + childMountPointName + " >> " + this.name + "." + parentMountPointName);
                itemGameObject.transform.rotation = parentMountPoint.transform.rotation;
                //Offset the child gameObject for instances where the child object's pivot is not the same as the mount point's position
                Vector3 childPivotRotDifference = childMountPoint.transform.rotation.eulerAngles - itemGameObject.transform.rotation.eulerAngles; 
                Vector3 childPivotPosDifference = childMountPoint.transform.position - itemGameObject.transform.position;
                itemGameObject.transform.localRotation = Quaternion.Euler(parentMountPoint.transform.rotation.eulerAngles - childPivotRotDifference);
                itemGameObject.transform.localPosition = parentMountPoint.transform.position - childPivotPosDifference;

                itemGameObject.transform.parent = parentMountPoint.transform;
            }
        }
    }

    
    void Update(){
        if (itemGameObject != null){
            itemGameObject.SetActive(childObjectActive);
        }
    }
    
}
