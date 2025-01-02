    using UnityEngine;
using System.Collections;
using UnityEngine.UI;

  

public class OrbUI : MonoBehaviour 
{
    public Material thisOrbMaterial;
	
    public void ChangeOrbValue(float value)
    {
        thisOrbMaterial.SetFloat("_Value", value);
    }

}
