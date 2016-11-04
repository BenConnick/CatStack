using UnityEngine;
using System.Collections;

public class StringToy : MonoBehaviour {

    public Transform rat;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Grabbed()
    {
        Manager.instance.ActiveToy = rat;
    }

    public void Released()
    {
        if (Manager.instance.ActiveToy == rat)
        {
            Manager.instance.ActiveToy = null;
        }
        EndTugOfWar();
    }

    public void BeginTugOfWar()
    {
        rat.GetComponent<SpringJoint>().maxDistance = 2;
        Manager.instance.ActiveToy = null;
    }

    public void EndTugOfWar()
    {
        if (rat.GetComponent<FixedJoint>() != null)
        {
            rat.GetComponent<FixedJoint>().connectedBody.GetComponent<Cat>().LetGo();
            rat.GetComponent<SpringJoint>().maxDistance = 0.5f;
        }
    }
}
