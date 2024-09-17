using UnityEngine;
using System.Collections;

public class State {

	public SMDelegate onUpdate;
	public string name;
	
	// Use this for initialization
	public State ( string name ) {
		this.name = name;
	}
	
	// Update is called once per frame
	void Update () {
		if (onUpdate != null)
		{
			onUpdate();
		}
	}
}
