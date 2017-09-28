using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Brad Beachells
/// <summary>
/// This needs to be attached to a mesh that surrounds the actual turret, and turn off the mesh renderer. 
/// </summary>
public class TurretAI : MonoBehaviour {

    Coroutine LookingTarget;

   
    //when a player enters into this area it will start the targetingSystem coroutine to look at the target.
    public void OnTriggerEnter (Collider Target) {

        LookingTarget = StartCoroutine(TargetingSystem(Target));
        print("intruder detected");
        
        //get the object that entered the area to target it. 
	}

    //this will target what ever is in the trap area and look at it. When the player leaves it will stop looking and shooting
    IEnumerator TargetingSystem(Collider newTarget) {
        while (true)
        {
            
           
            this.transform.LookAt(newTarget.transform);
            //NewWeaponScript.CallOverHeat();//---------------------activate again when it is figured out how to shut the overheat script when the target leaves
            yield return null;
          

        }
       
       
    }
    public void OnTriggerExit(Collider Target)
    {
        StopCoroutine(LookingTarget);
    }
}

